# IValidateWithClock パターンガイド

**バージョン**: 1.0  
**最終更新**: 2026-09-12  
**適用範囲**: Domain 層 ValueObject  
**重要度**: HIGH

---

## 📋 概要

IValidateWithClock は、**現在時刻に依存するビジネスロジック検証** を ValueObject に統合するパターンです。

- **Validate**: 形式検証（型、範囲、制約） — Clock 不要
- **ValidateWithClock**: ビジネス検証（時間軸ルール） — Clock 必須

これにより、Domain 層でテスト可能かつ型安全な検証ロジックを実現します。

---

## 🎯 なぜ必要か

### 問題

1. **DateTime.Now の直接使用** — テスト不可、現在時刻に依存
2. **ビジネスロジックと形式検証の混在** — 責務が明確でない
3. **外部依存（システム時刻）** — Domain 層の独立性を損なう

### 解決

- **IValidateWithClock**: Clock 経由で時刻を取得（テスト可能）
- **責務分離**: Validate（形式） vs ValidateWithClock（ビジネス）
- **型安全**: Clock パラメータを明示的に必須にする

---

## 📌 2 つの実装パターン

### パターン 1: 必須フィールド（Clock 使用必須）

**用途**: 作成日時（CreatedAt）など、時刻チェックが必須なフィールド

```csharp
public sealed class CreatedAt : PrimitiveValueObject<LocalDateTime>
{
    private CreatedAt(LocalDateTime value) : base(value, true) { }

    // ========== ファクトリメソッド ==========

    /// Clock を使用して検証済みインスタンスを生成
    public static CreatedAt From(LocalDateTime value, IClock clock)
    {
        var instance = new(value);  // Validate が自動実行（形式検証）
        instance.ValidateWithClock(value, clock);  // ビジネス検証
        return instance;
    }

    /// null安全に生成（null=失敗）
    public static bool TryFrom(LocalDateTime? input, IClock clock, out CreatedAt result)
    {
        if (input == null || !input.HasValue)
        {
            result = null!;
            return false;  // ← null は失敗（必須フィールド）
        }

        try
        {
            result = From(input.Value, clock);
            return true;
        }
        catch (ArgumentException)
        {
            result = null!;
            return false;
        }
    }

    // ========== 検証 ==========

    /// 形式検証（Clock 不要）
    public override void Validate(LocalDateTime value)
    {
        if (value == LocalDateTime.MinValue || value == LocalDateTime.MaxValue)
            throw new ArgumentException("CreatedAt must be a valid timestamp.");
    }

    /// ビジネス検証（Clock 必須）
    public void ValidateWithClock(LocalDateTime value, IClock clock)
    {
        // 過去日付（作成は現在以前）をチェック
        if (value > clock.JstNow)
            throw new ArgumentException("CreatedAt cannot be in the future.");
    }
}
```

**特徴**:
- ✅ Clock 必須（ビジネスルール検証が不可欠）
- ✅ From() メソッドが Clock パラメータを必須にする
- ✅ TryFrom() でも Clock が必須

---

### パターン 2: オプションフィールド（Clock 使用オプション）

**用途**: 更新日時（UpdatedAt）など、時刻チェックが条件付きなフィールド

```csharp
public sealed class UpdatedAt : PrimitiveValueObject<LocalDateTime?>, IEquatable<UpdatedAt>
{
    private UpdatedAt(LocalDateTime? value, bool isSet = true) : base(value, isSet) { }

    // ========== ファクトリメソッド ==========

    /// Clock を使用して検証済みインスタンスを生成
    public static UpdatedAt From(LocalDateTime value, IClock clock)
    {
        var instance = new(value, true);  // Validate が自動実行（形式検証）
        instance.ValidateWithClock(value, clock);  // ビジネス検証
        return instance;
    }

    /// 未更新状態を表す
    public static UpdatedAt Unset() => new(LocalDateTime.MinValue, false);

    /// null安全に生成（null=Unset）
    public static bool TryFrom(LocalDateTime? input, IClock clock, out UpdatedAt result)
    {
        if (input == null || !input.HasValue)
        {
            result = Unset();  // ← null は Unset で成功（オプション）
            return true;
        }

        try
        {
            result = From(input.Value, clock);
            return true;
        }
        catch (ArgumentException)
        {
            result = Unset();
            return false;
        }
    }

    // ========== 検証 ==========

    /// 形式検証（Clock 不要）
    public override void Validate(LocalDateTime? value)
    {
        if (value == null || !value.HasValue)
            return;  // null は許容（未更新状態）

        var v = value.Value;
        if (v == LocalDateTime.MinValue || v == LocalDateTime.MaxValue)
            throw new ArgumentException("UpdatedAt must be a valid timestamp.");
    }

    /// ビジネス検証（Clock 必須）
    public void ValidateWithClock(LocalDateTime value, IClock clock)
    {
        // 更新は現在以前をチェック
        if (value > clock.JstNow)
            throw new ArgumentException("UpdatedAt cannot be in the future.");
    }

    // ========== 別名プロパティ ==========

    /// HasUpdated の別名（可読性向上）
    public bool HasUpdated => IsSet;
}
```

