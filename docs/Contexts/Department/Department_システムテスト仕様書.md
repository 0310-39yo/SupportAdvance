# システムテスト仕様書 — Department BC

**プロジェクト:** SupportAdvance  
**テスト対象:** 部署ライフサイクル業務シナリオ  
**テストレベル:** システムテスト  
**版:** 1.0 / 2026-09-16

---

## 0. 本書の位置づけ

本書は、Department Bounded Context における部署ライフサイクル全体（新規作成→上位部署紐付け→管理者割当→廃止）が、設計仕様を満たすことを確認するシステムテスト仕様書です。

Employee BC との Cross-BC 連携（IQueryService パターン）を検証します。

---

## 1. テスト目的

部署ライフサイクルが以下を満たすことを確認する：

- **新規作成**: 部署が `m_departments` テーブルに保存
- **上位部署紐付け**: ParentDepartmentRowId が正しく設定
- **管理者割当**: ManagerEmployeeRowId が Employee BC クエリサービス経由で参照可能
- **廃止**: 論理削除が正しく設定される

---

## 2. テストシナリオ

### シナリオ概要

```
[ステップ1] 親部署作成（営業本部）
  → DepartmentRowId を取得

[ステップ2] 子部署作成（営業第一部）
  → ParentDepartmentRowId = 親部署ID
  → 親子関係確認

[ステップ3] 管理者従業員の参照
  → IQueryService<Employee, EmployeeRowId> 経由で管理者を取得
  → ManagerEmployeeRowId に設定

[ステップ4] 部署廃止
  → deleted_at が設定
  → GetById が null 返却
```

---

## 3. テスト観点一覧

| 観点ID | 観点 | 分類 |
|--------|------|------|
| SC-01 | 部署作成で row_id が自動採番 | 正常系 |
| SC-02 | ParentDepartmentRowId が正しく参照 | 正常系 |
| SC-03 | Employee BC との Cross-BC 参照が成功 | 連携 |
| SC-04 | 廃止で論理削除が設定される | 正常系 |

---

## 4. 実装ガイドライン

- テスト実装は任意（仕様書策定が優先）
- 実装時は `.slnx` に登録すること

---

**ドキュメント完成** ✅
