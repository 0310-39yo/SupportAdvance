# Employee.Infrastructure レイヤー

従業員管理の実装詳細（DB、ORM、外部サービス）。

## DbModel 設計

### EmployeeDbModel, DepartmentDbModel
- 監査カラム（row_id, row_version, created_at, created_by 等）8つ必須
- Domain エンティティのプロパティに対応
- **集約ID カラム（Guid）を追加**（EmployeeId, DepartmentId）
- **long でプリミティブ型保持**（RowId は Mapper で変換）
- **LocalDateTime で保持**（ORM グローバルマッピングで DateTime2 に変換）

### DbModel のコード例
```csharp
public class EmployeeDbModel
{
    // 監査フィールド: LocalDateTime
    public LocalDateTime CreatedAt { get; set; }       // ← LocalDateTime
    public LocalDateTime? UpdatedAt { get; set; }      // ← LocalDateTime?
    public LocalDateTime? DeletedAt { get; set; }      // ← LocalDateTime?
    
    // 集約ID: GUID（ビジネスID）
    public Guid EmployeeId { get; set; }               // ← 集約ID
    
    // ビジネスフィールド: LocalDateTime
    public LocalDateTime? HireDate { get; set; }       // ← LocalDateTime?
}
```

## Mapper 実装

### 双方向変換パターン（Entity<TId>）

#### ToDbModel: Entity → DbModel
```csharp
public EmployeeDbModel ToDbModel(Employee entity)
{
    return new EmployeeDbModel
    {
        RowId = entity.RowId?.Value ?? 0,              // RowId → long
        EmployeeId = entity.Id.Value,                  // AggregateId → Guid
        CreatedAt = entity.CreatedAt,                  // LocalDateTime
        HireDate = entity.HireDate,                    // LocalDateTime
        EmployeeNumber = entity.EmployeeNumber,
    };
}
```

#### ToDomainEntity: DbModel → Entity
```csharp
public Employee ToDomainEntity(EmployeeDbModel dbModel, IClock clock)
{
    var employeeId = EmployeeId.From(dbModel.EmployeeId);  // Guid → EmployeeId
    var rowId = RowId.From(dbModel.RowId);                 // long → RowId

    return new Employee(
        id: employeeId,                                     // 集約ID
        employeeNumber: dbModel.EmployeeNumber,
        hireDate: dbModel.HireDate,                         // LocalDateTime
        rowId: rowId,                                        // テーブル物理キー
        clock: clock);
}
```

## Repository 実装

- `RepositoryBase<Employee, EmployeeDbModel, EmployeeId>` 継承（Entity<EmployeeId>）
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

## ORM マッピング

Dapper/RepoDb のグローバル型マッピングに登録済み（汎用 Infrastructure）：

```csharp
// DapperTypeHandlerRegistration.cs
SqlMapper.AddTypeMap(typeof(RowId), DbType.Int64);
SqlMapper.AddTypeMap(typeof(LocalDateTime), DbType.DateTime2);
SqlMapper.AddTypeMap(typeof(LocalDateTime?), DbType.DateTime2);

// RepoDbTypeMapperRegistration.cs
TypeMapper.Add<RowId>(DbType.Int64, true);
TypeMapper.Add<LocalDateTime>(DbType.DateTime2, true);
TypeMapper.Add<LocalDateTime?>(DbType.DateTime2, true);
```

これにより、DbModel の long ↔ RowId、LocalDateTime ↔ DateTime2 は自動変換される。
