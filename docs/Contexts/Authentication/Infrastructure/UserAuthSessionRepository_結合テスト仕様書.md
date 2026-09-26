# 結合テスト仕様書 — UserAuthSessionRepository

**プロジェクト:** SupportAdvance
**テスト対象:** Application ↔ Infrastructure (UserAuthSessionRepository)
**テストレベル:** 結合テスト
**版:** 2.1 / 2026-09-18
**変更:** CreatedBy/UpdatedBy/DeletedBy のハードコード（`= 1`）を session.AuthorityRowId に修正

---

## 0. 本書の位置づけ

UserAuthSessionRepository は、UserAuthSession 集約の永続化（SaveAsync/GetByIdAsync/UpdateAsync/DeleteAsync）を担当する Infrastructure 層クラスです。単一テーブル（`t_user_auth_sessions`）に対応します。

### 【設計の特徴】

- **SaveAsync（新規作成専用）**: UserAuthSessionRowId を戻り値として返す（Department/Employee の SaveAsync とは異なり、Insert/Update 自動判定は行わない）
- **UpdateAsync（更新専用）**: 主にログアウト日時（LoggedOutAt）設定用。楽観ロック（row_version）による競合検出
- **監査フィールド管理**: Repository が CreatedAt/CreatedBy/UpdatedAt/UpdatedBy/DeletedAt/DeletedBy を自動設定
  - **CreatedBy/UpdatedBy/DeletedBy には `session.AuthorityRowId`（このセッションの主体）を使用**。
    ログイン試行中は `ICurrentUserService`（ログイン済みユーザー情報）が未設定の場合があるため、
    Entity 自身が保持する AuthorityRowId を記録する（Employee/Department の「操作した現在のユーザー」とは異なる設計判断）
  - DeleteAsync は RowId のみ受け取るため、事前に GetByIdAsync で対象セッションを取得し AuthorityRowId を得る
- **AuthorityRowId**: `m_employees.row_id` への FK 制約あり

本結合テストは、**Application層が UserAuthSessionRepository に依存して UserAuthSession 集約を永続化・取得する際、実SQL Serverとの統合が正常に動作すること** を検証します。

---

## 1. テスト目的

UserAuthSessionRepository が以下を満たすことを確認する：

- **SaveAsync**: UserAuthSession を `t_user_auth_sessions` テーブルに正しく保存し、CreatedAt/CreatedBy を設定
- **GetByIdAsync**: 保存したセッションを正しく復元
- **GetLatestByAuthorityRowIdAsync**: 指定従業員の最新セッションを取得（LoggedInAt 降順）
- **UpdateAsync**: LoggedOutAt を正しく設定し、UpdatedAt/UpdatedBy を設定
- **楽観ロック**: row_version による競合検出（他ユーザーによる更新後の UpdateAsync は例外）
- **DeleteAsync**: 論理削除（DeletedAt/DeletedBy）が正しく設定される

---

## 2. テスト対象の層間・コンポーネント

| 項目 | 内容 |
|-----|-----|
| **層間** | Application（UseCase）↔ Infrastructure（UserAuthSessionRepository）|
| **テスト用Mock/Stub** | 実SQL Server（テスト用DBインスタンス）|
| **テスト分類** | [Trait("Category", "Integration")] |

---

## 3. テスト環境要件

- SQL Server テスト用DB（3160EPOTAK / SupportAdvance）
- `t_user_auth_sessions` テーブル存在
- `s_test_row_id_sequence` 動作確認
- FK制約対応：AuthorityRowId は実装済みの System User（`SystemCurrentUserService.SystemUserEmployeeRowId = 2147483667`）を使用

---

## 4. テストケース

### TC-1: SaveAsync（新規作成）

**テスト名:** `VO_CRUD_01_SaveAsync_WithNewSession_InsertsSaveAsync_WithNewSessionInsertsSuccessfully`

**実行:**
```csharp
var session = UserAuthSession.Create(
    UserAuthSessionRowId.From(testRowId),
    AuthorityRowId.From(SystemCurrentUserService.SystemUserEmployeeRowId),
    isAdAuthenticated: true,
    loginSuccess: true,
    loggedInAt: now,
    loginCredentialsRowId: null);

var savedRowId = await repository.SaveAsync(session);
var retrieved = await repository.GetByIdAsync(savedRowId);
```

**期待結果:**
- retrieved != null
- retrieved.AuthorityRowId == session.AuthorityRowId
- retrieved.LoggedOutAt.HasLoggedOut == false（ログイン直後は未ログアウト）

---

### TC-2: SaveAsync（監査フィールド設定）

**テスト名:** `VO_AUDIT_01_SaveAsync_WithNewSession_SetsCratedAtAndBy`

**実行:**
```csharp
var savedRowId = await repository.SaveAsync(session);
var createdAt = QueryScalar("t_user_auth_sessions", savedRowId, "created_at");
var createdBy = QueryScalar("t_user_auth_sessions", savedRowId, "created_by");
```

