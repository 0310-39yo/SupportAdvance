# Employee.Infrastructure レイヤー

従業員管理の実装詳細（DB、ORM、外部サービス）。

## DbModel 設計

### EmployeeDbModel, DepartmentDbModel
- 監査カラム（row_id, row_version, created_at, created_by 等）8つ必須
- Domain エンティティのプロパティに対応
- long でプリミティブ型保持（RowId は Mapper で変換）
- DateTime でプリミティブ型保持（LocalDateTime は Mapper で変換）

## Mapper 実装

### RowId マッピング
```csharp
// ToDbModel: Entity → DbModel
public EmployeeDbModel ToDbModel(Employee entity)
{
    return new EmployeeDbModel
    {
        RowId = entity.Id.Value,  // RowId.Value で long に
        DepartmentId = entity.DepartmentId.Value,  // RowId.Value
        EmployeeNumber = entity.EmployeeNumber,
    };
}

// ToDomainEntity: DbModel → Entity
public Employee ToDomainEntity(EmployeeDbModel dbModel, IClock clock)
{
    var hireDate = dbModel.HireDate.HasValue
        ? LocalDateTime.From(dbModel.HireDate.Value)  // DateTime → LocalDateTime
        : null;

    return new Employee(
        employeeNumber: dbModel.EmployeeNumber,
        ...
        hireDate: hireDate,
        rowId: RowId.From(dbModel.RowId));  // long → RowId
}
```

### LocalDateTime マッピング
- Domain の LocalDateTime（JST）→ DbModel の DateTime（UTC）
- DbModel の DateTime（UTC）→ Domain の LocalDateTime（JST）
- グローバル ORM マッピング（Dapper/RepoDb）で自動変換

## Repository 実装

- `RepositoryBase<Employee, EmployeeDbModel, RowId>` 継承
- Mapper、ICurrentUserService、IClock を DI で受け取る
- 監査情報（createdBy, updatedBy）は RepositoryBase が自動設定

### 実装スタブ
現在は NotImplementedException で DB アクセスは未実装。
Dapper/RepoDb を使用した本格実装は後続フェーズ。

## 依存関係

### 許可される参照
- Employee.Application（Repository インターフェース）
- Employee.Domain（ビジネスロジック）
- Infrastructure（RepositoryBase 等）
- SharedKernel（ValueObject）
- Common（ユーティリティ）

### 禁止される参照
- Employee.Application の Use Case 実装
- Presentation

## RowId・LocalDateTime グローバルマッピング

Dapper/RepoDb のグローバル型マッピングに登録済み（汎用 Infrastructure）：

```csharp
// DapperTypeHandlerRegistration.cs
SqlMapper.AddTypeMap(typeof(RowId), DbType.Int64);

// RepoDbTypeMapperRegistration.cs
TypeMapper.Add<RowId>(DbType.Int64, true);
TypeMapper.Add<LocalDateTime>(DbType.DateTime2, true);
TypeMapper.Add<LocalDateTime?>(DbType.DateTime2, true);
```

これにより、DbModel の long/DateTime ↔ RowId/LocalDateTime は Mapper で自動変換される。
