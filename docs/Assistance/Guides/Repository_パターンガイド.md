# Repository パターンガイド

Generic Repository パターンと監査情報管理の実装ガイドです。

---

## 📋 基本原則

### RepositoryBase<TEntity, TDbModel, TId>

すべての Repository は `RepositoryBase` を継承し、監査情報の自動管理を実現します。

```csharp
using SupportAdvance.SharedKernel.Entities;

public abstract class RepositoryBase<TEntity, TDbModel, TId>
    where TEntity : Entity<TId>
    where TDbModel : class
    where TId : notnull
{
    /// <summary>
    /// TId: Entity の ID 型（AggregateId を継承した ValueObject）
    /// 例：OrderId, EmployeeId, UserPreferencesId
    /// </summary>
    protected readonly IEntityMapper<TEntity, TDbModel, TId> _mapper;
    protected readonly ICurrentUserService _currentUser;
    protected readonly IClock _clock;

    protected RepositoryBase(
        IEntityMapper<TEntity, TDbModel, TId> mapper,
        ICurrentUserService currentUser,
        IClock clock)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    // Mapper を利用した Entity ↔ DbModel 変換
    protected TEntity MapToDomain(TDbModel dbModel) => _mapper.ToDomainEntity(dbModel, _clock);

    // 監査情報を自動設定してから保存
    protected TDbModel MapToDatabaseForInsert(TEntity entity)
    {
        var dbModel = _mapper.ToDbModel(entity);
        dbModel.CreatedBy = _currentUser.EmployeeRowId;
        dbModel.CreatedAt = _clock.JstNow;
        return dbModel;
    }

    protected TDbModel MapToDatabaseForUpdate(TEntity entity)
    {
        var dbModel = _mapper.ToDbModel(entity);
        dbModel.UpdatedBy = _currentUser.EmployeeRowId;
        dbModel.UpdatedAt = _clock.JstNow;
        return dbModel;
    }

    // 論理削除
    protected void SetDeletedByAudit(TDbModel dbModel)
    {
        dbModel.DeletedBy = _currentUser.EmployeeRowId;
        dbModel.DeletedAt = _clock.JstNow;
    }
}
```

---

## 💡 実装パターン

### 1. Repository インターフェース定義

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Application/Repositories/IYourEntityRepository.cs

using SupportAdvance.Contexts.YourGroup.YourContext.Domain.Entities;
using SupportAdvance.Contexts.YourGroup.YourContext.Domain.ValueObjects;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Application.Repositories;

public interface IYourEntityRepository
{
    // 集約ID（AggregateId）で取得
    // 【推奨】ビジネスロジックは集約ID でアクセスすべき
    Task<YourEntity?> GetByIdAsync(YourEntityId id);

    // ビジネスID で取得（任意）
    Task<YourEntity?> GetByBusinessIdAsync(YourBusinessId businessId);

    // テーブルRowId で取得（内部用、通常は非推奨）
    Task<YourEntity?> GetByRowIdAsync(long rowId);

    Task<IReadOnlyList<YourEntity>> GetAllAsync();

    // 保存
    Task AddAsync(YourEntity entity);
    Task UpdateAsync(YourEntity entity);

