# Entity 設計ガイドライン

Domain Driven Design（DDD）における Entity と AggregateRoot の実装原則です。

---

## 📋 設計原則

### Entity<TId> パターン

SupportAdvance プロジェクトではすべての Entity が `Entity<TId>` を継承します。ここで `TId` は **long ベースの RowId ValueObject** です。

```csharp
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

public abstract class Entity<TId>
    where TId : notnull, RowId  // ← TId は RowId を継承する型のみ許可
{
    /// <summary>
    /// Entity の行識別子（RowId）
    /// 【責務】テーブル行を一意識別（DB の物理キー）
    /// 【型安全性】集約ごとに固有の RowId 型を定義（EmployeeRowId, PersonRowId など）
    /// </summary>
    public TId RowId { get; protected set; }
    
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
- `TId` = **long ベースの RowId を継承する ValueObject**（EmployeeRowId, PersonRowId など）
- type constraint: `where TId : notnull, RowId`
- 型安全性を確保（異なる Entity の RowId は互換性なし）
- テーブル行の物理キー（DB の主キーに対応）
- システムID（ISequenceProvider で採番）

---

## 🔑 Entity の ID：役割分離

Entity は **集約ルートを一意識別する RowId** を持ちます：

| 層 | ID | 型 | 用途 | 例 |
|----|----|----|------|-----|
| **Entity ID（TId）** | EmployeeRowId | long ベース RowId | Entity 識別（テーブル物理キー） | `EmployeeRowId.From(123)` |
| **テーブル対応** | row_id | bigint | DB の PRIMARY KEY | `m_employees.row_id` |
| **表示ID**（任意） | BizCode | ValueObject | ビジネス上の表示用 | `BizCode("EMP-2026-001")` |

**重要**: Entity は **TId（RowId）で一意識別**し、これがテーブルの物理キーに対応します。

---

## 💡 実装例

### RowId の定義

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Domain/ValueObjects/YourEntityRowId.cs

using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Domain.ValueObjects;

/// <summary>
/// YourEntity を識別する行ID（long ベース）
/// 【用途】YourEntity.RowId として使用（テーブルの物理キー）
/// </summary>
public sealed class YourEntityRowId : RowId, IEquatable<YourEntityRowId>
{
    public const long MinValue = 1L;
    public long Value => ValueField;

    private YourEntityRowId(long value) : base(value, true) { }

    public static YourEntityRowId From(long value) => new(value);

    public static bool TryFromDbValue(long value, out YourEntityRowId result)
    {
        try
        {
            result = From(value);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            result = null!;
            return false;
        }
    }

    public override void Validate(long normalized)
    {
        if (normalized < MinValue)
            throw new ArgumentOutOfRangeException(
                nameof(normalized),
                $"YourEntityRowId must be >= {MinValue}");
    }

    // Equals, GetHashCode の実装（省略）
}
```

### Entity の実装

#### 基本的な Entity

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Domain/Entities/YourEntity.cs

using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.Contexts.YourGroup.YourContext.Domain.ValueObjects;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Domain.Entities;

/// <summary>
/// Entity 実装例
/// 【ID型】YourEntityRowId（long ベース、テーブルの物理キー）
/// </summary>
public sealed class YourEntity : Entity<YourEntityRowId>
{
    private string _name = null!;
    private LocalDateTime _createdAt;

    public string Name => _name;
    public LocalDateTime CreatedAt => _createdAt;

    /// <summary>
    /// プライベートコンストラクタ
    /// </summary>
    private YourEntity(
        YourEntityRowId rowId,
        string name,
        LocalDateTime createdAt)
    {
        RowId = rowId;
        _name = name;
        _createdAt = createdAt;
    }

    /// <summary>
    /// 新規作成（ファクトリメソッド）
    /// </summary>
    /// <param name="rowId">テーブル行ID（ISequenceProvider で採番、ApplicationService で事前に確定）</param>
    /// <param name="name">名前</param>
    /// <param name="clock">クロック（optional）</param>
    public static YourEntity Create(
        YourEntityRowId rowId,
        string name,
        IClock? clock = null)
    {
        ArgumentNullException.ThrowIfNull(rowId);
        ArgumentNullException.ThrowIfNull(name);

        var createdAt = clock?.JstNow ?? LocalDateTime.Now;
        var entity = new YourEntity(rowId, name, createdAt);

        if (clock != null)
        {
            entity.RaiseDomainEvent(new YourEntityCreatedEvent(
                this.RowId,  // ← AggregateRootId = YourEntityRowId
                createdAt
            ));
        }

        return entity;
    }

