# 技術仕様書 — IOptionalValidateWithClock

**プロジェクト:** SupportAdvance  
**レイヤ:** Domain 層 / SharedKernel  
**種別:** インターフェース技術仕様  
**版:** 1.0 / 2026-07-07

---

## 1. インターフェース概要

### 1.1 定義

```csharp
public interface IOptionalValidateWithClock<TSelf, TValue> :
    IValidateWithClock<TValue>,
    IValidatable<TValue>
    where TSelf : IOptionalValidateWithClock<TSelf, TValue>
{
    bool TryGetValue(out TValue value);
    static abstract TSelf Unset();
    static abstract TSelf From(TValue value, IClock clock);
    static abstract bool TryFrom(TValue? input, IClock clock, out TSelf result);
}
```

| 項目 | 内容 |
|------|------|
| インターフェース名 | `IOptionalValidateWithClock<TSelf, TValue>` |
| 名前空間 | `SupportAdvance.SharedKernel.ValueObjects.Abstractions` |
| 継承元 | `IValidateWithClock<TValue>` + `IValidatable<TValue>` |
| ジェネリック型 | `TSelf` — 実装クラス自身, `TValue` — 内部値の型 |
| メソッド数 | 4 個 |

### 1.2 責務

- **オプション値の管理** — Unset（未設定）状態を型安全に表現
- **形式検証** — Clock 不要な基本制約チェック（`IValidatable`）
- **ビジネス検証** — Clock 必須な時間軸ルール検証（`IValidateWithClock`）
- **ファクトリ提供** — `From()`, `TryFrom()` で生成を制御
- **例外ハンドリング** — 検証失敗時の例外発行と `false` 返却の使い分け

---

## 2. 継承構造

### 2.1 インターフェース継承

```
IValidatable<TValue>
    ↑
    |
    IOptionalValidateWithClock<TSelf, TValue>
    ↑
    |
IValidateWithClock<TValue>
```

**継承の意味:**

このインターフェースを実装するクラスは、以下の両者の責務を持つ:
- `IValidatable<TValue>.Validate()` — 形式検証
- `IValidateWithClock<TValue>.ValidateWithClock()` — ビジネス検証

---

## 3. メソッド仕様

### 3.1 `TryGetValue(out TValue value)` — 値の安全な取得

| 項目 | 内容 |
|------|------|
| シグネチャ | `bool TryGetValue(out TValue value)` |
| パラメータ | `value` (out) — 取得した値 |
| 戻り値 | IsSet=true なら true、false なら false |
| 例外 | なし |

**責務:** Unset 状態での例外を避けるため、値を安全に取得する。

```csharp
public bool TryGetValue(out TValue value)
{
    if (!IsSet)
    {
        value = default!;
        return false;
    }

    value = ValueField;
    return true;
}
```

---

### 3.2 `Unset()` — 未設定インスタンス生成

| 項目 | 内容 |
|------|------|
| シグネチャ | `static abstract TSelf Unset()` |
| パラメータ | なし |
| 戻り値 | IsSet=false の未設定インスタンス |
| 例外 | なし |

**責務:** 未設定状態（null 代替）を表すインスタンスを返す。

```csharp
public static RespondentAt Unset() => new(false);
```

---

### 3.3 `From(TValue value, IClock clock)` — 検証付きインスタンス生成

| 項目 | 内容 |
|------|------|
| シグネチャ | `static abstract TSelf From(TValue value, IClock clock)` |
| パラメータ 1 | `value` — 設定する値（null でない想定） |
| パラメータ 2 | `clock` — 現在時刻を供給する IClock インスタンス |
| 戻り値 | IsSet=true の検証済みインスタンス（非null） |
| 例外 | 検証失敗時に例外をスロー |

**責務:** 形式検証 + ビジネス検証を実施し、有効なインスタンスを返す。

**実行順序:**

```
1. new インスタンス生成（Validate が自動実行）
   ↓
   └─ IValidatable.Validate(TValue) が内部で実行
2. ValidateWithClock() を明示的に呼び出し
   ↓
   └─ IValidateWithClock.ValidateWithClock(TValue, IClock) を実行
3. インスタンスを返却
```

