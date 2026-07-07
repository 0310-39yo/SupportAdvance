# 技術仕様書 — IValidateWithClock

**プロジェクト:** SupportAdvance  
**レイヤ:** Domain 層 / SharedKernel  
**種別:** インターフェース技術仕様  
**版:** 1.0 / 2026-07-07

---

## 1. インターフェース概要

### 1.1 定義

```csharp
public interface IValidateWithClock<TValue>
{
    void ValidateWithClock(TValue value, IClock clock);
}
```

| 項目 | 内容 |
|------|------|
| インターフェース名 | `IValidateWithClock<TValue>` |
| 名前空間 | `SupportAdvance.SharedKernel.ValueObjects` |
| ジェネリック型 | `TValue` — 検証対象の値の型 |
| パラメータ | `value` — 検証対象値、`clock` — 現在時刻供給元 |
| メソッド数 | 1 |

### 1.2 責務

- **時間軸依存のビジネスロジック検証** — 現在時刻に基づくルール判定
- **期間判定** — 「昨日以降か」「有効期限内か」など
- **時間帯ベースの制約** — 「営業時間内か」「予約可能な時間帯か」など
- **例外の発行** — 検証失敗時に適切な例外をスロー

**チェックしない責務:**
- ❌ 形式チェック（型、範囲、制約）— → `IValidatable`
- ❌ 外部リソース確認（DB、API） — → Application 層
- ❌ `DateTime.Now` の直接使用 — Clock 抽象化を利用

---

## 2. メソッド仕様

### 2.1 `ValidateWithClock(TValue value, IClock clock)`

| 項目 | 内容 |
|------|------|
| シグネチャ | `void ValidateWithClock(TValue value, IClock clock)` |
| パラメータ 1 | `value` — 検証対象の値 |
| パラメータ 2 | `clock` — `IClock` インスタンス（現在時刻供給） |
| 戻り値 | なし（検証成功時） |
| 例外 | `ArgumentException` 等（検証失敗時） |

**責務:**

現在時刻（`clock` 経由）に依存するビジネスロジック的な検証を実施する。

**検証対象の例:**

| ケース | 実装例 |
|-------|--------|
| 未来日は不可 | `if (value > clock.UtcNow) throw new ArgumentException(...)` |
| 営業時間内か | `if (value.Hour < 9 || value.Hour >= 18) throw ...` |
| 有効期限内か | `if (value < clock.UtcNow) throw ...` |
| 昨日以降か | `if (value.Date < clock.UtcNow.AddDays(-1).Date) throw ...` |

**実装ガイドライン:**

```csharp
public void ValidateWithClock(DateTime respondedAt, IClock clock)
{
    // ✅ 現在時刻と比較（Clock 経由）
    if (respondedAt > clock.UtcNow)
        throw new ArgumentException("回答日時は未来日になることはできません。");

    // ✅ 期間チェック
    var oneDayAgo = clock.UtcNow.AddDays(-1);
    if (respondedAt < oneDayAgo)
        throw new ArgumentException("回答日時は直近24時間以内である必要があります。");
}
```

---

## 3. IValidatable との関係

### 3.1 責務の分離

| インターフェース | 検証内容 | Clock 依存 | 実行タイミング |
|----------|----------|-----------|----------|
| `IValidatable` | 形式（型、範囲、制約） | No | コンストラクタ内 |
| `IValidateWithClock` | ビジネスロジック（時間軸） | **Yes** | `From()` / `TryFrom()` 内 |

### 3.2 一般的な使用パターン

**パターン A: 形式検証のみ（Clock 不要）**

```csharp
public sealed class RespondentAge : PrimitiveValueObject<int>
{
    private RespondentAge(int value) : base(value, true) { }

    public static RespondentAge From(int value) => new(value);

    public override void Validate(int age)
    {
        // Clock 不要（単純な範囲チェック）
        if (age < 0 || age > 150)
            throw new ArgumentOutOfRangeException(nameof(age));
    }
}
```

**パターン B: 形式検証 + ビジネスロジック検証（Clock 必須）**

```csharp
public sealed class RespondentAt : PrimitiveValueObject<DateTime>
{
    private RespondentAt(DateTime value) : base(value, true) { }

    public static RespondentAt From(DateTime value, IClock clock)
    {
        var instance = new(value);  // Validate が自動実行
        instance.ValidateWithClock(value, clock);  // ビジネス検証
        return instance;
    }

    // 形式検証（Clock 不要）
    public override void Validate(DateTime dt)
    {
        if (dt == DateTime.MinValue || dt == DateTime.MaxValue)
            throw new ArgumentException("有効な日時ではありません。");
    }

    // ビジネス検証（Clock 必須）
    public void ValidateWithClock(DateTime dt, IClock clock)
    {
        if (dt > clock.UtcNow)
            throw new ArgumentException("未来日は不可です。");
    }
}
```

