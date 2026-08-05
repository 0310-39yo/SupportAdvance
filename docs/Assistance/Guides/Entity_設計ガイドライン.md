# Entity 設計ガイドライン

Domain Driven Design（DDD）における Entity と AggregateRoot の実装原則です。

---

## 📋 設計原則

### Entity<RowId> パターン

SupportAdvance プロジェクトではすべての Entity が `Entity<RowId>` を継承します。ここで `RowId` は **row_id** を表す ValueObject です。

```csharp
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

public abstract class Entity<TId>
    where TId : notnull
{
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

**RowId ValueObject:**
- `RowId.From(long)` - 値が指定された RowId を生成
- `RowId.New()` - 未採番状態（value=0）を生成
- DB 採番後に実際の rowId に更新

### row_id とビジネス識別子の分離

Entity には **2つの識別子概念**があります：

| 種類 | row_id | ビジネス識別子 |
|---|---|---|
| **型** | RowId ValueObject | ValueObject（UserId など） |
| **用途** | システム技術的なPK | ドメイン上の識別子 |
| **参照** | DB での参照、Repository で使用 | ビジネスロジックで使用 |
| **例** | RowId.From(1), RowId.New() | UserId(12345), ProductCode("PROD-001") |

---

## 💡 実装例

### ビジネス識別子の定義

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Domain/ValueObjects/YourBusinessId.cs

using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Domain.ValueObjects;

public class YourBusinessId : ValueObject
{
    public int Value { get; }

    private YourBusinessId(int value)
    {
        if (value <= 0)
            throw new ArgumentException("ビジネスIDは正の値である必要があります。");
        Value = value;
    }

    public static YourBusinessId From(int value)
    {
        return new YourBusinessId(value);
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
```

### Entity の実装

#### 基本的な Entity

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Domain/Entities/YourEntity.cs

using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Domain.Entities;

public class YourEntity : Entity<RowId>
{
    private YourBusinessId _yourBusinessId = null!;
    private string _name = null!;
    private LocalDateTime _createdAt;

    // ビジネス識別子（外部から参照可能）
    public YourBusinessId YourBusinessId => _yourBusinessId;
    public string Name => _name;
    public LocalDateTime CreatedAt => _createdAt;

    // コンストラクタ（主にDB復元用）
    public YourEntity(YourBusinessId yourBusinessId, string name, IClock clock, RowId? rowId = null)
    {
        ArgumentNullException.ThrowIfNull(yourBusinessId);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(clock);

        // row_id (RowId ValueObject) を Entity.Id に設定
        Id = rowId ?? RowId.New();  // 未採番の場合は RowId.New()
        _yourBusinessId = yourBusinessId;
        _name = name;
        _createdAt = clock.JstNow;
    }

    // ビジネスロジック
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("名前は空にできません。");
        
        _name = newName;
    }
}
```

#### AggregateRoot

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Domain/Entities/YourAggregate.cs

using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Domain.Entities;

public class YourAggregate : AggregateRoot<RowId>
{
    private YourBusinessId _aggregateId = null!;
    private List<YourEntity> _children = new();

    public YourBusinessId AggregateId => _aggregateId;
    public IReadOnlyList<YourEntity> Children => _children.AsReadOnly();

    public YourAggregate(YourBusinessId aggregateId, IClock clock, RowId? rowId = null)
    {
        ArgumentNullException.ThrowIfNull(aggregateId);
        ArgumentNullException.ThrowIfNull(clock);

        Id = rowId ?? RowId.New();  // 未採番の場合は RowId.New()
        _aggregateId = aggregateId;
    }

    public void AddChild(YourEntity child)
    {
        ArgumentNullException.ThrowIfNull(child);
        
        if (_children.Any(c => c.Id == child.Id))
            throw new InvalidOperationException("同じ子要素は追加できません。");
        
        _children.Add(child);
        
        // ドメインイベント発行
        RaiseDomainEvent(new YourChildAddedEvent(DomainEventId.New()));
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

namespace SupportAdvance.Contexts.YourGroup.YourContext.Domain.DomainEvents;

public class YourEntityCreatedEvent : IDomainEvent
{
    public DomainEventId EventId { get; }
    public LocalDateTime OccurredAt { get; }

    public YourEntityCreatedEvent(DomainEventId eventId, LocalDateTime occurredAt)
    {
        ArgumentNullException.ThrowIfNull(eventId);
        EventId = eventId;
        OccurredAt = occurredAt;
    }
}
```

### イベント発行

