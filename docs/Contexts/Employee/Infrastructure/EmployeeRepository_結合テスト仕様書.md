# 結合テスト仕様書 — EmployeeRepository

**プロジェクト:** SupportAdvance  
**テスト対象:** Application ↔ Infrastructure (EmployeeRepository)  
**テストレベル:** 結合テスト  
**版:** 1.0 / 2026-09-16

---

## 0. 本書の位置づけ

EmployeeRepository は、Employee 集約の永続化（Save/Get/Update/Delete）を担当する Infrastructure 層クラスです。複数テーブル（`m_employees`・`m_persons`・`m_department_memberships`）に対応します。

本結合テストは、**Application層が EmployeeRepository に依存して Employee 集約を永続化・取得する際、実SQL Serverとの統合が正常に動作すること** を検証します。

---

## 1. テスト目的

EmployeeRepository が以下を満たすことを確認する：

- **SaveAsync**: Employee 集約を複数テーブルに正しく保存
- **GetByIdAsync**: 保存したデータを正しく復元
- **UpdateAsync**: 既存データを正しく更新（監査フィールド含む）
- **DeleteAsync**: 論理削除が正しく設定される
- **監査フィールド**: CreatedAt/CreatedBy/UpdatedAt/UpdatedBy/DeletedAt/DeletedBy の自動設定

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

### TC-1: SaveAsync → GetByIdAsync ラウンドトリップ

**テスト名:** `SaveAsync_WithNewEmployee_GetByIdAsync_ReturnsExactEntity`

**実行:**
```csharp
var repository = new EmployeeRepository(dbConnectionFactory, clock);
var employee = Employee.Create(/* parameters */);
await repository.SaveAsync(employee);

var retrieved = await repository.GetByIdAsync(employee.Id);
```

**期待結果:**
- retrieved != null
- retrieved.Id == employee.Id
- retrieved.RowId > 0
- CreatedAt/CreatedBy が自動設定

---

### TC-2: UpdateAsync で監査フィールド更新

**テスト名:** `UpdateAsync_WithExistingEmployee_SetsUpdatedAt`

**実行:**
```csharp
var repository = new EmployeeRepository(dbConnectionFactory, clock);
var employee = /* saved employee */;
employee.UpdateSomeField();
await repository.UpdateAsync(employee);

var updated = await repository.GetByIdAsync(employee.Id);
```

**期待結果:**
- UpdatedAt が新しい日時に設定
- UpdatedBy が現在ユーザーに設定

---

### TC-3: DeleteAsync で論理削除

**テスト名:** `DeleteAsync_WithExistingEmployee_SetsDeletedAt`

**実行:**
```csharp
await repository.DeleteAsync(employee.Id);
var deleted = await repository.GetByIdAsync(employee.Id);
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
