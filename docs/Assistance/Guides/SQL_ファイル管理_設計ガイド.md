# SQL ファイル管理 設計ガイド

**最終更新**: 2026-09-13  
**方針**: Context 内に SQL ファイルを完全に隔離  
**対応**: appsettings.Debug.json / appsettings.Intrinsic.json で DB 方言を自動切り替え

---

## 概要

### 設計方針

- **SQL ファイルは Context 内に完全隔離** — Context.Infrastructure/Persistence/Sql/
- **DB 方言を分離** — SqlServer/ と PostgreSQL/ フォルダで管理
- **環境別設定で自動切り替え** — appsettings.{Environment}.json の `Database:Dialect` で指定
- **ビルド時埋め込み** — DLL に埋め込んでランタイムで読み込み
- **ORM の使い分け** — SELECT は Dapper、INSERT/UPDATE/DELETE は RepoDb

---

## ディレクトリ構成

### Authentication BC の例

INSERT/UPDATE/DELETE は RepoDb のエンティティベース API を直接使用するため .sql ファイルを持たない。.sql ファイルがあるのは Dapper で実行する SELECT 系のみ。

```
src/Contexts/Authentication/Authentication.Infrastructure/
└── Persistence/
    └── Sql/
        ├── SqlServer/                          # SQL Server 用（当面使用）
        │   ├── LoginCredentials/
        │   │   └── GetLoginCredentialsByLoginId.sql
        │   └── Sessions/
        │       ├── GetUserAuthSessionById.sql
        │       ├── GetLatestUserAuthSessionByAuthorityRowId.sql
        │       └── GetLatestUserAuthSessionByLoginCredentialsRowId.sql
        └── PostgreSQL/                         # PostgreSQL 用（将来対応、未実装）
            ├── LoginCredentials/
            └── Sessions/
```

### 他の Context も同じ構成を踏襲

```
src/Contexts/Employee/Employee.Infrastructure/
└── Persistence/
    └── Sql/
        ├── SqlServer/
        │   ├── Employees/
        │   └── Persons/
        └── PostgreSQL/
            ├── Employees/
            └── Persons/

src/Contexts/Department/Department.Infrastructure/
└── Persistence/
    └── Sql/
        ├── SqlServer/
        │   └── Departments/
        └── PostgreSQL/
            └── Departments/
```

---

## 環境別設定ファイルの構成

### appsettings.json（ベース設定）

```json
{
  "Database": {
    "Dialect": "SqlServer",
    "ConnectionString": "Server=(local);Database=SupportAdvance;Trusted_Connection=true;"
  }
}
```

### appsettings.Debug.json（開発環境）

```json
{
  "Database": {
    "Dialect": "SqlServer",
    "ConnectionString": "Server=localhost;Database=SupportAdvance_Dev;Trusted_Connection=true;"
  }
}
```

### appsettings.Intrinsic.json（テスト/検証環境）

```json
{
  "Database": {
    "Dialect": "SqlServer",
    "ConnectionString": "Server=test-server;Database=SupportAdvance_Test;Trusted_Connection=false;User Id=sa;Password=****;"
  }
}
```

### appsettings.Production.json（本番環境・将来用）

```json
{
  "Database": {
    "Dialect": "PostgreSQL",
    "ConnectionString": "Host=prod-db.example.com;Database=supportadvance_prod;Username=appuser;Password=****;SSL Mode=Require;"
  }
}
```

---

## SqlQueryLoader の実装

### クラス設計

**ファイル**: `src/Infrastructure/Persistence/SqlQueryLoader.cs`

実際の実装は [SqlQueryLoader.cs](../../../src/Infrastructure/Persistence/SqlQueryLoader.cs) を参照。要点のみ抜粋:

```csharp
namespace SupportAdvance.Infrastructure.Persistence;

public class SqlQueryLoader
{
    private readonly IAppSettings _appSettings;

    public SqlQueryLoader(IAppSettings appSettings)
    {
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
    }

    /// <param name="queryPath">クエリパス（例: "LoginCredentials.GetLoginCredentialsByLoginId"）</param>
    /// <param name="repositoryType">Repository/QueryService の Type（Namespace からContext を特定するために使用）</param>
    public string LoadQuery(string queryPath, Type repositoryType)
    {
        var dbDialect = _appSettings.Database.Dialect ?? "SqlServer";

        // repositoryType.Namespace の末尾 ".Repositories" / ".Queries" を除去して Context の名前空間を得る
        // 例: SupportAdvance.Contexts.Authentication.Infrastructure.Repositories.UserAuthSessionRepository
        //   → SupportAdvance.Contexts.Authentication.Infrastructure

        // クエリパスを "Category.QueryName" として分割し、
        // {contextNamespace}.Persistence.Sql.{dbDialect}.{category}.{queryName}.sql
        // という埋め込みリソース名でアセンブリから検索する
    }
}
```