    /// <summary>
    /// DB から復元（ファクトリメソッド）
    /// </summary>
    public static YourEntity Reconstruct(
        YourEntityRowId rowId,
        string name,
        LocalDateTime createdAt,
        byte[]? rowVersion = null)
    {
        var entity = new YourEntity(rowId, name, createdAt);
        if (rowVersion != null)
        {
            entity.RowVersion = rowVersion;
        }
        return entity;
    }

    /// <summary>
    /// ビジネスロジック
    /// </summary>
    public void UpdateName(string newName)
    {
        ArgumentNullException.ThrowIfNull(newName);

        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("名前は空にできません。");

        _name = newName;

        RaiseDomainEvent(new YourEntityNameUpdatedEvent(
            this.RowId,  // ← AggregateRootId = YourEntityRowId
            newName
        ));
    }
}
```

#### AggregateRoot（単一テーブル）

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Domain/Entities/YourAggregate.cs

using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.Contexts.YourGroup.YourContext.Domain.ValueObjects;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Domain.Entities;

/// <summary>
/// AggregateRoot 実装例（単一テーブル集約）
/// 【ID型】YourAggregateRowId（long ベース、テーブルの物理キー）
/// 【トランザクション境界】このAggregateRoot で一貫性を保証
/// </summary>
public sealed class YourAggregate : AggregateRoot<YourAggregateRowId>
{
    private string _name = null!;
    private LocalDateTime _createdAt;

    public string Name => _name;
    public LocalDateTime CreatedAt => _createdAt;

    /// <summary>
    /// プライベートコンストラクタ
    /// </summary>
    private YourAggregate(
        YourAggregateRowId rowId,
        string name,
        LocalDateTime createdAt)
    {
        RowId = rowId;
        _name = name;
        _createdAt = createdAt;
    }

    /// <summary>
    /// 新規作成（ファクトリメソッド）
    /// </summary>
    /// <param name="rowId">テーブル行ID（ISequenceProvider で採番、ApplicationService で事前に確定）</param>
    /// <param name="name">名前</param>
    /// <param name="clock">クロック（optional）</param>
    public static YourAggregate Create(
        YourAggregateRowId rowId,
        string name,
        IClock? clock = null)
    {
        ArgumentNullException.ThrowIfNull(rowId);
        ArgumentNullException.ThrowIfNull(name);

        var createdAt = clock?.JstNow ?? LocalDateTime.Now;
        var aggregate = new YourAggregate(rowId, name, createdAt);

        if (clock != null)
        {
            aggregate.RaiseDomainEvent(new YourAggregateCreatedEvent(
                aggregate.RowId,  // ← AggregateRootId = YourAggregateRowId
                createdAt
            ));
        }

        return aggregate;
    }

    /// <summary>
    /// DB から復元（ファクトリメソッド）
    /// </summary>
    public static YourAggregate Reconstruct(
        YourAggregateRowId rowId,
        string name,
        LocalDateTime createdAt,
        byte[]? rowVersion = null)
    {
        var aggregate = new YourAggregate(rowId, name, createdAt);
        if (rowVersion != null)
        {
            aggregate.RowVersion = rowVersion;
        }
        return aggregate;
    }
}
```

#### AggregateRoot（複数テーブル集約）

複数テーブル集約の場合、親Entity（集約ルート）と子Entity（複数テーブル）を管理します。各テーブルの物理キー（RowId）をそれぞれ保持します：

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Domain/Entities/YourMultiTableAggregate.cs

using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.Contexts.YourGroup.YourContext.Domain.ValueObjects;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Domain.Entities;

/// <summary>
/// AggregateRoot 実装例（複数テーブル集約）
/// 
/// 【テーブル構成】
/// - t_your_aggregate: 集約ルート（親テーブル）
/// - t_your_children: 子Entity（子テーブル、親の行ごとに複数行）
/// 
/// 【ID 管理】
/// - 親Entity.RowId: YourAggregateRowId（long、t_your_aggregate の物理キー）
/// - 子Entity.RowId: YourChildRowId（long、t_your_children の物理キー）
/// 
/// 【マッピング】複数の1:1マッピング（各テーブル → 各Entity）
/// </summary>
public sealed class YourMultiTableAggregate : AggregateRoot<YourAggregateRowId>
{
    private List<YourChild> _children = new();  // 子Entity（各子テーブル行）
    private string _name = null!;
    private LocalDateTime _createdAt;

    public string Name => _name;
    public LocalDateTime CreatedAt => _createdAt;
    public IReadOnlyList<YourChild> Children => _children.AsReadOnly();

