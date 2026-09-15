# ParentDepartmentRowId 単体テスト仕様書

**プロジェクト:** SupportAdvance  
**テスト対象:** ParentDepartmentRowId ValueObject  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-09-16

---

## 0. 本書の位置づけ

本書は、Domain層の ValueObject クラス `ParentDepartmentRowId` が、設計仕様で定義された
**値の不変性・オプショナル値操作（Unset/HasParent）・等価性比較・ハッシュ整合性** を満たすことを確認するテスト仕様書。

このクラスは `IOptionalValueObject` を実装し、親部門がない場合（Unset状態）を表現する。

---

## 1. テスト目的

`ParentDepartmentRowId` の各メンバーが、以下の仕様を満たすことを確認する：

- **値の不変性**：オブジェクト生成後、内部状態が変更されない
- **オプショナル値操作**：Unset() で未設定状態、From(value) で設定状態を作成
- **HasParent プロパティ**：IsSet フラグの代替名。true で親部門あり、false で未設定
- **TryFrom の null 吸収**：null 入力で Unset インスタンスを返す（失敗ではなく正常）
- **等価性比較**：同じ IsSet と値を持つオブジェクトは等価
- **ハッシュ整合性**：Equals=true のオブジェクトは同一ハッシュ値

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | ParentDepartmentRowId |
| **名前空間** | SupportAdvance.Contexts.Department.Domain.ValueObjects |
| **インターフェース** | IOptionalValueObject |
| **依拠仕様** | ValueObject技術仕様書 v1.1 + IOptionalValueObject技術仕様書 v1.3 |
| **テストファイル** | tests/Contexts/Department.Domain.Tests/ValueObjects/ParentDepartmentRowIdTests.cs |

---

## 3. テスト観点一覧

### 観点グループ FRM：From メソッド（正系）

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-FRM-01 | 正の整数で ParentDepartmentRowId が生成される | 正常系 | From_ValidValue_ReturnsInstance |
| VO-FRM-02 | 大きな正の整数でも生成される | 正常系 | From_ValidValue_ReturnsInstance(9999) |

### 観点グループ FRM-ERR：From メソッド（異常系）

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-FRM-ERR-01 | 0 で ArgumentOutOfRangeException がスローされる | 異常系 | From_InvalidValue_ThrowsException(0) |
| VO-FRM-ERR-02 | 負数で ArgumentOutOfRangeException がスローされる | 異常系 | From_InvalidValue_ThrowsException(-1) |

### 観点グループ UNSET：Unset メソッド

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-UNSET-01 | Unset() は HasParent=false のインスタンスを返す | 正常系 | Unset_ReturnsInstance |

### 観点グループ TRY：TryFrom メソッド

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-TRY-01 | 有効な正の整数で TryFrom が true を返す | 正常系 | TryFrom_ValidValue_ReturnsInstance |
| VO-TRY-02 | null で TryFrom が true を返し、Unset を返す（null吸収） | 正常系（null吸収） | TryFrom_Null_ReturnsUnset |
| VO-TRY-03 | 0 で TryFrom が false を返す | 異常系 | TryFrom_InvalidValue_ReturnsFalse(0) |
| VO-TRY-04 | 負数で TryFrom が false を返す | 異常系 | TryFrom_InvalidValue_ReturnsFalse(-1) |

### 観点グループ TRYDB：TryFromDbValue メソッド

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-TRYDB-01 | 有効な正の整数で TryFromDbValue が true を返す | 正常系 | TryFromDbValue_ValidValue_ReturnsTrue |
| VO-TRYDB-02 | DB null で TryFromDbValue が true を返し、Unset を返す | 正常系 | TryFromDbValue_Null_ReturnsUnset |

### 観点グループ PROP：HasParent プロパティ

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-PROP-01 | From() で生成したインスタンスの HasParent は true | 正常系 | HasParent=true のパターン |
| VO-PROP-02 | Unset() で生成したインスタンスの HasParent は false | 正常系 | HasParent=false のパターン |

### 観点グループ EQ：Equals - 等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-EQ-01 | 同じ値・同じ HasParent を持つ2つのオブジェクトは等価 | 正常系 | Equals_SameValues_ReturnsTrue |
| VO-EQ-02 | 両方が Unset（HasParent=false）の場合も等価 | 正常系 | Equals_BothUnset_ReturnsTrue |

### 観点グループ NE：Equals - 非等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-NE-01 | 異なる値を持つオブジェクトは非等価 | 異常系 | Equals_DifferentValues_ReturnsFalse |
| VO-NE-02 | HasParent が異なる場合は非等価（値が同じでも） | 境界値テスト | Equals_DifferentIsSet_ReturnsFalse |

### 観点グループ TS：ToString

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-TS-01 | From() インスタンスの ToString は値の文字列を返す | 正常系 | ToString_WithValue_ReturnsStringRepresentation |
| VO-TS-02 | Unset インスタンスの ToString は "Unset" を返す | 正常系 | ToString_WithUnset_ReturnsUnset |

---

## 4. テスト観点別の検証シナリオ

