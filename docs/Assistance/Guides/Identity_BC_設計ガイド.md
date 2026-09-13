# Identity BC 設計ガイド

**最終更新**: 2026-09-13  
**作成者**: Claude + User (tyokkoto@hotmail.com)

---

## 概要

Identity BC は **認証（Authentication）に特化した Bounded Context** です。

### 責務（Responsibility）

✓ **認証**
- Windows AD 自動認証
- ID/パスワード ローカル認証
- ログインユーザーの身分情報を提供

✗ **認可（権限付与）は含まない**
- ロール・パーミッション管理は Employee BC または別の Authorization BC で実施
- Identity BC は「誰か」を確認するのみ、「何ができるか」は外部に委ねる

### 理由

- **責務分離**: 認証と認可を分離することで、各々の変更が独立
- **テスト容易性**: 権限付け替えが簡単（認証ロジックに影響しない）
- **拡張性**: 複雑な権限ルールが増えても Identity BC は変わらない

---

## ドメインモデル

### Entity: UserAuthSession

ユーザーのログイン状態と身分情報を表現。

```csharp
namespace SupportAdvance.Contexts.Identity.Domain.Entities;

/// <summary>
/// ユーザー認証セッション（ログイン状態を表現）
/// 
/// 【責務】
/// - ユーザーの認証状態管理
/// - 「誰がログインしているか」を保持
/// - 「何ができるか」は保持しない（認可は外部に委ねる）
/// </summary>
public class UserAuthSession : Entity<IdentityRowId>
{
    /// <summary>
    /// Windows AD 認証か否か
    /// 
    /// true: AD認証 → IdentityRowId は Employee.RowId
    /// false: ローカル認証 → IdentityRowId は LoginCredentials.RowId
    /// </summary>
    public bool IsADAuthenticated { get; private set; }

    /// <summary>
    /// ログインユーザーの身分識別子
    /// 
    /// AD認証: Employee.RowId
    /// ローカル認証: LoginCredentials.RowId
    /// </summary>
    public long IdentityRowId { get; private set; }

    /// <summary>
    /// 権限の主体（Employee.RowId）
    /// 
    /// AD認証: Windows ログイン → Employee に紐付いた RowId
    /// ローカル認証: mapping_employee_row_id から取得
    /// 
    /// 【注】権限の詳細（ロール・パーミッション）は、
    /// Employee BC や Authorization BC で管理
    /// </summary>
    public EmployeeRowId AuthorityRowId { get; private set; }

    /// <summary>
    /// ログイン日時
    /// </summary>
    public LocalDateTime LoggedInAt { get; private set; }

    /// <summary>
    /// 前回ログイン日時（ローカル認証の場合のみ更新）
    /// </summary>
    public LocalDateTime? PreviousLoginAt { get; private set; }

    // ================== Factory Methods ==================

    /// <summary>
    /// Windows AD 認証による UserAuthSession を生成
    /// </summary>
    public static UserAuthSession CreateFromAD(
        EmployeeRowId employeeRowId,
        LocalDateTime loggedInAt)
    {
        return new UserAuthSession
        {
            IsADAuthenticated = true,
            IdentityRowId = employeeRowId.Value,  // Employee.RowId
            AuthorityRowId = employeeRowId,
            LoggedInAt = loggedInAt,
            PreviousLoginAt = null
        };
    }

    /// <summary>
    /// ID/パスワード認証による UserAuthSession を生成
    /// </summary>
    public static UserAuthSession CreateFromLocalAuth(
        long loginCredentialsRowId,
        EmployeeRowId mappingEmployeeRowId,
        LocalDateTime loggedInAt)
    {
        return new UserAuthSession
        {
            IsADAuthenticated = false,
            IdentityRowId = loginCredentialsRowId,  // LoginCredentials.RowId
            AuthorityRowId = mappingEmployeeRowId,  // mapping_employee_row_id から
            LoggedInAt = loggedInAt,
            PreviousLoginAt = null
        };
    }

    // ================== ビジネスロジック ==================

    /// <summary>
    /// ログイン前回日時を更新（ローカル認証でのみ使用）
    /// </summary>
    public void UpdatePreviousLoginAt(LocalDateTime newLoggedInAt)
    {
        if (IsADAuthenticated)
            throw new InvalidOperationException("AD認証の場合は前回ログイン日時を更新しません");

        PreviousLoginAt = LoggedInAt;
        LoggedInAt = newLoggedInAt;
    }
}
```

