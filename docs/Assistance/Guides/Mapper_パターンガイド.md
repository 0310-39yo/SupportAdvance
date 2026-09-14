# Mapper パターンガイド

Domain Entity と Infrastructure DbModel 間の純粋なマッピング実装ガイドです。

---

## 📋 基本原則

### 責務の分離

Mapper の責務は **型変換のみ**です。ビジネスコンテキストや認証情報の管理は Repository の責務です。

| 層 | 責務 | 例 |
|---|---|---|
| **Mapper** | 純粋な型変換 | RowId（long）→ ValueObject、ValueObject → long の相互変換 |
| **Repository** | ビジネスコンテキスト管理 | createdBy を ICurrentUserService から取得、RowId の採番 |

### マッピング方向と ID の役割分離

```
Domain Entity ↔ DbModel（SQL Server）
    ↑                ↓
ビジネスロジック    技術的な永続化形式

Entity が保持する ID：
- RowId（long ベース ValueObject）：Entity の集約ID、テーブルの物理キー

Mapper の責務（ValueObject ↔ primitive 型の型変換のみ）：
- Entity.RowId（XXXRowId） → DbModel.RowId（long）
- その他の ValueObject → primitive 型の相互変換

【重要】Mapper は Entity の RowId と DbModel の他のカラムの対応付けは行わない
（Entity コンストラクタで RowId を保持）
```

---

## 💡 実装パターン

### インターフェース定義

```csharp
// src/Infrastructure/Mappers/IEntityMapper.cs

using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Infrastructure.Mappers;

/// <summary>
/// Entity ↔ DbModel マッパー（汎用インターフェース）
/// 
/// 【型パラメータ】
/// - TEntity: Entity<TId>（RowId を継承した集約ID型）
/// - TDbModel: データベースモデル
/// - TId: Entity の RowId 型（RowId を継承）
/// 
/// 【ID の役割】
/// - TId（RowId 継承）: Entity の集約ID、テーブルの物理キー
/// - DbModel.RowId: DB の long 値
/// </summary>
public interface IEntityMapper<TEntity, TDbModel, TId>
    where TEntity : Entity<TId>
    where TDbModel : class
    where TId : notnull, RowId
{
    /// <summary>
    /// Domain Entity → DbModel（保存用）
    /// 【責務】RowId と他の ValueObject → primitive 型の型変換のみ
    /// </summary>
    TDbModel ToDbModel(TEntity entity);

    /// <summary>
    /// DbModel → Domain Entity（読み取り用）
    /// 【責務】primitive 型 → RowId と他の ValueObject の型変換のみ
    /// </summary>
    TEntity ToDomainEntity(TDbModel dbModel);
}
```

### Mapper 実装例

#### シンプルな Entity マッピング（long ベース RowId）

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Infrastructure/Mappers/YourEntityMapper.cs

using SupportAdvance.Contexts.YourGroup.YourContext.Domain.Entities;
using SupportAdvance.Contexts.YourGroup.YourContext.Domain.ValueObjects;
using SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.DataAccess.Models;
using SupportAdvance.Infrastructure.Mappers;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.Mappers;

