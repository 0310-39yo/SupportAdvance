# Mapper パターンガイド

Domain Entity と Infrastructure DbModel 間の純粋なマッピング実装ガイドです。

---

## 📋 基本原則

### 責務の分離

Mapper の責務は **型変換のみ**です。ビジネスコンテキストや認証情報の管理は Repository の責務です。

| 層 | 責務 | 例 |
|---|---|---|
| **Mapper** | 純粋な型変換 | AggregateId（GUID）→ DbModel、RowId（long）の変換 |
| **Repository** | ビジネスコンテキスト管理 | createdBy を ICurrentUserService から取得 |

### マッピング方向と ID の役割分離

```
Domain Entity ↔ DbModel（SQL Server）
    ↑                ↓
ビジネスロジック    技術的な永続化形式

Entity が保持する ID（独立した2つ）：
- AggregateId（GUID ベース ValueObject）：ビジネスID
- RowId（long ValueObject）：テーブルの物理キー

Mapper の責務（ValueObject ↔ primitive 型の型変換のみ）：
- Entity.Id（AggregateId） → DbModel.AggregateIdカラム（Guid）
- Entity.RowId（RowId） → DbModel.RowIdカラム（long）
- その他の ValueObject → primitive 型の相互変換

【重要】Mapper は AggregateId と RowId の対応付けは行わない
（Entity コンストラクタで両方を保持）
```

---

## 💡 実装パターン

### インターフェース定義

```csharp
// src/Infrastructure/Mappers/IEntityMapper.cs

using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.Infrastructure.Mappers;

/// <summary>
/// Entity ↔ DbModel マッパー（汎用インターフェース）
/// 
/// 【型パラメータ】
/// - TEntity: Entity<TId>（集約固有のID型）
/// - TDbModel: データベースモデル
/// - TId: Entity の ID 型（AggregateId 継承の ValueObject）
/// 
/// 【ID の役割】
/// - TId（AggregateId）: ビジネスID（Entity の Identity）
/// - DbModel.RowId: テーブルの物理キー（long）
/// </summary>
public interface IEntityMapper<TEntity, TDbModel, TId>
    where TEntity : Entity<TId>
    where TDbModel : class
    where TId : notnull
{
    /// <summary>
    /// Domain Entity → DbModel（保存用）
    /// 【責務】AggregateId → RowId 変換、その他は型変換のみ
    /// </summary>
    TDbModel ToDbModel(TEntity entity);

    /// <summary>
    /// DbModel → Domain Entity（読み取り用）
    /// 【責務】RowId → AggregateId 変換、その他は型変換のみ
    /// </summary>
    TEntity ToDomainEntity(TDbModel dbModel, IClock clock);
}
```

### Mapper 実装例

#### シンプルな Entity マッピング（GUID ベース ID）

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Infrastructure/Mappers/YourEntityMapper.cs

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.YourGroup.YourContext.Domain.Entities;
using SupportAdvance.Contexts.YourGroup.YourContext.Domain.ValueObjects;
using SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.DataAccess.Models;
using SupportAdvance.Infrastructure.Mappers;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.Mappers;

/// <summary>
/// YourEntity ↔ YourEntityDbModel マッパー
/// 
/// 【ID マッピング】
/// - Entity.Id: YourEntityId（AggregateId 継承、GUID ベース）
/// - DbModel.RowId: long（テーブル物理キー）
/// - DbModel.YourEntityId: Guid（ビジネスID、Entity の ID を保存）
/// </summary>
public class YourEntityMapper : IEntityMapper<YourEntity, YourEntityDbModel, YourEntityId>
{
    /// <summary>
    /// Domain Entity → DbModel（保存用）
    /// 【ID 変換】AggregateId.Value（Guid） → DbModel.YourEntityId（Guid）
    /// 【RowId】テーブル行の物理キー（ApplicationService で ISequenceProvider により事前採番済み）
    /// 【監査情報】Repository が設定するため、ここでは初期値のみ
    /// </summary>
    public YourEntityDbModel ToDbModel(YourEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new YourEntityDbModel
        {
            // RowId: テーブルの物理キー（ApplicationService で事前採番済み）
            // 新規作成時は ISequenceProvider で採番済み、更新時は既存値
            RowId = entity.RowId.Value,  // ApplicationService で既に確定

            // YourEntityId: Entity の集約ID（GUID ベース）を保存
            YourEntityId = entity.Id.Value,  // AggregateId → Guid 変換

            // ビジネスカラム：Entity から直接マッピング
            YourBusinessId = entity.YourBusinessId.Value,
            Name = entity.Name,
            Amount = entity.Amount?.Amount,  // ValueObject → primitive

            // 監査情報：Repository が上書き
            CreatedAt = entity.CreatedAt,
            CreatedBy = 0,  // Repository が実行ユーザーに上書き
            UpdatedAt = entity.UpdatedAt,
            UpdatedBy = null,
            DeletedAt = null,
            DeletedBy = null
        };
    }