**期待結果:**
- createdAt が自動設定されている
- createdBy が設定されている

---

### TC-3: GetByIdAsync

**テスト名:** `VO_CRUD_03_GetByIdAsync_WithValidId_ReturnsSession` / `VO_CRUD_04_GetByIdAsync_WithNonExistentId_ReturnsNull`

**期待結果:**
- 存在するIDでは該当セッションを返す
- 存在しないIDでは null を返す

---

### TC-4: GetLatestByAuthorityRowIdAsync

**テスト名:** `VO_QUERY_01_GetLatestByAuthorityRowIdAsync_WithValidAuthorityRowId_ReturnsLatestSession`

**実行:**
```csharp
// 10分前と現在の2セッションを作成
await repository.SaveAsync(earlierSession);   // loggedInAt = now - 10min
await repository.SaveAsync(laterSession);     // loggedInAt = now

var result = await repository.GetLatestByAuthorityRowIdAsync(authorityRowId);
```

**期待結果:**
- 最新（laterSession）が返る（LoggedInAt DESC でソート）

---

### TC-5: UpdateAsync（ログアウト日時設定）

**テスト名:** `VO_CRUD_05_UpdateAsync_WithExistingSession_SetsLoggedOutAtSuccessfully`

**実行:**
```csharp
var loaded = await repository.GetByIdAsync(savedRowId);  // RowVersion が設定される
loaded.SetLoggedOutAt(now);
await repository.UpdateAsync(loaded);

var result = await repository.GetByIdAsync(savedRowId);
```

**期待結果:**
- result.LoggedOutAt.HasLoggedOut == true

---

### TC-6: UpdateAsync（監査フィールド更新）

**テスト名:** `VO_AUDIT_02_UpdateAsync_WithExistingSession_SetsUpdatedAtAndBy`

**期待結果:**
- updated_at / updated_by が設定される

---

### TC-7: 楽観ロック競合検出

**テスト名:** `VO_ERROR_01_UpdateAsync_WithStaleRowVersion_ThrowsInvalidOperationException`

**実行:**
```csharp
var loaded1 = await repository.GetByIdAsync(savedRowId);
var loaded2 = await repository.GetByIdAsync(savedRowId);  // 同じ RowVersion を保持

loaded1.SetLoggedOutAt(now);
await repository.UpdateAsync(loaded1);  // 成功、RowVersion が更新される

loaded2.SetLoggedOutAt(now);
await repository.UpdateAsync(loaded2);  // 失敗（古い RowVersion のまま）
```

**期待結果:**
- 2回目の UpdateAsync が `InvalidOperationException` をスロー（競合検出）

---

### TC-8: DeleteAsync（論理削除）

**テスト名:** `VO_CRUD_06_DeleteAsync_WithValidId_SetsDeletedAtLogicallyDeletes`

**期待結果:**
- deleted_at が設定される

---

## 5. 修正履歴（重要な実装バグ）

本仕様書の v2.0 策定過程で、以下の実装バグを発見・修正した：

| # | 問題 | 修正内容 |
|---|------|---------|
| 1 | `UserAuthSessionDbModel.RowVersion` が `[NotMapped]` でDBから取得されない | `[Column("row_version")]` に変更 |
| 2 | `UserAuthSessionMapper.ToDomainEntity` が RowVersion を `Reconstruct` に渡していない | Mapper修正、Entity側に `rowVersion` パラメータ追加 |
| 3 | `UpdateAsync`/`DeleteAsync` の `QueryField`/`Field` に `nameof()`（PascalCase）を使用しSQL列名と不一致 | DBカラム名（snake_case文字列）に修正 |
| 4 | `UpdateAsync` が `created_at`/`created_by` を SET 句から除外していない（SqlDateTime overflow リスク） | `FieldsExcludingRowVersionAndCreatedAudit()` を追加 |
| 5 | `UpdateAsync` の WHERE 句に `row_version` がなく楽観ロックが機能していなかった | WHERE 句に `row_version` を追加 |
| 6 | `CreatedBy`/`UpdatedBy`/`DeletedBy` がハードコード `= 1` のまま（v2.1で修正） | `session.AuthorityRowId.Value` を使用。DeleteAsync は事前に GetByIdAsync で取得 |

**結果**: これらの修正により、楽観ロックによる同時更新競合の検出が正しく機能するようになった（TC-7 で検証）。監査フィールドも実際のセッション主体を正しく記録するようになった（TC-2/TC-6/TC-8 で値検証を追加）。

---

## 6. 前提条件・制限事項

- **テスト環境**: 実SQL Server（3160EPOTAK / SupportAdvance）
- **テーブル**: `t_user_auth_sessions` 存在
- **クリーンアップ**: `IAsyncLifetime.DisposeAsync` で、テスト中に作成した行を物理 DELETE する（try/finally パターン）
- **テストデータ分離**: RowId は `s_test_row_id_sequence` から採番

---

**ドキュメント完成** ✅
