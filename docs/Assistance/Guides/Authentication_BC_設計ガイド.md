# Authentication BC 設計ガイド

**最終更新**: 2026-09-16（実装状況セクションを追加、実装と乖離していたコード例を修正）
**作成者**: Claude + User (tyokkoto@hotmail.com)

---

## 概要

Authentication BC は **認証（Authentication）に特化した Bounded Context** です。

### 責務（Responsibility）

✓ **認証**
- Windows AD 自動認証（未実装、下記「実装状況」参照）
- ID/パスワード ローカル認証
- ログインセッション管理
- ログイン履歴記録

✗ **認可（権限付与）は含まない**
- ロール・パーミッション管理は Employee BC または別の Authorization BC で実施
- Authentication BC は「誰か」を確認するのみ、「何ができるか」は外部に委ねる

### 理由

- **責務分離**: 認証と認可を分離することで、各々の変更が独立
- **テスト容易性**: 権限付け替えが簡単（認証ロジックに影響しない）
- **拡張性**: 複雑な権限ルールが増えても Authentication BC は変わらない

---

## テーブル構成

### 1. m_login_credentials（マスター）

ローカル認証用のログイン認証情報マスター。社外アクセス対象者を事前登録。

```sql
[row_id]                  [bigint] PK
[row_version]             [timestamp]
[created_at/created_by]   監査カラム
[updated_at/updated_by]   監査カラム
[deleted_at/deleted_by]   監査カラム
[mapping_employee_row_id] [bigint] NOT NULL FK → m_employees
[login_id]                [nvarchar](50) NOT NULL
[password_hash]           [nvarchar](255) NOT NULL
[is_active]               [bit] NOT NULL
[last_login_at]           [datetime2](7) NULL
```

**役割**: ローカル認証の認証情報を保持。認証時に参照される。

### 2. t_user_auth_sessions（トランザクション）

**ログインセッション履歴**。UserAuthSession 集約の主体テーブル。

```sql
[row_id]                  [bigint] PK DEFAULT (Sequence)
[row_version]             [timestamp]
[created_at/created_by]   監査カラム
[updated_at/updated_by]   監査カラム
[deleted_at/deleted_by]   監査カラム
[current_user_row_id]     [bigint] NOT NULL FK → m_employees
[is_ad_authenticated]     [bit] NOT NULL
[login_success]           [bit] NOT NULL
[logged_in_at]            [datetime2](7) NOT NULL
[logged_out_at]           [datetime2](7) NULL
[login_credentials_row_id] [bigint] NULL
```

**カラム説明**:
- `current_user_row_id` — 権限の主体（= Employee.RowId）。本番環境ではこの Employee の権限が適用される
- `is_ad_authenticated` — 認証方式（0=ローカル認証、1=AD認証）
- `login_success` — 認証成功/失敗（1=成功、0=失敗）
- `logged_in_at` — ログイン操作日時（失敗時も記録）
- `logged_out_at` — ログアウト日時（アプリ終了時に記録）。NULL = 非正常終了
- `login_credentials_row_id` — ローカル認証時のみ値あり。m_login_credentials.row_id

---

## ドメインモデル

### Entity: UserAuthSession

ユーザーのログイン試行結果（成功・失敗とも）を表現する `AggregateRoot`。実装は
[UserAuthSession.cs](../../../src/Contexts/Authentication/Authentication.Domain/Entities/UserAuthSession.cs) を参照。要点:

```csharp
namespace SupportAdvance.Contexts.Authentication.Domain.Entities;

public sealed class UserAuthSession : AggregateRoot<UserAuthSessionRowId>
{
    public AuthorityRowId AuthorityRowId { get; private set; }          // 権限主体（m_employees.row_id）
    public bool IsAdAuthenticated { get; private set; }                 // true=AD認証、false=ローカル認証
    public bool LoginSuccess { get; private set; }                      // 認証成功/失敗（失敗も記録）
    public LocalDateTime LoggedInAt { get; private set; }
    public LocalDateTime? LoggedOutAt { get; private set; }             // アプリ正常終了時のみ設定
    public LoginCredentialsRowId? LoginCredentialsRowId { get; private set; } // ローカル認証時のみ値あり
    public byte[] RowVersion { get; set; } = [];

    public static UserAuthSession Create(
        UserAuthSessionRowId id, AuthorityRowId authorityRowId,
        bool isAdAuthenticated, bool loginSuccess, LocalDateTime loggedInAt,
        LoginCredentialsRowId? loginCredentialsRowId = null);

    // DB から復元（Repository が使用）
    public static UserAuthSession Reconstruct(
        UserAuthSessionRowId id, AuthorityRowId authorityRowId,
        bool isAdAuthenticated, bool loginSuccess, LocalDateTime loggedInAt,
        LocalDateTime? loggedOutAt, LoginCredentialsRowId? loginCredentialsRowId);

    public void SetLoggedOutAt(LocalDateTime loggedOutAt);
    public bool IsActive() => LoginSuccess && LoggedOutAt == null;
}
```

