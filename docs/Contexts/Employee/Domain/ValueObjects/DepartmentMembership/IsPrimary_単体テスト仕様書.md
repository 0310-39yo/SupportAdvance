# 単体テスト仕様書 — IsPrimary

**プロジェクト:** SupportAdvance  
**テスト対象:** `SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership.IsPrimary`  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-09-16

---

## 0. 本書の位置づけ

IsPrimary は、従業員の部門所属が主要部門であるかどうかを表す ValueObject。

**設計上の特徴:**
- **型**: bool ベースの ValueObject（IsSet フラグなし）
- **パターン**: 必須型（bool は常に true/false で決定）
- **責務**: 従業員が複数部門に所属する場合、そのうちどの部門が主要かを識別
- **実装済みテスト**: IsPrimaryTests.cs

本仕様書は、IsPrimary の等価性・ハッシュ・ファクトリメソッド・文字列表現の動作を確認するテスト仕様。

---

## 1. テスト目的

IsPrimary が以下を満たすことを確認する：

- **ファクトリメソッド**: From(bool) で true/false から正しく構築できる
- **等価性**: 同じ値を持つ 2 つのインスタンスが等価である
- **ハッシュ整合性**: Equals=true のインスタンスは同一ハッシュ値
- **演算子**: == / != が正しく機能する
- **文字列化**: ToString が "Primary" / "Secondary" など適切に返す

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | IsPrimary |
| **名前空間** | SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership |
| **依拠仕様** | ValueObject 設計ガイド v1.0 |
| **継承** | ValueObject\<bool\>, IEquatable\<IsPrimary\> |

---

## 3. テスト観点

### VO-EQ: Equals — 等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-EQ-01 | 同じ値（true/true）のオブジェクトは等価 | 正常系 | Test-1 |
| VO-EQ-02 | 同じ値（false/false）のオブジェクトは等価 | 正常系 | Test-1 |
| VO-EQ-03 | 同一参照のオブジェクトは等価 | 正常系 | 内包 |

### VO-NE: Equals — 非等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-NE-01 | 異なる値（true vs false）は非等価 | 異常系 | Test-2 |
| VO-NE-02 | null との比較は非等価 | 異常系 | 内包 |

### VO-HC: GetHashCode — ハッシュ整合性

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-HC-01 | Equals=true の 2 つのオブジェクトは同一ハッシュ値 | 正常系 | Test-3 |
| VO-HC-02 | true と false のハッシュは異なる | 正常系 | 内包 |
| VO-HC-04 | ハッシュ値は複数呼び出しで一貫している | 正常系 | 内包 |

### VO-OP: 演算子

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-OP-01 | 等価なオブジェクトに == を適用すると true | 正常系 | 内包 |
| VO-OP-02 | 非等価なオブジェクトに == を適用すると false | 異常系 | 内包 |
| VO-OP-05 | != は == の否定と一致 | 正常系 | 内包 |

### VO-TS: ToString

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-TS-01 | Value=true のとき、適切な文字列を返す（"Primary" など） | 正常系 | Test-4 |
| VO-TS-02 | Value=false のとき、適切な文字列を返す（"Secondary" など） | 正常系 | 内包 |

---

## 4. テスト仕様別の検証シナリオ

### 観点 VO-EQ-01/02: 等価性

#### 4.1.1 テスト観点

IsPrimary を同じ値で複数回生成した場合、Equals が true を返す。

#### 4.1.2 テストパターン

| パターン | 説明 |
|---------|------|
| 4.1.2.1 | IsPrimary.From(true) × 2、Equals() で比較 |
| 4.1.2.2 | IsPrimary.From(false) × 2、Equals() で比較 |
| 4.1.2.3 | 同じ値、GetHashCode() が同じ |

#### 4.1.3 期待結果

- [ ] true 値のオブジェクトどうしは等価
- [ ] false 値のオブジェクトどうしは等価
- [ ] ハッシュ値は同じ

---

### 観点 VO-NE-01: 非等価性

#### 4.2.1 テスト観点

IsPrimary を異なる値で生成した場合、Equals が false を返す。

#### 4.2.2 テストパターン

| パターン | 説明 |
|---------|------|
| 4.2.2.1 | IsPrimary.From(true) vs From(false)、Equals() で比較 |

#### 4.2.3 期待結果

- [ ] 異なる値のオブジェクトは等価にならない
- [ ] != 演算子で true を返す

---

## 5. 前提条件・制限事項

- **テスト環境**: xUnit 2.0+ with Assert
- **テスト実装**: `tests/Contexts/Employee.Domain.Tests/ValueObjects/DepartmentMembership/IsPrimaryTests.cs`
- **依存モック**: なし（ValueObject は外部依存なし）
- **DB接続**: 不要
- **テストデータ**: true / false のみ

---

## 6. 改版履歴

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-09-16 | 初版作成（Phase 3-0 ドキュメント補完、VO- 観点ID 体系に準拠） |

