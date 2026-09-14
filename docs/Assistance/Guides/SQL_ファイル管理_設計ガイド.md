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

### Identity BC の例

```
src/Contexts/Identity/Identity.Infrastructure/
└── Persistence/
    └── Sql/
        ├── SqlServer/                          # SQL Server 用（当面使用）
        │   ├── Auth/
        │   │   ├── GetLoginCredentialsByLoginId.sql
        │   │   ├── InsertUserAuthSession.sql
        │   │   ├── UpdateLoginCredentialsLastLogin.sql
        │   │   └── GetUserAuthSessionByRowId.sql
        │   └── Identities/
        │       ├── GetUserAuthSessionByRowId.sql
        │       ├── GetLoginHistory.sql
        │       └── InsertUserAuthSession.sql
        └── PostgreSQL/                         # PostgreSQL 用（将来対応）
            ├── Auth/
            │   ├── GetLoginCredentialsByLoginId.sql
            │   ├── InsertUserAuthSession.sql
            │   ├── UpdateLoginCredentialsLastLogin.sql
            │   └── GetUserAuthSessionByRowId.sql
            └── Identities/
                ├── GetUserAuthSessionByRowId.sql
                ├── GetLoginHistory.sql
                └── InsertUserAuthSession.sql
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

```csharp
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SupportAdvance.Infrastructure.Persistence;

/// <summary>
/// SQL ファイルを埋め込みリソースから読み込むローダー
///
/// 【用途】
/// - 環境に応じて SqlServer / PostgreSQL の SQL を自動切り替え
/// - Dapper / RepoDb での SQL 実行時に呼び出し
///
/// 【責務】
/// 1. IConfiguration から DB 方言を読み込み
/// 2. Context の Assembly から埋め込み SQL リソースを検索
/// 3. DB 方言に応じて正しいファイルを返す
///
/// 【使用例】
/// var sql = SqlQueryLoader.LoadQuery("Auth.GetLoginCredentialsByLoginId", typeof(LoginCredentialsRepository));
/// // → Identity.Infrastructure/Persistence/Sql/[SqlServer|PostgreSQL]/Auth/GetLoginCredentialsByLoginId.sql
/// </summary>
public class SqlQueryLoader
{
    private readonly IConfiguration _configuration;

