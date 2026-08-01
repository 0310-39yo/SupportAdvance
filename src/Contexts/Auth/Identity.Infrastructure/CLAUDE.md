# Identity.Infrastructure レイヤー

認証・認可の実装詳細（DB、ORM、外部サービス）。

## DbModel 設計

### UserDbModel, RoleDbModel, UserRoleDbModel
- 監査カラム（row_id, row_version, created_at, created_by 等）8つ必須
- Domain エンティティのプロパティに対応
- long でプリミティブ型保持（RowId は Mapper で変換）

## Mapper 実装

### RowId マッピング
```csharp
// ToDbModel: Entity → DbModel
public UserDbModel ToDbModel(User entity)
{
    return new UserDbModel
    {
        RowId = entity.Id.Value,  // RowId.Value で long に
        LoginId = entity.LoginId,
    };
}

// ToDomainEntity: DbModel → Entity
public User ToDomainEntity(UserDbModel dbModel, IClock clock)
{
    return new User(
        loginId: dbModel.LoginId,
        email: dbModel.Email,
        hashedPassword: dbModel.HashedPassword,
        rowId: RowId.From(dbModel.RowId));  // long から RowId に
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
