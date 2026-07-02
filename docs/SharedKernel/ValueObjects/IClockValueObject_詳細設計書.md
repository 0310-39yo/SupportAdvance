# 詳細設計書 — IClockValueObject

**プロジェクト:** SupportAdvance  
**レイヤ:** Domain 層  
**種別:** インターフェース詳細設計  
**参照技術仕様書:** IClockValueObject技術仕様書 v1.0  
**版:** 1.0 / 2026-07-03

---

## 1. インターフェース概要

### 1.1 インターフェース定義

```csharp
public interface IClockValueObject<TValue>
{
    /// <summary>
    /// 指定された時刻を基準として、値を検証する。
    /// </summary>
    /// <param name="value">検証対象の値</param>
    /// <param name="clock">現在時刻を供給する IClock インスタンス</param>
    /// <exception cref="ArgumentException">検証失敗時（未来日など）</exception>
    void ValidateWithClock(TValue value, IClock clock);
}
```

### 1.2 実装者責務

| 責務 | 詳細 |
|------|------|
| メソッド実装 | `ValidateWithClock` の検証ロジック実装 |
| 検証ルール | TValue と IClock.Now を業務ルールに基づき比較 |
| 例外処理 | 検証失敗時は ArgumentException またはその派生 throw |
| ValueObject継承 | `IClockValueObject<TValue>` 実装クラスは `ValueObject` を継承 |

---

## 2. 実装パターン（代表例）

### 2.1 DateOfBirth（生年月日） – 過去日のみ許可

```csharp
// 設計例：Domain層での検証責務を示す
public class DateOfBirth : ValueObject, IClockValueObject<DateTime>
{
    private readonly DateTime _value;

    public void ValidateWithClock(DateTime value, IClock clock)
    {
        // ビジネスルール：生年月日は過去日のみ
        if (value.Date >= clock.Now.Date)
            throw new ArgumentException(
                "DateOfBirth cannot be future date or today.");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return _value;
    }
}
```

### 2.2 RegistrationAt（登録日） – 未来日禁止

ユーザーが入力した日付を `now` と比較する ValueObject。値 ≠ now のため、自己検証が意味を持つ。

```csharp
public class RegistrationAt : ValueObject, IClockValueObject<DateTime>
{
    private readonly DateTime _value;

    public void ValidateWithClock(DateTime value, IClock clock)
    {
        // ビジネスルール：登録日は過去・現在のみ、未来は不許可
        if (value > clock.Now)
            throw new ArgumentException(
                "RegistrationAt cannot be in the future.");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return _value;
    }
}
```

---

## 2.3 IClockValueObject を継承しないケース – CreatedAt（システム生成日時）

**継承してはならない条件：ValueObject の値そのものが UseCase の `now` である場合**

UseCase が `IClock.JstNow` を事前取得した `now`（DateTime）をそのまま受け取る ValueObject は、
`ValidateWithClock(now)` で「now より後か」を検証しても、値 = now であるため常に成立し自己検証が無意味になる。
このような ValueObject には `IClockValueObject` を継承させず、クラスコメントに設計理由を必ず明記すること。

```csharp
/// <summary>
/// システムが記録する生成日時を表す ValueObject。
///
/// 【設計注記】IClockValueObject を意図的に継承していない。
/// 値は UseCase が IClock.JstNow を 1 回取得した DateTime（now）をそのまま受け取る。
/// ValidateWithClock で「now より後か」を検証しても常に成立し無意味なため、
/// 自己検証メソッドを持たないシンプルな ValueObject として設計している。
/// </summary>
public class CreatedAt : ValueObject  // IClockValueObject<DateTime> は継承しない
{
    private readonly DateTime _value;

    // 検証なし — UseCase が IClock 経由で取得した値をそのまま受け取るため
    private CreatedAt(DateTime value) { _value = value; }

    public static CreatedAt From(DateTime now) => new(now);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return _value;
    }
}
```

---

## 3. Application 層（UseCase）での検証フロー

### 3.1 時刻検証の呼び出し

```csharp
public class CreateEmployeeUseCase
{
    private readonly IClock _clock;  // DI: Infrastructure → Application

    public void Execute(CreateEmployeeRequest request)
    {
        // 1. ValueObject生成（時刻検証なし）
        var dateOfBirth = DateOfBirth.From(request.DateOfBirth);

        // 2. 時刻を基準に検証
        dateOfBirth.ValidateWithClock(dateOfBirth.Value, _clock);

        // 3. Entity生成・永続化
        var employee = Employee.Create(/* ... */);
    }
}
```

---

## 4. Entity 統合例

```csharp
public class Employee : AggregateRoot
{
    public DateOfBirth DateOfBirth { get; private set; }
    public CreatedAt CreatedAt { get; private set; }

    public static Employee Create(
        DateOfBirth dateOfBirth,
        CreatedAt createdAt)
    {
        // Domain層は時刻検証済みインスタンスを受け取る
        return new Employee
        {
            DateOfBirth = dateOfBirth,
            CreatedAt = createdAt
        };
    }
}
```

---

## 5. テスト設計（典型パターン）

### 5.1 検証メソッドのテスト例

```csharp
[TestFixture]
public class DateOfBirthValidationTests
{
    private MockClock _clock;

    [SetUp]
    public void Setup()
    {
        _clock = new MockClock(new DateTime(2024, 1, 1));
    }

    [Test]
    public void ValidateWithClock_過去日_成功()
    {
        var dob = DateOfBirth.From(new DateTime(2000, 1, 1));
        Assert.DoesNotThrow(() =>
            dob.ValidateWithClock(dob.Value, _clock));
    }

    [Test]
    public void ValidateWithClock_未来日_例外()
    {
        var dob = DateOfBirth.From(new DateTime(2030, 1, 1));
        Assert.Throws<ArgumentException>(() =>
            dob.ValidateWithClock(dob.Value, _clock));
    }
}
```

---

## 6. 層責任の明確化

| 層 | 責務 |
|------|------|
| **Domain** | ValueObject に `ValidateWithClock` 実装；ビジネスルール定義 |
| **Application** | `IClock` を DI で取得；検証メソッド呼び出し |
| **Infrastructure** | `IClock` の実装提供（DI 登録） |

---

## 7. Domain 層制約

✅ **許可**
- IClock をメソッド引数として受け取る
- 値と clock.Now の比較

❌ **禁止**
- DateTime.Now 直接使用
- IClock フィールド保持
- ログ・DB アクセス

---

## 8. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-07-03 | 加藤 正人 | 初版作成 |