    /// <summary>
    /// DbModel → Domain Entity（読み取り用）
    /// 【ID 変換】DbModel.YourEntityId（Guid） → Entity.Id（YourEntityId）
    /// 【RowId】DbModel.RowId（long） → Entity.RowId（RowId ValueObject）
    /// 【監査情報】DbModel から Entity へ直接マッピング
    /// </summary>
    public YourEntity ToDomainEntity(YourEntityDbModel dbModel, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(dbModel);
        ArgumentNullException.ThrowIfNull(clock);

        // 1. ID を ValueObject に変換
        var yourEntityId = YourEntityId.From(dbModel.YourEntityId);  // Guid → YourEntityId
        var yourBusinessId = YourBusinessId.From(dbModel.YourBusinessId);
        var rowId = RowId.From(dbModel.RowId);  // long → RowId ValueObject
        var amount = dbModel.Amount.HasValue ? Money.From(dbModel.Amount.Value) : null;

        // 2. Entity を構築
        var entity = new YourEntity(
            id: yourEntityId,           // 集約ID（YourEntityId）
            businessId: yourBusinessId,
            name: dbModel.Name,
            amount: amount,
            rowId: rowId,               // テーブル物理キー
            clock: clock
        );

        // 3. Entity は構築完了（DomainEvents はクリア）
        return entity;
    }
}
```

#### AggregateRoot マッピング（複数テーブル集約）

複数テーブル集約は、**複数の1:1マッピングの組み合わせ** として実装されます：

```
YourAggregate（集約ルート）
  ├─ [1:1] → YourAggregateDbModel（t_your_aggregate）
  └─ List<YourChild>（複数の子Entity）
      └─ [各1:1] → YourChildDbModel（t_your_children の各行）
