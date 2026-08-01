# ORM マッピング戦略

Dapper と RepoDb の型マッピング統一戦略です。

---

## 📋 基本原則

### LocalDateTime を統一型に

SupportAdvance では **LocalDateTime** をすべてのテーブルに使用します。ORM レベルで LocalDateTime ↔ datetime2 のマッピングを統一化します。

| 層 | 型 | 用途 |
|---|---|---|
| **C# コード** | LocalDateTime | JST タイムゾーン情報 |
| **SQL Server** | datetime2(7) | タイムゾーン情報なし（JST と解釈） |
| **ORM** | グローバルマッピング | LocalDateTime ↔ datetime2 自動変換 |

---

## 💡 実装パターン

### 1. Dapper 型登録

```csharp
// src/Infrastructure/ORM/Dapper/DapperTypeHandlerRegistration.cs

using Dapper;
using System.Data;
using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Infrastructure.ORM.Dapper;

public static class DapperTypeHandlerRegistration
{
    /// <summary>
    /// Dapper のグローバル型マッピング初期化
    /// 【タイミング】アプリケーション起動時（DependencyInjection の AddInfrastructureModels で呼び出し）
    /// 【型マッピング】LocalDateTime ↔ DbType.DateTime2
    /// </summary>
    public static void Register()
    {
        // LocalDateTime → datetime2
        SqlMapper.AddTypeMap(typeof(LocalDateTime), DbType.DateTime2);
        
        // LocalDateTime? → datetime2
        SqlMapper.AddTypeMap(typeof(LocalDateTime?), DbType.DateTime2);
    }
}
```

### 2. RepoDb 型登録

```csharp
// src/Infrastructure/ORM/RepoDB/RepoDbTypeMapperRegistration.cs

using RepoDb;
using System.Data;
using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Infrastructure.ORM.RepoDB;

public static class RepoDbTypeMapperRegistration
{
    /// <summary>
    /// RepoDb のグローバル型マッピング初期化
    /// 【タイミング】アプリケーション起動時（DependencyInjection の AddInfrastructureModels で呼び出し）
    /// 【型マッピング】LocalDateTime ↔ DbType.DateTime2
    /// </summary>
    public static void Register()
    {
        // LocalDateTime → datetime2
        TypeMapper.Add<LocalDateTime>(DbType.DateTime2, force: true);
        
        // LocalDateTime? → datetime2
        TypeMapper.Add<LocalDateTime?>(DbType.DateTime2, force: true);
    }
}
```

### 3. DependencyInjection での初期化

```csharp
// src/Infrastructure/DependencyInjection.cs

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Common.Configuration;
using SupportAdvance.Infrastructure.ORM.Dapper;
using SupportAdvance.Infrastructure.ORM.RepoDB;

namespace SupportAdvance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureModels(this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // ステップ0a: Dapper 初期化（起動時の一度だけ）
        DapperTypeHandlerRegistration.Register();

        // ステップ0b: RepoDb 初期化（起動時の一度だけ）
        RepoDbTypeMapperRegistration.Register();

        // 以下、AppSettings などの初期化...

        return services;
    }
}
```

---

## 🔄 マッピング動作

### C# ↔ SQL Server のマッピング

```
【挿入時】
C# LocalDateTime → Dapper/RepoDb → DbType.DateTime2 → SQL datetime2(7)

【読み取り時】
SQL datetime2(7) → Dapper/RepoDb → DbType.DateTime2 → C# LocalDateTime
```

### コード例

