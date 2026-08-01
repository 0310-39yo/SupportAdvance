# Entity 設計ガイドライン

Domain Driven Design（DDD）における Entity と AggregateRoot の実装原則です。

---

## 📋 設計原則

### Entity<long> パターン

SupportAdvance プロジェクトではすべての Entity が `Entity<long>` を継承します。ここで `long` は **row_id** を表します。

```csharp
public abstract class Entity<TId>
{
    public TId Id { get; set; }
    public byte[] RowVersion { get; set; } = null!;
    
    // ドメインイベント
    protected List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    public void RaiseDomainEvent(IDomainEvent @event)
    {
        _domainEvents.Add(@event);
    }
}
```

### row_id とビジネス識別子の分離

Entity には **2つの識別子概念**があります：

| 種類 | row_id | ビジネス識別子 |
|---|---|---|
| **型** | long（Entity.Id） | ValueObject（UserId など） |
| **用途** | システム技術的なPK | ドメイン上の識別子 |
| **参照** | DB での参照、Repository で使用 | ビジネスロジックで使用 |
| **例** | 1, 2, 3, ... | UserId(12345), ProductCode("PROD-001") |

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

namespace SupportAdvance.Contexts.YourGroup.YourContext.Domain.Entities;

public class YourEntity : Entity<long>
{
    private YourBusinessId _yourBusinessId = null!;
    private string _name = null!;
    private LocalDateTime _createdAt;

    // ビジネス識別子（外部から参照可能）
    public YourBusinessId YourBusinessId => _yourBusinessId;
    public string Name => _name;
    public LocalDateTime CreatedAt => _createdAt;

    // コンストラクタ（主にDB復元用）
    public YourEntity(long rowId, YourBusinessId yourBusinessId, string name, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(yourBusinessId);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(clock);

        // row_id を Entity.Id に設定
        Id = rowId;
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

namespace SupportAdvance.Contexts.YourGroup.YourContext.Domain.Entities;

public class YourAggregate : AggregateRoot<long>
{
    private YourBusinessId _aggregateId = null!;
    private List<YourEntity> _children = new();

    public YourBusinessId AggregateId => _aggregateId;
    public IReadOnlyList<YourEntity> Children => _children.AsReadOnly();

    public YourAggregate(long rowId, YourBusinessId aggregateId, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(aggregateId);
        ArgumentNullException.ThrowIfNull(clock);

        Id = rowId;
        _aggregateId = aggregateId;
    }

    public void AddChild(YourEntity child)
    {
        ArgumentNullException.ThrowIfNull(child);
        
        if (_children.Any(c => c.Id == child.Id))
            throw new InvalidOperationException("同じ子要素は追加できません。");
        
        _children.Add(child);
        
        // ドメインイベント発行
        RaiseDomainEvent(new YourChildAddedEvent(this.Id, child.Id));
    }
}
```

---

## 🔄 ドメインイベント

Entity がドメイン内の重要な変更を発行します。

### イベント定義

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Domain/DomainEvents/YourEntityCreatedEvent.cs

using SupportAdvance.SharedKernel.DomainEvents;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Domain.DomainEvents;

public class YourEntityCreatedEvent : IDomainEvent
{
    public long AggregateRootId { get; }
    public DateTime OccurredOn { get; }

    public YourEntityCreatedEvent(long rowId)
    {
        AggregateRootId = rowId;
        OccurredOn = DateTime.UtcNow;
    }
}
```

### イベント発行

```csharp
public class YourEntity : Entity<long>
{
    public YourEntity(long rowId, YourBusinessId yourBusinessId, IClock clock)
    {
        Id = rowId;
        _yourBusinessId = yourBusinessId;

        // ドメインイベント発行（rowId を パラメータに使用）
        RaiseDomainEvent(new YourEntityCreatedEvent(rowId));
    }
}
```

---

## ✅ 実装チェックリスト

### Entity 定義

- [ ] **Entity<long>** を継承（row_id がID）
- [ ] **ビジネス識別子** は別 ValueObject プロパティ
- [ ] **不変性**: private フィールド、public プロパティ（get のみ）
- [ ] **コンストラクタ**: 必須フィールドをすべて引数に
- [ ] **ビジネスロジック**: DDD 仕様に沿った操作メソッド
- [ ] **ドメインイベント**: 重要な変更は RaiseDomainEvent()

### ValueObject

- [ ] **不変**: 作成後の変更禁止
- [ ] **等価性**: GetAtomicValues() で実装
- [ ] **Factory メソッド**: From() で構築
- [ ] **検証**: コンストラクタで不正値をチェック

### AggregateRoot

- [ ] **AggregateRoot<long>** を継承
- [ ] **ビジネス識別子** は ValueObject
- [ ] **子要素** は List<Entity> で管理
- [ ] **トランザクション境界**: Aggregate 内で一貫性を保証
- [ ] **ドメインイベント**: 重要な変更を発行

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