**特徴**:
- ✅ Unset() メソッドで未設定状態を表現
- ✅ TryFrom() で null → Unset に自動変換
- ✅ IsSet フラグで更新済み/未更新を判定

---

## 🔄 検証フロー

### From() での検証フロー

```
From(value, clock) 呼び出し
  ↓
new インスタンス化
  ├─ コンストラクタ内で Validate(value) 実行
  │  └─ 形式チェック（型、範囲、制約）
  │
  └─ 検証失敗時は例外スロー

↓（形式検証成功）

ValidateWithClock(value, clock) を明示的に呼び出し
  ├─ ビジネスロジックチェック（時間軸依存）
  │  ├─ 未来日は不可
  │  ├─ 有効期限内か
  │  └─ 期間制約など
  │
  └─ 検証失敗時は例外スロー

↓（ビジネス検証成功）

検証済みインスタンスを返却
```

### TryFrom() での検証フロー

```
TryFrom(input, clock) 呼び出し
  ↓
input が null か判定
  ├─ Yes（必須） → false を返却
  ├─ Yes（オプション） → Unset を返して true を返却
  │
  └─ No → try-catch で From() を呼び出し
      ├─ Validate 例外 → Unset/null、false を返却
      ├─ ValidateWithClock 例外 → Unset/null、false を返却
      └─ 成功 → true を返却
```

---

## 💡 実装ガイドライン

### 1. Validate は形式チェックのみ

```csharp
// ✅ 正しい
public override void Validate(DateTime value)
{
    if (value == LocalDateTime.MinValue)
        throw new ArgumentException("...");
    // 形式チェックのみ（Clock 不要）
}

// ❌ 間違い
public override void Validate(DateTime value)
{
    if (value > DateTime.UtcNow)  // ← Clock がないのに時刻チェック
        throw new ArgumentException("...");
}
```

### 2. ValidateWithClock はビジネスロジックのみ

```csharp
// ✅ 正しい
public void ValidateWithClock(DateTime value, IClock clock)
{
    if (value > clock.JstNow)  // ← Clock を使用
        throw new ArgumentException("Future date not allowed.");
}

// ❌ 間違い（重複チェック）
public void ValidateWithClock(DateTime value, IClock clock)
{
    if (value == LocalDateTime.MinValue)  // ← Validate で既にチェック
        throw new ArgumentException("...");
}
```

### 3. Clock は常に IClock 経由

```csharp
// ❌ NG
public void ValidateWithClock(DateTime value, IClock clock)
{
    if (value > DateTime.UtcNow)  // ← DateTime.UtcNow 直接使用
        throw new ArgumentException("...");
}

// ✅ OK
public void ValidateWithClock(DateTime value, IClock clock)
{
    if (value > clock.UtcNow)  // ← IClock 経由
        throw new ArgumentException("...");
}
```

### 4. From() で ValidateWithClock を明示的に呼び出す

```csharp
// ✅ 正しい
public static MyValue From(DateTime value, IClock clock)
{
    var instance = new(value);  // Validate が自動実行
    instance.ValidateWithClock(value, clock);  // ← 明示的に呼び出し
    return instance;
}

// ❌ 間違い（ビジネス検証がスキップされる）
public static MyValue From(DateTime value, IClock clock)
{
    return new(value);  // ValidateWithClock を呼ばない
}
```

---

## 📊 パターン比較表

| 項目 | 必須（形式+ビジネス） | オプション（形式+ビジネス） |
|-----|----------------------|--------------------------|
| **インターフェース** | IValidateWithClock | IOptionalValidateWithClock |
| **Unset()** | なし | あり |
| **TryFrom(null)** | false を返却 | Unset を返して true |
| **IsSet フラグ** | なし | あり（常に true） |
| **Domain での判定** | 常に有効 | IsSet で判定 |
| **使用例** | CreatedAt | UpdatedAt, DeletedAt |

---

## 🔧 実装時のチェックリスト

新規 ValueObject で IValidateWithClock を実装する際：