**注**: 監査フィールド（CreatedAt/CreatedBy 等）は Domain Entity には保持せず、Repository が保存・更新時に設定する（[LocalDateTime 使用規則](../../../CLAUDE.md#-localdatetime-使用規則)参照）。

### ValueObjects

Domain 層の実際の ValueObject は以下の5つ（[ValueObjects/](../../../src/Contexts/Authentication/Authentication.Domain/ValueObjects/) 配下）。

| ValueObject | 用途 |
|---|---|
| `UserAuthSessionRowId` | UserAuthSession の主キー（`t_user_auth_sessions.row_id`） |
| `AuthorityRowId` | 権限主体（`m_employees.row_id`）。EmployeeRowId と論理的に同一の値だが、BC境界を明確化するためこのBCでローカルに定義した独立した型（EmployeeRowId 型そのものではない） |
| `LoginCredentialsRowId` | ローカル認証マスターの行ID（`m_login_credentials.row_id`） |
| `LoginId` | ログインID（現状 `AuthenticateLocalUserRequest`/`LoginCredentialsDbModel` では素の `string` として扱われており、この ValueObject は未結線） |
| `AuthMethod` | 認証方式を表す enum ラッパー（`LocalAuth`/`WindowsAD`）。現状 `UserAuthSession.IsAdAuthenticated`（bool）が実際に使われており、この ValueObject は未結線 |

いずれも `From`/`TryFrom`（`AuthorityRowId` は `TryFromDbValue` も）を持つ標準パターン。`LoginId`/`AuthMethod` は将来の置き換え候補として定義済みだが、現在のコードパスでは未使用（要検証: 実装が追いついていない可能性）。

---

## 認証フロー

### 認証フロー図

```
起動時: WinTrial Program.cs → LoginDialog.ShowDialog()
│
├─ Step 1: Windows ログイン情報取得
│  ├─ Domain = AppSettings.ActiveDirectoryDomain（大文字正規化）
│  └─ UserId = Environment.UserName（大文字正規化）
│
├─ Step 2: AD 自動認証判定（優先・設計のみ、未実装）
│  ├─ 【未実装】FindEmployeeByADUseCase は現状 空のスタブクラス
│  │  （Presentation層からも呼び出されていない）
│  │  実装後は Employee テーブルで Domain+UserId を検索する想定
│  │     ├─ マッチした場合
│  │     │  └─ ✓ t_user_auth_sessions に INSERT
│  │     │     is_ad_authenticated=1, current_user_row_id=Employee.RowId
│  │     │     login_success=1, login_credentials_row_id=NULL
│  │     └─ マッチしない場合 → Step 3 へ（フォールバック）
│  └─ 現状は常に Step 3（ローカル認証）から開始する
│
├─ Step 3: ID/パスワード入力画面を表示
│  ├─ ユーザー入力: LoginId, Password
│  └─ Step 4 へ
│
└─ Step 4: ローカル認証（LoginDialog → AuthenticateLocalUserUseCase）
   ├─ m_login_credentials でローカル認証（判定順序）:
   │  │  ① login_id が一致するレコードが存在するか
   │  │  ② is_active = true か
   │  │  ③ password_hash が一致するか（IPasswordHashService、PBKDF2+Salt）
   │  │
   │  ├─ すべて OK
   │  │  └─ ✓ t_user_auth_sessions に INSERT（成功記録）
   │  │     is_ad_authenticated=0, current_user_row_id=mapping_employee_row_id
   │  │     login_success=1, login_credentials_row_id=LoginCredentials.RowId
   │  │
   │  └─ NG な場合
   │     └─ t_user_auth_sessions に INSERT（失敗記録、current_user_row_id=SYSTEM_USER_ID）
   │        続けて InvalidOperationException をスロー（エラーメッセージは次セクション）
```

### エラーメッセージ（ローカル認証の判定順）

実装は [AuthenticateLocalUserUseCase.cs](../../../src/Contexts/Authentication/Authentication.Application/UseCases/AuthenticateLocalUserUseCase.cs) を参照。判定順序と失敗時のメッセージ（例外の `Message`）:

| # | 判定項目 | 条件 | エラーメッセージ |
|---|---|---|---|
| 1 | login_id | `ILoginCredentialsQuery.GetByLoginIdAsync` が null を返す（該当なし） | "ログインIDが見つかりません" |
| 2 | is_active | false（アカウント無効） | "このアカウントは無効です" |
| 3 | password_hash | `IPasswordHashService.VerifyPassword` が false | "パスワードが間違っています" |

いずれの失敗時も、失敗ログ（`login_success=false`, `current_user_row_id=SYSTEM_USER_ID`）をまず保存してから例外をスローする。`mapping_employee_row_id` は DB上 NOT NULL のため null チェックや Employee 存在確認は行っていない（設計初期段階で検討されていたが実装では省略されている）。

**実装パターン（要点抜粋）:**

```csharp
public sealed class AuthenticateLocalUserUseCase
{
    private readonly ILoginCredentialsQuery _loginCredentialsQuery;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IUserAuthSessionRepository _sessionRepository;
    private readonly IClock _clock;
    private readonly ISequenceProvider _sequenceProvider;
    private const long SystemUserId = 2147483667; // 失敗ログの current_user_row_id

    public async Task<AuthenticateLocalUserResponse> ExecuteAsync(AuthenticateLocalUserRequest request)
    {
        // Step 1: ログインID で認証情報を取得（null → 失敗ログ保存 + 例外）
        var credentials = await _loginCredentialsQuery.GetByLoginIdAsync(request.LoginId);

        // Step 2: is_active 確認（false → 失敗ログ保存 + 例外）
        // Step 3: パスワード検証（不一致 → 失敗ログ保存 + 例外）
        // Step 4: 成功時の UserAuthSession を生成して保存
        // Step 5: AuthenticateLocalUserResponse を返す
    }
}
```

---

## Repository インターフェース

### IUserAuthSessionRepository

Domain 層が定義する永続化インターフェース。実装は
[IUserAuthSessionRepository.cs](../../../src/Contexts/Authentication/Authentication.Domain/Repositories/IUserAuthSessionRepository.cs) を参照。

```csharp
namespace SupportAdvance.Contexts.Authentication.Domain.Repositories;

public interface IUserAuthSessionRepository
{
    Task<UserAuthSession?> GetByIdAsync(UserAuthSessionRowId id);
    Task<UserAuthSession?> GetLatestByAuthorityRowIdAsync(AuthorityRowId authorityRowId);
    Task<UserAuthSession?> GetLatestByLoginCredentialsRowIdAsync(LoginCredentialsRowId loginCredentialsRowId);
    Task<UserAuthSessionRowId> SaveAsync(UserAuthSession session);   // 新規作成
    Task UpdateAsync(UserAuthSession session);                       // 楽観ロック付き更新（主に LoggedOutAt 用）
    Task DeleteAsync(UserAuthSessionRowId id);                       // 論理削除
}
```

**注**: `m_login_credentials` へのアクセスは Domain 層の Repository ではなく、Application 層の `ILoginCredentialsQuery`（[ILoginCredentialsQuery.cs](../../../src/Contexts/Authentication/Authentication.Application/Queries/ILoginCredentialsQuery.cs)）として定義され、Infrastructure 層の `LoginCredentialsQueryService` が実装する（SELECT専用の Query Service パターン。ILoginCredentialsRepository という Domain Repository は存在しない）。

---

## Application層: Use Cases

### AuthenticateLocalUserUseCase

Interface を挟まず、クラスを直接 DI 登録・注入する構成（`IUseCase` 実装ではない）。実装は
[AuthenticateLocalUserUseCase.cs](../../../src/Contexts/Authentication/Authentication.Application/UseCases/AuthenticateLocalUserUseCase.cs)、DTOは
[AuthenticateLocalUserRequest.cs](../../../src/Contexts/Authentication/Authentication.Application/Dtos/AuthenticateLocalUserRequest.cs) /
[AuthenticateLocalUserResponse.cs](../../../src/Contexts/Authentication/Authentication.Application/Dtos/AuthenticateLocalUserResponse.cs) を参照。

```csharp
public sealed class AuthenticateLocalUserUseCase
{
    public AuthenticateLocalUserUseCase(
        ILoginCredentialsQuery loginCredentialsQuery,
        IPasswordHashService passwordHashService,
        IUserAuthSessionRepository sessionRepository,
        IClock clock,
        ISequenceProvider sequenceProvider);

    public async Task<AuthenticateLocalUserResponse> ExecuteAsync(AuthenticateLocalUserRequest request);
    // 失敗時は AuthenticateLocalUserResponse を返さず InvalidOperationException をスロー
}

public sealed class AuthenticateLocalUserResponse
{
    public long UserAuthSessionRowId { get; init; }
    public long EmployeeRowId { get; init; }
    public string LoginId { get; init; } = string.Empty;
    public DateTime LoggedInAt { get; init; }
}
```

### FindEmployeeByADUseCase（未実装スタブ）

Windows AD 認証用に予約されたクラスだが、現状は空実装。実装は
[FindEmployeeByADUseCase.cs](../../../src/Contexts/Authentication/Authentication.Application/UseCases/FindEmployeeByADUseCase.cs) を参照。

```csharp
public sealed class FindEmployeeByADUseCase
{
    // 実装予定: Presentation層で AD 認証情報を取得し、
    // AuthorityRowId を受け取ってセッションを生成するロジック
    // 今後の実装で詳細化
}
```

### LogoutUseCase

実装は [LogoutUseCase.cs](../../../src/Contexts/Authentication/Authentication.Application/UseCases/LogoutUseCase.cs) を参照。`IUseCase` インターフェースは実装しておらず、クラスを直接 DI 登録する構成。

```csharp
public sealed class LogoutUseCase
{
    public LogoutUseCase(IUserAuthSessionRepository sessionRepository, IClock clock);

    /// <param name="sessionRowId">ログアウト対象のセッション RowId</param>
    /// <exception cref="InvalidOperationException">セッションが見つからない</exception>
    public async Task ExecuteAsync(UserAuthSessionRowId sessionRowId)
    {
        // GetByIdAsync → SetLoggedOutAt(現在時刻) → UpdateAsync
    }
}
```

**注（未実装部分）**: `LogoutUseCase` は実装済みだが、2026-09-16 時点で Presentation 層（`WinTrial`）から呼び出すコードはまだ存在しない。アプリ終了時の呼び出し配線は未実装。

---

## 依存関係

### プロジェクト参照（.csproj）

**✓ Authentication.Domain**

```xml
<ItemGroup>
    <ProjectReference Include="../../SharedKernel/SharedKernel.csproj" />
    <ProjectReference Include="../../Common/Common.csproj" />
</ItemGroup>
```

**✓ Authentication.Application**

```xml
<ItemGroup>
    <ProjectReference Include="../Authentication.Domain/Authentication.Domain.csproj" />
    <ProjectReference Include="../../Application/Application.csproj" />
    <ProjectReference Include="../../Crosscutting/Crosscutting.csproj" />
</ItemGroup>
```

**✓ Authentication.Infrastructure**

```xml
<ItemGroup>
    <ProjectReference Include="../Authentication.Domain/Authentication.Domain.csproj" />
    <ProjectReference Include="../Authentication.Application/Authentication.Application.csproj" />
    <ProjectReference Include="../../Infrastructure/Infrastructure.csproj" />
    <!-- ✓ 汎用層の Query Service インターフェース（DI 経由で取得） -->
    <!-- ✗ Employee.Application / Employee.Domain への直接参照は禁止 -->
</ItemGroup>
```

### 依存関係の正当性

| From | To | 許可 | 理由 |
|---|---|---|---|
| Authentication.Domain → SharedKernel, Common | ✓ | 基盤型のため |
| Authentication.Application → Authentication.Domain, Application（汎用）, Crosscutting | ✓ | Use Case 実装のため |
| Authentication.Infrastructure → Authentication.Domain, Authentication.Application（インターフェース）, Infrastructure | ✓ | Repository 実装のため |
| Authentication.Infrastructure → Employee.Application / Employee.Domain | ✗ | BC 参照禁止。代わりに `IQueryService<Employee, EmployeeRowId>` を DI 経由で使用 |

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

// Authentication BC
services
    .AddAuthenticationApplicationModels()
    .AddAuthenticationInfrastructureModels()
    // DI: IQueryService<Employee, EmployeeRowId> → EmployeeQueryService
    .AddScoped(typeof(IQueryService<>), typeof(EmployeeQueryService<>))
    ;
```

---

## 起動時フロー（WinForms: WinTrial）

### 実際の起動シーケンス

実装は [Program.cs](../../../src/Presentation/WinTrial/Program.cs) を参照。

```
Main()
├─ DI コンテナ構築（AddAuthenticationApplicationModels / AddAuthenticationInfrastructureModels 等）
├─ host.Start()
├─ LoginDialog を DI から取得して ShowDialog()（モーダル）
│  └─ LoginDialogViewModel.Login() が AuthenticateLocalUserUseCase.ExecuteAsync() を実行
│     ├─ 成功 → ICurrentUserService.SetLoggedInUser(...) → LoginSucceeded イベント → ダイアログを閉じる（DialogResult.OK）
│     └─ 失敗 → ErrorMessage 表示、パスワード欄クリア、ダイアログは閉じない（再入力可）
├─ DialogResult.OK の場合 → Form1（メイン画面）を表示
└─ キャンセルの場合 → アプリケーションを終了
```

**未実装のギャップ**:
- Windows AD 自動認証（認証フロー図の Step 2）は未実装のため、現状は常にこの ID/パスワードダイアログから開始する
- `LogoutUseCase` はアプリ終了時に呼び出されておらず、`logged_out_at` は記録されない

### ICurrentUserService の実装

`RealCurrentUserService`（[RealCurrentUserService.cs](../../../src/Presentation/WinTrial/Services/RealCurrentUserService.cs)、`SupportAdvance.Presentation.WinTrial.Services` 名前空間）は認証オーケストレーションを行わず、ログイン中のユーザー情報を保持するだけの単純な実装。

```csharp
namespace SupportAdvance.Presentation.WinTrial.Services;

public sealed class RealCurrentUserService : ICurrentUserService
{
    public long EmployeeRowId { get; }   // 未ログイン時は InvalidOperationException
    public string LoginId { get; }       // 未ログイン時は InvalidOperationException
    public bool IsLoggedIn { get; }
    public bool IsAuthenticated => IsLoggedIn;

    public void SetLoggedInUser(long employeeRowId, string loginId); // LoginDialogViewModel から呼ばれる
    public void SetLoggedOut();
}
```

起動時の `SystemCurrentUserService`（暫定スタブ）→ `RealCurrentUserService` への切り替えは [Program.cs](../../../src/Presentation/WinTrial/Program.cs) の DI 登録（`AddScoped<ICurrentUserService, RealCurrentUserService>()`）で完了済み。

---

## 実装状況（2026-09-16 時点）

- ✅ **Domain層**: UserAuthSession Entity、5つの ValueObject、IUserAuthSessionRepository 実装済み
- ✅ **Application層**: AuthenticateLocalUserUseCase、LogoutUseCase 実装済み。DTOs 定義済み
- ✅ **Infrastructure層**: UserAuthSessionRepository（RepoDb+Dapper）、LoginCredentialsQueryService、PasswordHashService（PBKDF2+Salt）実装済み
- ✅ **Presentation層との統合**: LoginDialog + LoginDialogViewModel（MVVM Toolkit）、RealCurrentUserService への切り替え完了
- ✅ **E2Eテスト**: ログイン成功/失敗フローの検証完了（[LoginDialogUITests.cs](../../../tests/Contexts/Authentication.Infrastructure.Tests/E2E/LoginDialogUITests.cs)）

**未実装・既知のギャップ**:
- ❌ **Windows AD 自動認証**: `FindEmployeeByADUseCase` は空のスタブ。常に ID/パスワード認証のダイアログから開始する
- ❌ **ログアウト時の記録**: `LogoutUseCase` は実装済みだが Presentation層から呼び出されておらず、`logged_out_at` はアプリ終了時に記録されない
- ⚠️ **LoginId / AuthMethod ValueObject**: 定義済みだが実際のコードパス（Entity/DbModel/DTO）では未使用（`string`/`bool` のまま）
- ⚠️ **RowVersion（楽観ロック）**: `UserAuthSessionDbModel.RowVersion` は `[NotMapped]` のプレースホルダーで、実際の DB カラムとは未連携

---

## 参考資料

- [CLAUDE.md - Context間のデータ共有パターン](../../CLAUDE.md#-context間のデータ共有パターン)
- [CLEAN_ARCHITECTURE_GUIDELINES.md](CLEAN_ARCHITECTURE_GUIDELINES.md)
- [Repository_パターンガイド.md](Repository_パターンガイド.md)
- [Entity_設計ガイドライン.md](Entity_設計ガイドライン.md)