```

Mapper の責務は集約全体を管理し、各Entity ↔ DbModel の1:1変換を組み合わせることです。

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Infrastructure/Mappers/YourAggregateMapper.cs

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.YourGroup.YourContext.Domain.Entities;
using SupportAdvance.Contexts.YourGroup.YourContext.Domain.ValueObjects;
using SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.DataAccess.Models;
using SupportAdvance.Infrastructure.Mappers;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.Mappers;

/// <summary>
/// YourAggregate ↔ DbModel マッパー（複数テーブル集約）
/// 
/// 【複数テーブル構成】
/// - t_your_aggregate: 集約ルート（YourAggregateId を保存）
/// - t_your_children: 子Entity（それぞれのRowIdと親YourAggregateIdを保存）
/// 
/// 【マッピング方式】複数の1:1マッピングの組み合わせ
/// - YourAggregate（1個） → YourAggregateDbModel（1個）【1:1】
/// - List&lt;YourChild&gt;（N個） → List&lt;YourChildDbModel&gt;（N個）【各1:1】
/// 
/// 【ID 管理】
/// - 親Entity: YourAggregateId（GUID ビジネスID）+ YourAggregateRowId（テーブル物理キー）
/// - 各子Entity: YourChildId（GUID ビジネスID）+ YourChildRowId（テーブル物理キー）
/// </summary>
public class YourAggregateMapper : IEntityMapper<YourAggregate, YourAggregateDbModel, YourAggregateId>
{
    private readonly YourChildEntityMapper _childMapper;

    public YourAggregateMapper(YourChildEntityMapper childMapper)
    {
        _childMapper = childMapper ?? throw new ArgumentNullException(nameof(childMapper));
    }

    /// <summary>
    /// YourAggregate → DbModels（複数の1:1マッピング）
    /// 
    /// 【処理内容】
    /// ① 親Entity → 親DbModel（1:1マッピング）
    /// ② 子Entity各々 → 子DbModel各々（複数の1:1マッピング）
    /// </summary>
    public YourAggregateDbModel ToDbModel(YourAggregate aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        // ① 親の DbModel を構築（YourAggregate → YourAggregateDbModel の1:1マッピング）
        var dbModel = new YourAggregateDbModel
        {
            // テーブルの物理キー
            RowId = aggregate.YourAggregateRowId?.Value ?? 0,

            // 集約ID（GUID ベース、ビジネスID）
            YourAggregateId = aggregate.Id.Value,

            // その他のカラム
            CreatedAt = aggregate.CreatedAt,
            CreatedBy = 0,  // Repository で設定
            UpdatedAt = aggregate.UpdatedAt,
            UpdatedBy = null,
            DeletedAt = null,
            DeletedBy = null
        };

        // ② 子要素も DbModel に変換（複数の1:1マッピング）
        // 各 YourChild → YourChildDbModel（1:1）
        dbModel.Children = aggregate.Children
            .Select(_childMapper.ToDbModel)  // 子Mapperが各1:1マッピングを処理
            .ToList();

        return dbModel;
    }

    /// <summary>
    /// DbModels → YourAggregate（複数の1:1マッピング）
    /// 
    /// 【処理内容】
    /// ① 親DbModel → 親Entity（1:1マッピング）
    /// ② 子DbModel各々 → 子Entity各々（複数の1:1マッピング）
    /// </summary>
    public YourAggregate ToDomainEntity(YourAggregateDbModel dbModel, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(dbModel);
        ArgumentNullException.ThrowIfNull(clock);

        // ① 親のID変換（親DbModel → 親Entity の1:1マッピング）
        var aggregateId = YourAggregateId.From(dbModel.YourAggregateId);  // Guid → YourAggregateId
        var aggregateRowId = RowId.From(dbModel.RowId);  // long → RowId

        // 親の Entity を構築
        var aggregate = new YourAggregate(
            id: aggregateId,
            aggregateRowId: aggregateRowId,
            clock: clock
        );

        // ② 子要素も Entity に変換して追加（複数の1:1マッピング）
        // 各 YourChildDbModel → YourChild（1:1）
        foreach (var childDbModel in dbModel.Children ?? Enumerable.Empty<YourChildEntityDbModel>())
        {
            var child = _childMapper.ToDomainEntity(childDbModel, clock);  // 子Mapperが各1:1マッピングを処理
            aggregate.AddChild(child);
        }

        return aggregate;
    }
}
```

**重要**: Mapper は集約全体を1つの単位として管理します。複数の Mapper を作成するのではなく、**1つの Mapper が複数の1:1変換を組み合わせる** ことで、テーブル構成に依存した設計判断を明示的に行います。

---

## 🔑 ID マッピングの詳細

### DbModel での ID 保持

DbModel には **2つの ID** を保持します：

```csharp
public class YourEntityDbModel
{
    /// <summary>
    /// テーブルの物理キー（row_id）
    /// 【型】long
    /// 【用途】テーブル行を識別（DB採番）
    /// 【可視性】通常は外部非公開
    /// </summary>
    public long RowId { get; set; }

    /// <summary>
    /// 集約ID（ビジネスID）
    /// 【型】Guid
    /// 【用途】Entity の ID（Entity 復元時に必須）
    /// 【可視性】Repository でクエリ条件に使用可能
    /// </summary>
    public Guid YourEntityId { get; set; }

    // ... その他のカラム
}
```

### Mapper での型変換（対応付けなし）

Mapper は **ValueObject → primitive 型の型変換のみ**。AggregateId と RowId は独立した ID で、対応付けは行いません：

