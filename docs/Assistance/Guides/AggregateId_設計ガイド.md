---
name: AggregateId_設計ガイド
description: 【廃版】GUID ベース AggregateId は実装されていません。代わりに Entity_設計ガイドライン.md の RowId ベースパターンを参照してください。
metadata:
  type: ガイドライン（廃版）
---

# ⛔ AggregateId 設計ガイド【廃版】

**このドキュメントは廃版です。**

実装では GUID ベースの AggregateId パターンは採用されていません。現在のプロジェクトでは **RowId（long ベース）** が集約IDとして使用されています。

**👉 代わりに以下を参照してください**:
- [Entity_設計ガイドライン.md](Entity_設計ガイドライン.md) — RowId ベースの現行パターン
- [RowId ドキュメント修正](../Reports/20260912_RowId採番方法ドキュメント修正.md) — RowId 採番フローの詳細

---

## ℹ️ 廃版の理由

初期設計では GUID ベースの AggregateId パターンを想定していましたが、実装では以下の理由により RowId（long ベース）に統一されました：

- **シンプル性**: GUID ベースの複雑さが不要だった
- **パフォーマンス**: long ベースの方が DB インデックス効率が良い
- **既存規約**: プロジェクトの他の部分で RowId が統一された ID として使用されていた

---

## 📋 基本原則【参考のみ：廃版内容】

### AggregateId とは（採用されていません）

**【注意】以下の内容は採用されていません。実装は Entity_設計ガイドライン.md を参照してください。**

**AggregateId** = 集約ルートを一意に識別するビジネスID（廃版）

| 項目 | 内容 |
|------|------|
| **型** | GUID ベースの ValueObject（**採用されず**） |
| **用途** | 集約全体を識別（テーブル構成に依存しない）（**採用されず**） |
| **生成** | システムが自動生成（`Guid.NewGuid()`）（**採用されず**） |
| **人間可読性** | なし（機械用）（**採用されず**） |

---

## 📋 基本原則

### AggregateId とは

**AggregateId** = 集約ルートを一意に識別するビジネスID

| 項目 | 内容 |
|------|------|
| **型** | GUID ベースの ValueObject |
| **用途** | 集約全体を識別（テーブル構成に依存しない） |
| **生成** | システムが自動生成（`Guid.NewGuid()`） |
| **人間可読性** | なし（機械用） |

---

## 🏗️ 実装パターン

### 1. 基底クラス：AggregateId（SharedKernel）

```csharp
// src/SharedKernel/ValueObjects/Identifiers/AggregateId.cs

namespace SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// 集約を一意識別する GUID ベース ValueObject の基底クラス
/// 
/// 【責務】
/// - GUID 値の保持
/// - 等価性判定（GUID ベース）
/// 
/// 【継承】
/// 各集約は AggregateId を継承し、固有の ID クラスを定義
/// 例：OrderId, EmployeeId, UserPreferencesId など
/// 
/// 【型安全性】
/// - 異なる集約のID を型チェックで区別
/// - OrderId と EmployeeId は互換性なし
/// </summary>
public abstract class AggregateId : ValueObject
{
    /// <summary>
    /// GUID 値
    /// </summary>
    public Guid Value { get; protected set; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="value">GUID 値</param>
    /// <exception cref="ArgumentException">value が Empty の場合</exception>
    protected AggregateId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AggregateId cannot be empty.", nameof(value));
        
        Value = value;
    }

    /// <summary>
    /// 等価性判定（GUID ベース）
    /// </summary>
    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    /// <summary>
    /// 文字列表現（デバッグ用）
    /// </summary>
    public override string ToString() => Value.ToString();
}
```

---

### 2. 具体的な集約ID：実装テンプレート

#### パターン A：シンプル（ビジネスルールなし）

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Domain/ValueObjects/YourAggregateId.cs

using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Domain.ValueObjects;

/// <summary>
/// YourAggregate を識別する ID（GUID ベース）
/// 【用途】YourAggregate.Id として使用
/// 【生成】通常は YourAggregateId.New() で自動生成
/// </summary>
public class YourAggregateId : AggregateId
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    public YourAggregateId(Guid value) : base(value) { }

    /// <summary>
    /// 新規 ID を生成
    /// </summary>
    public static YourAggregateId New() => new(Guid.NewGuid());

    /// <summary>
    /// GUID から ID を生成
    /// </summary>
    public static YourAggregateId From(Guid value) => new(value);
}
```

#### パターン B：ビジネスルール付き

```csharp
// ビジネス上の制約がある場合（例：系統によって採番パターンが異なるなど）

