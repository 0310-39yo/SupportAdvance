# 単体テスト仕様書 — PersonRowId

**プロジェクト:** SupportAdvance  
**テスト対象:** `SupportAdvance.Contexts.Employee.Domain.ValueObjects.Person.PersonRowId`  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-09-16

---

## 0. 本書の位置づけ

PersonRowId は、人物（Person Entity）に対応するデータベース行 ID を表す ValueObject。

**設計上の特徴:**
- **型**: long ベースの RowId（SharedKernel.RowId を継承）
- **パターン**: 必須型（IsSet フラグなし）
- **範囲**: 1 以上（MinValue = 1L）
- **責務**: m_persons.row_id の管理と検証
- **実装済みテスト**: PersonRowIdTests.cs（正常系 3 + 異常系 1）

本仕様書は、PersonRowId の等価性・ハッシュ・ファクトリメソッドの動作を確認するテスト仕様。

---

## 1. テスト目的

PersonRowId が以下を満たすことを確認する：

- **ファクトリメソッド**: From(long) / TryFrom(long, out) で正しく構築できる
- **等価性**: 同じ値を持つ 2 つのインスタンスが等価である
- **ハッシュ整合性**: Equals=true のインスタンスは同一ハッシュ値
- **演算子**: == / != が正しく機能する
- **文字列化**: ToString が数値文字列を返す

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | PersonRowId |
| **名前空間** | SupportAdvance.Contexts.Employee.Domain.ValueObjects.Person |
| **依拠仕様** | RowId 設計ガイド v1.0（SharedKernel） |
| **継承** | RowId（abstract base）, IEquatable\<PersonRowId\> |
| **テストファイル** | tests/Contexts/Employee.Domain.Tests/ValueObjects/Person/PersonRowIdTests.cs |

---

## 3. テスト対象メソッド

| メソッド | シグネチャ | 責務 |
|---------|-----------|------|
| **From** | `public static PersonRowId From(long value)` | 有効な値から PersonRowId を生成（例外発生可） |
| **TryFrom** | `public static bool TryFrom(long value, out PersonRowId result)` | 有効な値から PersonRowId を生成（型安全版） |
| **Equals** | `public override bool Equals(object? obj)` / `public bool Equals(PersonRowId? other)` | 等価性判定 |
| **GetHashCode** | `public override int GetHashCode()` | ハッシュ値取得 |
| **==** | `public static bool operator ==(PersonRowId? left, PersonRowId? right)` | 等価演算子 |
| **!=** | `public static bool operator !=(PersonRowId? left, PersonRowId? right)` | 非等価演算子 |
| **ToString** | `public override string ToString()` | 文字列化（数値文字列） |

---

## 4. テスト観点

### 観点グループ EQ: `Equals` — 等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-EQ-01 | 同じ値を持つ 2 つのオブジェクトは等価 | 正常系 | Test1-1 |
| VO-EQ-02 | 同一参照のオブジェクトは等価 | 正常系 | 内包（Test1-1） |
| VO-EQ-03 | 複数 RowId が同じ値で等価 | 正常系 | Test1-2 |
| VO-EQ-04 | 値が 1L の場合も等価 | 正常系（最小値） | 内包（Test1-1） |

### 観点グループ NE: `Equals` — 非等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-NE-01 | 異なる値を持つ場合は非等価 | 異常系 | Test1-3 |
| VO-NE-02 | 型が異なる場合は非等価 | 異常系 | 内包（Test1-3） |
| VO-NE-04 | null との比較は非等価 | 例外/異常系 | 内包（Test1-3） |

### 観点グループ HC: `GetHashCode` — ハッシュ整合性

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-HC-01 | Equals=true の 2 つのオブジェクトは同一ハッシュ値 | 正常系 | Test2-1 |
| VO-HC-03 | 異なる値のハッシュは通常異なる | 境界値テスト | 内包（Test1-3） |
| VO-HC-04 | ハッシュ値は複数呼び出しで一貫している | 正常系（副作用なし） | Test2-1 |

### 観点グループ OP: `==` / `!=` 演算子

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-OP-01 | 等価なオブジェクトに == を適用すると true | 正常系 | 内包（Test1-2） |
| VO-OP-02 | 非等価なオブジェクトに == を適用すると false | 異常系 | 内包（Test1-3） |
| VO-OP-05 | != は == の否定と一致 | 正常系 | 内包（Test1-2, Test1-3） |

### 観点グループ TS: `ToString`

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-TS-02 | IsSet=true のとき、Value を数値文字列で返す | 正常系 | 内包（Test1-1） |

---

## 5. テスト仕様別の検証シナリオ

### 観点 VO-EQ-01: 同じ値を持つ 2 つのオブジェクトは等価

#### 5.1.1 テスト観点

PersonRowId を同じ値（例：1L）で複数回生成した場合、Equals が true を返す。

#### 5.1.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 5.1.2.1 | 正常系 | `PersonRowId.From(1L)` × 2、Equals() で比較 |
| 5.1.2.2 | 正常系 | 同じ値、GetHashCode() が同じ |

#### 5.1.3 前提条件

- PersonRowId.From() が正常に機能する
- Equals メソッドが読み取り可能

#### 5.1.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 5.1.2.1 | PersonRowId.From(1L) | 最小有効値 |
| 5.1.2.2 | PersonRowId.From(999999L) | 大きな値 |

#### 5.1.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 5.1.2.1 | Equals == true | 等価性判定 |
| 5.1.2.2 | GetHashCode は同じ | ハッシュ整合性 |

#### 5.1.6 判定基準

- [ ] 同じ値で生成した 2 つの PersonRowId は Equals で true を返す
- [ ] その GetHashCode() は同じ値である
- [ ] == 演算子でも true を返す

---

### 観点 VO-NE-01: 異なる値を持つ場合は非等価

#### 5.2.1 テスト観点

PersonRowId を異なる値で生成した場合、Equals が false を返す。

#### 5.2.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 5.2.2.1 | 異常系 | `PersonRowId.From(1L)` vs `From(2L)`、Equals() で比較 |

#### 5.2.3 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 5.2.2.1 | Equals == false | 非等価判定 |

#### 5.2.4 判定基準

- [ ] 異なる値で生成した PersonRowId は Equals で false を返す
- [ ] != 演算子で true を返す

---

## 6. 前提条件・制限事項

- **テスト環境**: xUnit 2.0+ with Assert
- **テスト実装**: `tests/Contexts/Employee.Domain.Tests/ValueObjects/Person/PersonRowIdTests.cs`
- **依存モック**: なし（ValueObject は外部依存なし）
- **DB接続**: 不要
- **スキップテスト**: なし（全テストが実施可能）
- **テストデータ**: 範囲内の long 値のみ使用

---

## 7. テスト結果統計

**実装済みテスト: 3 + 1（異常系）= 4 件**

| テスト項目 | テスト数 | 観点カバレッジ |
|-----------|---------|------------|
| From() 正常系 | 2 | VO-EQ-01, VO-EQ-03, VO-HC-01 |
| Equals / == 非等価 | 1 | VO-NE-01, VO-OP-02 |
| TryFrom() 異常系 | 1 | 型安全性（null チェック） |
| **合計** | **4** | **8 観点** |

---

## 8. 改版履歴

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-09-16 | 初版作成（Phase 3-0 ドキュメント補完、VO- 観点ID 体系に準拠） |