**実装例:**

```csharp
public static RespondentAt From(DateTime value, IClock clock)
{
    var instance = new RespondentAt(value, true);  // Validate が自動実行
    instance.ValidateWithClock(value, clock);      // ビジネス検証（明示的）
    return instance;
}
```

**例外動作:**

```csharp
try
{
    // Validate で例外（形式チェック失敗）
    var respondentAt = RespondentAt.From(DateTime.MinValue, clock);
}
catch (ArgumentException ex)
{
    // コンストラクタ内の例外
}

try
{
    // ValidateWithClock で例外（ビジネスルール違反）
    var respondentAt = RespondentAt.From(DateTime.UtcNow.AddDays(1), clock);
}
catch (ArgumentException ex)
{
    // ビジネス検証での例外
}
```

---

### 3.4 `TryFrom(TValue? input, IClock clock, out TSelf result)` — 例外なし生成試行

| 項目 | 内容 |
|------|------|
| シグネチャ | `static abstract bool TryFrom(TValue? input, IClock clock, out TSelf result)` |
| パラメータ 1 | `input` — 生成に使用する値（null 許容） |
| パラメータ 2 | `clock` — 現在時刻を供給する IClock インスタンス |
| パラメータ 3 | `result` (out) — 生成結果（失敗時は Unset） |
| 戻り値 | 成功時 true、失敗時 false |
| 例外 | なし（例外を catch し false を返す） |

**責務:** Validate と ValidateWithClock を試行し、結果を bool で返す。

**返却ルール:**

| 入力 | Validate | ValidateWithClock | 戻り値 | result |
|-----|----------|------------------|--------|--------|
| null | - | - | **true** | Unset |
| 有効 | 成功 | 成功 | **true** | 設定済み |
| 無効 | 失敗 | - | **false** | Unset |
| 有効 | 成功 | 失敗 | **false** | Unset |

**実装例:**

```csharp
public static bool TryFrom(DateTime? input, IClock clock, out RespondentAt result)
{
    // null の場合は Unset として成功
    if (input is null)
    {
        result = Unset();
        return true;  // ← null は正常な未設定状態
    }

    try
    {
        result = From(input.Value, clock);  // Validate + ValidateWithClock 実行
        return true;  // 成功
    }
    catch (ArgumentException)
    {
        result = Unset();  // 失敗時は Unset
        return false;  // 検証失敗
    }
}
```

---

## 4. 検証フロー

### 4.1 From の検証フロー

```
From(TValue value, IClock clock) 呼び出し
    ↓
new インスタンス化
    ↓
コンストラクタで Normalize(value) 実行
    ↓
コンストラクタで Validate(正規化済み値) 実行
    ↓ （Validate で例外 → catch して例外送出）
    ↓
ValidateWithClock(value, clock) を明示的に呼び出し
    ↓
    ↓ （ValidateWithClock で例外 → catch して例外送出）
    ↓
インスタンスを返却
```

### 4.2 TryFrom の検証フロー

```
TryFrom(TValue? input, IClock clock, out TSelf result) 呼び出し
    ↓
input が null か判定
    ├─ Yes → Unset() を返して true 返却
    │
    └─ No → try-catch で From() を呼び出し
        ├─ 成功 → true を返却
        └─ 例外 → Unset() を result に設定、false を返却
```

---

## 5. 実装パターン

### 5.1 フル実装例