    // 削除
    Task<bool> DeleteAsync(YourEntityId id);
}
```

### 2. Repository 実装

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Infrastructure/Repositories/YourEntityRepository.cs

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.YourGroup.YourContext.Application.Repositories;
using SupportAdvance.Contexts.YourGroup.YourContext.Domain.Entities;
using SupportAdvance.Contexts.YourGroup.YourContext.Domain.ValueObjects;
using SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.DataAccess;
using SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.DataAccess.Models;
using SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.Mappers;
using SupportAdvance.Infrastructure.Repositories;
using SupportAdvance.Infrastructure.Services;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.Repositories;

/// <summary>
/// YourEntity の Repository 実装
/// 
/// 【ID の扱い】
/// - GetByIdAsync(YourEntityId): 推奨。集約ID（GUID）で検索
/// - GetByBusinessIdAsync(): ビジネスID で検索（任意）
/// - GetByRowIdAsync(): テーブル物理キー（long）で検索（内部用）
/// </summary>
public class YourEntityRepository
    : RepositoryBase<YourEntity, YourEntityDbModel, YourEntityId>,
      IYourEntityRepository
{
    private readonly IYourEntityDataAccess _dataAccess;

    public YourEntityRepository(
        YourEntityMapper mapper,
        ICurrentUserService currentUser,
        IClock clock,
        IYourEntityDataAccess dataAccess)
        : base(mapper, currentUser, clock)
    {
        _dataAccess = dataAccess ?? throw new ArgumentNullException(nameof(dataAccess));
    }

    /// <summary>
    /// 集約ID で検索（推奨）
    /// 【パラメータ】id: YourEntityId（GUID ベース）
    /// 【戻り値】見つからなければ null
    /// </summary>
    public async Task<YourEntity?> GetByIdAsync(YourEntityId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        // DataAccess でビジネスID で検索
        var dbModel = await _dataAccess.GetByYourEntityIdAsync(id.Value);
        
        if (dbModel == null)
            return null;

        // Mapper で Domain Entity に変換
        return MapToDomain(dbModel);
    }

    /// <summary>
    /// ビジネスID で検索（任意）
    /// </summary>
    public async Task<YourEntity?> GetByBusinessIdAsync(YourBusinessId businessId)
    {
        ArgumentNullException.ThrowIfNull(businessId);

        var dbModel = await _dataAccess.GetByBusinessIdAsync(businessId.Value);
        
        if (dbModel == null)
            return null;

        return MapToDomain(dbModel);
    }

    /// <summary>
    /// テーブル行ID で検索（内部用）
    /// 【通常は推奨されない】集約ID（AggregateId）での検索を推奨
    /// </summary>
    public async Task<YourEntity?> GetByRowIdAsync(long rowId)
    {
        if (rowId <= 0)
            throw new ArgumentException("RowId は正の値である必要があります。", nameof(rowId));

        var dbModel = await _dataAccess.GetByRowIdAsync(rowId);
        return dbModel == null ? null : MapToDomain(dbModel);
    }

    public async Task<IReadOnlyList<YourEntity>> GetAllAsync()
    {
        var dbModels = await _dataAccess.GetAllAsync();
        return dbModels
            .Select(MapToDomain)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Entity を新規追加
    /// 【監査情報】Repository が自動設定
    /// - CreatedBy: ICurrentUserService から取得
    /// - CreatedAt: IClock.JstNow
    /// </summary>
    public async Task AddAsync(YourEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        // Mapper で DbModel に変換＆監査情報自動設定
        var dbModel = MapToDatabaseForInsert(entity);

        // DataAccess で保存
        await _dataAccess.InsertAsync(dbModel);
    }

    /// <summary>
    /// Entity を更新
    /// 【監査情報】Repository が自動設定
    /// - UpdatedBy: ICurrentUserService から取得
    /// - UpdatedAt: IClock.JstNow
    /// </summary>
    public async Task UpdateAsync(YourEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        // Mapper で DbModel に変換＆監査情報自動設定
        var dbModel = MapToDatabaseForUpdate(entity);

        // DataAccess で更新
        await _dataAccess.UpdateAsync(dbModel);
    }

    /// <summary>
    /// Entity を論理削除
    /// 【監査情報】Repository が自動設定
    /// - DeletedBy: ICurrentUserService から取得
    /// - DeletedAt: IClock.JstNow
    /// </summary>
    public async Task<bool> DeleteAsync(YourEntityId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        // 集約ID で検索
        var dbModel = await _dataAccess.GetByYourEntityIdAsync(id.Value);
        if (dbModel == null)
            return false;

        // 論理削除：監査情報を設定
        SetDeletedByAudit(dbModel);

        // DataAccess で削除マーク
        await _dataAccess.UpdateAsync(dbModel);

        return true;
    }
}
```

### 3. DataAccess インターフェース

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Infrastructure/DataAccess/IYourEntityDataAccess.cs

using SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.DataAccess.Models;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.DataAccess;

public interface IYourEntityDataAccess
{
    // 集約ID（GUID）で検索
    Task<YourEntityDbModel?> GetByYourEntityIdAsync(Guid yourEntityId);

    // ビジネスID で検索
    Task<YourEntityDbModel?> GetByBusinessIdAsync(int businessId);

    // テーブル行ID で検索
    Task<YourEntityDbModel?> GetByRowIdAsync(long rowId);

