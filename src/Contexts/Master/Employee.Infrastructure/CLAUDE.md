# Employee.Infrastructure レイヤー

従業員管理の実装詳細（DB、ORM、外部サービス）。

## DbModel 設計

### EmployeeDbModel, DepartmentDbModel
- 監査カラム（row_id, row_version, created_at, created_by 等）8つ必須
- Domain エンティティのプロパティに対応
- **long でプリミティブ型保持**（RowId は Mapper で変換）
- **DateTime でプリミティブ型保持**（LocalDateTime は Mapper で変換）
- 理由：ORM マッピングの明確性と責務分離

### DbModel のコード例
```csharp
public class EmployeeDbModel
{
    // 監査フィールド: DateTime プリミティブ型
    public DateTime CreatedAt { get; set; }        // ← DateTime
    public DateTime? UpdatedAt { get; set; }       // ← DateTime?
    public DateTime? DeletedAt { get; set; }       // ← DateTime?
    
    // ビジネスフィールド: DateTime プリミティブ型
    public DateTime? HireDate { get; set; }        // ← DateTime?
}
```

## Mapper 実装

### 双方向変換パターン

#### ToDbModel: Entity → DbModel (LocalDateTime → DateTime)
```csharp
public EmployeeDbModel ToDbModel(Employee entity)
{
    return new EmployeeDbModel
    {
        RowId = entity.Id.Value,                    // RowId → long
        CreatedAt = entity.CreatedAt.Value,         // LocalDateTime → DateTime
        HireDate = entity.HireDate.HasValue 
            ? entity.HireDate.Value.Value           // LocalDateTime → DateTime
            : (DateTime?)null,
        EmployeeNumber = entity.EmployeeNumber,
    };
}
```

#### ToDomainEntity: DbModel → Entity (DateTime → LocalDateTime)
```csharp
public Employee ToDomainEntity(EmployeeDbModel dbModel, IClock clock)
{
    // DateTime → LocalDateTime 変換（型安全）
    var hireDate = dbModel.HireDate.HasValue
        ? new LocalDateTime(dbModel.HireDate.Value)  // DateTime → LocalDateTime
        : null;

    return new Employee(
        employeeNumber: dbModel.EmployeeNumber,
        hireDate: hireDate,
        rowId: RowId.From(dbModel.RowId));          // long → RowId
}
```

### LocalDateTime マッピング責務
- **Mapper の責務**: Entity（LocalDateTime） ↔ DbModel（DateTime）の明示的な変換
- **グローバル ORM マッピング**: Dapper/RepoDb でプリミティブ型を自動マッピング
- **型安全性**: 新しい LocalDateTime(...) で明示的な型変換を実施

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