public class OrderId : AggregateId
{
    public OrderId(Guid value) : base(value)
    {
        // OrderId 固有のビジネスルール検証
        // 例：将来、注文タイプごとに異なるIDフォーマットが必要になった場合に追加
    }

    public static OrderId New() => new(Guid.NewGuid());
    public static OrderId From(Guid value) => new(value);
    
    /// <summary>
    /// OrderId 固有メソッド例
    /// 将来、ビジネスルール追加が容易
    /// </summary>
    public string ToOrderReference() => $"ORD-{Value.ToString().Substring(0, 8).ToUpper()}";
}
```

---

## 💡 実装例

### 集約での使用

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Domain/Entities/YourAggregate.cs

using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.Contexts.YourGroup.YourContext.Domain.ValueObjects;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Domain.Entities;

/// <summary>
/// YourAggregate（集約ルート）
/// 【ID型】YourAggregateId（GUID ベース）で一意識別
/// </summary>
public class YourAggregate : AggregateRoot<YourAggregateId>
{
    private RowId _aggregateRowId = null!;  // テーブルの物理キー（非公開）
    private string _name = null!;

    public string Name => _name;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="id">集約ID（GUID ベース）</param>
    /// <param name="name">名前</param>
    /// <param name="aggregateRowId">テーブルの物理キー（ISequenceProvider で採番、ApplicationService で事前に確定）</param>
    /// <param name="clock">クロック</param>
    public YourAggregate(
        YourAggregateId id,
        string name,
        RowId aggregateRowId,
        IClock? clock = null)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(aggregateRowId);

        Id = id;  // GUID ベースのビジネスID
        _name = name;
        _aggregateRowId = aggregateRowId;  // テーブルキー（ApplicationService で事前採番）

        if (clock != null)
        {
            RaiseDomainEvent(new YourAggregateCreatedEvent(
                DomainEventId.New(),
                this.Id,  // ← AggregateRootId = YourAggregateId
                clock.JstNow
            ));
        }
    }

    public void UpdateName(string newName, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(newName);
        ArgumentNullException.ThrowIfNull(clock);

        var oldName = _name;
        _name = newName;

        RaiseDomainEvent(new YourAggregateNameUpdatedEvent(
            DomainEventId.New(),
            this.Id,  // ← この集約を識別（AggregateRootId）
            oldName,
            newName,
            clock.JstNow
        ));
    }
}
```

---

## 🔄 RowId との役割分離

### 三層の ID：役割の明確化

| ID | 型 | 用途 | 例 |
|----|----|----|-----|
| **AggregateId** | GUID ValueObject | 集約を一意識別（ビジネスID） | `YourAggregateId.New()` |
| **RowId** | long ValueObject | テーブル行の物理キー | `RowId.From(123)` |
| **ビジネスID** | 任意の ValueObject | ビジネス上の表示用ID（任意） | `OrderNumber("ORD-001")` |

### マッピング例

```csharp
// Domain層
public class Order : AggregateRoot<OrderId>  // TId = OrderId（GUID）
{
    public OrderId Id { get; }  // GUID ベース、ビジネスID
    public OrderNumber OrderNumber { get; }  // "ORD-001"、表示用
    private RowId _orderRowId { get; }  // テーブルの物理キー
}

// DbModel層（DB操作用）
public class OrderDbModel
{
    public long RowId { get; set; }  // テーブルの主キー（long）
    public Guid OrderId { get; set; }  // 集約のビジネスID（GUID）
    public string OrderNumber { get; set; }  // 表示用
    // ... 他のカラム
}

// Mapper層
public class OrderMapper
{
    public Order ToDomainEntity(OrderDbModel dbModel, IClock clock)
    {
        return new Order(
            id: OrderId.From(dbModel.OrderId),  // GUID から OrderId を復元
            orderNumber: OrderNumber.From(dbModel.OrderNumber),
            orderRowId: RowId.From(dbModel.RowId),
            clock: clock
        );
    }

    public OrderDbModel ToDbModel(Order entity)
    {
        return new OrderDbModel
        {
            RowId = entity.OrderRowId.Value,  // テーブルキー（long）
            OrderId = entity.Id.Value,  // GUID として保存
            OrderNumber = entity.OrderNumber.Value,
            // ...
        };
    }
}
```

---

## 📊 イベントでの使用