/// <summary>
/// YourEntity ↔ YourEntityDbModel マッパー
/// 
/// 【ID マッピング】
/// - Entity.RowId: YourEntityRowId（RowId 継承、long ベース）
/// - DbModel.RowId: long（テーブル物理キー）
/// </summary>
public class YourEntityMapper : IEntityMapper<YourEntity, YourEntityDbModel, YourEntityRowId>
{
    /// <summary>
    /// Domain Entity → DbModel（保存用）
    /// 【ID 変換】RowId.Value（long） → DbModel.RowId（long）
    /// 【監査情報】Repository が設定するため、ここでは値のみマッピング
    /// </summary>
    public YourEntityDbModel ToDbModel(YourEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new YourEntityDbModel
        {
            // RowId: テーブルの物理キー（ApplicationService で事前採番済み）
            RowId = entity.RowId.Value,

            // ビジネスカラム：Entity から直接マッピング
            Name = entity.Name,
            Amount = entity.Amount?.Amount,  // ValueObject → primitive
            Status = entity.Status.ToDbValue(),

            // 監査情報：Repository が上書き（ここでは Entity の値をそのままコピー）
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    /// <summary>
    /// DbModel → Domain Entity（読み取り用）
    /// 【ID 変換】DbModel.RowId（long） → Entity.RowId（YourEntityRowId）
    /// 【監査情報】DbModel から Entity へ直接マッピング
    /// </summary>
    public YourEntity ToDomainEntity(YourEntityDbModel dbModel)
    {
        ArgumentNullException.ThrowIfNull(dbModel);

        // 1. RowId を ValueObject に変換
        var rowId = YourEntityRowId.From(dbModel.RowId);  // long → YourEntityRowId

        // 2. その他の ValueObject に変換
        var amount = dbModel.Amount.HasValue ? Money.From(dbModel.Amount.Value) : null;
        var status = YourStatus.FromDbValue(dbModel.Status);

        // 3. Entity を復元
        return YourEntity.Reconstruct(
            rowId: rowId,
            name: dbModel.Name,
            amount: amount,
            status: status,
            createdAt: dbModel.CreatedAt,
            rowVersion: dbModel.RowVersion
        );
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

using SupportAdvance.Contexts.YourGroup.YourContext.Domain.Entities;
using SupportAdvance.Contexts.YourGroup.YourContext.Domain.ValueObjects;
using SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.DataAccess.Models;
using SupportAdvance.Infrastructure.Mappers;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.Mappers;

/// <summary>
/// YourAggregate ↔ DbModel マッパー（複数テーブル集約）
/// 
/// 【複数テーブル構成】
/// - t_your_aggregate: 集約ルート（RowId を保存）
/// - t_your_children: 子Entity（それぞれの RowId を保存）
/// 
/// 【マッピング方式】複数の1:1マッピングの組み合わせ
/// - YourAggregate（1個） → YourAggregateDbModel（1個）【1:1】
/// - List&lt;YourChild&gt;（N個） → List&lt;YourChildDbModel&gt;（N個）【各1:1】
/// 
/// 【ID 管理】
/// - 親Entity: YourAggregateRowId（テーブル物理キー）
/// - 各子Entity: YourChildRowId（テーブル物理キー）
/// </summary>
public class YourAggregateMapper : IEntityMapper<YourAggregate, YourAggregateDbModel, YourAggregateRowId>
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
            RowId = aggregate.RowId.Value,

            // その他のカラム
            Name = aggregate.Name,
            CreatedAt = aggregate.CreatedAt
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
    public YourAggregate ToDomainEntity(YourAggregateDbModel dbModel)
    {
        ArgumentNullException.ThrowIfNull(dbModel);

        // ① 親の RowId 変換（親DbModel → 親Entity の1:1マッピング）
        var aggregateRowId = YourAggregateRowId.From(dbModel.RowId);  // long → YourAggregateRowId

        // 親の Entity を復元
        var aggregate = YourAggregate.Reconstruct(
            rowId: aggregateRowId,
            name: dbModel.Name,
            createdAt: dbModel.CreatedAt,
            children: new List<YourChild>(),
            rowVersion: dbModel.RowVersion
        );

        // ② 子要素も Entity に変換して追加（複数の1:1マッピング）
        // 各 YourChildDbModel → YourChild（1:1）
        foreach (var childDbModel in dbModel.Children ?? Enumerable.Empty<YourChildDbModel>())
        {
            var child = _childMapper.ToDomainEntity(childDbModel);  // 子Mapperが各1:1マッピングを処理
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

DbModel には **RowId のみ** を保持します：

```csharp
public class YourEntityDbModel
{
    /// <summary>
    /// テーブルの物理キー（row_id）
    /// 【型】long
    /// 【用途】テーブル行を識別（DB の主キー）
    /// 【責務】Entity.RowId に対応
    /// </summary>
    public long RowId { get; set; }

    // ... その他のビジネスカラム
}
```

### Mapper での型変換

Mapper は **ValueObject → primitive 型の型変換のみ**。RowId の type-safe ValueObject への変換を行います：

```csharp
// Entity → DbModel（型変換のみ）
public YourEntityDbModel ToDbModel(YourEntity entity)
{
    return new YourEntityDbModel
    {
        // ✓ 型変換：RowId（ValueObject） → long
        RowId = entity.RowId.Value,  // Entity の RowId を DB 値に変換
        
        // ✓ その他の ValueObject → primitive 型の変換
        Name = entity.Name,
        Status = entity.Status.ToDbValue()
    };
}

// DbModel → Entity（型変換のみ）
public YourEntity ToDomainEntity(YourEntityDbModel dbModel)
{
    // ✓ 型変換：long → YourEntityRowId（ValueObject）
    var rowId = YourEntityRowId.From(dbModel.RowId);
    
    // ✓ その他の型変換
    var status = YourStatus.FromDbValue(dbModel.Status);
    
    // ✓ Entity を復元（RowId は Entity コンストラクタで設定）
    return YourEntity.Reconstruct(
        rowId: rowId,
        name: dbModel.Name,
        status: status
    );
}
```

**重要**: Mapper は Entity の RowId を long に変換するだけです。Entity コンストラクタで RowId が type-safe に管理されます。

---

## 🔑 オプション型 RowId のマッピング

オプション型 RowId（`ManagerEmployeeRowId` など）を持つ Entity の Mapper では、**null → Unset() 自動変換** を実装します。

```csharp
// 例：Department 集約のマッピング

public class DepartmentMapper : IEntityMapper<Department, DepartmentDbModel, DepartmentRowId>
{
    /// <summary>
    /// Domain Entity → DbModel（保存用）
    /// 【責務】オプション型 RowId を long または null に変換
    /// </summary>
    public DepartmentDbModel ToDbModel(Department entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new DepartmentDbModel
        {
            RowId = entity.RowId.Value,
            Name = entity.Name,
            
            // オプション型RowId → nullable long（IsSet=false なら null）
            ManagerRowId = entity.Manager.HasManager 
                ? (long?)entity.Manager.Value 
                : null  // Unset → null
        };
    }

    /// <summary>
    /// DbModel → Domain Entity（読み取り用）
    /// 【責務】nullable long → オプション型 RowId（null は Unset に自動変換）
    /// </summary>
    public Department ToDomainEntity(DepartmentDbModel dbModel)
    {
        ArgumentNullException.ThrowIfNull(dbModel);

        var rowId = DepartmentRowId.From(dbModel.RowId);

        // null は自動的に Unset() に変換
        if (!ManagerEmployeeRowId.TryFromDbValue(dbModel.ManagerRowId, out var manager))
            return null!;  // 検証失敗の場合

        return Department.Reconstruct(
            rowId: rowId,
            name: dbModel.Name,
            manager: manager,  // Unset または From(value)
            rowVersion: dbModel.RowVersion
        );
    }
}
```

### ポイント

| 項目 | 説明 |
|------|------|
| **ToDbModel** | `HasManager` フラグで null/値を判定。Unset → null |
| **ToDomainEntity** | `TryFromDbValue()` で null → Unset() 自動変換 |
| **Domain 層** | null を見ない。`IsSet` で状態判定 |

---

## ✅ 実装チェックリスト

### ToDbModel（Entity → DbModel）

- [ ] **Entity.RowId** → **DbModel.RowId**（long）
- [ ] **ValueObject** → **primitive 型** に変換
- [ ] **監査情報** は初期値のみ（Repository が上書き）
- [ ] **null チェック**: ArgumentNullException

### ToDomainEntity（DbModel → Entity）

- [ ] **DbModel.RowId**（long） → **Entity.RowId**（RowId ValueObject）
- [ ] **primitive 型** → **ValueObject** に変換
- [ ] **LocalDateTime** はそのまま使用（変換不要）
- [ ] **null チェック**: ArgumentNullException
- [ ] **ファクトリメソッド**: Reconstruct() を呼び出す（Clock 不要）

---

## ❌ 避けるべきパターン

### パターン1: Mapper が Entity.RowId を無視

```csharp
// ✗ 禁止：RowId の変換を忘れる
public YourEntity ToDomainEntity(YourEntityDbModel dbModel)
{
    return new YourEntity(
        name: dbModel.Name,
        // ✗ RowId を渡さない！
        status: dbModel.Status
    );
}

// ✓ 正しい：RowId も変換して渡す
public YourEntity ToDomainEntity(YourEntityDbModel dbModel)
{
    var rowId = YourEntityRowId.From(dbModel.RowId);  // long → YourEntityRowId
    
    return YourEntity.Reconstruct(
        rowId: rowId,
        name: dbModel.Name,
        status: dbModel.Status
    );
}
```

### パターン2: RowId を型安全でない long で管理

```csharp
// ✗ 禁止：基底 RowId 型を使用（型安全性なし）
public YourEntity ToDomainEntity(YourEntityDbModel dbModel)
{
    var rowId = RowId.From(dbModel.RowId);  // ✗ RowId 型が曖昧
    
    return new YourEntity(rowId: rowId, name: dbModel.Name);
}

// ✓ 正しい：集約固有の RowId 型を使用
public YourEntity ToDomainEntity(YourEntityDbModel dbModel)
{
    var rowId = YourEntityRowId.From(dbModel.RowId);  // YourEntityRowId（型安全）
    
    return YourEntity.Reconstruct(rowId: rowId, name: dbModel.Name);
}
```

### パターン3: Mapper が Clock に依存

```csharp
// ✗ 禁止：Mapper が Clock を引数に（テスト困難）
public YourEntity ToDomainEntity(YourEntityDbModel dbModel, IClock clock)
{
    var rowId = YourEntityRowId.From(dbModel.RowId);
    
    // ✗ Mapper が Clock を使用
    return new YourEntity(rowId: rowId, clock: clock);
}

// ✓ 正しい：Mapper は Clock に依存しない（純粋な型変換）
public YourEntity ToDomainEntity(YourEntityDbModel dbModel)
{
    var rowId = YourEntityRowId.From(dbModel.RowId);
    
    // ✓ Reconstruct() で DB 値をそのまま復元
    return YourEntity.Reconstruct(
        rowId: rowId,
        name: dbModel.Name,
        createdAt: dbModel.CreatedAt  // DB 値そのまま
    );
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
│   │   ├── YourChildDbModel.cs
│   │   └── YourAggregateDbModel.cs
```

---

## 参考資料

- **Entity_設計ガイドライン.md** — Entity<TId> パターン、RowId ベース ID 管理、複数テーブル集約の Entity 構造
- **RowId_設計ガイド.md** — long ベース RowId の実装パターン、採番方法、型安全性
- **Repository_パターンガイド.md** — Mapper の使用方法、複数テーブル集約の DataAccess 実装
- **ORM_マッピング戦略.md** — LocalDateTime マッピング、複数テーブル集約のマッピング戦略
- **DbModel_設計ルール.md** — DbModel の設計原則

---

## 📝 更新履歴

| 日付 | 更新内容 |
|------|---------|
| 2026-09-12（後）| **オプション型 RowId マッピング追加**。DepartmentMapper の例で、nullable long ↔ オプション型 RowId（ManagerEmployeeRowId）のマッピング方法を説明。TryFromDbValue() による null → Unset() 自動変換を明記。RowId_設計ガイド.md、Entity_設計ガイドライン.md との整合性を確認 |
| 2026-09-12 | **全面改版**。Mapper の説明を GUID ベース AggregateId から long ベース RowId ベースに変更。Entity_設計ガイドライン.md の実装準拠：`where TId : notnull, RowId`。実装例を RowId ベースに統一。AggregateId の記述をすべて削除。Mapper は Clock に依存しないことを明記 |