    /// <summary>
    /// プライベートコンストラクタ
    /// </summary>
    private YourMultiTableAggregate(
        YourAggregateRowId rowId,
        string name,
        LocalDateTime createdAt,
        List<YourChild> children)
    {
        RowId = rowId;
        _name = name;
        _createdAt = createdAt;
        _children = children ?? new();
    }

    /// <summary>
    /// 新規作成（ファクトリメソッド）
    /// </summary>
    public static YourMultiTableAggregate Create(
        YourAggregateRowId rowId,
        string name,
        IClock? clock = null)
    {
        ArgumentNullException.ThrowIfNull(rowId);
        ArgumentNullException.ThrowIfNull(name);

        var createdAt = clock?.JstNow ?? LocalDateTime.Now;
        var aggregate = new YourMultiTableAggregate(rowId, name, createdAt, new());

        if (clock != null)
        {
            aggregate.RaiseDomainEvent(new YourAggregateCreatedEvent(
                aggregate.RowId,  // ← AggregateRootId = YourAggregateRowId
                createdAt
            ));
        }

        return aggregate;
    }

    /// <summary>
    /// DB から復元（ファクトリメソッド）
    /// </summary>
    public static YourMultiTableAggregate Reconstruct(
        YourAggregateRowId rowId,
        string name,
        LocalDateTime createdAt,
        List<YourChild> children,
        byte[]? rowVersion = null)
    {
        var aggregate = new YourMultiTableAggregate(rowId, name, createdAt, children);
        if (rowVersion != null)
        {
            aggregate.RowVersion = rowVersion;
        }
        return aggregate;
    }