### ドメインイベント定義

```csharp
// イベント内の AggregateRootId は 集約固有の ID（GUID ベース）

public interface IDomainEvent
{
    /// <summary>
    /// イベント一意識別子（GUID）
    /// 【用途】このイベント自体を識別
    /// </summary>
    DomainEventId EventId { get; }

    /// <summary>
    /// 集約ID（集約固有の GUID ベース ValueObject）
    /// 【用途】どの集約が変更されたか特定
    /// 【例】OrderId, EmployeeId, UserPreferencesId など
    /// </summary>
    // TId AggregateRootId { get; }  ← 型パラメータで表現する方がベター
}

// 具体例
public class OrderCreatedEvent : IDomainEvent
{
    public DomainEventId EventId { get; }
    public OrderId AggregateRootId { get; }  // ← OrderId（この集約の型）
    public LocalDateTime OccurredAt { get; }

    public OrderCreatedEvent(
        DomainEventId eventId,
        OrderId aggregateRootId,
        LocalDateTime occurredAt)
    {
        ArgumentNullException.ThrowIfNull(eventId);
        ArgumentNullException.ThrowIfNull(aggregateRootId);

        EventId = eventId;
        AggregateRootId = aggregateRootId;
        OccurredAt = occurredAt;
    }
}
```

---

## 🔒 型安全性

### ✅ 正しい使用法

```csharp
var orderId = OrderId.New();
var employeeId = EmployeeId.New();

var order = new Order(orderId, ...);
var employee = new Employee(employeeId, ...);

// ✅ 型チェック：異なる型を代入できない
// order.Id = employeeId;  // ← コンパイルエラー！
```

### ❌ 共通の AggregateId を直接使う場合の問題

```csharp
// ❌ 危険：型安全性がない
public class Order : AggregateRoot<AggregateId>  // 共通型
{
    // ...
}

var orderId = new AggregateId(Guid.NewGuid());
var employeeId = new AggregateId(Guid.NewGuid());

var order = new Order(orderId, ...);
employee.Id = orderId;  // ← 誤ってアサイン可能（型チェックなし！）
```

---

## 📁 ファイル配置

```
src/SharedKernel/
└── ValueObjects/
    └── Identifiers/
        └── AggregateId.cs  ← 基底クラス

src/Contexts/YourGroup/YourContext/YourContext.Domain/
└── ValueObjects/
    ├── YourAggregateId.cs  ← 具体的な集約ID
    ├── OrderId.cs
    ├── EmployeeId.cs
    └── UserPreferencesId.cs
```

---

## ✅ 実装チェックリスト

### AggregateId 基底クラス

- [ ] `AggregateId` が `ValueObject` を継承
- [ ] `Guid Value { get; protected set; }` を持つ
- [ ] Empty チェック実装
- [ ] `GetAtomicValues()` で GUID を返す

### 具体的な集約ID

- [ ] `AggregateId` を継承
- [ ] `New()` ファクトリメソッド実装
- [ ] `From(Guid value)` ファクトリメソッド実装
- [ ] コンストラクタが `base(value)` を呼び出す

### 集約での使用

- [ ] `AggregateRoot<XXXId>` で継承（XXXId = 集約固有のID）
- [ ] `Id` プロパティが `XXXId` 型
- [ ] `RowId` をプライベート属性として保持（表示しない）
- [ ] イベント発行時に `this.Id`（AggregateId）を渡す

### Mapper での対応

- [ ] DbModel の ID カラム（Guid）を集約ID に変換
- [ ] 集約ID を DbModel の ID カラムに変換
- [ ] RowId と AggregateId の対応を明示

### Repository での対応

- [ ] `GetByIdAsync(XXXId id)` メソッド実装
- [ ] 古い `GetByRowIdAsync(long rowId)` メソッドは削除予定

---

## 🔗 関連ドキュメント

- **Entity_設計ガイドライン.md** — Entity と AggregateRoot の実装
- **ドメインイベント_設計ガイド.md** — イベント内の AggregateRootId 使用法
- **Mapper_パターンガイド.md** — AggregateId ↔ RowId マッピング詳細
- **Repository_パターンガイド.md** — AggregateId での検索実装
- **20260807_AggregateId設計変更_実装計画.md** — 段階的な実装手順

---

## 📝 更新履歴

| 日付 | 更新内容 |
|------|---------|
| 2026-08-07 | 初版作成。AggregateId 基底クラス、実装パターン、RowId との役割分離 |

