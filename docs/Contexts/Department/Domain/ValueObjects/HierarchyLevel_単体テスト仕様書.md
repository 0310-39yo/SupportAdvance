# HierarchyLevel 単体テスト仕様書

**プロジェクト:** SupportAdvance  
**テスト対象:** HierarchyLevel ValueObject  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-09-16

---

## 0. 本書の位置づけ

本書は、Domain層の ValueObject クラス `HierarchyLevel` が、設計仕様で定義された
**値の不変性・等価性比較・範囲検証（0-4）・ファクトリメソッド・文字列化** を満たすことを確認するテスト仕様書。

---

## 1. テスト目的

`HierarchyLevel` の各メンバーが、以下の仕様を満たすことを確認する：

- **値の不変性**：オブジェクト生成後、内部状態が変更されない
- **等価性比較**：同じ値を持つ2つのオブジェクトは等価である
- **ハッシュ整合性**：Equals=true のオブジェクトは同一ハッシュ値
- **範囲検証**：0-4（Company, Division, Department, Group, Team）のみ許容
- **ファクトリメソッド**：Company(), Division(), Department(), Group(), Team() が各レベルを返す
- **文字列化**：ToString が英文字（Company, Division など）を返す

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | HierarchyLevel |
| **名前空間** | SupportAdvance.Contexts.Department.Domain.ValueObjects |
| **依拠仕様** | ValueObject技術仕様書 v1.1 |
| **テストファイル** | tests/Contexts/Department.Domain.Tests/ValueObjects/HierarchyLevelTests.cs |

---

## 3. テスト観点一覧

### 観点グループ FRM：From メソッド（正系）

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-FRM-01 | レベル 0（Company） で HierarchyLevel が生成される | 正常系 | From_ValidLevels_ReturnsInstance(0) |
| VO-FRM-02 | レベル 1（Division） で HierarchyLevel が生成される | 正常系 | From_ValidLevels_ReturnsInstance(1) |
| VO-FRM-03 | レベル 2（Department） で HierarchyLevel が生成される | 正常系 | From_ValidLevels_ReturnsInstance(2) |
| VO-FRM-04 | レベル 3（Group） で HierarchyLevel が生成される | 正常系 | From_ValidLevels_ReturnsInstance(3) |
| VO-FRM-05 | レベル 4（Team） で HierarchyLevel が生成される | 正常系 | From_ValidLevels_ReturnsInstance(4) |

### 観点グループ FRM-ERR：From メソッド（異常系）

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-FRM-ERR-01 | 負数で ArgumentOutOfRangeException がスローされる | 異常系 | From_InvalidLevel_ThrowsException(-1) |
| VO-FRM-ERR-02 | 5以上で ArgumentOutOfRangeException がスローされる | 異常系 | From_InvalidLevel_ThrowsException(5) |

### 観点グループ TRY：TryFrom メソッド

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-TRY-01 | 有効なレベルで TryFrom が true を返す | 正常系 | TryFrom_ValidLevels_ReturnsTrue(0-4) |
| VO-TRY-02 | 負数で TryFrom が false を返す | 異常系 | TryFrom_InvalidLevels_ReturnsFalse(-1) |
| VO-TRY-03 | 5以上で TryFrom が false を返す | 異常系 | TryFrom_InvalidLevels_ReturnsFalse(5) |

### 観点グループ FAC：ファクトリメソッド

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-FAC-01 | Company() がレベル 0 を返す | 正常系 | Company_ReturnsLevel0 |
| VO-FAC-02 | Division() がレベル 1 を返す | 正常系 | Division_ReturnsLevel1 |
| VO-FAC-03 | Department() がレベル 2 を返す | 正常系 | Department_ReturnsLevel2 |
| VO-FAC-04 | Group() がレベル 3 を返す | 正常系 | Group_ReturnsLevel3() |
| VO-FAC-05 | Team() がレベル 4 を返す | 正常系 | Team_ReturnsLevel4() |

### 観点グループ TS：ToString

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-TS-01 | レベル 0 で "Company" を返す | 正常系 | ToString_With0_ReturnsCompany |
| VO-TS-02 | レベル 1 で "Division" を返す | 正常系 | ToString_With1_ReturnsDivision |
| VO-TS-03 | レベル 2 で "Department" を返す | 正常系 | ToString_With2_ReturnsDepartment |
| VO-TS-04 | レベル 3 で "Group" を返す | 正常系 | ToString_With3_ReturnsGroup |
| VO-TS-05 | レベル 4 で "Team" を返す | 正常系 | ToString_With4_ReturnsTeam |