### ValueObjects

#### 1. IdentityRowId

```csharp
namespace SupportAdvance.Contexts.Identity.Domain.ValueObjects;

/// <summary>
/// 身分識別子（IdentityRowId）
/// 
/// AD認証: Employee.RowId の値
/// ローカル認証: LoginCredentials.RowId の値
/// </summary>
public record IdentityRowId(long Value)
{
    public static IdentityRowId From(long value)
    {
        if (value <= 0)
            throw new ArgumentException("IdentityRowId は正数である必要があります", nameof(value));
        
        return new IdentityRowId(value);
    }

    public static bool TryFrom(long value, out IdentityRowId result)
    {
        if (value <= 0)
        {
            result = null!;
            return false;
        }

        result = new IdentityRowId(value);
        return true;
    }
}
```

#### 2. AuthorityRowId

実装は EmployeeRowId と同一。別名として使用：

```csharp
namespace SupportAdvance.Contexts.Identity.Domain.ValueObjects;

// EmployeeRowId の別名・エイリアス
public record AuthorityRowId(long Value)
{
    /// <summary>
    /// EmployeeRowId から AuthorityRowId に変換
    /// </summary>
    public static AuthorityRowId FromEmployeeRowId(EmployeeRowId employeeRowId)
        => new(employeeRowId.Value);

    /// <summary>
    /// AuthorityRowId から EmployeeRowId に変換
    /// </summary>
    public EmployeeRowId ToEmployeeRowId()
        => new(Value);
}
```

---

## 認証フロー

### 認証フロー図

```
起動時: RealCurrentUserService.Authenticate()
│
├─ Step 1: Windows ログイン情報取得
│  ├─ Domain = AppSettings.ActiveDirectoryDomain（大文字正規化）
│  └─ UserId = Environment.UserName（大文字正規化）
│
├─ Step 2: AD 自動認証判定
│  ├─ Employee テーブルで Domain+UserId を検索（IQueryService<Employee, EmployeeRowId>）
│  │  ├─ マッチした場合
│  │  │  └─ ✓ CreateFromAD( Employee.RowId )
│  │  │
│  │  └─ マッチしない場合
│  │     └─ Step 3 へ
│
├─ Step 3: ID/パスワード入力画面を表示
│  ├─ ユーザー入力: LoginId, Password
│  └─ Step 4 へ
│
└─ Step 4: ローカル認証判定
   ├─ m_login_credentials でローカル認証
   │  ├─ 判定順序:
   │  │  ① login_id が一致（大文字小文字区別あり）か
   │  │  ② password_hash が一致するか
   │  │  ③ is_active = true か
   │  │  ④ mapping_employee_row_id が null でないか
   │  │  ⑤ mapping_employee_row_id が Employee に存在するか（IQueryService経由）
   │  │
   │  ├─ すべて OK
   │  │  └─ ✓ CreateFromLocalAuth( LoginCredentials.RowId, mapping_employee_row_id )
   │  │
   │  └─ NG な場合 → エラーメッセージを表示
   │     (詳細は次セクション)
```

### エラーメッセージ（ローカル認証の判定順）

ユーザーが「ID/パスワード」を入力した時の検証：

| # | 判定項目 | 条件 | エラーメッセージ | 例 |
|---|---|---|---|---|
| 1 | login_id | DB に同じ ID が存在しない | "ログインIDが見つかりません" | 入力: `user01` → DB に なし |
| 2 | password_hash | パスワードハッシュが不一致 | "パスワードが間違っています" | ハッシュ値が異なる |
| 3 | is_active | false（アカウント無効） | "このアカウントは無効です" | is_active = 0 |
| 4 | mapping_employee_row_id | null（マッピング未設定） | "権限が設定されていません" | mapping_employee_row_id is null |
| 5 | Employee 存在確認 | mapping_employee_row_id が Employee.RowId に存在しない | "権限設定が無効です" | mapping_employee_row_id = 999（存在しない） |