```csharp
// Entity with LocalDateTime
public class YourEntity : Entity<long>
{
    public LocalDateTime CreatedAt { get; }
    public LocalDateTime? UpdatedAt { get; }
}

// DbModel with LocalDateTime
public class YourEntityDbModel
{
    public long RowId { get; set; }
    public LocalDateTime CreatedAt { get; set; }      // ✓ LocalDateTime
    public LocalDateTime? UpdatedAt { get; set; }     // ✓ LocalDateTime?
}

// SQL Server table
// CREATE TABLE t_YourEntity (
//   row_id bigint NOT NULL PRIMARY KEY,
//   created_at datetime2(7) NOT NULL,               -- datetime2
//   updated_at datetime2(7) NULL,                   -- datetime2
//   ...
// )

// Mapper: 直接マッピング（変換不要）
public class YourEntityMapper : IEntityMapper<YourEntity, YourEntityDbModel>
{
    public YourEntityDbModel ToDbModel(YourEntity entity)
    {
        return new YourEntityDbModel
        {
            RowId = entity.Id,
            CreatedAt = entity.CreatedAt,   // LocalDateTime → LocalDateTime
            UpdatedAt = entity.UpdatedAt    // LocalDateTime? → LocalDateTime?
        };
    }

    public YourEntity ToDomainEntity(YourEntityDbModel dbModel, IClock clock)
    {
        return new YourEntity(
            dbModel.RowId,
            dbModel.CreatedAt,   // LocalDateTime → LocalDateTime
            clock
        );
    }
}
```

---

## ✅ 実装チェックリスト

### Dapper

- [ ] **DapperTypeHandlerRegistration クラス**: ORM/Dapper フォルダに配置
- [ ] **SqlMapper.AddTypeMap**: LocalDateTime と LocalDateTime? の両方
- [ ] **DbType.DateTime2**: 型マッピングの対象
- [ ] **DependencyInjection で呼び出し**: アプリ起動時にグローバル登録

### RepoDb

- [ ] **RepoDbTypeMapperRegistration クラス**: ORM/RepoDB フォルダに配置
- [ ] **TypeMapper.Add**: LocalDateTime と LocalDateTime? の両方
- [ ] **DbType.DateTime2**: 型マッピングの対象
- [ ] **force: true**: 既存マッピングを上書き
- [ ] **DependencyInjection で呼び出し**: アプリ起動時にグローバル登録

### DbModel

- [ ] **すべての日時カラムが LocalDateTime**: DateTime ではなく
- [ ] **nullable 日時は LocalDateTime?**: updated_at, deleted_at など
- [ ] **Mapper で直接マッピング**: 変換不要

---

## ❌ 避けるべきパターン

### パターン1: DbModel に DateTime を使用

```csharp
// ✗ 禁止: DbModel に DateTime
public class YourEntityDbModel
{
    public DateTime CreatedAt { get; set; }  // ✗ DateTime
    public DateTime? UpdatedAt { get; set; }  // ✗ DateTime?
}
```

**修正:**
```csharp
// ✓ DbModel は LocalDateTime を使用
public class YourEntityDbModel
{
    public LocalDateTime CreatedAt { get; set; }  // ✓ LocalDateTime
    public LocalDateTime? UpdatedAt { get; set; }  // ✓ LocalDateTime?
}
```

### パターン2: Mapper で型変換を実行

```csharp
// ✗ 禁止: Mapper が LocalDateTime → DateTime 変換
public YourEntityDbModel ToDbModel(YourEntity entity)
{
    return new YourEntityDbModel
    {
        CreatedAt = entity.CreatedAt.ToDateTime(TimeOnly.MinValue)  // ✗ 変換
    };
}
```

**修正:**
```csharp
// ✓ Mapper は直接マッピング
public YourEntityDbModel ToDbModel(YourEntity entity)
{
    return new YourEntityDbModel
    {
        CreatedAt = entity.CreatedAt  // ✓ LocalDateTime → LocalDateTime
    };
}
```

### パターン3: 型マッピングを複数回登録

```csharp
// ✗ 禁止: 毎回リクエストで型マッピングを登録
public class YourRepository
{
    public async Task<YourEntity?> GetAsync(int id)
    {
        SqlMapper.AddTypeMap(typeof(LocalDateTime), DbType.DateTime2);  // ✗ 毎回登録
        // クエリ実行...
    }
}
```

**修正:**
```csharp
// ✓ アプリ起動時に一度だけ登録
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureModels(this IServiceCollection services,
        IConfiguration configuration)
    {
        DapperTypeHandlerRegistration.Register();  // ✓ 起動時に一度だけ
        RepoDbTypeMapperRegistration.Register();
        // ...
    }
}
```

