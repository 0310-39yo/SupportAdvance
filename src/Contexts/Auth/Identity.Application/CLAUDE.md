# Identity.Application レイヤー

認証・認可の Use Case。

## Repository インターフェース

### IUserRepository
- GetByIdAsync, GetByLoginIdAsync, GetByEmailAsync
- CreateAsync, UpdateAsync, DeleteAsync

### IRoleRepository
- GetByIdAsync, GetByNameAsync, GetAllAsync
- CreateAsync, UpdateAsync, DeleteAsync

### IUserRoleRepository
- GetByUserIdAsync, GetByRoleIdAsync
- CreateAsync, DeleteAsync, DeleteByUserAndRoleAsync

## Use Case

### CreateUserUseCase
- ログインID と Email の重複チェック
- User エンティティを生成してリポジトリに保存
- RowId を返す

### CreateRoleUseCase
- Role 名の重複チェック
- Role エンティティを生成してリポジトリに保存
- RowId を返す

### AssignRoleUseCase
- User と Role の関連付け
- LocalDateTime（JST）を使用して AssignedAt を設定
- 二重割り当てをチェック

## 依存関係

### 許可される参照
- Identity.Domain（ビジネスロジック）
- SharedKernel（基盤型）
- Common（ユーティリティ）
- Application（汎用層のインターフェース）

### 禁止される参照
- Infrastructure（実装のみ。DI で注入）
- Presentation

## 実装パターン

```csharp
public class CreateUserUseCase
{
    private readonly IUserRepository _userRepository;

    public CreateUserUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<RowId> ExecuteAsync(string loginId, string email, string hashedPassword)
    {
        var user = new User(loginId, email, hashedPassword);
        await _userRepository.CreateAsync(user);
        return user.Id; // RowId を返す
    }
}
```

## RowId マッピング

Repository は Mapper 経由で RowId ↔ long を変換：
- Entity の RowId → DbModel の long（ToDbModel）
- DbModel の long → Entity の RowId（ToDomainEntity）

詳細は Identity.Infrastructure の Mapper 参照。