**呼び出し例**: `_sqlQueryLoader.LoadQuery("LoginCredentials.GetLoginCredentialsByLoginId", typeof(LoginCredentialsQueryService))`
→ `Authentication.Infrastructure/Persistence/Sql/SqlServer/LoginCredentials/GetLoginCredentialsByLoginId.sql`

### DI への登録

**ファイル**: `src/Infrastructure/DependencyInjection.cs`

```csharp
public static IServiceCollection AddInfrastructureModels(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // ...
    services.AddSingleton<SqlQueryLoader>();  // IAppSettings はコンストラクタ引数として DI が自動解決
    services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
    // ...
    return services;
}
```

---

## Repository での使用方法

### Example 1: Dapper (SELECT)

**ファイル**: `src/Contexts/Authentication/Authentication.Infrastructure/Queries/LoginCredentialsQueryService.cs`

```csharp
using Dapper;
using SupportAdvance.Contexts.Authentication.Application.Queries;
using SupportAdvance.Infrastructure.Persistence;

namespace SupportAdvance.Contexts.Authentication.Infrastructure.Queries;

public sealed class LoginCredentialsQueryService(
    IDbConnectionFactory connectionFactory,
    SqlQueryLoader sqlQueryLoader) : ILoginCredentialsQuery
{
    public async Task<LoginCredentialsQueryResult?> GetByLoginIdAsync(string loginId)
    {
        // Persistence/Sql/SqlServer/LoginCredentials/GetLoginCredentialsByLoginId.sql
        var sql = sqlQueryLoader.LoadQuery("LoginCredentials.GetLoginCredentialsByLoginId", typeof(LoginCredentialsQueryService));

        using var connection = connectionFactory.CreateConnection();

        // Dapper で実行
        return await connection.QueryFirstOrDefaultAsync<LoginCredentialsQueryResult>(
            sql,
            new { LoginId = loginId });
    }
}
```

**SQL ファイル**: `src/Contexts/Authentication/Authentication.Infrastructure/Persistence/Sql/SqlServer/LoginCredentials/GetLoginCredentialsByLoginId.sql`

```sql
SELECT
    [row_id],
    [mapping_employee_row_id],
    [login_id],
    [password_hash],
    [is_active]
FROM
    [m_login_credentials]
WHERE
    [login_id] = @LoginId
    AND [deleted_at] IS NULL
```

### Example 2: RepoDb (INSERT/UPDATE)

RepoDb はエンティティベース API を使うため .sql ファイルは不要（SqlQueryLoader は関与しない）。

**ファイル**: `src/Contexts/Authentication/Authentication.Infrastructure/Repositories/UserAuthSessionRepository.cs`

```csharp
public async Task<UserAuthSessionRowId> SaveAsync(UserAuthSession session)
{
    var dbModel = _mapper.ToDbModel(session);

    if (dbModel.RowId == 0)
        dbModel.RowId = await _sequenceProvider.GetNextValueAsync();

    // 監査フィールドは Repository が設定する（Mapper では設定しない）
    dbModel.CreatedAt = _clock.JstNow.Value;
    dbModel.CreatedBy = 1; // TODO: 現在のユーザーを取得する仕組みが必要

    using var connection = _connectionFactory.CreateConnection();

    // RepoDb のエンティティベース API で INSERT（SQL ファイルなし）
    await connection.InsertAsync<UserAuthSessionDbModel>(dbModel);

    return UserAuthSessionRowId.From(dbModel.RowId);
}
```

`UserAuthSessionDbModel` は `[Table("t_user_auth_sessions")]` の属性で対応するテーブルを指定し、各プロパティに `[Column("...")]` でカラム名を明示している（[UserAuthSessionDbModel.cs](../../../src/Contexts/Authentication/Authentication.Infrastructure/DbModels/UserAuthSessionDbModel.cs) 参照）。

---

## .csproj での埋め込み設定

### Authentication.Infrastructure.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
    </PropertyGroup>

    <!-- SQL ファイルを埋め込みリソースとして登録 -->
    <ItemGroup>
        <EmbeddedResource Include="Persistence/Sql/SqlServer/**/*.sql" />
        <EmbeddedResource Include="Persistence/Sql/PostgreSQL/**/*.sql" />
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="../Authentication.Domain/Authentication.Domain.csproj" />
        <ProjectReference Include="../Authentication.Application/Authentication.Application.csproj" />
        <ProjectReference Include="../../Infrastructure/Infrastructure.csproj" />
    </ItemGroup>