---

## 🔧 トラブルシューティング

### 問題1: "Cannot convert from 'DateTime' to 'LocalDateTime'"

**原因**: Mapper が DateTime を使用している

**修正**:
```csharp
// ✗ 誤り
public YourEntity ToDomainEntity(YourEntityDbModel dbModel, IClock clock)
{
    var createdAt = DateTime.Parse(dbModel.CreatedAt);  // ✗ DateTime に変換
    return new YourEntity(dbModel.RowId, createdAt, clock);
}

// ✓ 正解
public YourEntity ToDomainEntity(YourEntityDbModel dbModel, IClock clock)
{
    return new YourEntity(dbModel.RowId, dbModel.CreatedAt, clock);  // LocalDateTime
}
```

### 問題2: SQL から読み取った LocalDateTime が null になる

**原因**: DbModel のプロパティが nullable でない

**修正**:
```csharp
// ✗ 誤り
public class YourEntityDbModel
{
    public LocalDateTime UpdatedAt { get; set; }  // ✗ Nullable でない
}

// ✓ 正解
public class YourEntityDbModel
{
    public LocalDateTime? UpdatedAt { get; set; }  // ✓ Nullable
}
```

### 問題3: SQL Server に挿入時に型エラー

**原因**: ORM 初期化が呼ばれていない

**確認**:
```csharp
// DependencyInjection で確実に呼び出されているか
public static IServiceCollection AddInfrastructureModels(this IServiceCollection services,
    IConfiguration configuration)
{
    DapperTypeHandlerRegistration.Register();  // ← 必須
    RepoDbTypeMapperRegistration.Register();   // ← 必須
    // ...
}

// Program.cs で呼び出されているか
var services = new ServiceCollection();
services.AddInfrastructureModels(configuration);  // ← アプリ起動時
```

---

## 📁 ファイル構造

```
Infrastructure/
├── ORM/
│   ├── Dapper/
│   │   └── DapperTypeHandlerRegistration.cs
│   └── RepoDB/
│       └── RepoDbTypeMapperRegistration.cs
└── DependencyInjection.cs
```

---

## 📊 マッピング比較表

| ORM | 初期化方法 | マッピング方法 | 型 | 用途 |
|---|---|---|---|---|
| **Dapper** | SqlMapper.AddTypeMap | グローバル | LocalDateTime | SELECT/INSERT/UPDATE |
| **Dapper** | SqlMapper.AddTypeMap | グローバル | RowId | SELECT/INSERT/UPDATE |
| **RepoDb** | TypeMapper.Add | グローバル | LocalDateTime | Query/Insert/Update |
| **RepoDb** | TypeMapper.Add | グローバル | RowId | Query/Insert/Update |

---

## 🔄 RowId ValueObject のマッピング

### DbModel ↔ Domain Entity

**DbModel**: `long RowId` → **Mapper** → **Entity**: `RowId RowId`

- **DbModel 層**: 素の long を使用（ORM との親和性）
- **Mapper 層**: long ↔ RowId 変換を実施
  - ToDbModel: `entity.Id.Value` で RowId → long
  - ToDomainEntity: `RowId.From(dbModel.RowId)` で long → RowId
- **Entity 層**: RowId ValueObject を使用（型安全）

### マッピング設定

```csharp
// Dapper
SqlMapper.AddTypeMap(typeof(RowId), DbType.Int64);

// RepoDb
TypeMapper.Add<RowId>(DbType.Int64, true);
```

**注意**: RowId は DbModel では long のまま保持し、ORM レベルでの自動変換は行わない。
Mapper が責務を持つことで、型変換を明示的・一元管理する。

---

## 参考資料

- **Mapper_パターンガイド.md**: Mapper 実装例（RowId 変換含む）
- **DbModel_設計ルール.md**: DbModel は long を使用
- **Repository_パターンガイド.md**: RepositoryBase の汎用化
- **LocalDateTime_タイムゾーン_ガイド.md**: LocalDateTime と JST
- **TABLE_DESIGN_STANDARDS.md**: SQL Server スキーマ（bigint）