```csharp
// Entity → DbModel（型変換のみ）
public YourEntityDbModel ToDbModel(YourEntity entity)
{
    return new YourEntityDbModel
    {
        // ✓ 型変換：RowId（ValueObject） → long
        RowId = entity.RowId?.Value ?? 0,
        
        // ✓ 型変換：AggregateId（ValueObject） → Guid
        YourEntityId = entity.Id.Value,
        
        // ✗ AggregateId と RowId の「対応付け」は行わない
        // Entity が両方を独立して保持している
    };
}

// DbModel → Entity（型変換のみ）
public YourEntity ToDomainEntity(YourEntityDbModel dbModel, IClock clock)
{
    // ✓ 型変換：Guid → YourEntityId（ValueObject）
    var aggregateId = YourEntityId.From(dbModel.YourEntityId);
    
    // ✓ 型変換：long → RowId（ValueObject）
    var rowId = RowId.From(dbModel.RowId);
    
    // ✓ Entity が両方のIDを保持（対応付けは Entity コンストラクタで行われる）
    return new YourEntity(
        id: aggregateId,
        rowId: rowId,
        clock: clock
    );
}
```

**重要**: Mapper は AggregateId と RowId の対応付けを行いません。Entity コンストラクタで両方の ID が設定されます。

---

## ✅ 実装チェックリスト

### ToDbModel（Entity → DbModel）

- [ ] **Entity.Id**（AggregateId） → **DbModel.YourEntityId**（Guid）
- [ ] **Entity.RowId** → **DbModel.RowId**（long）
- [ ] **ValueObject** → **primitive 型** に変換
- [ ] **監査情報** は初期値のみ（Repository が上書き）
- [ ] **null チェック**: ArgumentNullException

### ToDomainEntity（DbModel → Entity）

- [ ] **DbModel.YourEntityId**（Guid） → **Entity.Id**（AggregateId）
- [ ] **DbModel.RowId**（long） → **Entity.RowId**（RowId ValueObject）
- [ ] **primitive 型** → **ValueObject** に変換
- [ ] **LocalDateTime** はそのまま使用（変換不要）
- [ ] **null チェック**: ArgumentNullException
- [ ] **IClock** を引数に受け取る

---

## ❌ 避けるべきパターン

### パターン1: Mapper が Entity.RowId を無視

```csharp
// ✗ 禁止：RowId の変換を忘れる
public YourEntity ToDomainEntity(YourEntityDbModel dbModel, IClock clock)
{
    var aggregateId = YourEntityId.From(dbModel.YourEntityId);
    
    return new YourEntity(
        id: aggregateId,
        // ✗ RowId を渡さない！
        clock: clock
    );
}

// ✓ 正しい：RowId も変換して渡す
public YourEntity ToDomainEntity(YourEntityDbModel dbModel, IClock clock)
{
    var aggregateId = YourEntityId.From(dbModel.YourEntityId);
    var rowId = RowId.From(dbModel.RowId);
    
    return new YourEntity(
        id: aggregateId,
        rowId: rowId,
        clock: clock
    );
}
```

### パターン2: AggregateId と RowId を混同

```csharp
// ✗ 禁止：AggregateId を RowId のように扱う
public YourEntity ToDomainEntity(YourEntityDbModel dbModel, IClock clock)
{
    var rowId = RowId.From((long)dbModel.YourEntityId);  // ✗ Guid を long に!
    
    return new YourEntity(id: rowId, clock: clock);
}

// ✓ 正しい：AggregateId と RowId を分離
public YourEntity ToDomainEntity(YourEntityDbModel dbModel, IClock clock)
{
    var aggregateId = YourEntityId.From(dbModel.YourEntityId);  // Guid
    var rowId = RowId.From(dbModel.RowId);                      // long
    
    return new YourEntity(id: aggregateId, rowId: rowId, clock: clock);
}
```

---

## 📁 ファイル構造

```
YourContext.Infrastructure/
├── Mappers/
│   ├── YourEntityMapper.cs
│   ├── YourChildEntityMapper.cs
│   └── YourAggregateMapper.cs
├── DataAccess/
│   ├── Models/
│   │   ├── YourEntityDbModel.cs
│   │   ├── YourChildEntityDbModel.cs
│   │   └── YourAggregateDbModel.cs
```

---

## 参考資料

- **Repository_パターンガイド.md** — Mapper の使用方法、複数テーブル集約の DataAccess 実装
- **Entity_設計ガイドライン.md** — Entity と AggregateId、複数テーブル集約の Entity 構造
- **ORM_マッピング戦略.md** — LocalDateTime マッピング、複数テーブル集約のマッピング戦略
- **Entity_設計ガイドライン.md** — RowId ベースの ID 管理
- **DbModel_設計ルール.md** — DbModel の設計原則

