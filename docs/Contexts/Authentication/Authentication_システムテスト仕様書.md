# システムテスト仕様書 — Authentication BC

**プロジェクト:** SupportAdvance  
**テスト対象:** 認証セッションライフサイクル業務シナリオ  
**テストレベル:** システムテスト  
**版:** 1.0 / 2026-09-16

---

## 0. 本書の位置づけ

本書は、Authentication Bounded Context における認証セッションライフサイクル全体（ログイン試行→認証成功→セッション発行→操作→ログアウト、および認証失敗フロー）が、設計仕様を満たすことを確認するシステムテスト仕様書です。

複数 UseCase、暗号化処理（PasswordHashService）、Repository 永続化を統合したテストです。

---

## 1. テスト目的

認証セッション完全ライフサイクルが以下を満たすことを確認する：

- **ログイン成功フロー**: LoginId + 正しいパスワード → UserAuthSession 発行
- **ログイン失敗フロー**: 誤ったパスワード → 例外発生（認証失敗）
- **パスワード暗号化**: PBKDF2 + Salt で安全に検証
- **セッション管理**: ログアウト時に LogoutAt が設定される
- **監査追跡**: ログイン・ログアウト全操作が記録される

---

## 2. テスト対象のビジネスシナリオ

| 項目 | 内容 |
|-----|------|
| **シナリオA** | ログイン成功→セッション発行→ログアウト |
| **シナリオB** | ログイン試行→パスワード不一致→失敗 |
| **関連 UseCase** | AuthenticateLocalUserUseCase / LogoutUseCase |
| **関連 Service** | PasswordHashService（暗号化） |
| **関連 Aggregate** | UserAuthSession / LoginCredentials |

---

## 3. テストシナリオ詳細

### シナリオA: ログイン成功～ログアウト

```
[ステップ1] AuthenticateLocalUserUseCase
  入力: LoginId="user@example.com", Password="secure-password"
  処理: PasswordHashService.Verify() で検証
  出力: UserAuthSession 発行
  
[ステップ2] セッション確認
  入力: SessionId
  出力: UserAuthSession が存在、LogoutAt = null
  
[ステップ3] LogoutUseCase
  入力: SessionId
  処理: LogoutAt を設定
  出力: セッション終了
```

### シナリオB: ログイン失敗

```
[ステップ1] AuthenticateLocalUserUseCase
  入力: LoginId="user@example.com", Password="wrong-password"
  処理: PasswordHashService.Verify() で不一致検出
  出力: AuthenticationException スロー
```

---

## 4. テスト観点一覧

| 観点ID | 観点（説明） | 分類 |
|--------|------|------|
| SC-01 | 正しいパスワード提供で UserAuthSession が発行される | 正常系 |
| SC-02 | 発行されたセッションに AuthMethod・LoginAt が設定 | 正常系 |
| SC-03 | セッション発行直後は LogoutAt が null | 正常系 |
| SC-04 | ログアウト処理で LogoutAt が設定される | 正常系 |
| SC-05 | 誤ったパスワードで AuthenticationException スロー | 異常系 |
| SC-06 | 存在しない LoginId で EntityNotFoundException スロー | 異常系 |
| SC-07 | パスワードハッシュが PBKDF2 仕様を満たす | 暗号化 |

---

## 5. テスト実行シーケンス（シナリオA）

```csharp
// 1. ログイン試行（成功）
var authRequest = new AuthenticateRequest 
{ 
    LoginId = "user@example.com", 
    Password = "correct-password" 
};
var session = await authenticateUseCase.ExecuteAsync(authRequest);

Assert.NotNull(session);
Assert.NotNull(session.SessionId);
Assert.NotNull(session.LoginAt);
Assert.Null(session.LogoutAt);

// 2. セッション確認
var retrieved = await sessionQuery.GetByIdAsync(session.SessionId);
Assert.NotNull(retrieved);
Assert.True(retrieved.LoginAt < DateTime.UtcNow);

// 3. ログアウト
await logoutUseCase.ExecuteAsync(new LogoutRequest { SessionId = session.SessionId });

// 4. セッション再確認（ログアウト状態）
var loggedOut = await sessionQuery.GetByIdAsync(session.SessionId);
Assert.Null(loggedOut);  // 論理削除なので見えない、または LogoutAt が設定
```

---

## 6. テスト実行シーケンス（シナリオB）

```csharp
// 1. ログイン試行（失敗）
var authRequest = new AuthenticateRequest 
{ 
    LoginId = "user@example.com", 
    Password = "wrong-password" 
};

// 2. 例外発生確認
await Assert.ThrowsAsync<AuthenticationException>(
    () => authenticateUseCase.ExecuteAsync(authRequest)
);
```

---

## 7. 前提条件・制限事項

- **テスト環境**: 実SQL Server（テスト用DB）
- **パスワード**: テスト用ユーザーの LoginCredentials が事前にセットアップされていること
- **Clock**: FixedClock で日時を制御

---

## 8. 実装ガイドライン

- テスト実装は任意（仕様書策定が優先）
- 実装時はテストプロジェクト（`tests/SystemTests/` 等）を新設し `.slnx` に登録すること

---

**ドキュメント完成** ✅
