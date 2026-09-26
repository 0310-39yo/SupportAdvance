# Bounded Context テンプレート

新規 Bounded Context（BC）を追加する際のチェックリストと実装テンプレートです。

---

## 📋 新規 BC 追加チェックリスト

### ステップ 1: プロジェクト構造の準備

```
src/Contexts/YourGroup/YourContext/
├── YourContext.Domain/
├── YourContext.Application/
└── YourContext.Infrastructure/
```

**チェック項目：**
- [ ] フォルダ構造を作成
- [ ] `.csproj` ファイルを作成（テンプレート参照）
- [ ] `CLAUDE.md` を配置

### ステップ 2: .csproj 設定

**YourContext.Domain.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\..\SharedKernel\SharedKernel.csproj" />
    <ProjectReference Include="..\..\..\Common\Common.csproj" />
  </ItemGroup>
</Project>
```

**YourContext.Application.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\..\Application\Application.csproj" />
    <ProjectReference Include="..\..\..\SharedKernel\SharedKernel.csproj" />
    <ProjectReference Include="..\..\..\Common\Common.csproj" />
    <ProjectReference Include="..\YourContext.Domain\YourContext.Domain.csproj" />
  </ItemGroup>
</Project>
```

**YourContext.Infrastructure.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\..\Application\Application.csproj" />
    <ProjectReference Include="..\..\..\Infrastructure\Infrastructure.csproj" />
    <ProjectReference Include="..\YourContext.Application\YourContext.Application.csproj" />
    <ProjectReference Include="..\YourContext.Domain\YourContext.Domain.csproj" />
  </ItemGroup>
  <ItemGroup>
    <EmbeddedResource Include="Migrations\*.sql" />
  </ItemGroup>
</Project>
```

**チェック項目：**
- [ ] Domain: SharedKernel, Common のみ依存
- [ ] Application: Domain, SharedKernel, Common, Application（汎用）依存
- [ ] Infrastructure: Domain, Application（自BC）, Infrastructure依存
- [ ] 循環参照がない

### ステップ 3: 依存関係の検証

```bash
# .csproj ファイルで ProjectReference を確認
cat src/Contexts/YourGroup/YourContext/YourContext.Domain/YourContext.Domain.csproj

# 逆向き依存がないことを確認
grep -r "YourContext.Domain" src/Application/ src/Infrastructure/
# マッチなし = OK
```

**チェック項目：**
- [ ] Domain ← Application (禁止)
- [ ] Domain ← Infrastructure (禁止)
- [ ] Application ← Infrastructure (禁止)

### ステップ 4: アーキテクチャの実装

#### Domain 層

**Entity の定義：**
```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Domain/Entities/YourEntity.cs

using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Domain.Entities;

public class YourEntity : AggregateRoot<RowId>
{
    private YourBusinessId _yourBusinessId = null!;
    private string _name = null!;

    public YourBusinessId YourBusinessId => _yourBusinessId;
    public string Name => _name;

    public YourEntity(YourBusinessId yourBusinessId, string name, IClock clock, RowId? rowId = null)
    {
        ArgumentNullException.ThrowIfNull(yourBusinessId);
        ArgumentNullException.ThrowIfNull(name);

        Id = rowId ?? RowId.New();  // 未採番の場合は RowId.New()
        _yourBusinessId = yourBusinessId;
        _name = name;
        // クロック設定など
    }
}
```

**チェック項目：**
- [ ] Entity<RowId> で RowId ValueObject ベースの識別子
- [ ] ビジネス識別子は別プロパティ（YourBusinessId）
- [ ] RowId.From(long) または RowId.New() で生成
- [ ] Domain イベント発行能力
- [ ] LocalDateTime 使用

#### Application 層

**Repository インターフェース：**
```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Application/Repositories/IYourEntityRepository.cs

using SupportAdvance.Contexts.YourGroup.YourContext.Domain.Entities;
using SupportAdvance.Contexts.YourGroup.YourContext.Domain.ValueObjects;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Application.Repositories;

