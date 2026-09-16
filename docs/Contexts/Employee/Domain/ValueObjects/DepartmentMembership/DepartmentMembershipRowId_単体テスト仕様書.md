# 単体テスト仕様書 — DepartmentMembershipRowId

**プロジェクト:** SupportAdvance  
**テスト対象:** `SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership.DepartmentMembershipRowId`  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-09-16

---

## 0. 本書の位置づけ

DepartmentMembershipRowId は、従業員の部署所属情報（DepartmentMembership Entity）に対応するデータベース行 ID を表す ValueObject。

**設計上の特徴:**
- **型**: long ベースの RowId（SharedKernel.RowId を継承）
- **パターン**: 必須型（IsSet フラグなし）
- **範囲**: 1 以上（MinValue = 1L）
- **責務**: Department・Employee の所属管理における行 ID 検証
- **実装済みテスト**: DepartmentMembershipRowIdTests.cs

本仕様書は、DepartmentMembershipRowId の等価性・ハッシュ・ファクトリメソッドの動作を確認するテスト仕様。

---

## 1. テスト目的

DepartmentMembershipRowId が以下を満たすことを確認する：

- **ファクトリメソッド**: From(long) / TryFrom(long, out) で正しく構築できる
- **等価性**: 同じ値を持つ 2 つのインスタンスが等価である
- **ハッシュ整合性**: Equals=true のインスタンスは同一ハッシュ値
- **演算子**: == / != が正しく機能する

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | DepartmentMembershipRowId |
| **名前空間** | SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership |
| **依拠仕様** | RowId 設計ガイド v1.0 |
| **継承** | RowId, IEquatable\<DepartmentMembershipRowId\> |

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

- DepartmentMembershipRowId.From() が正常に機能
- Equals/GetHashCode が実装されている
- DB接続不要

---

## 5. 改版履歴

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-09-16 | 初版作成（Phase 3-0 ドキュメント補完） |