    Task<List<YourEntityDbModel>> GetAllAsync();
    Task InsertAsync(YourEntityDbModel model);
    Task UpdateAsync(YourEntityDbModel model);
}
```

### 4. DataAccess 実装

#### 単一テーブル集約

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Infrastructure/DataAccess/YourEntityDataAccess.cs

using RepoDb;
using System.Data.SqlClient;
using SupportAdvance.Common.Configuration;
using SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.DataAccess.Models;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.DataAccess;

public class YourEntityDataAccess : IYourEntityDataAccess
{
    private readonly IDatabaseSettings _dbSettings;

    public YourEntityDataAccess(IDatabaseSettings dbSettings)
    {
        _dbSettings = dbSettings ?? throw new ArgumentNullException(nameof(dbSettings));
    }

    private string GetConnectionString() 
        => _dbSettings.ConnectionStrings["SupportAdvance"];

    /// <summary>
    /// 集約ID（GUID）で検索（推奨）
    /// </summary>
    public async Task<YourEntityDbModel?> GetByYourEntityIdAsync(Guid yourEntityId)
    {
        using (var connection = new SqlConnection(GetConnectionString()))
        {
            return await connection.QueryAsync<YourEntityDbModel>(
                x => x.YourEntityId == yourEntityId && x.DeletedAt == null
            ).ContinueWith(t => t.Result.FirstOrDefault());
        }
    }

    /// <summary>
    /// ビジネスID で検索
    /// </summary>
    public async Task<YourEntityDbModel?> GetByBusinessIdAsync(int businessId)
    {
        using (var connection = new SqlConnection(GetConnectionString()))
        {
            return await connection.QueryAsync<YourEntityDbModel>(
                x => x.YourBusinessId == businessId && x.DeletedAt == null
            ).ContinueWith(t => t.Result.FirstOrDefault());
        }
    }

    /// <summary>
    /// テーブル行ID で検索
    /// </summary>
    public async Task<YourEntityDbModel?> GetByRowIdAsync(long rowId)
    {
        using (var connection = new SqlConnection(GetConnectionString()))
        {
            return await connection.QueryAsync<YourEntityDbModel>(
                x => x.RowId == rowId && x.DeletedAt == null
            ).ContinueWith(t => t.Result.FirstOrDefault());
        }
    }

    public async Task<List<YourEntityDbModel>> GetAllAsync()
    {
        using (var connection = new SqlConnection(GetConnectionString()))
        {
            return (await connection.QueryAsync<YourEntityDbModel>(
                x => x.DeletedAt == null
            )).ToList();
        }
    }

    public async Task InsertAsync(YourEntityDbModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        using (var connection = new SqlConnection(GetConnectionString()))
        {
            await connection.InsertAsync("t_YourEntity", model);
        }
    }

    public async Task UpdateAsync(YourEntityDbModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        using (var connection = new SqlConnection(GetConnectionString()))
        {
            await connection.UpdateAsync("t_YourEntity", model);
        }
    }
}
```

#### 複数テーブル集約

複数テーブル集約の場合、DataAccess は複数テーブルの操作を調整します：