### 観点グループ EQ：Equals - 等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-EQ-01 | 同じレベルを持つ2つのオブジェクトは等価である | 正常系 | Equals_SameValue_ReturnsTrue |

### 観点グループ NE：Equals - 非等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-NE-01 | 異なるレベルを持つオブジェクトは非等価である | 異常系 | Equals_DifferentValue_ReturnsFalse |

---

## 4. テスト観点別の検証シナリオ

### 観点 VO-FRM-01 ～ VO-FRM-05：0-4の各レベルで生成される

#### 4.1.1 テスト観点

HierarchyLevel.From() に 0 ～ 4 の範囲内の整数を渡した場合、
正常なインスタンスが返される。

#### 4.1.2 テストパターン

| パターン | 分類 | 入力値 | 意味 | 期待値 |
|---------|------|--------|------|--------|
| 4.1.2.1 | 正常系 | 0 | Company | Value == 0 |
| 4.1.2.2 | 正常系 | 1 | Division | Value == 1 |
| 4.1.2.3 | 正常系 | 2 | Department | Value == 2 |
| 4.1.2.4 | 正常系 | 3 | Group | Value == 3 |
| 4.1.2.5 | 正常系 | 4 | Team | Value == 4 |

#### 4.1.3 前提条件

- HierarchyLevel.From() メソッドが利用可能
- Value プロパティが読み取り可能

#### 4.1.4 期待結果

すべてのパターンで、From() が例外をスローせず、
返却オブジェクトの Value が入力値と一致する。

#### 4.1.5 判定基準

- [ ] From() が例外をスローしない
- [ ] Value が入力値と一致する
- [ ] IsSet は常に true

---

### 観点 VO-FRM-ERR-01 ～ VO-FRM-ERR-02：範囲外で例外がスローされる

#### 4.2.1 テスト観点

HierarchyLevel.From() に 0 未満または 4 より大きい値を渡した場合、
ArgumentOutOfRangeException がスローされる。

#### 4.2.2 テストパターン

| パターン | 分類 | 入力値 | 説明 |
|---------|------|--------|------|
| 4.2.2.1 | 異常系 | -1 | 負数 |
| 4.2.2.2 | 異常系 | -100 | 大きな負数 |
| 4.2.2.3 | 異常系 | 5 | 上限超過 |
| 4.2.2.4 | 異常系 | 100 | 大きな正数 |

#### 4.2.3 期待結果

| パターン | 期待動作 |
|---------|---------|
| 4.2.2.1-4 | ArgumentOutOfRangeException がスローされる |

#### 4.2.4 判定基準

- [ ] ArgumentOutOfRangeException が発生する
- [ ] 例外メッセージが適切である

---

### 観点 VO-TRY-01：有効なレベルで TryFrom が true を返す

#### 4.3.1 テスト観点

HierarchyLevel.TryFrom() に有効なレベル（0-4）を渡した場合、
true を返し、out パラメータに正常なインスタンスが設定される。

#### 4.3.2 テストパターン

| パターン | 分類 | 入力値 |
|---------|------|--------|
| 4.3.2.1 | 正常系 | 0 |
| 4.3.2.2 | 正常系 | 4 |

#### 4.3.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.3.2.1-2 | result == true && level.Value == 入力値 |

#### 4.3.4 判定基準

- [ ] TryFrom() が true を返す
- [ ] out パラメータが null でない
- [ ] Value が入力値と一致

---

### 観点 VO-TRY-02 ～ VO-TRY-03：範囲外で TryFrom が false を返す

#### 4.4.1 テスト観点

HierarchyLevel.TryFrom() に無効なレベルを渡した場合、
false を返す。例外はスローされない（TryPattern）。

#### 4.4.2 テストパターン

| パターン | 分類 | 入力値 |
|---------|------|--------|
| 4.4.2.1 | 異常系 | -1 |
| 4.4.2.2 | 異常系 | 5 |

#### 4.4.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.4.2.1-2 | result == false |

#### 4.4.4 判定基準

- [ ] TryFrom() が false を返す
- [ ] 例外がスローされない

---

### 観点 VO-FAC-01 ～ VO-FAC-05：ファクトリメソッドが各レベルを返す

#### 4.5.1 テスト観点

HierarchyLevel.Company(), Division(), Department(), Group(), Team() が
各レベルを返す。

#### 4.5.2 テストパターン

| メソッド | 期待レベル |
|---------|---------|
| Company() | 0 |
| Division() | 1 |
| Department() | 2 |
| Group() | 3 |
| Team() | 4 |

#### 4.5.3 期待結果

