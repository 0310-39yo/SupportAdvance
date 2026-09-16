# システムテスト仕様書 — Employee BC

**プロジェクト:** SupportAdvance  
**テスト対象:** 従業員ライフサイクル業務シナリオ  
**テストレベル:** システムテスト  
**版:** 1.0 / 2026-09-16

---

## 0. 本書の位置づけ

本書は、Employee Bounded Context における従業員ライフサイクル全体（新規登録→部署配属→情報更新→退職）が、設計仕様を満たすことを確認するシステムテスト仕様書です。

複数の UseCase と Domain イベント、Infrastructure 永続化を統合したテストです。

---

## 1. テスト目的

従業員ライフサイクルの完全なワークフローが以下を満たすことを確認する：

- **業務フロー連続性**: 新規登録→部署配属→情報更新→退職が順序通り実行
- **ドメインイベント**: 各ステップで適切なドメインイベント発行
- **データ整合性**: 複数テーブル（Employee・Person・DepartmentMembership）が同期
- **監査フィールド**: 全操作が CreatedBy/UpdatedBy/DeletedBy で追跡可能

---

## 2. テスト対象のビジネスシナリオ

| 項目 | 内容 |
|-----|------|
| **シナリオ名** | 従業員ライフサイクル（新規登録～退職） |
| **関連 UseCase** | CreateEmployeeUseCase / UpdateEmployeeUseCase / RetireEmployeeUseCase |
| **関連 Domain イベント** | EmployeeCreatedEvent / EmployeeUpdatedEvent / EmployeeRetiredEvent |
| **関連 Aggregate** | Employee / Person / DepartmentMembership |
| **複数ステップ** | 4ステップ（新規登録→部署配属→情報更新→退職） |

---

## 3. テストシナリオ詳細

### シナリオ概要

```
[ステップ1] CreateEmployeeUseCase
  入力: PersonRowId=1, DivisionCode="M", EmployeeNumber=1001
  出力: EmployeeId, EmployeeRowId
  イベント: EmployeeCreatedEvent

[ステップ2] DepartmentMembership 追加（Application層で実施）
  入力: EmployeeId, DepartmentRowId=10, IsPrimary=true
  出力: DepartmentMembership 追加
  イベント: なし（ビジネスロジック）

[ステップ3] UpdateEmployeeUseCase
  入力: EmployeeId, 新しい情報
  出力: 更新完了
  イベント: EmployeeUpdatedEvent

[ステップ4] RetireEmployeeUseCase（未実装時はスキップ可）
  入力: EmployeeId, RetireDate
  出力: Employee.DeletedAt 設定
  イベント: EmployeeRetiredEvent
```

---

## 4. テスト観点一覧

| 観点ID | 観点（説明） | 分類 |
|--------|------|------|
| SC-01 | ステップ1: 新規登録で Employee が created_at と共に保存 | 正常系 |
| SC-02 | ステップ2: DepartmentMembership が追加されると Department 参照確認 | 正常系 |
| SC-03 | ステップ3: 更新で updated_at が新しく設定 | 正常系 |
| SC-04 | ステップ4: 退職で deleted_at が設定、GetById が null 返却 | 正常系 |
| SC-05 | ドメインイベント: 各ステップでイベント が DomainEvents に格納 | 正常系 |
| SC-06 | 監査追跡: 全ステップの CreatedBy/UpdatedBy/DeletedBy が記録 | 正常系 |

---

## 5. テスト実行シーケンス

```csharp
// 1. 従業員作成
var createRequest = new CreateEmployeeRequest { /* ... */ };
var createResult = await createUseCase.ExecuteAsync(createRequest);
Assert.NotNull(createResult);
var employeeId = createResult.Id;

// 2. 部署配属確認（Query Service 経由）
var employee = await employeeQuery.GetByIdAsync(employeeId);
Assert.NotNull(employee);
Assert.NotNull(employee.CreatedAt.Value);

// 3. 従業員情報更新
var updateRequest = new UpdateEmployeeRequest { Id = employeeId, /* ... */ };
await updateUseCase.ExecuteAsync(updateRequest);

var updated = await employeeQuery.GetByIdAsync(employeeId);
Assert.NotNull(updated.UpdatedAt.Value);
Assert.True(updated.UpdatedAt.Value > updated.CreatedAt.Value);

// 4. 退職処理（実装時）
// await retireUseCase.ExecuteAsync(new RetireEmployeeRequest { Id = employeeId });
// Assert.Null(await employeeQuery.GetByIdAsync(employeeId));
```

---

## 6. 前提条件・制限事項

- **テスト環境**: 実SQL Server（テスト用DB）
- **Clock**: FixedClock で日時を制御
- **トランザクション**: 各テスト後にロールバック

---

## 7. 実装ガイドライン

- テスト実装は任意（仕様書策定が優先）
- 実装時はテストプロジェクト（`tests/SystemTests/` 等）を新設し `.slnx` に登録すること

---

**ドキュメント完成** ✅
