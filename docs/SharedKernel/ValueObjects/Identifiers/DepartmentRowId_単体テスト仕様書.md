# DepartmentRowId 単体テスト仕様書

**プロジェクト:** SupportAdvance  
**テスト対象:** DepartmentRowId ValueObject（SharedKernel版）  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-09-16

---

## 0. 本書の位置づけ

本書は、SharedKernel層の ValueObject クラス `DepartmentRowId` が、設計仕様で定義された
**値の不変性・等価性比較・ハッシュ整合性・範囲検証（1以上）・文字列化** を満たすことを確認するテスト仕様書。

このクラスは **必須な Identifier** で、IsSet は常に true です。（`IOptionalValueObject` は実装していない）

---

## 1. テスト目的

`DepartmentRowId` の各メンバーが、以下の仕様を満たすことを確認する：

- **値の不変性**：オブジェクト生成後、Value が変更されない
- **等価性比較**：同じ値を持つ2つのオブジェクトは等価である
- **ハッシュ整合性**：Equals=true のオブジェクトは同一ハッシュ値
- **範囲検証**：1以上の値のみを許容、0以下は例外またはTryFromで失敗
- **文字列化**：ToString が Value の文字列を返す
- **演算子オーバーロード**：== / != が Equals と一貫している

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | DepartmentRowId |
| **名前空間** | SupportAdvance.SharedKernel.ValueObjects.Identifiers |
| **依拠仕様** | ValueObject技術仕様書 v1.1 |
| **テストファイル** | tests/Contexts/Department.Domain.Tests/ValueObjects/DepartmentRowIdTests.cs |

---

## 3. テスト観点一覧

### 観点グループ FRM：From メソッド（正系）

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-FRM-01 | 正の整数（1以上）で DepartmentRowId が生成される | 正常系 | From_ValidValue_ReturnsInstance |
| VO-FRM-02 | 大きな正の整数でも生成される | 正常系 | From_ValidValue_ReturnsInstance(9999) |

### 観点グループ FRM-ERR：From メソッド（異常系）

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-FRM-ERR-01 | 0 で ArgumentOutOfRangeException がスローされる | 異常系 | From_InvalidValue_ThrowsException(0) |
| VO-FRM-ERR-02 | 負数で ArgumentOutOfRangeException がスローされる | 異常系 | From_InvalidValue_ThrowsException(-1) |

### 観点グループ TRY：TryFrom メソッド

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-TRY-01 | 有効な正の整数で TryFrom が true を返す | 正常系 | TryFrom_ValidValue_ReturnsTrue |
| VO-TRY-02 | 0 で TryFrom が false を返す | 異常系 | TryFrom_InvalidValue_ReturnsFalse(0) |
| VO-TRY-03 | 負数で TryFrom が false を返す | 異常系 | TryFrom_InvalidValue_ReturnsFalse(-1) |

### 観点グループ TRYDB：TryFromDbValue メソッド

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-TRYDB-01 | 有効な正の整数で TryFromDbValue が true を返す | 正常系 | TryFromDbValue_ValidValue_ReturnsTrue |
| VO-TRYDB-02 | 0 で TryFromDbValue が false を返す | 異常系 | 実装で検証 |

### 観点グループ EQ：Equals - 等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-EQ-01 | 同じ値を持つ2つのオブジェクトは等価である | 正常系 | Equals_SameValue_ReturnsTrue |

### 観点グループ NE：Equals - 非等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-NE-01 | 異なる値を持つオブジェクトは非等価である | 異常系 | Equals_DifferentValue_ReturnsFalse |
| VO-NE-02 | null との比較は非等価である | 異常系 | Equals_WithNull_ReturnsFalse |

### 観点グループ HC：GetHashCode

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-HC-01 | Equals=true のオブジェクトペアは同一ハッシュ値を返す | 正常系 | 実装で検証 |
| VO-HC-02 | 異なる値のオブジェクトは異なるハッシュ値を返す | 異常系 | 実装で検証 |

### 観点グループ OP：演算子（==, !=）

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-OP-01 | 等価なオブジェクトに == を適用すると true | 正常系 | OperatorEqual_SameValues_ReturnsTrue |
| VO-OP-02 | 非等価なオブジェクトに == を適用すると false | 異常系 | 実装で検証 |
| VO-OP-03 | != は == の否定と一致 | 正常系 | OperatorNotEqual_DifferentValues_ReturnsTrue |

### 観点グループ TS：ToString

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-TS-01 | ToString が Value の文字列を返す | 正常系 | ToString_ReturnsLongString |

---

## 4. テスト観点別の検証シナリオ

### 観点 VO-FRM-01 ～ VO-FRM-02：正の整数（1以上）で生成される

#### 4.1.1 テスト観点

DepartmentRowId.From() に 1 以上の整数を渡した場合、
正常なインスタンスが返される。

#### 4.1.2 テストパターン

| パターン | 分類 | 入力値 | 説明 |
|---------|------|--------|------|
| 4.1.2.1 | 正常系 | 123 | 典型的な部門ID |
| 4.1.2.2 | 正常系 | 9999 | 大きな値 |

