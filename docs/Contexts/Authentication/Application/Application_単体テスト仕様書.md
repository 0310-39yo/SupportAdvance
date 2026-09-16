# Authentication Context Application層 - 単体テスト仕様書

**作成日:** 2026-09-16  
**対象読者:** テスト実装者  
**内容:** テストケース一覧、グループ定義、検証方法

---

## 📋 概要

本書は、Authentication Context Application層の **単体テスト仕様** です。

各 Use Case・Query Service のテストケースを定義し、認証ロジックの正確性を検証します。

---

## 🧪 テスト構成

### テストプロジェクト構造

```
tests/Contexts/Authentication.Application.Tests/
├── Authentication.Application.Tests.csproj
├── UseCases/
│   ├── AuthenticateLocalUserUseCaseTests.cs
│   └── LogoutUseCaseTests.cs
├── Queries/
│   └── LoginCredentialsQueryServiceTests.cs
└── Fixtures/
    └── AuthenticationApplicationFixture.cs
```

---

## 1. テスト観点一覧

### AuthenticateLocalUserUseCase

#### VO-EXEC: 実行（正常系）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-EXEC-01 | 有効な認証情報で認証できる | 正常系 | ⏸️ Skip: 結合テスト化予定 |
| VO-EXEC-02 | パスワード一致で認証成功 | 正常系 | ⏸️ Skip: 結合テスト化予定 |
| VO-EXEC-03 | 複数認証でそれぞれ独立したセッションが作成される | 正常系 | ⏸️ Skip: 結合テスト化予定 |

#### VO-RESULT: 戻り値

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-RESULT-01 | UserAuthSession が返却される | 正常系 | ⏸️ Skip: 結合テスト化予定 |
| VO-RESULT-02 | SessionId が設定される | 正常系 | ⏸️ Skip: 結合テスト化予定 |
| VO-RESULT-03 | AuthMethod, LoginAt が正しく設定される | 正常系 | ⏸️ Skip: 結合テスト化予定 |

#### VO-ERROR: 異常系

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-ERROR-01 | パスワード不一致で AuthenticationException が発生する | 異常系 | ⏸️ Skip: 結合テスト化予定 |
| VO-ERROR-02 | 存在しない LoginId で EntityNotFoundException が発生する | 異常系 | ⏸️ Skip: 結合テスト化予定 |
| VO-ERROR-03 | 削除済みユーザーで EntityNotFoundException が発生する | 異常系 | ⏸️ Skip: 結合テスト化予定 |
| VO-ERROR-04 | 期限切れ認証情報で ArgumentException が発生する | 異常系 | ⏸️ Skip: 結合テスト化予定 |

---

### LogoutUseCase

#### VO-EXEC: 実行

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-EXEC-01 | 有効なセッションでログアウトできる | 正常系 | ⏸️ Skip: 結合テスト化予定 |
| VO-EXEC-02 | 存在しないセッションで例外が発生する | 異常系 | ⏸️ Skip: 結合テスト化予定 |
| VO-EXEC-03 | 既にログアウト済みセッションで例外が発生する | 異常系 | ⏸️ Skip: 結合テスト化予定 |

#### VO-STATE: 状態変化

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-STATE-01 | LogoutAt が設定される | 正常系 | ⏸️ Skip: 結合テスト化予定 |

---

### LoginCredentialsQueryService

#### VO-QUERY: クエリ

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-QUERY-01 | 存在する LoginId で LoginCredentials を返す | 正常系 | ⏸️ Skip: 結合テスト化予定 |
| VO-QUERY-02 | 存在しない LoginId で null を返す | 正常系 | ⏸️ Skip: 結合テスト化予定 |
| VO-QUERY-03 | 削除済み認証情報で null を返す | 正常系 | ⏸️ Skip: 結合テスト化予定 |

---

## 🧪 Use Case テストケース

### 1. AuthenticateLocalUserUseCaseTests

**ファイル:** `UseCases/AuthenticateLocalUserUseCaseTests.cs`

**責務:** ローカル認証（LoginId + パスワード）→ UserAuthSession 生成

| # | テスト | 条件 | 検証 |
|---|--------|------|------|
| 1.1 | Authenticate_WithValidCredentials_ReturnsSession | 有効な認証情報 | UserAuthSession返却 |
| 1.2 | Authenticate_WithCorrectPassword_Succeeds | パスワード一致 | 認証成功 |
| 1.3 | Authenticate_WithIncorrectPassword_ThrowsException | パスワード不一致 | AuthenticationException |
| 1.4 | Authenticate_WithNonExistentLoginId_ThrowsException | 存在しないID | EntityNotFoundException |
| 1.5 | Authenticate_WithDeletedUser_ThrowsException | 削除済みユーザー | EntityNotFoundException |
| 1.6 | Authenticate_SetsSessionDetails_Correctly | セッション作成 | AuthMethod, LoginAt等設定 |
| 1.7 | Authenticate_WithMultipleAttempts_IndependentSessions | 複数認証 | 各セッション独立 |
| 1.8 | Authenticate_WithExpiredCredentials_ThrowsException | 期限切れ認証情報 | ArgumentException |

**テストケース数:** 8

---

### 2. LogoutUseCaseTests

**ファイル:** `UseCases/LogoutUseCaseTests.cs`

**責務:** UserAuthSession を終了（LogoutAt設定）

| # | テスト | 条件 | 検証 |
|---|--------|------|------|
| 2.1 | Logout_WithValidSession_CompletesSession | 有効なセッション | LogoutAt設定 |
| 2.2 | Logout_WithNonExistentSession_ThrowsException | 存在しないセッション | EntityNotFoundException |
| 2.3 | Logout_AlreadyLoggedOut_ThrowsException | 既にログアウト済み | InvalidOperationException |

**テストケース数:** 3

---

### 3. LoginCredentialsQueryServiceTests

**ファイル:** `Queries/LoginCredentialsQueryServiceTests.cs`

**責務:** LoginId からログイン認証情報を取得

| # | テスト | 条件 | 検証 |
|---|--------|------|------|
| 3.1 | GetByLoginIdAsync_WithValidId_ReturnsCredentials | 存在するLoginId | LoginCredentials返却 |
| 3.2 | GetByLoginIdAsync_WithNonExistentId_ReturnsNull | 存在しないLoginId | null返却 |
| 3.3 | GetByLoginIdAsync_WithDeletedCredentials_ReturnsNull | 削除済み認証情報 | null返却 |

**テストケース数:** 3

---

## 📊 テストケース集計

| Use Case / Query Service | テスト数 | 合計 |
|----------|---------|------|
| AuthenticateLocalUserUseCase | 8 | |
| LogoutUseCase | 3 | |
| LoginCredentialsQueryService | 3 | |
| **合計** | | **14** |

---

## 📋 テスト実装チェックリスト

### テストファイル作成
- [ ] AuthenticateLocalUserUseCaseTests (8テスト)
- [ ] LogoutUseCaseTests (3テスト)
- [ ] LoginCredentialsQueryServiceTests (3テスト)

### Fixture/Mock 実装
- [ ] AuthenticationApplicationFixture.cs
- [ ] MockUserAuthSessionRepository.cs
- [ ] MockLoginCredentialsRepository.cs
- [ ] MockPasswordHashService.cs

### テスト検証
- [ ] Red 状態確認（14 失敗）
- [ ] 実装実施
- [ ] Green 状態確認（14 成功）

---

**ドキュメント作成完了** ✅
