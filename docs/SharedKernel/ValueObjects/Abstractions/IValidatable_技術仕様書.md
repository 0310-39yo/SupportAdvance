# 技術仕様書 — IValidatable

**プロジェクト:** SupportAdvance  
**レイヤ:** Domain 層 / SharedKernel  
**種別:** インターフェース技術仕様  
**版:** 1.0 / 2026-07-07

---

## 1. インターフェース概要

### 1.1 定義

```csharp
public interface IValidatable<TValue>
{
    void Validate(TValue value);
}
```

| 項目 | 内容 |
|------|------|
| インターフェース名 | `IValidatable<TValue>` |
| 名前空間 | `SupportAdvance.SharedKernel.ValueObjects.Abstractions` |
| ジェネリック型 | `TValue` — 検証対象の値の型 |
| メソッド数 | 1 |

### 1.2 責務

- **形式検証** — Clock に依存しない基本的な制約チェック
- **型の有効性チェック** — `LocalDateTime.MinValue/MaxValue` など不正な値の除外
- **範囲チェック** — 年齢 0～150 など基本的な範囲制限
- **例外の発行** — 検証失敗時に適切な例外をスロー

**チェックしない責務:**
- ❌ 時刻を基準とするビジネスロジック検証（→ `IValidateWithClock`）
- ❌ 外部リソース（DB、API）の確認（→ Application 層）

---

## 2. メソッド仕様

### 2.1 `Validate(TValue value)` — 形式検証

| 項目 | 内容 |
|------|------|
| シグネチャ | `void Validate(TValue value)` |
| パラメータ | `value` — 検証対象の値（null 可能性あり） |
| 戻り値 | なし（検証成功時） |
| 例外 | `ArgumentException` / `ArgumentOutOfRangeException` / `FormatException` 等 |

**責務:**

検証対象の値が形式として有効かを判定する。Clock に依存しない基本的な制約チェックのみを実施。

**検証対象の例:**

| 値の型 | 検証項目 |
|-------|---------|
| `DateTime` | `LocalDateTime.MinValue` / `MaxValue` の除外 |
| `int`（年齢） | 0～150 の範囲内か |
| `string` | null または空文字列の除外、長さ制限 |
| `decimal`（金額） | 負数の除外、小数点以下の精度 |

**検証非対象（ビジネスロジック）:**

| ケース | 理由 | 使用すべきインターフェース |
|-------|------|-------------|
| 「未来日は不可」 | Clock 依存 | `IValidateWithClock` |
| 「DB に登録済みか」 | 外部リソース | Application 層で実施 |
| 「営業時間内か」 | Clock 依存 | `IValidateWithClock` |

**実装ガイドライン:**

```csharp
// ✅ 形式検証（正しい例）
public override void Validate(int age)
{
    if (age < 0 || age > 150)
        throw new ArgumentOutOfRangeException(nameof(age), "年齢は0～150です。");
}

// ❌ ビジネスロジック検証（NG — IValidateWithClock へ）
public override void Validate(DateTime respondedAt)
{
    var now = DateTime.UtcNow;  // Clock に依存！
    if (respondedAt > now)
        throw new ArgumentException("未来日は不可です。");
}

// ❌ 外部リソース確認（NG — Application 層へ）
public override void Validate(string email)
{
    // DB 検索は形式検証ではない
    if (await db.ExistsByEmail(email))
        throw new ArgumentException("既に登録されています。");
}
```

---

## 3. 例外仕様

### 3.1 例外の種類

| 例外 | 用途 | 例 |
|------|------|-----|
| `ArgumentException` | 値が無効 | 空文字列、不正な形式 |
| `ArgumentOutOfRangeException` | 値が許容範囲外 | 年齢が 0 未満、150 超過 |
| `FormatException` | フォーマット不正 | 日付フォーマットエラー |
| その他 | 特殊なケース | 必要に応じて使用 |

### 3.2 例外メッセージの指針

```csharp
// ✅ 良い例
throw new ArgumentOutOfRangeException(
    nameof(age),
    "年齢は0～150の範囲内である必要があります。"
);

// ❌ 悪い例
throw new Exception("Invalid");  // メッセージが不明確
```

---

## 4. 実装パターン

### 4.1 PrimitiveValueObject での実装

