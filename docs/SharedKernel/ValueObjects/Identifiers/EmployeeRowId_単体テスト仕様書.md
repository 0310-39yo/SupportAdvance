# 単体テスト仕様書 — EmployeeRowId

**プロジェクト:** SupportAdvance  
**テスト対象:** `SupportAdvance.SharedKernel.ValueObjects.Identifiers.EmployeeRowId`  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-09-16

---

## 0. 本書の位置づけ

EmployeeRowId は、従業員マスタ（Employee Entity）に対応するデータベース行 ID を表す ValueObject。

**注記**: このドキュメントは SharedKernel層のEmployeeRowId に関する仕様です。全Bounded Contextで従業員情報を参照する際に使用される基盤型です。

**設計上の特徴:**
- **型**: long ベースの RowId（SharedKernel.RowId を継承）
- **パターン**: 必須型（IsSet フラグなし）
- **範囲**: 1 以上（MinValue = 1L）
- **責務**: t_employees.row_id の管理と検証
- **実装済みテスト**: SharedKernel テストで RowId ベースの fixture テスト

本仕様書は、EmployeeRowId の等価性・ハッシュ・ファクトリメソッドの動作を確認するテスト仕様。

---

## 1. テスト目的

EmployeeRowId が以下を満たすことを確認する：

- **ファクトリメソッド**: From(long) / TryFrom(long, out) / TryFromDbValue(long, out) で正しく構築できる
- **等価性**: 同じ値を持つ 2 つのインスタンスが等価である
- **ハッシュ整合性**: Equals=true のインスタンスは同一ハッシュ値
- **演算子**: == / != が正しく機能する

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | EmployeeRowId |
| **名前空間** | SupportAdvance.SharedKernel.ValueObjects.Identifiers |
| **依拠仕様** | RowId 設計ガイド v1.0 |
| **継承** | RowId, IEquatable\<EmployeeRowId\> |
| **テストファイル** | tests/SharedKernel.Tests/ValueObjects/Identifiers/EmployeeRowIdTests.cs |

---

## 3. テスト対象メソッド

| メソッド | シグネチャ | 責務 |
|---------|-----------|------|
| **From** | `public static EmployeeRowId From(long value)` | 有効な値から EmployeeRowId を生成 |
| **TryFrom** | `public static bool TryFrom(long value, out EmployeeRowId result)` | 型安全版 |
| **TryFromDbValue** | `public static bool TryFromDbValue(long value, out EmployeeRowId result)` | DB値変換版 |
| **Equals** | `public override bool Equals(object? obj)` / `public bool Equals(EmployeeRowId? other)` | 等価性判定 |
| **GetHashCode** | `public override int GetHashCode()` | ハッシュ値取得 |
| **==** / **!=** | 演算子 | 等価/非等価判定 |
| **ToString** | `public override string ToString()` | 数値文字列表現 |

---

## 4. テスト観点

### VO-EQ: Equals — 等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-EQ-01 | 同じ値を持つ 2 つのオブジェクトは等価 | 正常系 | Test-1 |
| VO-EQ-02 | 同一参照のオブジェクトは等価 | 正常系 | 内包 |
| VO-EQ-03 | 複数 RowId が同じ値で等価 | 正常系 | Test-2 |

### VO-NE: Equals — 非等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-NE-01 | 異なる値を持つ場合は非等価 | 異常系 | Test-3 |
| VO-NE-02 | null との比較は非等価 | 異常系 | 内包 |

### VO-HC: GetHashCode — ハッシュ整合性

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-HC-01 | Equals=true の 2 つのオブジェクトは同一ハッシュ値 | 正常系 | Test-4 |
| VO-HC-04 | ハッシュ値は複数呼び出しで一貫している | 正常系 | 内包 |

### VO-OP: 演算子

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-OP-01 | 等価なオブジェクトに == を適用すると true | 正常系 | 内包（Test-2） |
| VO-OP-02 | 非等価なオブジェクトに == を適用すると false | 異常系 | 内包（Test-3） |
| VO-OP-05 | != は == の否定と一致 | 正常系 | 内包 |

---

## 5. 前提条件・制限事項

- **テスト環境**: xUnit 2.0+ with Assert
- **テスト実装**: `tests/SharedKernel.Tests/ValueObjects/Identifiers/EmployeeRowIdTests.cs`
- **依存モック**: なし（ValueObject は外部依存なし）
- **DB接続**: 不要
- **テストデータ**: 範囲内の long 値のみ使用

---

## 6. 改版履歴

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-09-16 | 初版作成（Phase 3-0 ドキュメント補完、VO- 観点ID 体系に準拠） |