public interface IYourEntityRepository
{
    Task<YourEntity?> GetAsync(YourBusinessId yourBusinessId);
    Task AddAsync(YourEntity entity);
    Task UpdateAsync(YourEntity entity);
    Task<bool> DeleteAsync(YourBusinessId yourBusinessId);
    Task<IReadOnlyList<YourEntity>> GetAllAsync();
}
```

**UseCase の定義：**
```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Application/UseCases/Create/CreateYourEntityUseCase.cs

using SupportAdvance.Application.UseCases;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.YourGroup.YourContext.Application.Repositories;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Application.UseCases.Create;

public class CreateYourEntityUseCase
    : IUseCase<CreateYourEntityRequest, CreateYourEntityResponse>
{
    private readonly IYourEntityRepository _repository;
    private readonly IClock _clock;

    public CreateYourEntityUseCase(IYourEntityRepository repository, IClock clock)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<CreateYourEntityResponse> ExecuteAsync(CreateYourEntityRequest request)
    {
        // 実装
        throw new NotImplementedException();
    }
}
```

**チェック項目：**
- [ ] Repository インターフェース定義（Domain に依存しない）
- [ ] UseCase の実装
- [ ] Domain イベント処理

#### Infrastructure 層

**DbModel の定義：**
```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Infrastructure/DataAccess/Models/YourEntityDbModel.cs

using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.DataAccess.Models;

public class YourEntityDbModel
{
    public long RowId { get; set; }
    public int YourBusinessId { get; set; }
    public byte[] RowVersion { get; set; } = null!;
    
    // 監査カラム（必須）
    public LocalDateTime CreatedAt { get; set; }
    public long CreatedBy { get; set; }
    public LocalDateTime? UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public LocalDateTime? DeletedAt { get; set; }
    public long? DeletedBy { get; set; }

    // ビジネスカラム
    public string Name { get; set; } = null!;
}
```

**Mapper の実装：**
```csharp
// src/Contexts/YourGroup/YourContext/YourContext.Infrastructure/Mappers/YourEntityMapper.cs

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.YourGroup.YourContext.Domain.Entities;
using SupportAdvance.Contexts.YourGroup.YourContext.Domain.ValueObjects;
using SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.DataAccess.Models;
using SupportAdvance.Infrastructure.Mappers;

namespace SupportAdvance.Contexts.YourGroup.YourContext.Infrastructure.Mappers;

public class YourEntityMapper : IEntityMapper<YourEntity, YourEntityDbModel>
{
    public YourEntityDbModel ToDbModel(YourEntity entity)
    {
        return new YourEntityDbModel
        {
            RowId = entity.Id,
            YourBusinessId = entity.YourBusinessId.Value,
            CreatedAt = entity.CreatedAt,
            CreatedBy = 0,  // Repository で設定
            Name = entity.Name
        };
    }

    public YourEntity ToDomainEntity(YourEntityDbModel dbModel, IClock clock)
    {
        var yourBusinessId = YourBusinessId.From(dbModel.YourBusinessId);
        return new YourEntity(dbModel.RowId, yourBusinessId, dbModel.Name, clock);
    }
}
```

**Repository の実装：**
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
using SupportAdvance.Application.Abstractions.Services;

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

    public async Task<YourEntity?> GetAsync(YourBusinessId yourBusinessId)
    {
        var dbModel = await _dataAccess.GetByYourBusinessIdAsync(yourBusinessId.Value);
        return dbModel == null ? null : MapToDomain(dbModel);
    }

    public async Task AddAsync(YourEntity entity)
    {
        var dbModel = MapToDatabaseForInsert(entity);
        await _dataAccess.InsertAsync(dbModel);
    }

    // 他のメソッド...
}
```

**チェック項目：**
- [ ] DbModel に LocalDateTime 使用（DateTime ではなく）
- [ ] 監査カラムの8つを含める
- [ ] IEntityMapper を実装
- [ ] RepositoryBase を継承
- [ ] ICurrentUserService で createdBy 自動設定

### ステップ 5: マイグレーション