    public SqlQueryLoader(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// SQL ファイルを読み込む
    /// </summary>
    /// <param name="queryPath">クエリパス（例: "Auth.GetLoginCredentialsByLoginId"）</param>
    /// <param name="repositoryType">Repository の Type（リソース検索用）</param>
    /// <returns>SQL ファイルの内容</returns>
    /// <exception cref="FileNotFoundException">SQL ファイルが見つからない場合</exception>
    public string LoadQuery(string queryPath, Type repositoryType)
    {
        ArgumentException.ThrowIfNullOrEmpty(queryPath, nameof(queryPath));
        ArgumentNullException.ThrowIfNull(repositoryType, nameof(repositoryType));

        // パスの検証と分割
        var parts = queryPath.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
            throw new ArgumentException(
                $"Invalid query path format: '{queryPath}'. Expected format: 'Category.QueryName' (e.g., 'Auth.GetLoginCredentialsByLoginId')",
                nameof(queryPath));

        var category = parts[0];       // "Auth"
        var queryName = parts[1];      // "GetLoginCredentialsByLoginId"

        // DB 方言を取得（appsettings.json / appsettings.{Environment}.json）
        var dbDialect = _configuration["Database:Dialect"] ?? "SqlServer";

        // Namespace 構築
        // repositoryType: SupportAdvance.Contexts.Identity.Infrastructure.Repositories.LoginCredentialsRepository
        // Namespace: SupportAdvance.Contexts.Identity.Infrastructure
        var contextNamespace = repositoryType.Namespace;
        if (string.IsNullOrEmpty(contextNamespace))
            throw new InvalidOperationException($"Cannot determine namespace for type {repositoryType.FullName}");

        // リソース名構築
        // SupportAdvance.Contexts.Identity.Infrastructure.Persistence.Sql.SqlServer.Auth.GetLoginCredentialsByLoginId.sql
        var resourceName = $"{contextNamespace}.Persistence.Sql.{dbDialect}.{category}.{queryName}.sql";

        // Assembly から埋め込みリソースを取得
        var assembly = repositoryType.Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException(
                $"SQL file not found: {resourceName}\n" +
                $"Expected location: [Context].Infrastructure/Persistence/Sql/{dbDialect}/{category}/{queryName}.sql\n" +
                $"Database Dialect: {dbDialect}\n" +
                $"Assembly: {assembly.FullName}");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// 現在の DB 方言を取得
    /// </summary>
    public string GetCurrentDialect()
    {
        return _configuration["Database:Dialect"] ?? "SqlServer";
    }
}
```

### DI への登録

**ファイル**: `src/Infrastructure/DependencyInjection.cs`

```csharp
public static IServiceCollection AddInfrastructureModels(
    this IServiceCollection services,
    IConfiguration configuration)
{
    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(configuration);

    // SqlQueryLoader を DI に登録
    services.AddSingleton(new SqlQueryLoader(configuration));

    // その他の Infrastructure サービス
    services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

    return services;
}
```

---

## Repository での使用方法

### Example 1: Dapper (SELECT)

**ファイル**: `src/Contexts/Identity/Identity.Infrastructure/Repositories/LoginCredentialsRepository.cs`

```csharp
using SupportAdvance.Infrastructure.Persistence;

namespace SupportAdvance.Contexts.Identity.Identity.Infrastructure.Repositories;

public class LoginCredentialsRepository(
    SqlQueryLoader queryLoader,
    IDbConnectionFactory connectionFactory) : ILoginCredentialsRepository
{
    public async Task<LoginCredentialsDbModel?> GetByLoginIdAsync(string loginId)
    {
        // SQL ファイルを読み込み
        // Persistence/Sql/[SqlServer|PostgreSQL]/Auth/GetLoginCredentialsByLoginId.sql
        var sql = _queryLoader.LoadQuery("Auth.GetLoginCredentialsByLoginId", typeof(LoginCredentialsRepository));

        using var connection = _connectionFactory.CreateConnection();
        
        // Dapper で実行
        return await connection.QuerySingleOrDefaultAsync<LoginCredentialsDbModel>(
            sql,
            new { LoginId = loginId });
    }
}
```

**SQL ファイル**: `src/Contexts/Identity/Identity.Infrastructure/Persistence/Sql/SqlServer/Auth/GetLoginCredentialsByLoginId.sql`

```sql
SELECT 
    [row_id],
    [row_version],
    [login_id],
    [password_hash],
    [is_active],
    [mapping_employee_row_id],
    [last_login_at],
    [created_at],
    [created_by],
    [updated_at],
    [updated_by],
    [deleted_at],
    [deleted_by]
FROM [dbo].[m_login_credentials]
WHERE [login_id] = @LoginId
AND [deleted_at] IS NULL
```

### Example 2: RepoDb (INSERT)

**ファイル**: `src/Contexts/Identity/Identity.Infrastructure/Repositories/UserAuthSessionRepository.cs`

```csharp
public class UserAuthSessionRepository(
    SqlQueryLoader queryLoader,
    IDbConnectionFactory connectionFactory,
    ICurrentUserService currentUser,
    IClock clock) : IUserAuthSessionRepository
{
    public async Task SaveAsync(UserAuthSession session)
    {
        // SQL ファイルを読み込み
        var sql = _queryLoader.LoadQuery("Identities.InsertUserAuthSession", typeof(UserAuthSessionRepository));

        using var connection = _connectionFactory.CreateConnection();
        
        // RepoDb で実行
        await connection.ExecuteAsync(sql, new DbParameter[]
        {
            new("@RowId", session.RowId.Value),
            new("@IsADAuthenticated", session.IsADAuthenticated),
            new("@IdentityRowId", session.IdentityRowId),
            new("@AuthorityRowId", session.AuthorityRowId.Value),
            new("@LoggedInAt", session.LoggedInAt.Value),
            new("@PreviousLoginAt", session.PreviousLoginAt?.Value),
            new("@CreatedAt", _clock.JstNow.Value),
            new("@CreatedBy", _currentUser.EmployeeRowId)
        });
    }
}
```

**SQL ファイル**: `src/Contexts/Identity/Identity.Infrastructure/Persistence/Sql/SqlServer/Identities/InsertUserAuthSession.sql`

```sql
INSERT INTO [dbo].[m_user_auth_sessions]
    ([row_id], [is_ad_authenticated], [identity_row_id], [authority_row_id], 
     [logged_in_at], [previous_login_at], [created_at], [created_by])
VALUES
    (@RowId, @IsADAuthenticated, @IdentityRowId, @AuthorityRowId,
     @LoggedInAt, @PreviousLoginAt, @CreatedAt, @CreatedBy)
```

---

## .csproj での埋め込み設定

### Identity.Infrastructure.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net8.0</TargetFramework>
    </PropertyGroup>

    <!-- SQL ファイルを埋め込みリソースとして登録 -->
    <ItemGroup>
        <EmbeddedResource Include="Persistence/Sql/SqlServer/**/*.sql" />
        <EmbeddedResource Include="Persistence/Sql/PostgreSQL/**/*.sql" />
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="../Identity.Domain/Identity.Domain.csproj" />
        <ProjectReference Include="../Identity.Application/Identity.Application.csproj" />
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

# Persistence/Sql/SqlServer/Auth/GetLoginCredentialsByLoginId.sql が読み込まれる
var sql = queryLoader.LoadQuery("Auth.GetLoginCredentialsByLoginId", typeof(LoginCredentialsRepository));
// → SqlServer 方言の SQL が実行
```

### テスト環境（Intrinsic）

```bash
# appsettings.Intrinsic.json が適用される
# "Database:Dialect": "SqlServer"

# Persistence/Sql/SqlServer/Auth/GetLoginCredentialsByLoginId.sql が読み込まれる
# テスト用接続文字列で実行
```

### 本番環境（Production・将来用）

```bash
# appsettings.Production.json が適用される
# "Database:Dialect": "PostgreSQL"

# Persistence/Sql/PostgreSQL/Auth/GetLoginCredentialsByLoginId.sql が読み込まれる
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
- **フォルダ**: 操作対象テーブル/エンティティ（例: `Auth/`, `Identities/`）
- **クエリパス**: `Category.QueryName`（例: `Auth.GetLoginCredentialsByLoginId`）

### SQL 方言の統一

- **SqlServer**: `[Schema].[TableName]`, `CAST(...AS int)`
- **PostgreSQL**: `"schema"."table_name"`, `CAST(...AS INTEGER)`
- **複数ファイル内の統一**: IDE の Find & Replace で確認

---

## トラブルシューティング

### エラー: SQL ファイルが見つからない

```
FileNotFoundException: SQL file not found: SupportAdvance.Contexts.Identity.Infrastructure.Persistence.Sql.SqlServer.Auth.GetLoginCredentialsByLoginId.sql
```

**原因と対策:**
1. **ファイルパスが間違っている** → `[Context].Infrastructure/Persistence/Sql/SqlServer/Auth/GetLoginCredentialsByLoginId.sql` を確認
2. **.csproj に `<EmbeddedResource>` が未登録** → Identity.Infrastructure.csproj に `<EmbeddedResource Include="Persistence/Sql/**/*.sql" />` を追加
3. **クエリパスが間違っている** → `LoadQuery("Auth.GetLoginCredentialsByLoginId", ...)` を確認（ドットの位置）

### エラー: DB 方言が見つからない

```
NullReferenceException: Database:Dialect is not configured in appsettings.json
```

**原因と対策:**
- `appsettings.json` に `"Database": { "Dialect": "SqlServer" }` を追加

---

## 参考資料

- [Identity_BC_設計ガイド.md](Identity_BC_設計ガイド.md) — Identity BC の全体設計
- [SqlQueryLoader.cs](../../Infrastructure/Persistence/SqlQueryLoader.cs) — ローダーの実装