```csharp
public sealed class RespondentAt : PrimitiveValueObject<DateTime>,
    IOptionalValidateWithClock<RespondentAt, DateTime>,
    IEquatable<RespondentAt>
{
    private RespondentAt(bool isSet) : base(isSet) { }
    private RespondentAt(DateTime value, bool isSet) : base(value, isSet) { }

    // ========== ファクトリメソッド ==========

    public static RespondentAt Unset() => new(false);

    public static RespondentAt From(DateTime value, IClock clock)
    {
        var instance = new RespondentAt(value, true);  // Validate が自動実行
        instance.ValidateWithClock(value, clock);      // ビジネス検証
        return instance;
    }

    public static bool TryFrom(DateTime? input, IClock clock, out RespondentAt result)
    {
        if (input is null)
        {
            result = Unset();
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

    // ========== プロパティ ==========

    public DateTime Value => ValueField;

    public bool Equals(RespondentAt? other) => base.Equals(other);

    // ========== 検証 ==========

    public override void Validate(DateTime dt)
    {
        // 形式検証（Clock 不要）
        if (dt == DateTime.MinValue || dt == DateTime.MaxValue)
            throw new ArgumentException("有効な日時ではありません。");
    }

    public void ValidateWithClock(DateTime dt, IClock clock)
    {
        // ビジネス検証（Clock 必須）
        if (dt > clock.UtcNow)
            throw new ArgumentException("回答日時は未来日にできません。");
    }

    // ========== 等価性 ==========

    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet) yield return ValueField;
    }
}
```

---

## 6. 使用例

### 6.1 API リクエスト処理

```csharp
[HttpPost]
public IActionResult Create(
    [FromBody] CreateRespondentRequest request,
    IClock clock)
{
    // TryFrom で検証
    if (!RespondentAt.TryFrom(request.RespondedAt, clock, out var respondedAt))
    {
        return BadRequest(new { message = "Invalid responded at" });
    }

    // respondedAt が null 安全に扱える
    if (respondedAt.IsSet && respondedAt.TryGetValue(out var dt))
    {
        // 値を利用
    }

    // ... UseCase に渡す
}
```

### 6.2 DB から復元

```csharp
public RespondentAt RestoreFromDb(DateTime? dbValue, IClock clock)
{
    // DB から読み込んだ null 許容値を復元
    return RespondentAt.TryFrom(dbValue, clock, out var result)
        ? result
        : RespondentAt.Unset();
}
```

---

## 7. Clock との関係

### 7.1 Clock パラメータが必須な理由

- **テスト友好性** — テストで時刻を固定可能
- **責任の明示** — 「この検証は時刻に依存する」が明確
- **Domain 層の独立性** — システム時刻に直接依存しない

### 7.2 Clock の使用方法

```csharp
// ❌ NG：DateTime.Now を直接使用
if (value > DateTime.UtcNow)
    throw new ArgumentException("...");

// ✅ OK：Clock を使用
if (value > clock.UtcNow)
    throw new ArgumentException("...");
```

---

## 8. IOptionalValueObject との違い

| 項目 | IOptionalValueObject | IOptionalValidateWithClock |
|------|---------------------|--------------------------|
| 形式検証 | `Validate()` | `Validate()` |
| ビジネス検証 | なし | `ValidateWithClock()` |
| Clock 依存 | なし | **必須** |
| From | `From(TValue)` | `From(TValue, IClock)` |
| TryFrom | `TryFrom(TValue?)` | `TryFrom(TValue?, IClock)` |

---

## 9. チェックリスト

- [ ] `IOptionalValidateWithClock<TSelf, TValue>` を実装
- [ ] `Validate()` を実装（形式検証）
- [ ] `ValidateWithClock()` を実装（ビジネス検証）
- [ ] `Unset()` を実装
- [ ] `From(TValue, IClock)` を実装
- [ ] `TryFrom(TValue?, IClock, out TSelf)` を実装
- [ ] `TryGetValue()` を実装
- [ ] `DateTime.Now/UtcNow` を直接使用していない
- [ ] Clock パラメータ経由で時刻を取得
- [ ] 検証失敗時に適切な例外をスロー
- [ ] テストで Clock をモック化できることを確認

---

## 10. よくある間違い

| 間違い | 理由 | 正しい方法 |
|--------|------|----------|
| `From()` で Clock を省略 | ビジネス検証ができない | Clock パラメータを必須にする |
| `From()` で ValidateWithClock を呼ばない | ビジネス検証スキップ | 明示的に呼び出す |
| `Validate()` でビジネス検証 | 責務混在 | `ValidateWithClock()` で実施 |
| `TryFrom()` で null を false にする | null は正常な未設定状態 | null なら true を返す |

---

## 11. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-07-07 | Claude Code | 初版作成 |
