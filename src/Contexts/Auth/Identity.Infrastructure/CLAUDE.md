# Identity.Infrastructure レイヤー

認証・認可の実装詳細（DB、ORM、外部サービス）。

## DbModel 設計

### UserDbModel, RoleDbModel, UserRoleDbModel
- 監査カラム（row_id, row_version, created_at, created_by 等）8つ必須
- Domain エンティティのプロパティに対応
- **long でプリミティブ型保持**（RowId は Mapper で変換）
- **DateTime でプリミティブ型保持**（LocalDateTime は Mapper で変換）

### DbModel のコード例
```csharp
public class UserRoleDbModel
{
    // 監査フィールド: DateTime プリミティブ型
    public DateTime CreatedAt { get; set; }        // ← DateTime
    public DateTime? UpdatedAt { get; set; }       // ← DateTime?
    public DateTime? DeletedAt { get; set; }       // ← DateTime?
    
    // ビジネスフィールド: DateTime プリミティブ型
    public DateTime AssignedAt { get; set; }       // ← DateTime
}
```

## Mapper 実装

### 双方向変換パターン

#### ToDbModel: Entity → DbModel (LocalDateTime → DateTime)
```csharp
public UserRoleDbModel ToDbModel(UserRole entity)
{
    return new UserRoleDbModel
    {
        RowId = entity.Id.Value,                    // RowId → long
        UserId = entity.UserId.Value,               // RowId → long
        RoleId = entity.RoleId.Value,               // RowId → long
        AssignedAt = entity.AssignedAt.Value,       // LocalDateTime → DateTime
    };
}
```

#### ToDomainEntity: DbModel → Entity (DateTime → LocalDateTime)
```csharp
public UserRole ToDomainEntity(UserRoleDbModel dbModel, IClock clock)
{
    // DateTime → LocalDateTime 変換（型安全）
    LocalDateTime assignedAt = new LocalDateTime(dbModel.AssignedAt);

    return new UserRole(
        userId: RowId.From(dbModel.UserId),        // long → RowId
        roleId: RowId.From(dbModel.RoleId),        // long → RowId
        assignedAt: assignedAt,
        rowId: RowId.From(dbModel.RowId));         // long → RowId
}
```

## Repository 実装

- `RepositoryBase<User, UserDbModel, RowId>` 継承
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

## RowId グローバルマッピング

Dapper/RepoDb のグローバル型マッピングに RowId を登録済み（汎用 Infrastructure）：

```csharp
// DapperTypeHandlerRegistration.cs
SqlMapper.AddTypeMap(typeof(RowId), DbType.Int64);

// RepoDbTypeMapperRegistration.cs
TypeMapper.Add<RowId>(DbType.Int64, true);
```

これにより、DbModel の long ↔ RowId は Mapper で自動変換される。