**基本**:
- [ ] IValidateWithClock または IOptionalValidateWithClock を実装
- [ ] Validate() を実装（形式検証のみ）
- [ ] ValidateWithClock() を実装（ビジネス検証のみ）

**ファクトリメソッド**:
- [ ] From(TValue, IClock) で両方の検証を実行
- [ ] TryFrom(TValue?, IClock, ...) で null安全に処理
- [ ] 検証失敗時に ArgumentException をスロー

**Clock の使用**:
- [ ] DateTime.Now/UtcNow を直接使用していない
- [ ] clock パラメータを経由して時刻を取得
- [ ] 検証失敗メッセージが明確か確認

**テスト可能性**:
- [ ] FixedClock をモック化してテスト可能か確認
- [ ] 時刻に依存するテストケースが存在するか

---

## 💥 よくあるエラー

### ❌ エラー 1: Validate で時刻チェック

```csharp
public override void Validate(DateTime value)
{
    if (value > DateTime.Now)  // ← Clock がないのに時刻チェック
        throw new ArgumentException("...");
}
```

**問題**: テスト不可、責務混在  
**正しい**: Validate は形式チェックのみ、ValidateWithClock でビジネス検証

### ❌ エラー 2: DateTime.UtcNow を直接使用

```csharp
public void ValidateWithClock(DateTime value, IClock clock)
{
    if (value > DateTime.UtcNow)  // ← clock を使わない
        throw new ArgumentException("...");
}
```

**問題**: テスト不可、IClock 抽象化を無視  
**正しい**: clock.UtcNow で取得

### ❌ エラー 3: From() で ValidateWithClock を呼ばない

```csharp
public static MyValue From(DateTime value, IClock clock)
{
    return new(value);  // ビジネス検証がスキップ
}
```

**問題**: ビジネスルールが適用されない  
**正しい**: 明示的に instance.ValidateWithClock(value, clock) を呼び出す

### ❌ エラー 4: TryFrom で null を false にする（オプションの場合）

```csharp
public static bool TryFrom(DateTime? input, IClock clock, out UpdatedAt result)
{
    if (input == null)
    {
        result = null!;
        return false;  // ← null は正常な未設定状態（true を返すべき）
    }
    // ...
}
```

**問題**: null を異常状態と扱っている  
**正しい**: 
```csharp
if (input == null)
{
    result = Unset();
    return true;  // null は正常
}
```

---

## 🎨 使用シーン

### シーン 1: API リクエスト処理

```csharp
[HttpPost]
public IActionResult Create(
    [FromBody] CreateEmployeeRequest request,
    IClock clock,
    ICreateEmployeeUseCase useCase)
{
    // Clock を使用して ValueObject を生成
    if (!CreatedAt.TryFrom(request.CreatedAt, clock, out var createdAt))
    {
        return BadRequest(new { message = "Invalid created at" });
    }

    // UseCase に渡す
    return Ok(useCase.Execute(createdAt));
}
```

### シーン 2: Repository での復元

```csharp
public async Task<Employee?> GetByIdAsync(EmployeeRowId id)
{
    var dbModel = await LoadFromDatabaseAsync(id);

    // DB から読み込んだ値を検証付きで復元
    if (!CreatedAt.TryFrom(dbModel.CreatedAt, _clock, out var createdAt))
        throw new InvalidOperationException($"Invalid CreatedAt for {id}");

    return new Employee(createdAt: createdAt, ...);
}
```

### シーン 3: Entity でのビジネスロジック

```csharp
public class Employee : AggregateRoot<EmployeeId>
{
    public CreatedAt CreatedAt { get; private set; }
    public UpdatedAt UpdatedAt { get; private set; }

    public void Update(EmployeeName name, IClock clock)
    {
        // 更新時に新しい UpdatedAt を生成（Clock 依存）
        this.UpdatedAt = UpdatedAt.From(clock.JstNow, clock);
    }
}
```

---

## 📖 参考ドキュメント

- [IValidateWithClock_技術仕様書.md](../../SharedKernel/ValueObjects/Abstractions/IValidateWithClock_技術仕様書.md) — インターフェース定義
- [IOptionalValidateWithClock_技術仕様書.md](../../SharedKernel/ValueObjects/Abstractions/IOptionalValidateWithClock_技術仕様書.md) — オプション版定義
- [LocalDateTime_タイムゾーン_ガイド.md](LocalDateTime_タイムゾーン_ガイド.md) — Clock と LocalDateTime の使用規則
- [null厳格性設計ガイド.md](null厳格性設計ガイド.md) — Unset() と IsSet フラグの設計