```csharp
public class YourEntity : Entity<RowId>
{
    public YourEntity(RowId rowId, YourBusinessId yourBusinessId, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(rowId);
        ArgumentNullException.ThrowIfNull(yourBusinessId);
        ArgumentNullException.ThrowIfNull(clock);

        Id = rowId;
        _yourBusinessId = yourBusinessId;

        // ドメインイベント発行（EventId を新規生成）
        RaiseDomainEvent(new YourEntityCreatedEvent(DomainEventId.New(), clock.JstNow));
    }
}
```

---

## ✅ 実装チェックリスト

### Entity 定義

- [ ] **Entity<RowId>** を継承（RowId ValueObject がID）
- [ ] **ビジネス識別子** は別 ValueObject プロパティ
- [ ] **不変性**: private フィールド、public プロパティ（get のみ）
- [ ] **コンストラクタ**: 必須フィールドをすべて引数に
- [ ] **ビジネスロジック**: DDD 仕様に沿った操作メソッド
- [ ] **ドメインイベント**: 重要な変更は RaiseDomainEvent()で DomainEventId.New() を使用

### ValueObject

- [ ] **不変**: 作成後の変更禁止
- [ ] **等価性**: GetAtomicValues() で実装
- [ ] **Factory メソッド**: From() で構築
- [ ] **検証**: コンストラクタで不正値をチェック

### AggregateRoot

- [ ] **AggregateRoot<RowId>** を継承（RowId ValueObject ベース）
- [ ] **ビジネス識別子** は ValueObject
- [ ] **子要素** は List<Entity> で管理
- [ ] **トランザクション境界**: Aggregate 内で一貫性を保証
- [ ] **ドメインイベント**: 重要な変更を発行（DomainEventId.New() で識別子生成）

---

## ❌ 避けるべきパターン

### パターン1: Entity が外側の層に依存

```csharp
// ✗ 禁止: Application 層の DTO を使用
public class YourEntity : Entity<long>
{
    public YourEntityDto Dto { get; }  // ✗ Application 型への依存
}
```

**修正:**
```csharp
// ✓ Domain 層の ValueObject を使用
public class YourEntity : Entity<long>
{
    public YourBusinessId YourBusinessId { get; }  // ✓ Domain ValueObject
}
```

### パターン2: Entity が DB へ直接アクセス

```csharp
// ✗ 禁止: Entity が Repository を依存
public class YourEntity : Entity<long>
{
    private IYourEntityRepository _repository;  // ✗ Infrastructure への依存
    
    public void Save()
    {
        _repository.AddAsync(this);  // ✗ Entity が永続化を決定
    }
}
```

**修正:**
```csharp
// ✓ Entity はビジネスロジックのみ
public class YourEntity : Entity<long>
{
    public void UpdateName(string newName)
    {
        _name = newName;  // ✓ ビジネスロジックのみ
    }
}

// Application/Repository が永続化を決定
public class YourUseCase
{
    public async Task Execute(UpdateEntityRequest request)
    {
        var entity = await _repository.GetAsync(request.Id);
        entity.UpdateName(request.NewName);  // Entity のビジネスロジック
        await _repository.UpdateAsync(entity);  // Repository が永続化
    }
}
```

### パターン3: ビジネス識別子がない

```csharp
// ✗ 禁止: row_id のみで Entity を識別
public class YourEntity : Entity<long>
{
    public string Name { get; }
    // ビジネス上の識別子がない
}
```

**修正:**
```csharp
// ✓ ビジネス識別子を定義
public class YourEntity : Entity<long>
{
    public YourBusinessId YourBusinessId { get; }  // ビジネス識別子
    public string Name { get; }
}
```

---

## 🔗 LocalDateTime と Entity

Entity は **常に LocalDateTime** を使用します。

```csharp
public class YourEntity : Entity<long>
{
    private LocalDateTime _createdAt;
    private LocalDateTime? _updatedAt;

    public LocalDateTime CreatedAt => _createdAt;
    public LocalDateTime? UpdatedAt => _updatedAt;

    public YourEntity(long rowId, IClock clock)
    {
        Id = rowId;
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
│   ├── YourBusinessId.cs
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

- **SharedKernel**: Entity<TId>, AggregateRoot<TId>, ValueObject の基底実装
- **ドメインイベント_設計ガイド.md**: イベント駆動設計
- **LocalDateTime_タイムゾーン_ガイド.md**: 日時型の使用規則
- **CLEAN_ARCHITECTURE_GUIDELINES.md**: アーキテクチャ違反の例