| メソッド | 期待値 |
|---------|--------|
| Company() | Value == 0 |
| Division() | Value == 1 |
| Department() | Value == 2 |
| Group() | Value == 3 |
| Team() | Value == 4 |

#### 4.5.4 判定基準

- [ ] 各ファクトリメソッドが対応するレベルを返す

---

### 観点 VO-TS-01 ～ VO-TS-05：ToString が英文字を返す

#### 4.6.1 テスト観点

HierarchyLevel.ToString() が、レベルに対応する英文字（Company, Division など）
を返す。

#### 4.6.2 テストパターン

| レベル | 期待文字列 |
|--------|---------|
| 0 | "Company" |
| 1 | "Division" |
| 2 | "Department" |
| 3 | "Group" |
| 4 | "Team" |

#### 4.6.3 期待結果

各レベルで ToString() が対応する英文字を返す。

#### 4.6.4 判定基準

- [ ] ToString() が正確に英文字を返す
- [ ] 大文字小文字が正確に一致

---

### 観点 VO-EQ-01：同じレベルを持つ2つのオブジェクトは等価である

#### 4.7.1 テスト観点

HierarchyLevel.From(2) で生成した2つのインスタンスが Equals で等価と判定される。

#### 4.7.2 テストパターン

| パターン | 分類 | level1 | level2 |
|---------|------|--------|--------|
| 4.7.2.1 | 正常系 | 2 | 2 |

#### 4.7.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.7.2.1 | level1.Equals(level2) == true |

#### 4.7.4 判定基準

- [ ] Equals() が true を返す

---

### 観点 VO-NE-01：異なるレベルを持つオブジェクトは非等価である

#### 4.8.1 テスト観点

HierarchyLevel.From(2) と HierarchyLevel.From(3) が Equals で非等価と判定される。

#### 4.8.2 テストパターン

| パターン | 分類 | level1 | level2 |
|---------|------|--------|--------|
| 4.8.2.1 | 異常系 | 2 | 3 |

#### 4.8.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.8.2.1 | level1.Equals(level2) == false |

#### 4.8.4 判定基準

- [ ] Equals() が false を返す

---

## 5. 判定基準

### 5.1 全般的な判定基準

| 項目 | 基準 |
|------|------|
| **不変性** | オブジェクト生成後、Value が変更されないこと |
| **等価性** | Equals の結果が論理的に一貫していること |
| **ハッシュ整合性** | Equals=true のペアが同一ハッシュ値を返すこと |
| **範囲検証** | From で 0-4 の範囲をチェック、範囲外で例外またはTryFromで失敗 |
| **ファクトリメソッド** | Company(), Division() など各レベルを返すこと |
| **文字列化** | ToString が英文字を返すこと |

---

## 6. テスト実装ガイド

### テストコード例

```csharp
// 正常系テスト（各レベル）
[Theory]
[InlineData(0)]
[InlineData(1)]
[InlineData(2)]
[InlineData(3)]
[InlineData(4)]
public void From_ValidLevels_ReturnsInstance(int level)
{
    var hierarchyLevel = HierarchyLevel.From(level);
    Assert.Equal(level, hierarchyLevel.Value);
}

// 異常系テスト（範囲外）
[Fact]
public void From_InvalidLevel_ThrowsException()
{
    Assert.Throws<ArgumentOutOfRangeException>(() => HierarchyLevel.From(-1));
    Assert.Throws<ArgumentOutOfRangeException>(() => HierarchyLevel.From(5));
}

// ファクトリメソッドテスト
[Fact]
public void Company_ReturnsLevel0()
{
    var level = HierarchyLevel.Company();
    Assert.Equal(0, level.Value);
}

// 等価性テスト
[Fact]
public void Equals_SameValue_ReturnsTrue()
{
    var level1 = HierarchyLevel.From(2);
    var level2 = HierarchyLevel.From(2);
    Assert.Equal(level1, level2);
}

// 非等価性テスト
[Fact]
public void Equals_DifferentValue_ReturnsFalse()
{
    var level1 = HierarchyLevel.From(2);
    var level2 = HierarchyLevel.From(3);
    Assert.NotEqual(level1, level2);
}
```

---

## 7. 改版履歴

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-09-16 | 初版作成。テンプレート準拠、実装コード検証済み |

---

## 8. 参考資料

- ValueObject 技術仕様書 v1.1
- テンプレート：`docs/Assistance/Templates/Test/TEMPLATE_単体テスト仕様書_ValueObject.md`
- 実装：`tests/Contexts/Department.Domain.Tests/ValueObjects/HierarchyLevelTests.cs`
