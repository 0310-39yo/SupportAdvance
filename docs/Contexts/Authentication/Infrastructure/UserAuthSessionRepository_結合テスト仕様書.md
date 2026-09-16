# 結合テスト仕様書 — UserAuthSessionRepository

**プロジェクト:** SupportAdvance  
**テスト対象:** Application ↔ Infrastructure (UserAuthSessionRepository)  
**テストレベル:** 結合テスト  
**版:** 1.0 / 2026-09-16

---

## 0. 本書の位置づけ

UserAuthSessionRepository は、UserAuthSession 集約の永続化（Save/Get/Delete）を担当する Infrastructure 層クラスです。

本結合テストは、実SQL Serverとの統合が正常に動作することを検証します。

---

## 1. テスト目的

UserAuthSessionRepository が以下を満たすことを確認する：

- **SaveAsync**: UserAuthSession を `m_user_auth_sessions` テーブルに保存
- **GetByIdAsync**: 保存したセッションを正しく復元
- **GetLatestByLoginCredentialsRowIdAsync**: 最新セッションを取得
- **UpdateAsync**: セッション情報（LogoutAt など）を更新
- **DeleteAsync**: セッションを論理削除

---

## 2. テストケース

### TC-1: SaveAsync → GetByIdAsync

```csharp
var session = UserAuthSession.Issue(/* parameters */);
await repository.SaveAsync(session);
var retrieved = await repository.GetByIdAsync(session.Id);
Assert.NotNull(retrieved);
Assert.Equal(session.Id, retrieved.Id);
```

### TC-2: GetLatestByLoginCredentialsRowIdAsync

```csharp
await repository.SaveAsync(session);
var latest = await repository.GetLatestByLoginCredentialsRowIdAsync(
    session.LoginCredentialsRowId);
Assert.NotNull(latest);
Assert.Equal(session.Id, latest.Id);
```

### TC-3: UpdateAsync（LogoutAt 設定）

```csharp
session.Logout(_clock);
await repository.UpdateAsync(session);
var updated = await repository.GetByIdAsync(session.Id);
Assert.NotNull(updated.LogoutAt);
```

---

## 3. 前提条件・制限事項

- **テスト環境**: 実SQL Server
- **テーブル**: `m_user_auth_sessions` 存在
- **トランザクション**: 各テスト後にロールバック

---

**Phase 5 完成** ✅ **全7仕様書完成**

1. ✅ SequenceProvider_結合テスト仕様書.md
2. ✅ EmployeeRepository_結合テスト仕様書.md
3. ✅ DepartmentRepository_結合テスト仕様書.md
4. ✅ UserAuthSessionRepository_結合テスト仕様書.md
5. ✅ Employee_システムテスト仕様書.md
6. ✅ Authentication_システムテスト仕様書.md
7. ✅ Department_システムテスト仕様書.md
