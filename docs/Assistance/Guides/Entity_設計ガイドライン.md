# Entity 設計ガイドライン

Domain Driven Design（DDD）における Entity と AggregateRoot の実装原則です。

---

## 📋 設計原則

### Entity<TId> パターン

SupportAdvance プロジェクトではすべての Entity が `Entity<TId>` を継承します。ここで `TId` は集約を一意に識別する **GUID ベースの ValueObject** です。

```csharp
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

public abstract class Entity<TId>
    where TId : notnull
{
    /// <summary>集約を識別する ID（GUID ベース ValueObject）</summary>
    public TId Id { get; protected set; }
    
    public byte[] RowVersion { get; set; } = null!;
    
    // ドメインイベント
    private protected List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected void RaiseDomainEvent(IDomainEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        _domainEvents.Add(@event);
    }
}
```

**型パラメータ TId について:**
- `TId` = 集約固有の ID ValueObject（OrderId, EmployeeId, UserPreferencesId など）
- GUID ベース（型安全性を確保）
- テーブル構成に依存しない論理ID
- RowId はテーブルの物理キーのみ（別途保持）

---

## 🔑 三層の ID：役割分離

Entity には **3つの ID 概念**があります：

| 層 | ID | 型 | 用途 | 例 |
|----|----|----|------|-----|
| **集約ID（TId）** | OrderId | GUID ValueObject | 集約を一意識別（ビジネスID） | `OrderId.New()` |
| **テーブルキー** | RowId | long ValueObject | テーブル行の物理キー | `RowId.From(123)` |
| **表示ID**（任意） | OrderNumber | ValueObject | ビジネス上の表示用 | `OrderNumber("ORD-001")` |

**重要**: 集約は **TId（集約ID）で識別**し、RowId はプライベート属性として隠蔽します。

---

## 💡 実装例

### 集約ID の定義

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Domain/ValueObjects/YourAggregateId.cs

using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Domain.ValueObjects;

/// <summary>
/// YourAggregate を識別する ID（GUID ベース）
/// 【用途】YourAggregate.Id として使用（テーブル構成に依存しない）
/// </summary>
public class YourAggregateId : AggregateId
{
    public YourAggregateId(Guid value) : base(value) { }

    public static YourAggregateId New() => new(Guid.NewGuid());
    public static YourAggregateId From(Guid value) => new(value);
}
```

### Entity の実装

#### 基本的な Entity

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Domain/Entities/YourEntity.cs

using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using SupportAdvance.Contexts.YourGroup.YourContext.Domain.ValueObjects;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Domain.Entities;

/// <summary>
/// Entity 実装例
/// 【ID型】YourEntityId（集約固有の GUID ベース ValueObject）
/// 【RowId】プライベート属性（テーブルの物理キー）
/// </summary>
public class YourEntity : Entity<YourEntityId>
{
    private RowId _entityRowId = null!;  // テーブルの物理キー（非公開）
    private string _name = null!;
    private LocalDateTime _createdAt;

    public string Name => _name;
    public LocalDateTime CreatedAt => _createdAt;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="id">Entity の集約ID（YourEntityId）</param>
    /// <param name="name">名前</param>
    /// <param name="entityRowId">テーブルの物理キー（ISequenceProvider で採番、ApplicationService で事前に確定）</param>
    /// <param name="clock">クロック</param>
    public YourEntity(
        YourEntityId id,
        string name,
        RowId entityRowId,
        IClock? clock = null)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(entityRowId);

        // 集約ID を Entity.Id に設定（GUID ベース、集約を一意識別）
        Id = id;
        _name = name;
        _entityRowId = entityRowId;  // テーブルキー（プライベート、ApplicationService で事前採番）
        _createdAt = clock?.JstNow ?? LocalDateTime.Now;

        if (clock != null)
        {
            RaiseDomainEvent(new YourEntityCreatedEvent(
                DomainEventId.New(),
                this.Id,  // ← AggregateRootId = YourEntityId
                _createdAt
            ));
        }
    }

    /// <summary>
    /// ビジネスロジック
    /// </summary>
    public void UpdateName(string newName, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(newName);
        ArgumentNullException.ThrowIfNull(clock);

        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("名前は空にできません。");

        var oldName = _name;
        _name = newName;

        RaiseDomainEvent(new YourEntityNameUpdatedEvent(
            DomainEventId.New(),
            this.Id,  // ← AggregateRootId = YourEntityId
            oldName,
            newName,
            clock.JstNow
        ));
    }
}
```

