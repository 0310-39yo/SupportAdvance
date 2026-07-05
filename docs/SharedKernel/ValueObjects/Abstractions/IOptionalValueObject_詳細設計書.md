# 詳細設計書 — IOptionalValueObject

**プロジェクト:** SupportAdvance  
**レイヤ:** Domain 層  
**種別:** インターフェース詳細設計  
**参照技術仕様書:** IOptionalValueObject技術仕様書 v1.0  
**版:** 1.0 / 2026-07-03

---

## 1. インターフェース概要

### 1.1 インターフェース定義

```csharp
public interface IOptionalValueObject<TSelf, TValue>
    where TSelf : IOptionalValueObject<TSelf, TValue>
{
    bool TryGetValue(out TValue value);
    static abstract TSelf Unset();
    static abstract TSelf From(TValue value);
    static abstract bool TryFrom(TValue? input, out TSelf result);
}
```

### 1.2 実装者責務

| 責務 | 詳細 |
|------|------|
| メソッド実装 | TryGetValue, Unset, From, TryFrom の実装 |
| null 吸収 | TryFrom() で null を Unset に変換 |
| 値検証 | From() でビジネスルールに基づき検証 |
| 例外処理 | 検証失敗時は ArgumentException またはその派生 throw |
| ValueObject継承 | IOptionalValueObject<TSelf, TValue> 実装クラスは ValueObject を継承 |

---

## 2. 実装パターン

IOptionalValueObject を実装する ValueObject は、外部入力の null を安全に吸収し、Domain 層に null を入れません。

実装ファイルの例：
- `src/domain/SupportAdvance.Domain/ValueObjects/Samples/SampleName.cs`
- `src/domain/SupportAdvance.Domain/ValueObjects/Samples/SampleAge.cs`

---

## 3. Application 層での検証フロー

```csharp
public class CreateEmployeeUseCase
{
    public void Execute(CreateEmployeeRequest request)
    {
        // TryFrom() で null を Unset に変換
        SampleName.TryFrom(request.Name, out var name);
        SampleAge.TryFrom(request.Age, out var age);

        // Domain層では null を一切知らない
        var employee = Employee.Create(name, age);
    }
}
```

---

## 4. Entity 統合例

```csharp
public class Employee : AggregateRoot
{
    public SampleName Name { get; private set; }
    public SampleAge Age { get; private set; }

    public static Employee Create(SampleName name, SampleAge age)
    {
        // Domain層は IsSet で判定（null チェックなし）
        if (!name.TryGetValue(out _))
            throw new ArgumentException("Name must be set");

        return new Employee { Name = name, Age = age };
    }
}
```

---

## 5. 層責任の明確化

| 層 | 責務 |
|------|------|
| **Presentation/API** | null を許容；`TryFrom()` で Unset に変換 |
| **Application** | null を知らない；`TryFrom()` 済みの VO を受け取る |
| **Domain** | null を一切知らない；`IsSet` / `TryGetValue()` で判定 |

---

## 6. Domain 層制約

✅ **許可**
- `TryGetValue()` / `IsSet` で判定
- `Unset()` インスタンス返却

❌ **禁止**
- `null` チェック（`== null` など）
- `null` 値返却
- ログ・DB アクセス

---

## 7. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-07-03 | 加藤 正人 | 初版作成 |
