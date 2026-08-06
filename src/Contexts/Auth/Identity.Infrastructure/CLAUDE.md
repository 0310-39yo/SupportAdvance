# Identity.Infrastructure レイヤー

認証・認可の実装詳細（DB、ORM、外部サービス）。

## DbModel 設計

### UserDbModel, RoleDbModel, UserRoleDbModel
- 監査カラム（row_id, row_version, created_at, created_by 等）8つ必須
- Domain エンティティのプロパティに対応
- **集約ID カラム（Guid）を追加**（UserId, RoleId, UserRoleId）
- **long でプリミティブ型保持**（RowId は Mapper で変換）
- **LocalDateTime で保持**（ORM グローバルマッピングで DateTime2 に変換）

### DbModel のコード例
```csharp
public class UserRoleDbModel
{
    // 監査フィールド: LocalDateTime
    public LocalDateTime CreatedAt { get; set; }       // ← LocalDateTime
    public LocalDateTime? UpdatedAt { get; set; }      // ← LocalDateTime?
    public LocalDateTime? DeletedAt { get; set; }      // ← LocalDateTime?
    
    // 集約ID: GUID（ビジネスID）
    public Guid UserRoleId { get; set; }               // ← 集約ID
    
    // ビジネスフィールド: LocalDateTime
    public LocalDateTime AssignedAt { get; set; }      // ← LocalDateTime
}
```

## Mapper 実装

### 双方向変換パターン（Entity<TId>）

#### ToDbModel: Entity → DbModel
```csharp
public UserRoleDbModel ToDbModel(UserRole entity)
{
    return new UserRoleDbModel
    {
        RowId = entity.RowId?.Value ?? 0,              // RowId → long
        UserRoleId = entity.Id.Value,                  // AggregateId → Guid
        UserId = entity.UserId.Value,                  // RowId → long
        RoleId = entity.RoleId.Value,                  // RowId → long
        AssignedAt = entity.AssignedAt,                // LocalDateTime → LocalDateTime
    };
}
```

#### ToDomainEntity: DbModel → Entity
```csharp
public UserRole ToDomainEntity(UserRoleDbModel dbModel, IClock clock)
{
    var userRoleId = UserRoleId.From(dbModel.UserRoleId);  // Guid → UserRoleId
    var rowId = RowId.From(dbModel.RowId);                 // long → RowId

    return new UserRole(
        id: userRoleId,                                     // 集約ID
        userId: RowId.From(dbModel.UserId),               // long → RowId
        roleId: RowId.From(dbModel.RoleId),               // long → RowId
        assignedAt: dbModel.AssignedAt,                    // LocalDateTime
        rowId: rowId,                                       // テーブル物理キー
        clock: clock);
}
```

## Repository 実装

- `RepositoryBase<User, UserDbModel, UserId>` 継承（Entity<UserId>）
- Mapper、ICurrentUserService、IClock を DI で受け取る
- 監査情報（createdBy, updatedBy）は RepositoryBase が自動設定

### 実装スタブ
現在は NotImplementedException で DB アクセスは未実装。
Dapper/RepoDb を使用した本格実装は後続フェーズ。

## 依存関係

### 許可される参照
- Identity.Application（Repository インターフェース）
- Identity.Domain（ビジネスロジック）
- Infrastructure（RepositoryBase 等）
- SharedKernel（ValueObject）
- Common（ユーティリティ）

### 禁止される参照
- Identity.Application の Use Case 実装
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
