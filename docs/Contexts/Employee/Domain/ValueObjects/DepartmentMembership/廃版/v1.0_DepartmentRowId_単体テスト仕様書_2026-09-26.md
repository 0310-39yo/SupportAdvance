# 単体テスト仕様書 — DepartmentRowId（Employee側）

**プロジェクト:** SupportAdvance  
**テスト対象:** `SupportAdvance.SharedKernel.ValueObjects.Identifiers.DepartmentRowId`  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-09-16

---

## 0. 本書の位置づけ

DepartmentRowId（SharedKernel版）は、部門マスタ（Department Entity）に対応するデータベース行 ID を表す ValueObject。

**注記**: このドキュメントは、Employee Context の DepartmentMembership エンティティが参照する DepartmentRowId（SharedKernel由来）に関する仕様です。同名のクラスが Department.Domain に別に存在しますが、混同しないよう注意してください。

**設計上の特徴:**
- **型**: long ベースの RowId（SharedKernel.RowId を継承）
- **パターン**: 必須型（IsSet フラグなし）
- **範囲**: 1 以上（MinValue = 1L）
- **責務**: t_departments.row_id の管理と検証
- **実装済みテスト**: SharedKernel テストで RowId ベースの fixture テスト

本仕様書は、Employee側から部門を参照する際の DepartmentRowId（SharedKernel 基底）の等価性・ハッシュ・ファクトリメソッドの動作を確認するテスト仕様。

---

## 1. テスト目的

DepartmentRowId（SharedKernel版）が以下を満たすことを確認する：

- **ファクトリメソッド**: From(long) / TryFrom(long, out) で正しく構築できる
- **等価性**: 同じ値を持つ 2 つのインスタンスが等価である
- **ハッシュ整合性**: Equals=true のインスタンスは同一ハッシュ値
- **演算子**: == / != が正しく機能する

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | DepartmentRowId |
| **名前空間** | SupportAdvance.SharedKernel.ValueObjects.Identifiers |
| **依拠仕様** | RowId 設計ガイド v1.0 |
| **継承** | RowId, IEquatable\<DepartmentRowId\> |

---

## 3. テスト観点

### 主要観点

| 観点ID | 観点 | 分類 |
|--------|------|------|
| VO-EQ-01 | 同じ値で生成した 2 インスタンスは等価 | 正常系 |
| VO-EQ-02 | 同一参照のオブジェクトは等価 | 正常系 |
| VO-NE-01 | 異なる値は非等価 | 異常系 |
| VO-HC-01 | Equals=true なら同一ハッシュ値 | 正常系 |
| VO-OP-01 | == で等価判定 | 正常系 |
| VO-OP-02 | == で非等価判定 | 異常系 |

---

## 4. 前提条件

- DepartmentRowId.From() が正常に機能
- Equals/GetHashCode が実装されている
- DB接続不要

---

## 5. 改版履歴

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-09-16 | 初版作成（Phase 3-0 ドキュメント補完） |