#### AggregateRoot（単一テーブル）

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Domain/Entities/YourAggregate.cs

using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using SupportAdvance.Contexts.YourGroup.YourContext.Domain.ValueObjects;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Domain.Entities;

/// <summary>
/// AggregateRoot 実装例（単一テーブル集約）
/// 【ID型】YourAggregateId（集約固有の GUID ベース ValueObject）
/// 【RowId】プライベート属性（テーブルの物理キー）
/// </summary>
public class YourAggregate : AggregateRoot<YourAggregateId>
{
    private RowId _aggregateRowId = null!;  // テーブルの物理キー（非公開）

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="id">集約ID（YourAggregateId）</param>
    /// <param name="aggregateRowId">テーブルの物理キー（ISequenceProvider で採番、ApplicationService で事前に確定）</param>
    /// <param name="clock">クロック</param>
    public YourAggregate(
        YourAggregateId id,
        RowId aggregateRowId,
        IClock? clock = null)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(aggregateRowId);

        // 集約ID を AggregateRoot.Id に設定（GUID ベース）
        Id = id;
        _aggregateRowId = aggregateRowId;  // テーブルキー（プライベート、ApplicationService で事前採番）

        if (clock != null)
        {
            RaiseDomainEvent(new YourAggregateCreatedEvent(
                DomainEventId.New(),
                this.Id,  // ← AggregateRootId = YourAggregateId
                clock.JstNow
            ));
        }
    }
}
```

#### AggregateRoot（複数テーブル集約）

複数テーブル集約の場合、親Entity（集約ルート）と子Entity（複数テーブル）を管理します。各テーブルの物理キー（RowId）をそれぞれ保持します：

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Domain/Entities/YourMultiTableAggregate.cs

using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using SupportAdvance.Contexts.YourGroup.YourContext.Domain.ValueObjects;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Domain.Entities;

/// <summary>
/// AggregateRoot 実装例（複数テーブル集約）
/// 
/// 【テーブル構成】
/// - t_your_aggregate: 集約ルート（parent table）
/// - t_your_children: 子Entity（child table）
/// 
/// 【ID 管理】
/// - 親Entity.Id: YourAggregateId（GUID、ビジネスID）
/// - 親Entity._aggregateRowId: RowId（long、t_your_aggregate の物理キー）
/// - 子Entity.Id: YourChildId（GUID、ビジネスID）
/// - 子Entity._childRowId: RowId（long、t_your_children の物理キー）
/// 
/// 【マッピング】各テーブルは1:1マッピング（Mapper で組み合わせ）
/// </summary>
public class YourMultiTableAggregate : AggregateRoot<YourAggregateId>
{
    private RowId _aggregateRowId = null!;  // t_your_aggregate の物理キー（非公開）
    private List<YourChild> _children = new();  // 子Entity（複数テーブル t_your_children）

    public IReadOnlyList<YourChild> Children => _children.AsReadOnly();

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="id">集約ID（YourAggregateId、GUID ビジネスID）</param>
    /// <param name="aggregateRowId">親テーブル（t_your_aggregate）の物理キー</param>
    /// <param name="clock">クロック</param>
    public YourMultiTableAggregate(
        YourAggregateId id,
        RowId? aggregateRowId = null,
        IClock? clock = null)
    {
        ArgumentNullException.ThrowIfNull(id);

        Id = id;
        _aggregateRowId = aggregateRowId ?? RowId.New();

        if (clock != null)
        {
            RaiseDomainEvent(new YourAggregateCreatedEvent(
                DomainEventId.New(),
                this.Id,
                clock.JstNow
            ));
        }
    }

    /// <summary>
    /// 子要素を追加
    /// 各子Entity は独立した RowId を保持（t_your_children テーブルの各行）
    /// </summary>
    public void AddChild(YourChild child, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(child);
        ArgumentNullException.ThrowIfNull(clock);

        if (_children.Any(c => c.Id == child.Id))
            throw new InvalidOperationException("同じ子要素は追加できません。");

        _children.Add(child);

        RaiseDomainEvent(new YourChildAddedEvent(
            DomainEventId.New(),
            this.Id,  // ← AggregateRootId = YourAggregateId
            child.Id,
            clock.JstNow
        ));
    }

    /// <summary>
    /// すべての子要素が発行したイベントを含む
    /// </summary>
    public IEnumerable<IDomainEvent> GetAllDomainEvents()
    {
        var events = DomainEvents.ToList();

        foreach (var child in _children)
        {
            events.AddRange(child.DomainEvents);
        }

        return events;
    }
}

/// <summary>
/// 子Entity（複数テーブル集約の一部）
/// 
/// 【ID 管理】
/// - Id: YourChildId（GUID ビジネスID）
/// - _childRowId: RowId（t_your_children テーブルの物理キー）
/// </summary>
public class YourChild : Entity<YourChildId>
{
    private RowId _childRowId = null!;  // t_your_children テーブルの物理キー（非公開）
    private string _name = null!;
    private LocalDateTime _createdAt;

    public string Name => _name;
    public LocalDateTime CreatedAt => _createdAt;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="id">子Entity の ID（YourChildId、GUID ビジネスID）</param>
    /// <param name="name">名前</param>
    /// <param name="childRowId">子テーブル（t_your_children）の物理キー</param>
    /// <param name="clock">クロック</param>
    public YourChild(
        YourChildId id,
        string name,
        RowId? childRowId = null,
        IClock? clock = null)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(name);

        Id = id;
        _name = name;
        _childRowId = childRowId ?? RowId.New();
        _createdAt = clock?.JstNow ?? LocalDateTime.Now;

        if (clock != null)
        {
            RaiseDomainEvent(new YourChildCreatedEvent(
                DomainEventId.New(),
                this.Id,  // ← AggregateRootId = YourChildId
                _createdAt
            ));
        }
    }
}
```