    /// <summary>
    /// 子要素を追加
    /// 各子Entity は独立した RowId を持つ（t_your_children テーブルの各行）
    /// </summary>
    public void AddChild(YourChild child)
    {
        ArgumentNullException.ThrowIfNull(child);

        if (_children.Any(c => c.RowId.Equals(child.RowId)))
            throw new InvalidOperationException("同じ子要素は追加できません。");

        _children.Add(child);

        RaiseDomainEvent(new YourChildAddedEvent(
            this.RowId,  // ← AggregateRootId = YourAggregateRowId
            child.RowId
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
/// - RowId: YourChildRowId（long、t_your_children テーブルの物理キー）
/// </summary>
public sealed class YourChild : Entity<YourChildRowId>
{
    private string _name = null!;
    private LocalDateTime _createdAt;

    public string Name => _name;
    public LocalDateTime CreatedAt => _createdAt;

    /// <summary>
    /// プライベートコンストラクタ
    /// </summary>
    private YourChild(
        YourChildRowId rowId,
        string name,
        LocalDateTime createdAt)
    {
        RowId = rowId;
        _name = name;
        _createdAt = createdAt;
    }

    /// <summary>
    /// 新規作成（ファクトリメソッド）
    /// </summary>
    public static YourChild Create(
        YourChildRowId rowId,
        string name,
        IClock? clock = null)
    {
        ArgumentNullException.ThrowIfNull(rowId);
        ArgumentNullException.ThrowIfNull(name);

        var createdAt = clock?.JstNow ?? LocalDateTime.Now;
        var child = new YourChild(rowId, name, createdAt);

        if (clock != null)
        {
            child.RaiseDomainEvent(new YourChildCreatedEvent(
                child.RowId,  // ← AggregateRootId = YourChildRowId
                createdAt
            ));
        }

        return child;
    }

    /// <summary>
    /// DB から復元（ファクトリメソッド）
    /// </summary>
    public static YourChild Reconstruct(
        YourChildRowId rowId,
        string name,
        LocalDateTime createdAt,
        byte[]? rowVersion = null)
    {
        var child = new YourChild(rowId, name, createdAt);
        if (rowVersion != null)
        {
            child.RowVersion = rowVersion;
        }
        return child;
    }
}
```

---

## 🔄 ドメインイベント

Entity がドメイン内の重要な変更を発行します。

### イベント定義

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Domain/DomainEvents/YourEntityCreatedEvent.cs

using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.Contexts.YourGroup.YourContext.Domain.ValueObjects;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Domain.DomainEvents;

/// <summary>
/// YourEntity が作成されたことを表すドメインイベント
/// 【AggregateRootId】YourEntityRowId（Entity の RowId）
/// </summary>
public sealed class YourEntityCreatedEvent : IDomainEvent
{
    public YourEntityRowId AggregateRootId { get; }  // ← Entity の RowId（long ベース）
    public LocalDateTime OccurredAt { get; }

    public YourEntityCreatedEvent(
        YourEntityRowId aggregateRootId,
        LocalDateTime occurredAt)
    {
        ArgumentNullException.ThrowIfNull(aggregateRootId);

        AggregateRootId = aggregateRootId;
        OccurredAt = occurredAt;
    }
}
```

### イベント発行

```csharp
public sealed class YourEntity : Entity<YourEntityRowId>
{
    public YourEntity(YourEntityRowId rowId, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(rowId);
        ArgumentNullException.ThrowIfNull(clock);

        RowId = rowId;  // Entity の RowId（long ベース、テーブル行ID）

        // ドメインイベント発行
        // AggregateRootId：この Entity の RowId（YourEntityRowId）
        RaiseDomainEvent(new YourEntityCreatedEvent(
            this.RowId,  // ← AggregateRootId = YourEntityRowId
            clock.JstNow
        ));
    }
}
```

---

## ✅ 実装チェックリスト

### Entity 定義

- [ ] **Entity<XXXRowId>** を継承（XXXRowId = long ベース RowId）
- [ ] **RowId（TId）** が Entity.RowId に設定される
- [ ] **不変性**: private フィールド、public プロパティ（get のみ）
- [ ] **ファクトリメソッド**: Create() と Reconstruct() を実装
- [ ] **Create()** パラメータ: RowId（事前採番済み）、ビジネスフィールド、clock（optional）
- [ ] **Reconstruct()** パラメータ: RowId、ビジネスフィールド、rowVersion（optional）
- [ ] **ビジネスロジック**: DDD 仕様に沿った操作メソッド
- [ ] **ドメインイベント**: 重要な変更は `RaiseDomainEvent()` で発行

### ValueObject（RowId）

- [ ] **RowId** を継承（SharedKernel.ValueObjects.Identifiers）
- [ ] **long Value** を持つ
- [ ] **不変**: 作成後の変更禁止
- [ ] **Factory メソッド**: `From(long value)` で構築
- [ ] **TryFromDbValue()**: DB値から安全に復元
- [ ] **Validate()**: MinValue チェック（`if (normalized < MinValue) throw ...`）
- [ ] **Equals / GetHashCode**: RowId ベースで実装

### AggregateRoot

- [ ] **AggregateRoot<XXXRowId>** を継承（XXXRowId = long ベース RowId）
- [ ] **子要素** は List<Entity<YYYRowId>> で管理（プライベート）
- [ ] **トランザクション境界**: AggregateRoot 内で一貫性を保証
- [ ] **ドメインイベント**: 重要な変更を発行（RowId で識別）
- [ ] **GetAllDomainEvents()**: 子要素のイベントも含む（必要に応じて）

---

## ❌ 避けるべきパターン

### パターン1：基底 RowId 型を直接使用

```csharp
// ✗ 禁止：基底RowIdを直接使用
public class YourEntity : Entity<RowId>
{
    // RowId の型が曖昧「どのテーブルのRowId？」
}

// ✓ 正しい：集約固有の RowId 型を継承
public class YourEntity : Entity<YourEntityRowId>
{
    // YourEntityRowId は RowId を継承（型安全性確保）
}
```

### パターン2：Entity が外側の層に依存

```csharp
// ✗ 禁止: Application 層の DTO を使用
public class YourEntity : Entity<YourEntityRowId>
{
    public YourEntityDto Dto { get; }  // ✗ Application 型への依存
}

// ✓ 正しい: Domain 層の ValueObject を使用
public class YourEntity : Entity<YourEntityRowId>
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

// ✓ 正しい：集約固有の RowId 型を使用
public class YourEntityCreatedEvent : IDomainEvent
{
    public YourEntityRowId AggregateRootId { get; }  // ✓ 型安全性確保
}
```

### パターン4：Create() で RowId を null 許容にする

```csharp
// ✗ 禁止：RowId を null 許容
public static YourEntity Create(
    YourEntityRowId? rowId = null,  // null 許容
    string name)
{
    var id = rowId ?? RowId.New();  // ✗ RowId.New() は存在しない
}

// ✓ 正しい：RowId は非null、事前採番
public static YourEntity Create(
    YourEntityRowId rowId,  // 非null（ISequenceProvider で採番済み）
    string name)
{
    // rowId は既に確定
}
```

---

## 🔑 オプション型 RowId を持つ Entity

複数テーブル集約の一部の Entity は、**オプション型の RowId** を持つ場合があります。

### パターン：オプション型 RowId の Entity

```csharp
// 例：Department 集約で「管理者」がいない場合
public sealed class Department : Entity<DepartmentRowId>
{
    private string _name;
    private ManagerEmployeeRowId _manager;  // オプション型RowId

    public string Name => _name;
    public ManagerEmployeeRowId Manager => _manager;
    
    /// <summary>
    /// 管理者が設定されているかを判定
    /// </summary>
    public bool HasManager => _manager.HasManager;  // IsSet の別名プロパティ使用

    private Department(
        DepartmentRowId rowId,
        string name,
        ManagerEmployeeRowId manager)
    {
        RowId = rowId;
        _name = name;
        _manager = manager ?? ManagerEmployeeRowId.Unset();  // null は Unset に変換
    }

    public static Department Create(
        DepartmentRowId rowId,
        string name,
        ManagerEmployeeRowId? manager)  // オプション
    {
        ArgumentNullException.ThrowIfNull(rowId);
        ArgumentNullException.ThrowIfNull(name);

        // null は自動的に Unset() に変換
        return new Department(
            rowId,
            name,
            manager ?? ManagerEmployeeRowId.Unset()
        );
    }

    public static Department Reconstruct(
        DepartmentRowId rowId,
        string name,
        ManagerEmployeeRowId manager,
        byte[]? rowVersion = null)
    {
        var dept = new Department(rowId, name, manager);
        if (rowVersion != null)
        {
            dept.RowVersion = rowVersion;
        }
        return dept;
    }
}
```

### 重要な特性

| 特性 | 説明 |
|------|------|
| **Unset() 状態** | `null` ではなく `Unset()` インスタンスで「値なし」を表現（型安全） |
| **IsSet フラグ** | `_manager.HasManager` で「管理者あり」を判定 |
| **TryFromDbValue()** | DB null は自動的に Unset() に変換（Mapper層） |
| **Domain 層は null 免除** | Domain ロジックでは `_manager` が常に null でない |

---

## 🔗 LocalDateTime と Entity

Entity は **常に LocalDateTime** を使用します。

```csharp
public sealed class YourEntity : Entity<YourEntityRowId>
{
    private LocalDateTime _createdAt;
    private LocalDateTime? _updatedAt;

    public LocalDateTime CreatedAt => _createdAt;
    public LocalDateTime? UpdatedAt => _updatedAt;

    private YourEntity(YourEntityRowId rowId, LocalDateTime createdAt)
    {
        RowId = rowId;
        _createdAt = createdAt;  // ✓ LocalDateTime を使用
    }

    public static YourEntity Create(
        YourEntityRowId rowId,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(rowId);
        ArgumentNullException.ThrowIfNull(clock);

        var createdAt = clock.JstNow;  // ✓ IClock から LocalDateTime を取得
        return new YourEntity(rowId, createdAt);
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
│   ├── YourEntityRowId.cs       ← Entity ID（RowId を継承）
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

- **RowId_設計ガイド.md** - long ベース RowId の実装パターン、採番方法
- **Mapper_パターンガイド.md** - 複数テーブル集約の複数1:1マッピング実装
- **Repository_パターンガイド.md** - RowId での取得パターン、DataAccess との役割分離
- **ORM_マッピング戦略.md** - LocalDateTime マッピング、複数テーブル集約の構成
- **SharedKernel** - Entity<TId>, AggregateRoot<TId>, RowId の基底実装
- **ドメインイベント_設計ガイド.md** - イベント駆動設計
- **LocalDateTime_タイムゾーン_ガイド.md** - 日時型の使用規則
- **CLEAN_ARCHITECTURE_GUIDELINES.md** - アーキテクチャ違反の例

---

## 📝 更新履歴

| 日付 | 更新内容 |
|------|---------|
| 2026-09-12（後）| **オプション型 RowId パターン追加**。Department 集約の例で、管理者がいない場合の ManagerEmployeeRowId（オプション型）の使用方法を説明。IsSet フラグと Unset() パターンを明記。RowId_設計ガイド.md との整合性を確認 |
| 2026-09-12 | **全面改版**。Entity<TId> パターンを GUID ベース AggregateId から long ベース RowId ベースに変更。実装準拠：`where TId : notnull, RowId`。Create()/Reconstruct() ファクトリメソッドの実装例を追加。複数テーブル集約の RowId 管理を明確化。ドメインイベントの ID 型を RowId ベースに統一 |
| 2026-08-07（後）| 複数テーブル集約の Entity 構造を追加。単一テーブル vs 複数テーブル集約の実装パターンを明記 |
| 2026-08-07 | AggregateId 設計への全面改版（廃版） |