```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Infrastructure/DataAccess/IYourAggregateDataAccess.cs

using SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.DataAccess.Models;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.DataAccess;

/// <summary>
/// 複数テーブル集約の DataAccess インターフェース
/// 【責務】複数テーブル（親 + 子）の操作を調整
/// </summary>
public interface IYourAggregateDataAccess
{
    // 集約ID で復元（親テーブル + 子テーブルを読み込み）
    Task<YourAggregateDbModel?> GetByAggregateIdAsync(Guid aggregateId);

    Task<List<YourAggregateDbModel>> GetAllAsync();

    // 複数テーブルに保存
    Task InsertAsync(YourAggregateDbModel model);
    
    // 複数テーブルを更新
    Task UpdateAsync(YourAggregateDbModel model);
}

// src/Contexts/YourGroup/YourContext/YourContext.Infrastructure/DataAccess/YourAggregateDataAccess.cs

using RepoDb;
using System.Data.SqlClient;
using System.Transactions;
using SupportAdvance.Common.Configuration;
using SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.DataAccess.Models;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.DataAccess;

/// <summary>
/// 複数テーブル集約の DataAccess 実装
/// 【処理内容】
/// - GetByAggregateIdAsync: 親テーブルから読み込み、子テーブルを JOIN で取得
/// - InsertAsync: 親テーブル → 子テーブルの順番で挿入
/// - UpdateAsync: トランザクション内で親→子を更新
/// </summary>
public class YourAggregateDataAccess : IYourAggregateDataAccess
{
    private readonly IDatabaseSettings _dbSettings;

    public YourAggregateDataAccess(IDatabaseSettings dbSettings)
    {
        _dbSettings = dbSettings ?? throw new ArgumentNullException(nameof(dbSettings));
    }

    private string GetConnectionString() 
        => _dbSettings.ConnectionStrings["SupportAdvance"];

    /// <summary>
    /// 集約ID で復元（複数テーブル読み込み）
    /// 【処理】
    /// ① t_your_aggregate から親レコードを取得
    /// ② t_your_children から子レコードを取得（FK: parent_aggregate_id）
    /// </summary>
    public async Task<YourAggregateDbModel?> GetByAggregateIdAsync(Guid aggregateId)
    {
        using (var connection = new SqlConnection(GetConnectionString()))
        {
            // ① 親テーブル：t_your_aggregate
            var aggregate = await connection.QueryAsync<YourAggregateDbModel>(
                x => x.YourAggregateId == aggregateId && x.DeletedAt == null
            ).ContinueWith(t => t.Result.FirstOrDefault());

            if (aggregate == null)
                return null;

            // ② 子テーブル：t_your_children（FK で関連付け）
            var children = await connection.QueryAsync<YourChildDbModel>(
                x => x.ParentAggregateId == aggregateId && x.DeletedAt == null
            );

            aggregate.Children = children.ToList();
            return aggregate;
        }
    }

    public async Task<List<YourAggregateDbModel>> GetAllAsync()
    {
        using (var connection = new SqlConnection(GetConnectionString()))
        {
            // 親テーブルをすべて取得
            var aggregates = await connection.QueryAsync<YourAggregateDbModel>(
                x => x.DeletedAt == null
            );

            // 各親レコードに対して子レコードを取得
            foreach (var aggregate in aggregates)
            {
                var children = await connection.QueryAsync<YourChildDbModel>(
                    x => x.ParentAggregateId == aggregate.YourAggregateId && x.DeletedAt == null
                );
                aggregate.Children = children.ToList();
            }

            return aggregates.ToList();
        }
    }

    /// <summary>
    /// 複数テーブルに新規保存
    /// 【処理】
    /// ① 親レコードを t_your_aggregate に挿入
    /// ② 子レコードを t_your_children に挿入（各子 FK: parent_aggregate_id）
    /// </summary>
    public async Task InsertAsync(YourAggregateDbModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        using (var connection = new SqlConnection(GetConnectionString()))
        {
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // ① 親レコード保存
                await connection.InsertAsync("t_YourAggregate", model);

                // ② 子レコード保存（複数テーブルの場合）
                if (model.Children != null && model.Children.Count > 0)
                {
                    foreach (var child in model.Children)
                    {
                        // FK: ParentAggregateId を設定（親の ID）
                        child.ParentAggregateId = model.YourAggregateId;
                        await connection.InsertAsync("t_YourChildren", child);
                    }
                }

                transaction.Complete();
            }
        }
    }

    /// <summary>
    /// 複数テーブルを更新
    /// 【処理】
    /// ① 親レコードを t_your_aggregate で更新
    /// ② 子レコードを削除＆再挿入（論理削除パターン）
    /// </summary>
    public async Task UpdateAsync(YourAggregateDbModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        using (var connection = new SqlConnection(GetConnectionString()))
        {
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // ① 親レコード更新
                await connection.UpdateAsync("t_YourAggregate", model);

                // ② 子レコード更新（既存を削除マーク＆新規レコード追加）
                if (model.Children != null && model.Children.Count > 0)
                {
                    // 既存子レコード削除マーク
                    await connection.ExecuteAsync(
                        "UPDATE t_YourChildren SET deleted_at = @now WHERE parent_aggregate_id = @parentId",
                        new { now = DateTime.UtcNow, parentId = model.YourAggregateId }
                    );

                    // 新規子レコード追加
                    foreach (var child in model.Children)
                    {
                        child.ParentAggregateId = model.YourAggregateId;
                        await connection.InsertAsync("t_YourChildren", child);
                    }
                }

                transaction.Complete();
            }
        }
    }
}
```