### 観点 VO-FRM-01 ～ VO-FRM-02：正の整数で生成される

#### 4.1.1 テスト観点

ParentDepartmentRowId.From() に正の整数を渡した場合、
正常なインスタンスが返される。

#### 4.1.2 テストパターン

| パターン | 分類 | 入力値 | 説明 |
|---------|------|--------|------|
| 4.1.2.1 | 正常系 | 100 | 典型的な部門ID |
| 4.1.2.2 | 正常系 | 9999 | 大きな値 |

#### 4.1.3 前提条件

- ParentDepartmentRowId.From() メソッドが利用可能
- Value プロパティが読み取り可能

#### 4.1.4 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.1.2.1 | Value == 100 && HasParent == true | Value が入力値と一致、HasParent は true |
| 4.1.2.2 | Value == 9999 && HasParent == true | Value が入力値と一致、HasParent は true |

#### 4.1.5 判定基準

- [ ] From() が例外をスローしない
- [ ] Value が入力値と一致する
- [ ] HasParent が true である

---

### 観点 VO-FRM-ERR-01 ～ VO-FRM-ERR-02：0以下で例外がスローされる

#### 4.2.1 テスト観点

ParentDepartmentRowId.From() に 0 以下の値を渡した場合、
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

### 観点 VO-UNSET-01：Unset() は HasParent=false のインスタンスを返す

#### 4.3.1 テスト観点

ParentDepartmentRowId.Unset() を呼び出した場合、
HasParent=false のインスタンスが返される。

#### 4.3.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.3.2.1 | 正常系 | Unset() の呼び出し |

#### 4.3.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.3.2.1 | HasParent == false |

#### 4.3.4 判定基準

- [ ] Unset() が例外をスローしない
- [ ] HasParent が false である
- [ ] ToString() が "Unset" を返す

---

### 観点 VO-TRY-01：有効な正の整数で TryFrom が true を返す

#### 4.4.1 テスト観点

ParentDepartmentRowId.TryFrom() に有効な正の整数を渡した場合、
true を返し、out パラメータに設定済みインスタンスが設定される。

#### 4.4.2 テストパターン

| パターン | 分類 | 入力値 |
|---------|------|--------|
| 4.4.2.1 | 正常系 | 50 |

#### 4.4.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.4.2.1 | result == true && parentId.Value == 50 && parentId.HasParent == true |

#### 4.4.4 判定基準

- [ ] TryFrom() が true を返す
- [ ] out パラメータが null でない
- [ ] Value が入力値と一致
- [ ] HasParent が true である

---

### 観点 VO-TRY-02：null で TryFrom が true を返し、Unset を返す（null吸収）

#### 4.5.1 テスト観点

ParentDepartmentRowId.TryFrom(null) を呼び出した場合、
**true を返し（失敗ではなく正常処理）、Unset インスタンスが out パラメータに設定される。**

これが IOptionalValueObject の核となる仕様である。

#### 4.5.2 テストパターン

| パターン | 分類 | 入力値 |
|---------|------|--------|
| 4.5.2.1 | 正常系（null吸収） | null |

#### 4.5.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.5.2.1 | result == true && parentId.HasParent == false |

#### 4.5.4 判定基準

- [ ] TryFrom() が true を返す（例外ではなく正常系）
- [ ] HasParent が false である（Unset状態）
- [ ] ToString() が "Unset" を返す

---

### 観点 VO-TRY-03 ～ VO-TRY-04：0以下で TryFrom が false を返す

#### 4.6.1 テスト観点

ParentDepartmentRowId.TryFrom() に無効な値を渡した場合、
false を返す。例外はスローされない（TryPattern）。

#### 4.6.2 テストパターン

| パターン | 分類 | 入力値 |
|---------|------|--------|
| 4.6.2.1 | 異常系 | 0 |
| 4.6.2.2 | 異常系 | -1 |

#### 4.6.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.6.2.1-2 | result == false |

#### 4.6.4 判定基準

- [ ] TryFrom() が false を返す
- [ ] 例外がスローされない

---

### 観点 VO-EQ-01：同じ値・同じ HasParent を持つ2つのオブジェクトは等価

#### 4.7.1 テスト観点

ParentDepartmentRowId.From(1) で生成した2つのインスタンスが Equals で等価と判定される。

#### 4.7.2 テストパターン

| パターン | 分類 | value1 | value2 |
|---------|------|--------|--------|
| 4.7.2.1 | 正常系 | 1 | 1 |

#### 4.7.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.7.2.1 | parentId1.Equals(parentId2) == true |

#### 4.7.4 判定基準

- [ ] Equals() が true を返す

---

### 観点 VO-EQ-02：両方が Unset（HasParent=false）の場合も等価

#### 4.8.1 テスト観点

ParentDepartmentRowId.Unset() で生成した2つのインスタンスが Equals で等価と判定される。

#### 4.8.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.8.2.1 | 正常系 | 両方が Unset |

#### 4.8.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.8.2.1 | unset1.Equals(unset2) == true |

#### 4.8.4 判定基準

- [ ] Equals() が true を返す