```csharp
public sealed class RespondentAge : PrimitiveValueObject<int>, IEquatable<RespondentAge>
{
    private RespondentAge(int value) : base(value, true) { }

    public static RespondentAge From(int value) => new(value);
    public int Value => ValueField;

    // Validate は PrimitiveValueObject<int> が呼び出す
    public override void Validate(int normalized)
    {
        if (normalized < 0 || normalized > 150)
            throw new ArgumentOutOfRangeException(
                nameof(normalized),
                "年齢は0～150の範囲内です。"
            );
    }

    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet) yield return ValueField;
    }
}
```

### 4.2 EnumValueObject での実装

```csharp
public sealed class RespondentStatus : EnumValueObject<int>
{
    public static readonly RespondentStatus Active = new(1);
    public static readonly RespondentStatus Inactive = new(2);

    private RespondentStatus(int value) : base(value) { }

    public override void Validate(int value)
    {
        if (value < 1 || value > 2)
            throw new ArgumentOutOfRangeException(
                nameof(value),
                "無効なステータス値です。"
            );
    }

    protected override string GetDisplayName() => ValueField switch
    {
        1 => "有効",
        2 => "無効",
        _ => "不明"
    };

    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet) yield return ValueField;
    }
}
```

---

## 5. 利用シーン

### 5.1 ValueObject 派生クラスでの自動実行

```csharp
// PrimitiveValueObject のコンストラクタで自動実行
// 1. Normalize(value) → 正規化
// 2. Validate(正規化済み値) → 検証（この step）
private PrimitiveValueObject(TValue value, bool isSet)
{
    IsSet = isSet;
    ValueField = isSet ? Normalize(value) : default!;
    if (isSet)
    {
        Validate(ValueField);  // ← ここで実行
    }
}
```

### 5.2 From メソッドでの実行フロー

```csharp
public static RespondentAge From(int value)
{
    // コンストラクタが呼ばれ、その中で Validate が実行される
    return new(value);
    // 検証失敗時は例外をスロー
}

// 使用側
try
{
    var age = RespondentAge.From(25);  // 成功
}
catch (ArgumentOutOfRangeException ex)
{
    // 検証失敗時処理
}
```

---

## 6. Clock との関係

### 6.1 分責の原則

| インターフェース | 責務 | Clock | 実行タイミング |
|----------|------|-------|----------|
| `IValidatable` | 形式検証 | 不要 | コンストラクタ内 |
| `IValidateWithClock` | ビジネスロジック検証 | 必須 | `From()` / `TryFrom()` 明示的実行 |

### 6.2 両方を実装する場合

```csharp
public sealed class RespondentAt : PrimitiveValueObject<DateTime>,
    IValidatable<DateTime>,  // 形式検証
    IValidateWithClock<DateTime>  // ビジネスロジック検証
{
    private RespondentAt(DateTime value) : base(value, true) { }

    public static RespondentAt From(DateTime value, IClock clock)
    {
        var instance = new(value);  // Validate が自動実行
        instance.ValidateWithClock(value, clock);  // ビジネス検証（明示的）
        return instance;
    }

    // 形式検証（Clock 不要）
    public override void Validate(DateTime normalized)
    {
        if (normalized == LocalDateTime.MinValue || normalized == LocalDateTime.MaxValue)
            throw new ArgumentException("有効な日時ではありません。");
    }

    // ビジネス検証（Clock 必須）
    public void ValidateWithClock(DateTime value, IClock clock)
    {
        if (value > clock.UtcNow)
            throw new ArgumentException("未来日は不可です。");
    }
}
```

---

## 7. 設計上の注意点

### 7.1 null の扱い

```csharp
// ⚠️ 参照型の場合は null チェックが必要
public override void Validate(string? value)
{
    // null を受け入れない場合
    if (string.IsNullOrEmpty(value))
        throw new ArgumentException("値は必須です。");

    // null を受け入れる場合
    if (value != null && value.Length > 100)
        throw new ArgumentException("長さが制限を超えています。");
}
```

### 7.2 検証は純粋な制約チェック

```csharp
// ✅ 正しい（検証のみ）
public override void Validate(int value)
{
    if (value < 0)
        throw new ArgumentException("負数は不可。");
}

// ❌ 間違い（副作用あり）
public override void Validate(int value)
{
    if (value < 0)
    {
        logger.LogError("Invalid value");  // 副作用！
        throw new ArgumentException("Invalid");
    }
}
```

---

## 8. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-07-07 | Claude Code | 初版作成 |