**重要**: 複数テーブル集約の DataAccess は、複数テーブル間の **操作順序** と **トランザクション管理** を担当します。Mapper は単なる型変換で、テーブル操作の複雑性は DataAccess に委譲されます。

---

## ✅ 実装チェックリスト

### Repository インターフェース

- [ ] **GetByIdAsync(TId id)**: 集約ID で取得（推奨）
- [ ] **ビジネスID メソッド**: GetByBusinessIdAsync など（任意）
- [ ] **RowId メソッド**: GetByRowIdAsync など（内部用、非推奨）
- [ ] **Add/Update/Delete**: CRUD 操作をすべてカバー
- [ ] **Domain Entity を返す**: DbModel ではなく Entity を返す

### Repository 実装

- [ ] **RepositoryBase を継承**: 監査情報を自動管理
- [ ] **ICurrentUserService 依存**: createdBy/updatedBy を自動設定
- [ ] **Mapper 使用**: Entity ↔ DbModel 変換は Mapper に委譲
- [ ] **DataAccess 依存**: 実際の DB 操作は DataAccess に委譲
- [ ] **null チェック**: すべてのパラメータで ArgumentNullException

### DataAccess 実装

- [ ] **GetByIdAsync(Guid id)**: 集約ID（GUID）で検索
- [ ] **DeletedAt IS NULL**: 論理削除を WHERE 句で自動フィルタ
- [ ] **RepoDb lambda WHERE**: connection.QueryAsync<T>(x => ...) パターン
- [ ] **ConnectionString**: IDatabaseSettings から取得
- [ ] **例外処理**: async Task を正しく返す

---

## ❌ 避けるべきパターン

### パターン1: Repository が DbModel を返す

```csharp
// ✗ 禁止: Repository が DbModel を返す
public interface IYourEntityRepository
{
    Task<YourEntityDbModel?> GetAsync(YourEntityId id);  // ✗ DbModel
}

// ✓ Repository は Entity を返す
public interface IYourEntityRepository
{
    Task<YourEntity?> GetByIdAsync(YourEntityId id);  // ✓ Entity
}
```

### パターン2: Repository が Mapper に依存しない

```csharp
// ✗ 禁止: Repository が手動で Entity を構築
var entity = new YourEntity(
    dbModel.RowId,
    YourBusinessId.From(dbModel.YourBusinessId),
    dbModel.Name
);

// ✓ Mapper を使用
var entity = MapToDomain(dbModel);
```

### パターン3: Guid と long を混同

```csharp
// ✗ 禁止：集約ID（Guid）とRowId（long）を混同
public async Task<YourEntity?> GetByIdAsync(long rowId)  // ✗ long を受け取る
{
    var dbModel = await _dataAccess.GetByYourEntityIdAsync(rowId);  // ✗ Guid を Longに
}

// ✓ 正しい：それぞれの型で検索
public async Task<YourEntity?> GetByIdAsync(YourEntityId id)  // ✓ YourEntityId（Guid ベース）
{
    var dbModel = await _dataAccess.GetByYourEntityIdAsync(id.Value);  // Guid で検索
}
```

---

## 🔗 監査情報の流れ

```
ICurrentUserService
    ↓
Repository (RepositoryBase)
    ↓ MapToDatabaseForInsert/Update
DbModel (createdBy, updatedBy, createdAt, updatedAt)
    ↓
DataAccess
    ↓
SQL Server (t_YourEntity テーブル)
```

---

## 📁 ファイル構造

```
YourContext.Infrastructure/
├── Repositories/
│   └── YourEntityRepository.cs
├── DataAccess/
│   ├── Models/
│   │   └── YourEntityDbModel.cs
│   ├── IYourEntityDataAccess.cs
│   └── YourEntityDataAccess.cs
└── Mappers/
    └── YourEntityMapper.cs

YourContext.Application/
└── Repositories/
    └── IYourEntityRepository.cs
```

---

## 参考資料

- **Mapper_パターンガイド.md** — AggregateId ↔ RowId マッピング、複数テーブル集約の複数1:1マッピング
- **Entity_設計ガイドライン.md** — Entity と AggregateId、複数テーブル集約の Entity 構造
- **ORM_マッピング戦略.md** — LocalDateTime マッピング、複数テーブル集約のマッピング例
- **AggregateId_設計ガイド.md** — GUID ベース ID の実装
- **DbModel_設計ルール.md** — DbModel 設計