</Project>
```

---

## 環境別での動作

### 開発環境（Debug）

```bash
# appsettings.Debug.json が適用される
# "Database:Dialect": "SqlServer"

# Persistence/Sql/SqlServer/LoginCredentials/GetLoginCredentialsByLoginId.sql が読み込まれる
var sql = sqlQueryLoader.LoadQuery("LoginCredentials.GetLoginCredentialsByLoginId", typeof(LoginCredentialsQueryService));
// → SqlServer 方言の SQL が実行
```

### テスト環境（Intrinsic）

```bash
# appsettings.Intrinsic.json が適用される
# "Database:Dialect": "SqlServer"

# Persistence/Sql/SqlServer/LoginCredentials/GetLoginCredentialsByLoginId.sql が読み込まれる
# テスト用接続文字列で実行
```

### 本番環境（Production・将来用）

```bash
# appsettings.Production.json が適用される
# "Database:Dialect": "PostgreSQL"

# Persistence/Sql/PostgreSQL/LoginCredentials/GetLoginCredentialsByLoginId.sql が読み込まれる
// → PostgreSQL 方言の SQL が実行
```

---

## SQL Server → PostgreSQL 移行手順

### Step 1: PostgreSQL SQL ファイルの準備

```bash
# SqlServer フォルダの内容をコピー
cp -r Persistence/Sql/SqlServer/* Persistence/Sql/PostgreSQL/

# PostgreSQL 方言に修正
# 例：
# - [column] → "column"（クォート形式）
# - CAST(...AS int) → CAST(...AS INTEGER)
# - IDENTITY() → SERIAL
# など
```

### Step 2: appsettings.Production.json を更新

```json
{
  "Database": {
    "Dialect": "PostgreSQL",
    "ConnectionString": "Host=prod-db.example.com;Database=supportadvance_prod;Username=appuser;Password=****;"
  }
}
```

### Step 3: ビルド & デプロイ

```bash
# ビルド時に PostgreSQL SQL ファイルも埋め込まれる
dotnet build

# 本番環境にデプロイ
# SqlQueryLoader が自動的に PostgreSQL フォルダの SQL を読み込む
```

---

## チェックリスト

### Context 作成時

- [ ] `[Context].Infrastructure/Persistence/Sql/SqlServer/[Category]/` フォルダ作成
- [ ] `[Context].Infrastructure/Persistence/Sql/PostgreSQL/[Category]/` フォルダ作成（空でOK）
- [ ] `[Context].Infrastructure.csproj` に `<EmbeddedResource>` 設定を追加
- [ ] Repository で `SqlQueryLoader.LoadQuery()` を使用
- [ ] SQL ファイルをカテゴリごとにフォルダ分け

### SQL ファイル命名規則

- **ファイル名**: PascalCase + .sql（例: `GetLoginCredentialsByLoginId.sql`）
- **フォルダ**: 操作対象テーブル/エンティティ（例: `LoginCredentials/`, `Sessions/`）
- **クエリパス**: `Category.QueryName`（例: `LoginCredentials.GetLoginCredentialsByLoginId`）

### SQL 方言の統一

- **SqlServer**: `[Schema].[TableName]`, `CAST(...AS int)`
- **PostgreSQL**: `"schema"."table_name"`, `CAST(...AS INTEGER)`
- **複数ファイル内の統一**: IDE の Find & Replace で確認

---

## トラブルシューティング

### エラー: SQL ファイルが見つからない

```
FileNotFoundException: SQL file not found: SupportAdvance.Contexts.Authentication.Infrastructure.Persistence.Sql.SqlServer.LoginCredentials.GetLoginCredentialsByLoginId.sql
```

**原因と対策:**
1. **ファイルパスが間違っている** → `[Context].Infrastructure/Persistence/Sql/SqlServer/LoginCredentials/GetLoginCredentialsByLoginId.sql` を確認
2. **.csproj に `<EmbeddedResource>` が未登録** → Authentication.Infrastructure.csproj に `<EmbeddedResource Include="Persistence/Sql/**/*.sql" />` を追加
3. **クエリパスが間違っている** → `LoadQuery("LoginCredentials.GetLoginCredentialsByLoginId", ...)` を確認（ドットの位置）

### エラー: DB 方言が見つからない

```
NullReferenceException: Database:Dialect is not configured in appsettings.json
```

**原因と対策:**
- `appsettings.json` に `"Database": { "Dialect": "SqlServer" }` を追加

---

## 参考資料

- [Authentication_BC_設計ガイド.md](Authentication_BC_設計ガイド.md) — Authentication BC の全体設計
- [SqlQueryLoader.cs](../../../src/Infrastructure/Persistence/SqlQueryLoader.cs) — ローダーの実装