**実装パターン:**

```csharp
public class AuthenticateLocalUserUseCase : IAuthenticateLocalUserUseCase
{
    private readonly ILoginCredentialsRepository _loginCredentialsRepository;
    private readonly IQueryService<Employee, EmployeeRowId> _employeeQuery;  // ← DI注入
    private readonly IClock _clock;

    public async Task<Result<UserAuthSessionDto>> ExecuteAsync(string loginId, string password)
    {
        // ① login_id チェック
        var credential = await _loginCredentialsRepository.GetByLoginIdAsync(loginId);
        if (credential == null)
            return Result.Failure("ログインIDが見つかりません");

        // ② password_hash チェック
        if (!VerifyPasswordHash(password, credential.PasswordHash))
            return Result.Failure("パスワードが間違っています");

        // ③ is_active チェック
        if (!credential.IsActive)
            return Result.Failure("このアカウントは無効です");

        // ④ mapping_employee_row_id is null チェック
        if (credential.MappingEmployeeRowId == null)
            return Result.Failure("権限が設定されていません");

        // ⑤ Employee 存在確認（ジェネリック Query Service 経由）
        var employee = await _employeeQuery.GetByIdAsync(
            new EmployeeRowId(credential.MappingEmployeeRowId.Value));
        if (employee == null)
            return Result.Failure("権限設定が無効です");

        // 認証成功
        var session = UserAuthSession.CreateFromLocalAuth(
            credential.RowId,
            new EmployeeRowId(credential.MappingEmployeeRowId.Value),
            _clock.JstNow);

        return Result.Success(MapToDto(session));
    }

    private bool VerifyPasswordHash(string password, string hash)
    {
        // TODO: BCrypt など、適切なハッシュ検証ロジック
        throw new NotImplementedException();
    }

    private UserAuthSessionDto MapToDto(UserAuthSession session)
    {
        return new UserAuthSessionDto
        {
            IdentityRowId = session.IdentityRowId,
            IsADAuthenticated = session.IsADAuthenticated,
            AuthorityRowId = session.AuthorityRowId.Value,
            LoggedInAt = session.LoggedInAt
        };
    }
}
```

---

## Repository インターフェース

### IUserAuthSessionRepository

```csharp
namespace SupportAdvance.Contexts.Identity.Domain.Repositories;

/// <summary>
/// UserAuthSession の永続化インターフェース
/// </summary>
public interface IUserAuthSessionRepository
{
    /// <summary>
    /// UserAuthSession を保存（新規作成）
    /// </summary>
    Task SaveAsync(UserAuthSession session);

    /// <summary>
    /// 最新のログインセッションを取得（ユーザー確認用）
    /// </summary>
    Task<UserAuthSession?> GetLatestAsync(EmployeeRowId employeeRowId);

    /// <summary>
    /// ログイン履歴を取得
    /// </summary>
    Task<IEnumerable<UserAuthSession>> GetLoginHistoryAsync(
        EmployeeRowId employeeRowId,
        int limit = 10);
}

/// <summary>
/// LoginCredentials テーブルアクセス用インターフェース
/// 
/// 【注】Identity BC 内部で使用。外部には公開しない。
/// </summary>
public interface ILoginCredentialsRepository
{
    Task<LoginCredentialsDbModel?> GetByLoginIdAsync(string loginId);
}
```

---

## Application層: Use Cases

### IAuthenticateLocalUserUseCase（インターフェース）

```csharp
namespace SupportAdvance.Contexts.Identity.Application.UseCases;

/// <summary>
/// ローカル認証 Use Case インターフェース
/// </summary>
public interface IAuthenticateLocalUserUseCase : IUseCase
{
    Task<Result<UserAuthSessionDto>> ExecuteAsync(string loginId, string password);
}

public class UserAuthSessionDto
{
    public long IdentityRowId { get; set; }
    public bool IsADAuthenticated { get; set; }
    public long AuthorityRowId { get; set; }
    public LocalDateTime LoggedInAt { get; set; }
}
```

