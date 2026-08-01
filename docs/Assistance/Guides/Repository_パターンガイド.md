# Repository パターンガイド

Generic Repository パターンと監査情報管理の実装ガイドです。

---

## 📋 基本原則

### RepositoryBase<TEntity, TDbModel>

すべての Repository は `RepositoryBase` を継承し、監査情報の自動管理を実現します。

```csharp
public abstract class RepositoryBase<TEntity, TDbModel>
{
    protected readonly IEntityMapper<TEntity, TDbModel> _mapper;
    protected readonly ICurrentUserService _currentUser;
    protected readonly IClock _clock;

    protected RepositoryBase(
        IEntityMapper<TEntity, TDbModel> mapper,
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
    // 取得
    Task<YourEntity?> GetByBusinessIdAsync(YourBusinessId businessId);
    Task<YourEntity?> GetByRowIdAsync(long rowId);
    Task<IReadOnlyList<YourEntity>> GetAllAsync();

    // 保存
    Task AddAsync(YourEntity entity);
    Task UpdateAsync(YourEntity entity);

    // 削除
    Task<bool> DeleteAsync(YourBusinessId businessId);
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

public class YourEntityRepository
    : RepositoryBase<YourEntity, YourEntityDbModel>,
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

    public async Task<YourEntity?> GetByBusinessIdAsync(YourBusinessId businessId)
    {
        ArgumentNullException.ThrowIfNull(businessId);

        // 1. DataAccess でビジネス ID で検索
        var dbModel = await _dataAccess.GetByBusinessIdAsync(businessId.Value);
        
        // 2. DbModel が見つからなかった
        if (dbModel == null)
            return null;

        // 3. Mapper で Domain Entity に変換
        return MapToDomain(dbModel);
    }

    public async Task<YourEntity?> GetByRowIdAsync(long rowId)
    {
        if (rowId <= 0)
            throw new ArgumentException("RowId は正の値である必要があります。");

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

    public async Task AddAsync(YourEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        // 1. Mapper で DbModel に変換
        var dbModel = MapToDatabaseForInsert(entity);  // ← createdBy, createdAt を自動設定

        // 2. DataAccess で保存
        await _dataAccess.InsertAsync(dbModel);
    }

    public async Task UpdateAsync(YourEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        // 1. Mapper で DbModel に変換
        var dbModel = MapToDatabaseForUpdate(entity);  // ← updatedBy, updatedAt を自動設定

        // 2. DataAccess で更新
        await _dataAccess.UpdateAsync(dbModel);
    }

    public async Task<bool> DeleteAsync(YourBusinessId businessId)
    {
        ArgumentNullException.ThrowIfNull(businessId);

        // 1. ビジネス ID で検索
        var dbModel = await _dataAccess.GetByBusinessIdAsync(businessId.Value);
        if (dbModel == null)
            return false;

        // 2. 論理削除：監査情報を設定
        SetDeletedByAudit(dbModel);  // ← deletedBy, deletedAt を自動設定

        // 3. DataAccess で削除マーク
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
    Task<YourEntityDbModel?> GetByBusinessIdAsync(int businessId);
    Task<YourEntityDbModel?> GetByRowIdAsync(long rowId);
    Task<List<YourEntityDbModel>> GetAllAsync();
    Task InsertAsync(YourEntityDbModel model);
    Task UpdateAsync(YourEntityDbModel model);
}
```

### 4. DataAccess 実装

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

    public async Task<YourEntityDbModel?> GetByBusinessIdAsync(int businessId)
    {
        using (var connection = new SqlConnection(GetConnectionString()))
        {
            return await connection.QueryAsync<YourEntityDbModel>(
                x => x.YourBusinessId == businessId && x.DeletedAt == null
            ).ContinueWith(t => t.Result.FirstOrDefault());
        }
    }

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

---

## ✅ 実装チェックリスト

### Repository インターフェース

- [ ] **Domain に依存しない**: Domain Entity 型を返す
- [ ] **Application に定義**: Repository インターフェースは Application 層
- [ ] **ビジネス ID メソッド**: GetByBusinessIdAsync など
- [ ] **RowId メソッド**: GetByRowIdAsync など
- [ ] **Add/Update/Delete**: CRUD 操作をすべてカバー

### Repository 実装

- [ ] **RepositoryBase を継承**: 監査情報を自動管理
- [ ] **ICurrentUserService 依存**: createdBy/updatedBy を自動設定
- [ ] **Mapper 使用**: Entity ↔ DbModel 変換は Mapper に委譲
- [ ] **DataAccess 依存**: 実際の DB 操作は DataAccess に委譲
- [ ] **null チェック**: すべてのパラメータで ArgumentNullException

### DataAccess 実装

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
    Task<YourEntityDbModel?> GetAsync(int businessId);  // ✗ DbModel
}
```

**修正:**
```csharp
// ✓ Repository は Entity を返す
public interface IYourEntityRepository
{
    Task<YourEntity?> GetAsync(YourBusinessId businessId);  // ✓ Entity
}
```

### パターン2: Repository が Mapper に依存しない

```csharp
// ✗ 禁止: Repository が手動で Entity を構築
public class YourEntityRepository : IYourEntityRepository
{
    public async Task<YourEntity?> GetAsync(YourBusinessId businessId)
    {
        var dbModel = await _dataAccess.GetByBusinessIdAsync(businessId.Value);
        
        // ✗ 手動で Entity を構築
        return new YourEntity(
            dbModel.RowId,
            YourBusinessId.From(dbModel.YourBusinessId),
            dbModel.Name,
            _clock
        );
    }
}
```

**修正:**
```csharp
// ✓ Mapper を使用
public class YourEntityRepository : IYourEntityRepository
{
    public async Task<YourEntity?> GetAsync(YourBusinessId businessId)
    {
        var dbModel = await _dataAccess.GetByBusinessIdAsync(businessId.Value);
        return dbModel == null ? null : MapToDomain(dbModel);  // Mapper 使用
    }
}
```

### パターン3: 監査情報を手動で設定

```csharp
// ✗ 禁止: Repository が監査情報を手動で設定
public async Task AddAsync(YourEntity entity)
{
    var dbModel = _mapper.ToDbModel(entity);
    dbModel.CreatedBy = _currentUser.EmployeeRowId;  // ✗ 手動設定
    dbModel.CreatedAt = _clock.JstNow;  // ✗ 手動設定
    await _dataAccess.InsertAsync(dbModel);
}
```

**修正:**
```csharp
// ✓ MapToDatabaseForInsert を使用
public async Task AddAsync(YourEntity entity)
{
    var dbModel = MapToDatabaseForInsert(entity);  // 自動設定
    await _dataAccess.InsertAsync(dbModel);
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

### フロー例

```csharp
// 1. UseCase が Entity を作成
var entity = new YourEntity(rowId, businessId, name, clock);

// 2. Repository.AddAsync(entity) を呼び出し
await _repository.AddAsync(entity);

// 3. Repository が Mapper で DbModel に変換
var dbModel = _mapper.ToDbModel(entity);

// 4. Repository が監査情報を自動設定
dbModel.CreatedBy = _currentUser.EmployeeRowId;  // ICurrentUserService から取得
dbModel.CreatedAt = _clock.JstNow;

// 5. DataAccess で DB に保存
await _dataAccess.InsertAsync(dbModel);
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

- **Mapper_パターンガイド.md**: Mapper 実装方法
- **Entity_設計ガイドライン.md**: Entity 設計
- **DbModel_設計ルール.md**: DbModel 設計
- **src/Contexts/Samples/CarPreferences.Infrastructure**: 実装例