#### 4.1.3 前提条件

- DepartmentRowId.From() メソッドが利用可能
- Value プロパティが読み取り可能

#### 4.1.4 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.1.2.1 | rowId.Value == 123 | Value が入力値と一致 |
| 4.1.2.2 | rowId.Value == 9999 | Value が入力値と一致 |

#### 4.1.5 判定基準

- [ ] From() が例外をスローしない
- [ ] 返却オブジェクトが null でない
- [ ] Value が入力値と一致する

---

### 観点 VO-FRM-ERR-01 ～ VO-FRM-ERR-02：0以下で例外がスローされる

#### 4.2.1 テスト観点

DepartmentRowId.From() に 0 以下の値を渡した場合、
ArgumentOutOfRangeException がスローされる。

#### 4.2.2 テストパターン

| パターン | 分類 | 入力値 | 説明 |
|---------|------|--------|------|
| 4.2.2.1 | 異常系 | 0 | ゼロ |
| 4.2.2.2 | 異常系 | -1 | 負数 |

#### 4.2.3 期待結果

| パターン | 期待動作 |
|---------|---------|
| 4.2.2.1-2 | ArgumentOutOfRangeException がスローされる |

#### 4.2.4 判定基準

- [ ] ArgumentOutOfRangeException が発生する

---

### 観点 VO-TRY-01：有効な正の整数で TryFrom が true を返す

#### 4.3.1 テスト観点

DepartmentRowId.TryFrom() に有効な正の整数を渡した場合、
true を返し、out パラメータに正常なインスタンスが設定される。

#### 4.3.2 テストパターン

| パターン | 分類 | 入力値 |
|---------|------|--------|
| 4.3.2.1 | 正常系 | 456 |

#### 4.3.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.3.2.1 | result == true && rowId.Value == 456 |

#### 4.3.4 判定基準

- [ ] TryFrom() が true を返す
- [ ] out パラメータが null でない
- [ ] Value が入力値と一致

---

### 観点 VO-TRY-02 ～ VO-TRY-03：0以下で TryFrom が false を返す

#### 4.4.1 テスト観点

DepartmentRowId.TryFrom() に 0 以下の値を渡した場合、
false を返す。例外はスローされない（TryPattern）。

#### 4.4.2 テストパターン

| パターン | 分類 | 入力値 |
|---------|------|--------|
| 4.4.2.1 | 異常系 | 0 |
| 4.4.2.2 | 異常系 | -1 |

#### 4.4.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.4.2.1-2 | result == false |

#### 4.4.4 判定基準

- [ ] TryFrom() が false を返す
- [ ] 例外がスローされない

---

### 観点 VO-TRYDB-01：DB値で TryFromDbValue が true を返す

#### 4.5.1 テスト観点

DepartmentRowId.TryFromDbValue() に有効な正の整数を渡した場合、
true を返す。（DB値の読み込み用メソッド）

#### 4.5.2 テストパターン

| パターン | 分類 | 入力値 |
|---------|------|--------|
| 4.5.2.1 | 正常系 | 789 |

#### 4.5.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.5.2.1 | result == true && rowId.Value == 789 |

#### 4.5.4 判定基準

- [ ] TryFromDbValue() が true を返す

---

### 観点 VO-EQ-01：同じ値を持つ2つのオブジェクトは等価である

#### 4.6.1 テスト観点

DepartmentRowId.From(100) で生成した2つのインスタンスが Equals で等価と判定される。

#### 4.6.2 テストパターン

| パターン | 分類 | value1 | value2 |
|---------|------|--------|--------|
| 4.6.2.1 | 正常系 | 100 | 100 |

#### 4.6.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.6.2.1 | rowId1.Equals(rowId2) == true |

#### 4.6.4 判定基準

- [ ] Equals() が true を返す
- [ ] == 演算子でも true を返す

---

### 観点 VO-NE-01：異なる値を持つオブジェクトは非等価である

#### 4.7.1 テスト観点

DepartmentRowId.From(100) と DepartmentRowId.From(200) は非等価と判定される。

#### 4.7.2 テストパターン

| パターン | 分類 | value1 | value2 |
|---------|------|--------|--------|
| 4.7.2.1 | 異常系 | 100 | 200 |

#### 4.7.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.7.2.1 | rowId1.Equals(rowId2) == false |

#### 4.7.4 判定基準

- [ ] Equals() が false を返す
- [ ] != 演算子で true を返す

---

### 観点 VO-NE-02：null との比較は非等価である

#### 4.8.1 テスト観点

DepartmentRowId.From(1) と null は非等価と判定される。

#### 4.8.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.8.2.1 | 異常系 | rowId.Equals(null) |

#### 4.8.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.8.2.1 | result == false |

#### 4.8.4 判定基準

- [ ] Equals(null) が false を返す

---

### 観点 VO-OP-01：等価なオブジェクトに == を適用すると true

#### 4.9.1 テスト観点

DepartmentRowId の == 演算子が Equals と一貫している。

#### 4.9.2 テストパターン