### IFindEmployeeByADUseCase（Windows AD 検索）

```csharp
namespace SupportAdvance.Contexts.Identity.Application.UseCases;

/// <summary>
/// Windows AD 情報から Employee を検索する Use Case
/// </summary>
public interface IFindEmployeeByADUseCase
{
    Task<EmployeeDto?> ExecuteAsync(string domain, string userId);
}

public class EmployeeDto
{
    public long RowId { get; set; }
    public string BisCode { get; set; } = null!;
}
```

**実装例:**

```csharp
public class FindEmployeeByADUseCase : IFindEmployeeByADUseCase
{
    private readonly IQueryService<Employee, EmployeeRowId> _employeeQuery;

    public async Task<EmployeeDto?> ExecuteAsync(string domain, string userId)
    {
        // Employee BC の Query Service 経由で検索
        // （Employee BC では AD情報を持つ Employee を検索可能と想定）
        
        var employee = await _employeeQuery.FindByADAsync(domain, userId);
        
        if (employee == null)
            return null;

        return new EmployeeDto
        {
            RowId = employee.RowId.Value,
            BisCode = employee.BisCode.Value
        };
    }
}
```

---

## 依存関係

### プロジェクト参照（.csproj）

**✓ Identity.Domain**

```xml
<ItemGroup>
    <ProjectReference Include="../../SharedKernel/SharedKernel.csproj" />
    <ProjectReference Include="../../Common/Common.csproj" />
</ItemGroup>
```

**✓ Identity.Application**

```xml
<ItemGroup>
    <ProjectReference Include="../Identity.Domain/Identity.Domain.csproj" />
    <ProjectReference Include="../../Application/Application.csproj" />
    <ProjectReference Include="../../Crosscutting/Crosscutting.csproj" />
</ItemGroup>
```

**✓ Identity.Infrastructure**

```xml
<ItemGroup>
    <ProjectReference Include="../Identity.Domain/Identity.Domain.csproj" />
    <ProjectReference Include="../Identity.Application/Identity.Application.csproj" />
    <ProjectReference Include="../../Infrastructure/Infrastructure.csproj" />
    <!-- ✓ 汎用層の Query Service インターフェース（DI 経由で取得） -->
    <!-- ✗ Employee.Application / Employee.Domain への直接参照は禁止 -->
</ItemGroup>
```

### 依存関係の正当性

| From | To | 許可 | 理由 |
|---|---|---|---|
| Identity.Domain → SharedKernel, Common | ✓ | 基盤型のため |
| Identity.Application → Identity.Domain, Application（汎用）, Crosscutting | ✓ | Use Case 実装のため |
| Identity.Infrastructure → Identity.Domain, Identity.Application（インターフェース）, Infrastructure | ✓ | Repository 実装のため |
| Identity.Infrastructure → Employee.Application / Employee.Domain | ✗ | BC 参照禁止。代わりに `IQueryService<Employee, EmployeeRowId>` を DI 経由で使用 |

### DI 設定（Program.cs）

```csharp
// 汎用層
services
    .AddApplicationModels()  // IQueryService<T, TId> インターフェース定義
    ;

// Employee BC
services
    .AddEmployeeApplicationModels()  // EmployeeQueryService 実装
    .AddEmployeeInfrastructureModels()
    ;

// Identity BC
services
    .AddIdentityApplicationModels()
    .AddIdentityInfrastructureModels()
    // DI: IQueryService<Employee, EmployeeRowId> → EmployeeQueryService
    .AddScoped(typeof(IQueryService<>), typeof(EmployeeQueryService<>))
    ;
```

---

## 起動時フロー

### RealCurrentUserService の実装方針

認証は **起動時に1回実行**。以下のような構成で実装：

