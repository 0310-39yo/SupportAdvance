# 詳細設計書 — IEnumValueObject

**プロジェクト:** SupportAdvance  
**レイヤ:** Domain 層  
**種別:** インターフェース詳細設計  
**参照技術仕様書:** IEnumValueObject技術仕様書 v1.0  
**版:** 1.0 / 2026-07-03

---

## 1. インターフェース概要

### 1.1 インターフェース定義

```csharp
public interface IEnumValueObject<TSelf, TValue>
    where TSelf : class, IEnumValueObject<TSelf, TValue>
    where TValue : struct
{
    // インスタンスメソッド
    bool TryGetValue(out TValue value);
    TValue GetValue();
    
    // 静的メソッド（C# 11 以降）
    static abstract TSelf Unset();
    static abstract TSelf From(TValue value);
    static abstract bool TryFrom(TValue? input, out TSelf result);
}
```

### 1.2 実装者責務（C# 11 以降）

`IEnumValueObject<TSelf, TValue>` を実装するクラスは、以下の3つのスタティックメソッドをインターフェースの契約に従って **必ず実装** すること。

| メソッド | 役割 | 戻り値 | 例外 |
|--------|------|--------|------|
| `static TSelf Unset()` | 未設定インスタンス返却 | 未設定状態 | - |
| `static TSelf From(TValue)` | 値から生成（検証あり） | インスタンス | ArgumentOutOfRangeException |
| `static bool TryFrom(TValue?, out TSelf)` | 値から生成（例外なし） | true/false | - |

---

## 2. 実装パターン（代表例）

### 2.1 OrderStatus（int Enum） – 注文ステータス

```csharp
// 実装例
public sealed class OrderStatus : ValueObject, IEnumValueObject<OrderStatus, int>
{
    public static readonly OrderStatus Draft = new(1);
    public static readonly OrderStatus Approved = new(2);
    public static readonly OrderStatus Completed = new(3);

    private readonly int _value;

    private OrderStatus(int value)
    {
        _value = value;
        IsSet = value != 0;
    }

    // IEnumValueObject実装
    public bool TryGetValue(out int value)
    {
        if (_value == 0)
        {
            value = 0;
            return false;
        }
        value = _value;
        return true;
    }

    public int GetValue()
    {
        if (_value == 0)
            throw new ArgumentException("OrderStatus is not set");
        return _value;
    }

    // スタティックメソッド（インターフェース実装 / C# 11 以降）
    public static OrderStatus Unset() => default;

    public static OrderStatus From(int value) => value switch
    {
        1 => Draft,
        2 => Approved,
        3 => Completed,
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    public static bool TryFrom(int? input, out OrderStatus result)
    {
        if (input.HasValue)
        {
            try
            {
                result = From(input.Value);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                result = Unset();
                return false;
            }
        }
        result = Unset();
        return true;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return _value;
    }
}
```

---

## 3. Entity 統合例

```csharp
public class Order : AggregateRoot
{
    public OrderId Id { get; private set; }
    public OrderStatus Status { get; private set; }

    public static Order Create(OrderId id, OrderStatus status)
    {
        if (status == OrderStatus.Unset())
            throw new ArgumentException("Status must be set");

        return new Order
        {
            Id = id,
            Status = status
        };
    }
}
```

---

## 4. API 処理例（null許容 → Unset変換）

```csharp
[HttpPost]
public IActionResult CreateOrder([FromBody] CreateOrderRequest request)
{
    // DTOのnullableなstatusIdをOrderStatusに変換
    OrderStatus.TryFrom(request.StatusId, out var status);

    var command = new CreateOrderCommand(Status: status);
    _useCase.Execute(command);
    return Ok();
}
```

---

## 5. テスト例（典型パターン）

```csharp
[TestFixture]
public class OrderStatusTests
{
    [Test]
    public void From_有効な値_インスタンス返却()
    {
        var status = OrderStatus.From(1);
        Assert.That(status, Is.EqualTo(OrderStatus.Draft));
    }

    [Test]
    public void From_無効な値_例外()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => OrderStatus.From(99));
    }

    [Test]
    public void TryFrom_null_Unset返却()
    {
        bool result = OrderStatus.TryFrom(null, out var status);
        Assert.That(result, Is.True);
        Assert.That(status, Is.EqualTo(OrderStatus.Unset()));
    }

    [Test]
    public void TryGetValue_設定済み_true()
    {
        var status = OrderStatus.Draft;
        bool result = status.TryGetValue(out var value);
        Assert.That(result, Is.True);
        Assert.That(value, Is.EqualTo(1));
    }
}
```

---

## 6. 層責任

| 層 | 責務 |
|------|------|
| **Domain** | OrderStatus など enum VO の定義・制約；TryGetValue・GetValue 実装 |
| **Application** | `From()` / `TryFrom()` を呼び出し；enum 値の変換 |
| **Presentation/API** | null を許容；`TryFrom()` で Unset に変換してから Application 層へ |

---

## 7. Domain 層制約

✅ **許可**
- `TryGetValue()` / `GetValue()` 使用
- `Unset()` インスタンス返却
- enum 値の大小比較

❌ **禁止**
- `null` 値返却
- ログ・DB アクセス

---

## 8. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-07-03 | 加藤 正人 | 初版作成 |
