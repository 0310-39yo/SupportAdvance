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
| VO-EXEC-01 | 有効な認証情報で認証でき、成功のセッションが保存される | 正常系 | ✅ AuthenticateLocalUserUseCaseTests.cs::VO_EXEC_01_ExecuteAsync_ValidCredentials_SavesSuccessfulSession |
| VO-EXEC-02 | パスワード一致で認証成功 | 正常系 | ✅ VO-EXEC-01 に統合（成功セッションの保存で検証） |
| VO-EXEC-03 | 複数認証でそれぞれ独立したセッションが作成される | 正常系 | ⏸️ Skip: 結合テスト化予定（セッション行ID の採番が実 DB のシーケンスに依存するため） |

#### VO-RESULT: 戻り値

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-RESULT-01 | 応答（AuthenticateLocalUserResponse）が返却される | 正常系 | ✅ VO-RESULT-02／03 で検証 |
| VO-RESULT-02 | セッションの行ID が設定される | 正常系 | ✅ AuthenticateLocalUserUseCaseTests.cs::VO_RESULT_02_ExecuteAsync_ValidCredentials_ReturnsSessionRowId |
| VO-RESULT-03 | 従業員行ID・ログインID・ログイン日時（クロックの現在時刻）が正しく設定される | 正常系 | ✅ AuthenticateLocalUserUseCaseTests.cs::VO_RESULT_03_ExecuteAsync_ValidCredentials_ReturnsEmployeeLoginIdAndClockNowAsLoggedInAt |

#### VO-ERROR: 異常系

失敗はすべて `InvalidOperationException` で通知される。認証の失敗（VO-ERROR-01〜03）は、例外の前に失敗のセッション（`LoginSuccess = false`）を保存する。

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-ERROR-01 | パスワード不一致で例外が発生し、失敗のセッションが記録される | 異常系 | ✅ AuthenticateLocalUserUseCaseTests.cs::VO_ERROR_01_ExecuteAsync_PasswordMismatch_ThrowsAndRecordsFailedSession |
| VO-ERROR-02 | 存在しない LoginId で例外が発生し、特定できない人物の失敗セッションが記録される | 異常系 | ✅ AuthenticateLocalUserUseCaseTests.cs::VO_ERROR_02_ExecuteAsync_UnknownLoginId_ThrowsAndRecordsFailedSessionForUnknownUser |
| VO-ERROR-03 | 無効なアカウントで例外が発生し、失敗のセッションが記録される | 異常系 | ✅ AuthenticateLocalUserUseCaseTests.cs::VO_ERROR_03_ExecuteAsync_InactiveAccount_ThrowsAndRecordsFailedSession |
| VO-ERROR-04 | 期限切れ認証情報で例外が発生する | 異常系 | ➖ 対象外（現行の実装に期限の概念がない） |
| VO-ERROR-05 | LoginId／Password が空・空白で ArgumentException が発生し、何も保存されない | 異常系 | ✅ AuthenticateLocalUserUseCaseTests.cs::VO_ERROR_05_ExecuteAsync_EmptyLoginIdOrPassword_ThrowsArgumentExceptionWithoutSaving（4 ケース） |
| VO-ERROR-06 | LoginId が 50 文字を超えると ArgumentException が発生し、何も保存されない | 異常系 | ✅ AuthenticateLocalUserUseCaseTests.cs::VO_ERROR_06_ExecuteAsync_TooLongLoginId_ThrowsArgumentExceptionWithoutSaving |

---

### LogoutUseCase

#### VO-EXEC: 実行

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-EXEC-01 | 有効なセッションでログアウトでき、セッションが更新される | 正常系 | ✅ LogoutUseCaseTests.cs::VO_EXEC_01_ExecuteAsync_ExistingSession_UpdatesSession |
| VO-EXEC-02 | 存在しないセッションで InvalidOperationException が発生し、更新されない | 異常系 | ✅ LogoutUseCaseTests.cs::VO_EXEC_02_ExecuteAsync_MissingSession_ThrowsInvalidOperationExceptionWithoutUpdate |
| VO-EXEC-03 | 既にログアウト済みセッションで例外が発生する | 異常系 | ➖ 仕様と実装の差異（現行の LogoutUseCase は例外を送出しない。方針が決まるまで未実装） |

#### VO-STATE: 状態変化

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-STATE-01 | ログアウト日時にクロックの現在時刻が設定され、セッションが有効でなくなる | 正常系 | ✅ LogoutUseCaseTests.cs::VO_STATE_01_ExecuteAsync_ExistingSession_SetsLoggedOutAtFromClock |

#### VO-ERROR: 異常系

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-ERROR-01 | セッションの行ID が null で ArgumentNullException が発生する | 異常系 | ✅ LogoutUseCaseTests.cs::VO_ERROR_01_ExecuteAsync_NullSessionRowId_ThrowsArgumentNullException |

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

**責務:** ローカル認証（LoginId + パスワード）→ 認証結果のセッション保存と応答の生成

観点IDとテストメソッドの対応は「1. テスト観点一覧」を参照。

**テストケース数:** 8（Theory の展開を含めて 11 ケース）

---

### 2. LogoutUseCaseTests

**ファイル:** `UseCases/LogoutUseCaseTests.cs`

**責務:** UserAuthSession にログアウト日時を設定して更新

観点IDとテストメソッドの対応は「1. テスト観点一覧」を参照。

**テストケース数:** 4

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
| LogoutUseCase | 4 | |
| LoginCredentialsQueryService | 3 | |
| **合計** | | **15** |

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
