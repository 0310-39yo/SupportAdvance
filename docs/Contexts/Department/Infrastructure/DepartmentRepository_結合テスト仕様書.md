# 結合テスト仕様書 — DepartmentRepository

**プロジェクト:** SupportAdvance  
**テスト対象:** Application ↔ Infrastructure (DepartmentRepository)  
**テストレベル:** 結合テスト  
**版:** 1.0 / 2026-09-16

---

## 0. 本書の位置づけ

DepartmentRepository は、Department 集約の永続化（Save/Get/Update/Delete）を担当する Infrastructure 層クラスです。

本結合テストは、実SQL Serverとの統合が正常に動作することを検証します。

---

## 1. テスト目的

DepartmentRepository が以下を満たすことを確認する：

- **SaveAsync**: Department を `m_departments` テーブルに保存
- **GetByIdAsync**: 保存データを正しく復元
- **UpdateAsync**: 既存データを更新（監査フィールド含む）
- **DeleteAsync**: 論理削除が正しく設定される

---

## 2. テストケース

### TC-1: SaveAsync → GetByIdAsync

```csharp
var dept = Department.Create(/* parameters */);
await repository.SaveAsync(dept);
var retrieved = await repository.GetByIdAsync(dept.Id);
Assert.NotNull(retrieved);
Assert.Equal(dept.Id, retrieved.Id);
```

### TC-2: UpdateAsync

```csharp
dept.UpdateName("NewName");
await repository.UpdateAsync(dept);
var updated = await repository.GetByIdAsync(dept.Id);
Assert.NotNull(updated.UpdatedAt);
```

### TC-3: DeleteAsync

```csharp
await repository.DeleteAsync(dept.Id);
var deleted = await repository.GetByIdAsync(dept.Id);
Assert.Null(deleted);
```

---

## 3. 前提条件・制限事項

- **テスト環境**: 実SQL Server
- **テーブル**: `m_departments` 存在
- **トランザクション**: 各テスト後にロールバック

---

**ドキュメント完成** ✅