```csharp
namespace SupportAdvance.Infrastructure.Services;

/// <summary>
/// 本番環境用 CurrentUserService
/// 
/// 【起動時フロー】
/// 1. Windows AD 認証を試行
/// 2. 失敗時は ID/パスワード入力画面を表示
/// 3. 認証成功後、ユーザー情報をメモリに保持
/// 4. 以後、メモリからユーザー情報を返す
/// </summary>
public class RealCurrentUserService : ICurrentUserService
{
    private UserAuthSessionDto? _currentSession;
    private readonly IFindEmployeeByADUseCase _findEmployeeByAD;
    private readonly IAuthenticateLocalUserUseCase _authenticateLocal;
    private readonly IConfiguration _configuration;

    public long EmployeeRowId 
    {
        get
        {
            if (_currentSession == null)
                throw new InvalidOperationException("ユーザーが認証されていません");
            
            return _currentSession.AuthorityRowId;
        }
    }

    public bool IsAuthenticated => _currentSession != null;

    public async Task AuthenticateAsync()
    {
        // Step 1: Windows AD 認証
        var adResult = await TryADAuthenticationAsync();
        if (adResult != null)
        {
            _currentSession = adResult;
            return;
        }

        // Step 2: ローカル認証（ID/パスワード入力画面）
        var localResult = await ShowLoginDialogAndAuthenticateAsync();
        if (localResult != null)
        {
            _currentSession = localResult;
            return;
        }

        // 認証失敗
        throw new AuthenticationFailedException("ユーザー認証に失敗しました");
    }

    private async Task<UserAuthSessionDto?> TryADAuthenticationAsync()
    {
        try
        {
            var domain = _configuration["ActiveDirectoryDomain"];
            var userId = Environment.UserName;

            var employee = await _findEmployeeByAD.ExecuteAsync(domain, userId);
            
            if (employee != null)
            {
                return new UserAuthSessionDto
                {
                    IdentityRowId = employee.RowId,
                    IsADAuthenticated = true,
                    AuthorityRowId = employee.RowId,
                    LoggedInAt = DateTime.Now
                };
            }
        }
        catch (Exception ex)
        {
            // ログ出力
            Console.WriteLine($"[WARN] AD 認証失敗: {ex.Message}");
        }

        return null;
    }

    private async Task<UserAuthSessionDto?> ShowLoginDialogAndAuthenticateAsync()
    {
        // TODO: WinForms で ID/パスワード入力ダイアログを表示
        // 入力後、_authenticateLocal.ExecuteAsync() を呼び出し
        
        throw new NotImplementedException();
    }
}
```

---

## 次のステップ

### フェーズ 1: Domain層の実装

- [ ] Identity.Domain プロジェクト作成
- [ ] UserAuthSession Entity 実装
- [ ] IdentityRowId, AuthorityRowId ValueObject 実装
- [ ] Repository インターフェース定義
- [ ] Unit Tests（Entity のファクトリメソッド、ビジネスロジック）

### フェーズ 2: Application層の実装

- [ ] Identity.Application プロジェクト作成
- [ ] IAuthenticateLocalUserUseCase 実装
- [ ] IFindEmployeeByADUseCase 実装
- [ ] DTOs 定義
- [ ] Use Case Unit Tests

### フェーズ 3: Infrastructure層の実装

- [ ] Identity.Infrastructure プロジェクト作成
- [ ] UserAuthSessionRepository 実装
- [ ] LoginCredentialsRepository 実装（m_login_credentials テーブル）
- [ ] DbModel マッピング（LocalDateTime ↔ DateTime）
- [ ] Integration Tests（実DB接続）

### フェーズ 4: Presentation層との統合

- [ ] RealCurrentUserService 実装
- [ ] ID/パスワード入力ダイアログ実装（WinForms）
- [ ] 起動時認証フロー実装
- [ ] SystemCurrentUserService → RealCurrentUserService 切り替え

### フェーズ 5: テスト・検証

- [ ] 全体統合テスト
- [ ] Windows AD 認証テスト
- [ ] ローカル認証テスト
- [ ] エラーメッセージの正確性確認

---

## 参考資料

- [CLAUDE.md - Context間のデータ共有パターン](../../CLAUDE.md#-context間のデータ共有パターン)
- [CLEAN_ARCHITECTURE_GUIDELINES.md](CLEAN_ARCHITECTURE_GUIDELINES.md)
- [Repository_パターンガイド.md](Repository_パターンガイド.md)
- [Entity_設計ガイドライン.md](Entity_設計ガイドライン.md)