| パターン | 分類 | value1 | value2 |
|---------|------|--------|--------|
| 4.9.2.1 | 正常系 | 1 | 1 |

#### 4.9.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.9.2.1 | rowId1 == rowId2 == true |

#### 4.9.4 判定基準

- [ ] == 演算子が true を返す
- [ ] Equals() と一貫している

---

### 観点 VO-OP-03：!= は == の否定と一致

#### 4.10.1 テスト観点

DepartmentRowId の != 演算子が == の否定を返す。

#### 4.10.2 テストパターン

| パターン | 分類 | value1 | value2 |
|---------|------|--------|--------|
| 4.10.2.1 | 異常系 | 1 | 2 |

#### 4.10.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.10.2.1 | rowId1 != rowId2 == true |

#### 4.10.4 判定基準

- [ ] != 演算子が true を返す

---

### 観点 VO-TS-01：ToString が Value の文字列を返す

#### 4.11.1 テスト観点

DepartmentRowId.ToString() が Value の文字列表現を返す。

#### 4.11.2 テストパターン

| パターン | 分類 | 入力値 |
|---------|------|--------|
| 4.11.2.1 | 正常系 | 999 |

#### 4.11.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.11.2.1 | ToString() == "999" |

#### 4.11.4 判定基準

- [ ] ToString() が値の文字列を返す

---

## 5. 判定基準

### 5.1 全般的な判定基準

| 項目 | 基準 |
|------|------|
| **不変性** | オブジェクト生成後、Value が変更されないこと |
| **等価性** | Equals の結果が論理的に一貫していること |
| **ハッシュ整合性** | Equals=true のペアが同一ハッシュ値を返すこと |
| **演算子整合性** | == と Equals の結果が矛盾しないこと |
| **範囲検証** | From で 1以上の値をチェック、0以下で例外またはTryFromで失敗 |
| **文字列化** | ToString が Value の文字列を返すこと |

---

## 6. テスト実装ガイド

### テストコード例

```csharp
// 正常系テスト
[Fact]
public void From_ValidValue_ReturnsInstance()
{
    var rowId = DepartmentRowId.From(123);
    Assert.Equal(123, rowId.Value);
}

// 異常系テスト（From）
[Fact]
public void From_InvalidValue_ThrowsException()
{
    Assert.Throws<ArgumentOutOfRangeException>(() => DepartmentRowId.From(0));
    Assert.Throws<ArgumentOutOfRangeException>(() => DepartmentRowId.From(-1));
}

// TryFrom テスト
[Fact]
public void TryFrom_ValidValue_ReturnsTrue()
{
    var result = DepartmentRowId.TryFrom(456, out var rowId);
    Assert.True(result);
    Assert.Equal(456, rowId.Value);
}

// TryFrom 失敗テスト
[Fact]
public void TryFrom_InvalidValue_ReturnsFalse()
{
    var result = DepartmentRowId.TryFrom(0, out _);
    Assert.False(result);
}

// TryFromDbValue テスト
[Fact]
public void TryFromDbValue_ValidValue_ReturnsTrue()
{
    var result = DepartmentRowId.TryFromDbValue(789, out var rowId);
    Assert.True(result);
    Assert.Equal(789, rowId.Value);
}

// 等価性テスト
[Fact]
public void Equals_SameValue_ReturnsTrue()
{
    var rowId1 = DepartmentRowId.From(100);
    var rowId2 = DepartmentRowId.From(100);
    Assert.Equal(rowId1, rowId2);
}

// 非等価性テスト
[Fact]
public void Equals_DifferentValue_ReturnsFalse()
{
    var rowId1 = DepartmentRowId.From(100);
    var rowId2 = DepartmentRowId.From(200);
    Assert.NotEqual(rowId1, rowId2);
}

// 演算子テスト
[Fact]
public void OperatorEqual_SameValues_ReturnsTrue()
{
    var rowId1 = DepartmentRowId.From(1);
    var rowId2 = DepartmentRowId.From(1);
    Assert.True(rowId1 == rowId2);
}

// != 演算子テスト
[Fact]
public void OperatorNotEqual_DifferentValues_ReturnsTrue()
{
    var rowId1 = DepartmentRowId.From(1);
    var rowId2 = DepartmentRowId.From(2);
    Assert.True(rowId1 != rowId2);
}

// ToString テスト
[Fact]
public void ToString_ReturnsValue()
{
    var rowId = DepartmentRowId.From(999);
    Assert.Equal("999", rowId.ToString());
}
```

---

## 7. 改版履歴

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-09-16 | 初版作成。テンプレート準拠、SharedKernel版、実装コード検証済み |

---

## 8. 参考資料

- ValueObject 技術仕様書 v1.1
- RowId 設計ガイド
- テンプレート：`docs/Assistance/Templates/Test/TEMPLATE_単体テスト仕様書_ValueObject.md`
- 実装：`tests/Contexts/Department.Domain.Tests/ValueObjects/DepartmentRowIdTests.cs`
- SharedKernel クラス：`src/SharedKernel/ValueObjects/Identifiers/DepartmentRowId.cs`
