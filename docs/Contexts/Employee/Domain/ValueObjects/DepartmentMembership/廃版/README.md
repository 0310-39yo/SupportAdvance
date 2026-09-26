# 廃版ファイル管理 — Employee 所属の値オブジェクト領域

**版:** 1.0
**作成日:** 2026-09-26

## 1. 本フォルダの位置づけ

本フォルダ（`docs/Contexts/Employee/Domain/ValueObjects/DepartmentMembership/廃版/`）は、旧バージョン・廃止済みドキュメントを保管する。

## 2. ファイル名規則

廃版ファイルは以下の形式で命名する。

```
v{廃版時の版番号}_{元のファイル名}_{廃版日YYYY-MM-DD}.md
```

## 3. 廃版ファイル一覧

| # | ファイル名 | 元のファイル名 | 廃版化日 | 理由 | 後継ドキュメント |
|---|-----------|--------------|--------|------|----------------|
| 1 | v1.0_DepartmentRowId_単体テスト仕様書_2026-09-26.md | DepartmentRowId_単体テスト仕様書.md | 2026-09-26 | DepartmentRowId は SharedKernel の型。仕様書は SharedKernel 側に一本化（Employee 側は簡易版の重複） | `docs/SharedKernel/ValueObjects/Identifiers/DepartmentRowId_単体テスト仕様書.md` |
