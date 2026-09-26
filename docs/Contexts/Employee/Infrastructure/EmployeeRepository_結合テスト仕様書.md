# 結合テスト仕様書 — EmployeeRepository

**プロジェクト:** SupportAdvance  
**テスト対象:** Application ↔ Infrastructure (EmployeeRepository)  
**テストレベル:** 結合テスト  
**版:** 2.0 / 2026-09-17  
**変更:** SaveAsync 統一化により観点ID体系を再編成（AddAsync/UpdateAsync 分岐 → SaveAsync 一本化）

---

## 0. 本書の位置づけ

EmployeeRepository は、Employee 集約の永続化（SaveAsync/GetByIdAsync/DeleteAsync）を担当する Infrastructure 層クラスです。複数テーブル（`m_employees`・`m_persons`・`m_department_memberships`）に対応します。

### 【設計の特徴】

- **SaveAsync（統一メソッド）**: RowVersion の有無で自動的に Insert/Update を判定
  - RowVersion が空（byte[0]）→ Employee.Create() → INSERT（AddAsync）
  - RowVersion が非空（8バイト）→ Employee.Reconstruct() → UPDATE（UpdateAsync）
- **複数テーブル対応**: 単一トランザクション内で複数テーブルの一貫性を保証
- **監査フィールド管理**: Repository が CreatedAt/CreatedBy/UpdatedAt/UpdatedBy/DeletedAt/DeletedBy を自動設定

本結合テストは、**Application層が EmployeeRepository に依存して Employee 集約を永続化・取得する際、実SQL Serverとの統合が正常に動作すること** を検証します。

---

## 1. テスト目的

EmployeeRepository が以下を満たすことを確認する：

- **SaveAsync（新規）**: Employee 集約を複数テーブルに正しく保存し、CreatedAt/CreatedBy を設定
- **SaveAsync（更新）**: 既存 Employee を複数テーブルで正しく更新し、UpdatedAt/UpdatedBy を設定
- **GetByIdAsync**: 保存したデータを正しく復元
- **DeleteAsync**: 論理削除が正しく設定される（DeletedAt/DeletedBy）
- **RowVersion による判定**: SaveAsync が RowVersion の有無で Insert/Update を自動判定

---

## 2. テスト対象の層間・コンポーネント

| 項目 | 内容 |
|-----|-----|
| **層間** | Application（UseCase）↔ Infrastructure（EmployeeRepository）|
| **テスト用Mock/Stub** | 実SQL Server（テスト用DBインスタンス）|
| **テスト分類** | [Trait("Category", "Integration")] |

---

## 3. テスト環境要件

- SQL Server テスト用DB
- `m_employees`・`m_persons`・`m_department_memberships` テーブル存在
- SequenceProvider 動作確認

---

## 4. テストケース

### TC-1: SaveAsync（新規作成）

**テスト名:** `VO_CRUD_01_SaveAsync_WithNewEmployee_InsertsSaveAsync_WithNewEmployeeInsertsSuccessfully`

**実行:**
```csharp
var repository = new EmployeeRepository(dbConnectionFactory, clock);
var employee = Employee.Create(/* parameters */);
await repository.SaveAsync(employee);

var retrieved = await repository.GetByIdAsync(employee.RowId);
```

**期待結果:**
- retrieved != null
- retrieved.RowId == employee.RowId
- 複数テーブル（m_employees, m_persons, m_department_memberships）に正しく INSERT

---

### TC-2: SaveAsync（監査フィールド設定）

**テスト名:** `VO_AUDIT_01_SaveAsync_WithNewEmployee_SetsCratedAtAndBy`

**実行:**
```csharp
var employee = Employee.Create(/* parameters */);
await repository.SaveAsync(employee);
// DB から直接確認
var createdAt = QueryScalar("m_employees", employee.RowId, "created_at");
var createdBy = QueryScalar("m_employees", employee.RowId, "created_by");
```