---

### 観点 VO-NE-02：HasParent が異なる場合は非等価（値が同じでも）

#### 4.9.1 テスト観点

ParentDepartmentRowId.From(1) と ParentDepartmentRowId.Unset() は、
HasParent が異なるため非等価と判定される（値の比較だけでなく IsSet も比較）。

#### 4.9.2 テストパターン

| パターン | 分類 | obj1 | obj2 |
|---------|------|------|------|
| 4.9.2.1 | 境界値テスト | From(1) | Unset() |

#### 4.9.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.9.2.1 | obj1.Equals(obj2) == false |

#### 4.9.4 判定基準

- [ ] Equals() が false を返す
- [ ] GetHashCode() も異なる値を返す

---

### 観点 VO-TS-01：ToString は値の文字列を返す

#### 4.10.1 テスト観点

ParentDepartmentRowId.From(1) の ToString() が値の文字列を返す。

#### 4.10.2 テストパターン

| パターン | 分類 | 入力値 |
|---------|------|--------|
| 4.10.2.1 | 正常系 | 1 |

#### 4.10.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.10.2.1 | ToString() == "1" |

#### 4.10.4 判定基準

- [ ] ToString() が値の文字列を返す

---

### 観点 VO-TS-02：Unset インスタンスの ToString は "Unset" を返す

#### 4.11.1 テスト観点

ParentDepartmentRowId.Unset() の ToString() が "Unset" を返す。

#### 4.11.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.11.2.1 | 正常系 | Unset状態 |

#### 4.11.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.11.2.1 | ToString() == "Unset" |

#### 4.11.4 判定基準

- [ ] ToString() が正確に "Unset" を返す

---

## 5. 判定基準

### 5.1 全般的な判定基準

| 項目 | 基準 |
|------|------|
| **不変性** | オブジェクト生成後、Value/HasParent が変更されないこと |
| **等価性** | Equals で IsSet と Value の両方を比較すること |
| **ハッシュ整合性** | Equals=true のペアが同一ハッシュ値を返すこと |
| **null吸収** | TryFrom(null) が true + Unset を返すこと |
| **値操作安全性** | TryFrom/TryFromDbValue が例外ではなく bool で結果を示すこと |
| **HasParent プロパティ** | IsSet を Business ロジックに適した名前で公開すること |

---

## 6. テスト実装ガイド

### テストコード例

```csharp
// 正常系テスト
[Fact]
public void From_ValidValue_ReturnsInstance()
{
    var parentId = ParentDepartmentRowId.From(100);
    Assert.True(parentId.HasParent);
    Assert.Equal(100, parentId.Value);
}

// 異常系テスト（From）
[Fact]
public void From_InvalidValue_ThrowsException()
{
    Assert.Throws<ArgumentOutOfRangeException>(() => ParentDepartmentRowId.From(0));
    Assert.Throws<ArgumentOutOfRangeException>(() => ParentDepartmentRowId.From(-1));
}

// Unset テスト
[Fact]
public void Unset_ReturnsInstance()
{
    var parentId = ParentDepartmentRowId.Unset();
    Assert.False(parentId.HasParent);
}

// TryFrom テスト
[Fact]
public void TryFrom_ValidValue_ReturnsInstance()
{
    var result = ParentDepartmentRowId.TryFrom(50, out var parentId);
    Assert.True(result);
    Assert.Equal(50, parentId.Value);
}

// null吸収テスト（IOptionalValueObject の核）
[Fact]
public void TryFrom_Null_ReturnsUnset()
{
    var result = ParentDepartmentRowId.TryFrom(null, out var parentId);
    Assert.True(result);          // true を返す（失敗ではない）
    Assert.False(parentId.HasParent);  // Unset状態
}

// TryFromDbValue テスト
[Fact]
public void TryFromDbValue_Null_ReturnsUnset()
{
    var result = ParentDepartmentRowId.TryFromDbValue(null, out var parentId);
    Assert.True(result);
    Assert.False(parentId.HasParent);
}

// 等価性テスト
[Fact]
public void Equals_SameValues_ReturnsTrue()
{
    var id1 = ParentDepartmentRowId.From(100);
    var id2 = ParentDepartmentRowId.From(100);
    Assert.Equal(id1, id2);
}

// HasParent 異なるテスト
[Fact]
public void Equals_DifferentIsSet_ReturnsFalse()
{
    var id1 = ParentDepartmentRowId.From(100);
    var id2 = ParentDepartmentRowId.Unset();
    Assert.NotEqual(id1, id2);
}
```

---

## 7. 改版履歴

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-09-16 | 初版作成。テンプレート準拠、IOptionalValueObject実装、実装コード検証済み |

---

## 8. 参考資料

- ValueObject 技術仕様書 v1.1
- IOptionalValueObject 技術仕様書 v1.3
- テンプレート：`docs/Assistance/Templates/Test/TEMPLATE_単体テスト仕様書_ValueObject.md`
- 実装：`tests/Contexts/Department.Domain.Tests/ValueObjects/ParentDepartmentRowIdTests.cs`