**マイグレーションスクリプト：**
```sql
-- src/Contexts/YourGroup/YourContext/YourContext.Infrastructure/Migrations/001_CreateYourEntityTable.sql

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_YourEntity')
BEGIN
    CREATE TABLE [dbo].[t_YourEntity]
    (
        -- 監査カラム（必須）
        [row_id] [bigint] NOT NULL PRIMARY KEY 
          DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence]),
        [row_version] [timestamp] NOT NULL,
        [created_at] [datetime2](7) NOT NULL,
        [created_by] [bigint] NOT NULL,
        [updated_at] [datetime2](7) NULL,
        [updated_by] [bigint] NULL,
        [deleted_at] [datetime2](7) NULL,
        [deleted_by] [bigint] NULL,

        -- ビジネスカラム
        [your_business_id] [int] NOT NULL UNIQUE,
        [name] [nvarchar](100) NOT NULL
    )
    ON [PRIMARY]
END
GO
```

**チェック項目：**
- [ ] 監査カラムの8つをすべて含める
- [ ] row_id に Sequence デフォルト設定
- [ ] LocalDateTime が datetime2(7) にマッピング
- [ ] ビジネスID に UNIQUE 制約

### ステップ 6: ビルド検証

```bash
# ビルド確認
dotnet build --no-incremental

# 依存関係確認
grep -r "YourContext" src/*/YourContext.csproj src/Application/ src/Infrastructure/
```

**チェック項目：**
- [ ] ビルド成功（0 エラー）
- [ ] 循環参照がない
- [ ] 逆向き依存がない

---

## 📁 ファイル構造テンプレート

```
src/Contexts/YourGroup/YourContext/
├── YourContext.Domain/
│   ├── Entities/
│   │   └── YourEntity.cs
│   ├── ValueObjects/
│   │   └── YourBusinessId.cs
│   ├── DomainEvents/
│   │   └── YourEntityCreatedEvent.cs
│   └── YourContext.Domain.csproj
│
├── YourContext.Application/
│   ├── Repositories/
│   │   └── IYourEntityRepository.cs
│   ├── UseCases/
│   │   ├── Create/
│   │   │   ├── CreateYourEntityRequest.cs
│   │   │   ├── CreateYourEntityResponse.cs
│   │   │   └── CreateYourEntityUseCase.cs
│   │   └── Get/
│   ├── EventHandlers/
│   │   └── YourEntityCreatedEventHandler.cs
│   └── YourContext.Application.csproj
│
└── YourContext.Infrastructure/
    ├── DataAccess/
    │   ├── Models/
    │   │   └── YourEntityDbModel.cs
    │   ├── IYourEntityDataAccess.cs
    │   └── YourEntityDataAccess.cs
    ├── Mappers/
    │   └── YourEntityMapper.cs
    ├── Repositories/
    │   └── YourEntityRepository.cs
    ├── Migrations/
    │   ├── 001_CreateYourEntityTable.sql
    │   └── 002_AddIndexes.sql
    └── YourContext.Infrastructure.csproj
```

---

## ✅ 完成チェックリスト

- [ ] **依存関係**: 循環参照なし、逆向き依存なし
- [ ] **Entity**: Entity<long>、RowId ベース、ビジネス識別子は別プロパティ
- [ ] **DbModel**: LocalDateTime 使用、監査カラムの8つ
- [ ] **Mapper**: IEntityMapper 実装、純粋なマッピング
- [ ] **Repository**: RepositoryBase 継承、ICurrentUserService 使用
- [ ] **マイグレーション**: 監査カラム完備、Sequence 設定
- [ ] **ビルド**: 0 エラー、テスト通過

---

## 参考資料

- **ENTITY_設計ガイドライン.md**: Entity の設計原則
- **Mapper_パターンガイド.md**: Mapper の実装方法
- **Repository_パターンガイド.md**: Repository の実装方法
- **TABLE_DESIGN_STANDARDS.md**: テーブル設計規則
- **src/Contexts/Samples/CarPreferences**: 実装例