---

## 🔄 ドメインイベント

Entity がドメイン内の重要な変更を発行します。

### イベント定義

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Domain/DomainEvents/YourEntityCreatedEvent.cs

using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using SupportAdvance.Contexts.YourGroup.YourContext.Domain.ValueObjects;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Domain.DomainEvents;

/// <summary>
/// YourEntity が作成されたことを表すドメインイベント
/// 【AggregateRootId】YourEntityId（どの Entity が変更されたか）
/// 【EventId】DomainEventId（このイベント自体の識別子）
/// </summary>
public class YourEntityCreatedEvent : IDomainEvent
{
    public DomainEventId EventId { get; }
    public YourEntityId AggregateRootId { get; }  // ← 集約固有の ID 型
    public LocalDateTime OccurredAt { get; }

    public YourEntityCreatedEvent(
        DomainEventId eventId,
        YourEntityId aggregateRootId,
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

### イベント発行

```csharp
public class YourEntity : Entity<YourEntityId>
{
    public YourEntity(YourEntityId id, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(clock);

        Id = id;  // 集約ID（YourEntityId）

        // ドメインイベント発行
        // EventId：イベント自体のID
        // AggregateRootId：この Entity の ID（YourEntityId）
        RaiseDomainEvent(new YourEntityCreatedEvent(
            DomainEventId.New(),
            this.Id,  // ← AggregateRootId = YourEntityId
            clock.JstNow
        ));
    }
}
```

---

## ✅ 実装チェックリスト

### Entity 定義

- [ ] **Entity<XXXId>** を継承（XXXId = 集約固有の GUID ベース ValueObject）
- [ ] **集約ID（TId）** が Entity.Id に設定される
- [ ] **RowId** はプライベート属性（表示しない）
- [ ] **不変性**: private フィールド、public プロパティ（get のみ）
- [ ] **コンストラクタ**: 集約ID、RowId、必須フィールドを引数に
- [ ] **ビジネスロジック**: DDD 仕様に沿った操作メソッド
- [ ] **ドメインイベント**: 重要な変更は `RaiseDomainEvent()` で発行

### ValueObject（集約ID）

- [ ] **AggregateId** を継承（SharedKernel.ValueObjects.Identifiers）
- [ ] **Guid Value** を持つ
- [ ] **不変**: 作成後の変更禁止
- [ ] **Factory メソッド**: `New()`, `From(Guid value)` で構築
- [ ] **等価性**: `GetAtomicValues()` で GUID を返す
- [ ] **検証**: Empty チェック（`if (value == Guid.Empty) throw ...`）

### AggregateRoot

- [ ] **AggregateRoot<XXXId>** を継承（XXXId = 集約固有のID）
- [ ] **子要素** は List<Entity> で管理（プライベート）
- [ ] **トランザクション境界**: Aggregate 内で一貫性を保証
- [ ] **ドメインイベント**: 重要な変更を発行（`DomainEventId.New()` で識別子生成）
- [ ] **GetAllDomainEvents()**: 子要素のイベントも含む（必要に応じて）

---

## ❌ 避けるべきパターン

### パターン1：RowId を集約ID として使用

```csharp
// ✗ 禁止：RowId（long）を集約IDに
public class YourEntity : Entity<RowId>
{
    // 複数テーブル集約では「どのテーブルのRowId」か不明確
}

// ✓ 正しい：集約固有の GUID ベース ID を使用
public class YourEntity : Entity<YourEntityId>
{
    // YourEntityId は GUID ベース
    // RowId はプライベート属性
}
```

### パターン2：Entity が外側の層に依存

```csharp
// ✗ 禁止: Application 層の DTO を使用
public class YourEntity : Entity<YourEntityId>
{
    public YourEntityDto Dto { get; }  // ✗ Application 型への依存
}

// ✓ 正しい: Domain 層の ValueObject を使用
public class YourEntity : Entity<YourEntityId>
{
    // Domain 型のみ使用
}
```

### パターン3：イベント内で AggregateRootId に long を使用

```csharp
// ✗ 禁止：型不安全
public class YourEntityCreatedEvent : IDomainEvent
{
    public long AggregateRootId { get; }  // ✗ 型が曖昧、異なる集約と混在可能
}

// ✓ 正しい：集約固有の ID 型を使用
public class YourEntityCreatedEvent : IDomainEvent
{
    public YourEntityId AggregateRootId { get; }  // ✓ 型安全性確保
}
```

---

## 🔗 LocalDateTime と Entity

Entity は **常に LocalDateTime** を使用します。

```csharp
public class YourEntity : Entity<YourEntityId>
{
    private LocalDateTime _createdAt;
    private LocalDateTime? _updatedAt;

    public LocalDateTime CreatedAt => _createdAt;
    public LocalDateTime? UpdatedAt => _updatedAt;

    public YourEntity(YourEntityId id, IClock clock)
    {
        Id = id;
        _createdAt = clock.JstNow;  // ✓ IClock から LocalDateTime を取得
    }

    public void Update(IClock clock)
    {
        _updatedAt = clock.JstNow;  // ✓ LocalDateTime を使用
    }
}
```

---

## 📁 ファイル構造

```
YourContext.Domain/
├── Entities/
│   ├── YourEntity.cs
│   ├── YourAggregate.cs
│   └── YourChildEntity.cs
├── ValueObjects/
│   ├── YourAggregateId.cs      ← 集約ID（AggregateId を継承）
│   ├── YourEntityId.cs          ← Entity ID（AggregateId を継承）
│   ├── YourAmount.cs
│   └── YourStatus.cs
├── DomainEvents/
│   ├── YourEntityCreatedEvent.cs
│   ├── YourEntityUpdatedEvent.cs
│   └── YourEntityDeletedEvent.cs
└── Repositories/
    └── IYourEntityRepository.cs
```

---

## 参考資料

- **RowId**: long ベースの集約ID（テーブルの物理キーと統一）
- **Mapper_パターンガイド.md**: 複数テーブル集約の複数1:1マッピング実装
- **Repository_パターンガイド.md**: 集約ID での取得パターン、DataAccess との役割分離
- **ORM_マッピング戦略.md**: LocalDateTime マッピング、複数テーブル集約の構成
- **SharedKernel**: Entity<TId>, AggregateRoot<TId>, AggregateId の基底実装
- **ドメインイベント_設計ガイド.md**: イベント駆動設計
- **LocalDateTime_タイムゾーン_ガイド.md**: 日時型の使用規則
- **CLEAN_ARCHITECTURE_GUIDELINES.md**: アーキテクチャ違反の例

---

## 📝 更新履歴

| 日付 | 更新内容 |
|------|---------|
| 2026-08-07（後）| 複数テーブル集約の Entity 構造を追加。単一テーブル vs 複数テーブル集約の実装パターンを明記。複数 RowId 管理と子Entity構造を具体例で説明 |
| 2026-08-07 | AggregateId 設計への全面改版。Entity<RowId> → Entity<TId> パターンに変更。TId = 集約固有の GUID ベース ValueObject。RowId をテーブル物理キーに限定。三層の ID 役割分離を明記 |