---

## 4. Clock の役割

### 4.1 Clock インターフェースの利用

```csharp
// ❌ 直接使用（絶対NG）
var now = DateTime.UtcNow;  // テスト不可、仕様に違反

// ✅ Clock 経由（正しい）
var now = clock.UtcNow;  // テスト可能、仕様準拠
```

### 4.2 IClock の特徴

- **テスト友好的** — テストで時刻を固定可能
- **仕様準拠** — Domain 層が外部依存（システム時刻）を避ける
- **一貫性** — プロジェクト全体で同じ時刻供給元を使用

---

## 5. 例外仕様

### 5.1 例外の種類

| 例外 | 用途 | 例 |
|------|------|-----|
| `ArgumentException` | 最も一般的なビジネスロジック違反 | 「未来日は不可」 |
| その他 | 特殊なケースで使用可能 | 必要に応じて判断 |

### 5.2 例外メッセージの指針

```csharp
// ✅ 良い例（ユーザーに分かりやすい）
throw new ArgumentException(
    "回答日時は現在より後の日時にすることはできません。"
);

// ❌ 悪い例
throw new Exception("Invalid datetime");  // 技術的すぎる
```

---

## 6. 実装パターン

### 6.1 IOptionalValueObject との組み合わせ

```csharp
public sealed class RespondentAt : PrimitiveValueObject<DateTime>,
    IOptionalValueObject<RespondentAt, DateTime>
{
    private RespondentAt(bool isSet) : base(isSet) { }
    private RespondentAt(DateTime value, bool isSet) : base(value, isSet) { }

    // Unset インスタンスを返す
    public static RespondentAt Unset() => new(false);

    // From — Clock を使用して検証済みインスタンスを生成
    public static RespondentAt From(DateTime value, IClock clock)
    {
        var instance = new RespondentAt(value, true);
        instance.ValidateWithClock(value, clock);
        return instance;
    }

    // TryFrom — 検証失敗時は false を返す
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

    public DateTime Value => ValueField;

    // 形式検証
    public override void Validate(DateTime dt)
    {
        if (dt == DateTime.MinValue || dt == DateTime.MaxValue)
            throw new ArgumentException("有効な日時ではありません。");
    }

    // ビジネス検証（Clock 依存）
    public void ValidateWithClock(DateTime dt, IClock clock)
    {
        if (dt > clock.UtcNow)
            throw new ArgumentException("回答日時は未来日にできません。");
    }

    protected override IEnumerable<object?> GetValueComponents()
    {
        if (IsSet) yield return ValueField;
    }
}
```

---

## 7. 実装チェックリスト

- [ ] `IValidateWithClock<TValue>` を実装する
- [ ] `ValidateWithClock()` メソッドを実装
- [ ] 時刻比較で `DateTime.Now/UtcNow` を直接使用していない
- [ ] `clock` パラメータを経由して時刻を取得
- [ ] ビジネスロジック検証のみを実施（形式チェックは `Validate()` で）
- [ ] 検証失敗時に適切な例外をスロー
- [ ] `From(TValue, IClock)` で `ValidateWithClock()` を明示的に呼び出し
- [ ] `TryFrom()` で例外をキャッチして false を返す
- [ ] テストで Clock をモック化できることを確認

---

## 8. 使用シーン

### 8.1 API リクエスト処理

```csharp
[HttpPost]
public IActionResult Create([FromBody] CreateRespondentRequest request, IClock clock)
{
    // API 層で Clock を注入して UseCase に渡す
    try
    {
        var respondedAt = RespondentAt.From(request.RespondedAt, clock);
        // ... UseCase 実行
    }
    catch (ArgumentException ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}
```

### 8.2 リポジトリの復元

```csharp
public RespondentAt RestoreFromDb(DateTime dbValue, IClock clock)
{
    // DB から読み込んだ値を検証付きで復元
    return RespondentAt.From(dbValue, clock);
}
```

---

## 9. よくある間違い

| 間違い | 理由 | 正しい方法 |
|--------|------|----------|
| `DateTime.Now` を使用 | テスト不可 | `clock.UtcNow` |
| `Validate()` で時刻比較 | Clock 依存性の混在 | `ValidateWithClock()` で実施 |
| `ValidateWithClock()` をコンストラクタで呼び出す | Clock パラメータがない | `From()` / `TryFrom()` で呼び出し |
| Exception を直接スロー | 標準例外でない | `ArgumentException` 等を使用 |

---

## 10. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-07-07 | Claude Code | 初版作成 |