**期待結果:**
- createdAt が自動設定されている
- createdBy が現在ユーザーに設定されている

---

### TC-3: SaveAsync（既存データ更新）

**テスト名:** `VO_CRUD_05_SaveAsync_WithExistingEmployee_UpdatesSaveAsync_WithExistingEmployeeUpdatesSuccessfully`

**実行:**
```csharp
var employee = /* saved employee */;
var loaded = await repository.GetByIdAsync(employee.RowId);  // RowVersion が設定される
var updated = Employee.Reconstruct(loaded.RowId, /* modified fields */, loaded.RowVersion);
await repository.SaveAsync(updated);  // SaveAsync が RowVersion で UPDATE を判定

var result = await repository.GetByIdAsync(employee.RowId);
```

**期待結果:**
- SaveAsync が RowVersion の有無で自動的に UPDATE を実行
- UpdatedAt/UpdatedBy が新しい値に設定
- 複数テーブルが正しく更新される

---

### TC-4: DeleteAsync で論理削除

**テスト名:** `VO_CRUD_06_DeleteAsync_WithValidId_WithValidIdSetsDeletedAtLogicallyDeletes`

**実行:**
```csharp
await repository.DeleteAsync(employee.RowId);
// DB から直接確認
var deletedAt = QueryScalar("m_employees", employee.RowId, "deleted_at");
```

**期待結果:**
- deleted == null（論理削除のため見えない）
- DB上の deleted_at が設定されている

---

### TC-4: 複数テーブル集約の整合性

**テスト名:** `SaveAsync_WithDepartmentMemberships_AllTablesUpdated`

**実行:**
- Employee に複数の DepartmentMembership を追加
- SaveAsync で永続化
- m_department_memberships を直接クエリで確認

**期待結果:**
- 全テーブルが正しく同期

---

### 実装済みで、TC として詳細を書いていないテスト

`EmployeeRepositoryTests.cs` には、上記 TC のほかに次の取得系テストがある（2026-09-26 追記。詳細な手順は書かず、テスト名のみ列挙）。

| テスト名 | 観点 |
|---|---|
| `VO_CRUD_03_GetByIdAsync_WithValidId_WithValidIdReturnsEmployee` | 存在する ID で Employee が返る |
| `VO_CRUD_04_GetByIdAsync_WithMultipleEmployees_WithMultipleEmployeesReturnsCorrectOne` | 複数件から指定した 1 件が返る |
| `VO_CRUD_04_GetByIdAsync_WithInvalidId_WithInvalidIdReturnsNull` | 存在しない ID で null |
| `VO_CRUD_03_GetByRowIdAsync_WithValidRowId_WithValidRowIdReturnsEmployee` | 存在する行ID で Employee が返る |
| `VO_CRUD_04_GetByRowIdAsync_WithInvalidRowId_WithInvalidRowIdReturnsNull` | 存在しない行ID で null |
| `VO_EXEC_01_GetByPersonRowIdAsync_WithValidPersonRowId_WithValidPersonRowIdReturnsEmployees` | Person の行ID で Employee 一覧が返る |
| `VO_EXEC_01_GetByPersonRowIdAsync_WithInvalidPersonRowId_WithInvalidPersonRowIdReturnsEmpty` | 存在しない Person で空 |

> テスト名に同じ語句が重複している（例: `WithValidId_WithValidIdReturnsEmployee`）。名前の整理は今後の課題（テストコードの変更を伴うため、ここでは記録のみ）。

---

## 5. 実行方法

```bash
# 結合テストのみ実行
dotnet test --filter "Category=Integration&FullyQualifiedName~EmployeeRepository"
```

---

## 6. 前提条件・制限事項

- **必須環境**: 実SQL Server
- **テスト分離**: 各テスト前にテーブルをクリア or トランザクションロールバック
- **Clock**: FixedClock または MockClock を使用して時刻を制御

---

**ドキュメント完成** ✅
