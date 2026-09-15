# DepartmentCode 単体テスト仕様書

**プロジェクト:** SupportAdvance  
**テスト対象:** DepartmentCode ValueObject  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-09-16

---

## 0. 本書の位置づけ

本書は、Domain層の ValueObject クラス `DepartmentCode` が、設計仕様で定義された
**値の不変性・等価性比較・ハッシュ整合性・文字列化・検証（4文字英数字）** を満たすことを確認するテスト仕様書。

---

## 1. テスト目的

`DepartmentCode` の各メンバーが、以下の仕様を満たすことを確認する：

- **値の不変性**：オブジェクト生成後、内部状態が変更されない
- **等価性比較**：同じ値を持つ2つのオブジェクトは等価である
- **ハッシュ整合性**：Equals=true のオブジェクトは同一ハッシュ値
- **文字列化**：ToString が仕様通りの形式で返される
- **検証**：4文字の英数字のみを許容、それ以外は例外またはTryFromで失敗

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | DepartmentCode |
| **名前空間** | SupportAdvance.Contexts.Department.Domain.ValueObjects |
| **依拠仕様** | ValueObject技術仕様書 v1.1 |
| **テストファイル** | tests/Contexts/Department.Domain.Tests/ValueObjects/DepartmentCodeTests.cs |

---

## 3. テスト観点一覧

### 観点グループ FRM：From メソッド（正系）

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-FRM-01 | 4文字の英数字入力で DepartmentCode が生成される | 正常系 | From_ValidCode_ReturnsInstance |
| VO-FRM-02 | 異なる4文字コードで複数のインスタンスが生成される | 正常系 | 複数パターン（ABCD, TEST など） |

### 観点グループ FRM-ERR：From メソッド（異常系）

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-FRM-ERR-01 | 3文字以下で ArgumentException がスローされる | 異常系 | From_InvalidLength_ThrowsException |
| VO-FRM-ERR-02 | 5文字以上で ArgumentException がスローされる | 異常系 | From_InvalidLength_ThrowsException |
| VO-FRM-ERR-03 | 英数字以外（特殊文字）で ArgumentException がスローされる | 異常系 | From_NonAlphanumeric_ThrowsException |
| VO-FRM-ERR-04 | 空白文字を含む場合 ArgumentException がスローされる | 異常系 | From_NonAlphanumeric_ThrowsException |
| VO-FRM-ERR-05 | null 入力で ArgumentException がスローされる | 異常系 | From に null 渡す場合 |

### 観点グループ TRY：TryFrom メソッド

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-TRY-01 | 有効な4文字で TryFrom が true を返す | 正常系 | TryFrom_ValidCode_ReturnsTrue |
| VO-TRY-02 | 無効な長さで TryFrom が false を返す | 異常系 | TryFrom_InvalidCode_ReturnsFalse |
| VO-TRY-03 | null で TryFrom が false を返す | 異常系 | TryFrom_Null_ReturnsFalse |

### 観点グループ EQ：Equals - 等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-EQ-01 | 同じ値を持つ2つのオブジェクトは等価である | 正常系 | Equals_SameValue_ReturnsTrue |

### 観点グループ NE：Equals - 非等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-NE-01 | 異なる値を持つオブジェクトは非等価である | 異常系 | Equals_DifferentValue_ReturnsFalse |

### 観点グループ HC：GetHashCode

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-HC-01 | Equals=true のオブジェクトペアは同一ハッシュ値を返す | 正常系 | 実装コードから確認 |

### 観点グループ TS：ToString

| 観点ID | 観点（説明） | 分類 | テストケース |
|--------|------|------|------------|
| VO-TS-01 | ToString が入力値と同じ文字列を返す | 正常系 | ToString_ReturnsValue |

---

## 4. テスト観点別の検証シナリオ

### 観点 VO-FRM-01：4文字の英数字入力で DepartmentCode が生成される

#### 4.1.1 テスト観点

DepartmentCode.From() に4文字の英数字（大文字・小文字・数字の組み合わせ）を渡した場合、
正常なインスタンスが返される。

#### 4.1.2 テストパターン

| パターン | 分類 | 入力例 | 説明 |
|---------|------|--------|------|
| 4.1.2.1 | 正常系 | "ABCD" | 大文字4文字 |
| 4.1.2.2 | 正常系 | "TEST" | 異なる大文字 |
| 4.1.2.3 | 正常系 | "1234" | 数字4個 |
| 4.1.2.4 | 正常系 | "AbC1" | 大文字・小文字・数字混在 |

#### 4.1.3 前提条件

- DepartmentCode.From() メソッドが利用可能
- Value プロパティが読み取り可能

#### 4.1.4 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.1.2.1 | code.Value == "ABCD" | Value プロパティが入力値と一致 |
| 4.1.2.2 | code.Value == "TEST" | Value プロパティが入力値と一致 |
| 4.1.2.3 | code.Value == "1234" | Value プロパティが入力値と一致 |
| 4.1.2.4 | code.Value == "AbC1" | Value プロパティが入力値と一致 |

#### 4.1.5 判定基準

- [ ] From() メソッドが例外をスローしない
- [ ] 返却オブジェクトが null でない
- [ ] Value プロパティが入力値と一致する

---

### 観点 VO-FRM-ERR-01：3文字以下で ArgumentException がスローされる

#### 4.2.1 テスト観点

DepartmentCode.From() に3文字以下の文字列を渡した場合、ArgumentException がスローされる。

#### 4.2.2 テストパターン

| パターン | 分類 | 入力例 | 説明 |
|---------|------|--------|------|
| 4.2.2.1 | 異常系 | "ABC" | 3文字 |
| 4.2.2.2 | 異常系 | "AB" | 2文字 |
| 4.2.2.3 | 異常系 | "A" | 1文字 |
| 4.2.2.4 | 異常系 | "" | 空文字列 |

#### 4.2.3 期待結果

| パターン | 期待動作 |
|---------|---------|
| 4.2.2.1-4 | ArgumentException がスローされる |

#### 4.2.4 判定基準

- [ ] ArgumentException が発生する
- [ ] 例外メッセージが適切である

---

### 観点 VO-FRM-ERR-02：5文字以上で ArgumentException がスローされる

#### 4.3.1 テスト観点

DepartmentCode.From() に5文字以上の文字列を渡した場合、ArgumentException がスローされる。

#### 4.3.2 テストパターン

| パターン | 分類 | 入力例 | 説明 |
|---------|------|--------|------|
| 4.3.2.1 | 異常系 | "ABCDE" | 5文字 |
| 4.3.2.2 | 異常系 | "ABCDEF" | 6文字以上 |

#### 4.3.3 期待結果

| パターン | 期待動作 |
|---------|---------|
| 4.3.2.1-2 | ArgumentException がスローされる |

#### 4.3.4 判定基準

- [ ] ArgumentException が発生する

---

### 観点 VO-FRM-ERR-03：英数字以外（特殊文字）で ArgumentException がスローされる

#### 4.4.1 テスト観点

DepartmentCode.From() に特殊文字（-, @, 空白など）を含む文字列を渡した場合、
ArgumentException がスローされる。

#### 4.4.2 テストパターン

| パターン | 分類 | 入力例 | 説明 |
|---------|------|--------|------|
| 4.4.2.1 | 異常系 | "AB-D" | ハイフン |
| 4.4.2.2 | 異常系 | "AB@D" | @記号 |
| 4.4.2.3 | 異常系 | "AB CD" | 空白 |

#### 4.4.3 期待結果

| パターン | 期待動作 |
|---------|---------|
| 4.4.2.1-3 | ArgumentException がスローされる |

#### 4.4.4 判定基準

- [ ] ArgumentException が発生する

---

### 観点 VO-TRY-01：有効な4文字で TryFrom が true を返す

#### 4.5.1 テスト観点

DepartmentCode.TryFrom() に有効な4文字を渡した場合、true を返し、
out パラメータに正常なインスタンスが設定される。

#### 4.5.2 テストパターン

| パターン | 分類 | 入力例 |
|---------|------|--------|
| 4.5.2.1 | 正常系 | "TEST" |

#### 4.5.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.5.2.1 | result == true && code.Value == "TEST" |

#### 4.5.4 判定基準

- [ ] TryFrom() が true を返す
- [ ] out パラメータが null でない
- [ ] Value が入力値と一致

---

### 観点 VO-TRY-02：無効な長さで TryFrom が false を返す

#### 4.6.1 テスト観点

DepartmentCode.TryFrom() に無効な長さの文字列を渡した場合、false を返す。
例外はスローされない（TryPattern）。

#### 4.6.2 テストパターン

| パターン | 分類 | 入力例 |
|---------|------|--------|
| 4.6.2.1 | 異常系 | "INVALID"（7文字） |

#### 4.6.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.6.2.1 | result == false |

#### 4.6.4 判定基準

- [ ] TryFrom() が false を返す
- [ ] 例外がスローされない

---

### 観点 VO-TRY-03：null で TryFrom が false を返す

#### 4.7.1 テスト観点

DepartmentCode.TryFrom() に null を渡した場合、false を返す。

#### 4.7.2 テストパターン

| パターン | 分類 | 入力値 |
|---------|------|--------|
| 4.7.2.1 | 異常系 | null |

#### 4.7.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.7.2.1 | result == false |

#### 4.7.4 判定基準

- [ ] TryFrom() が false を返す

---

### 観点 VO-EQ-01：同じ値を持つ2つのオブジェクトは等価である

#### 4.8.1 テスト観点

DepartmentCode.From("ABCD") で生成した2つのインスタンスが Equals で等価と判定される。

#### 4.8.2 テストパターン

| パターン | 分類 | code1 | code2 |
|---------|------|--------|--------|
| 4.8.2.1 | 正常系 | "ABCD" | "ABCD" |

#### 4.8.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.8.2.1 | code1.Equals(code2) == true |

#### 4.8.4 判定基準

- [ ] Equals() が true を返す

---

### 観点 VO-NE-01：異なる値を持つオブジェクトは非等価である

#### 4.9.1 テスト観点

DepartmentCode.From("ABCD") と DepartmentCode.From("EFGH") が Equals で非等価と判定される。

#### 4.9.2 テストパターン

| パターン | 分類 | code1 | code2 |
|---------|------|--------|--------|
| 4.9.2.1 | 異常系 | "ABCD" | "EFGH" |

#### 4.9.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.9.2.1 | code1.Equals(code2) == false |

#### 4.9.4 判定基準

- [ ] Equals() が false を返す

---

### 観点 VO-TS-01：ToString が入力値と同じ文字列を返す

#### 4.10.1 テスト観点

DepartmentCode の ToString() が Value と同じ文字列を返す。

#### 4.10.2 テストパターン

| パターン | 分類 | 入力値 |
|---------|------|--------|
| 4.10.2.1 | 正常系 | "ABCD" |

#### 4.10.3 期待結果

| パターン | 期待値 |
|---------|--------|
| 4.10.2.1 | code.ToString() == "ABCD" |

#### 4.10.4 判定基準

- [ ] ToString() が正確に入力値を返す

---

## 5. 判定基準

### 5.1 全般的な判定基準

| 項目 | 基準 |
|------|------|
| **不変性** | オブジェクト生成後、Value が変更されないこと |
| **等価性** | Equals の結果が論理的に一貫していること |
| **ハッシュ整合性** | Equals=true のペアが同一ハッシュ値を返すこと |
| **検証** | From で無効値をエラーハンドリング、TryFrom でエラーを bool で返すこと |
| **文字列化** | ToString が Value と同じ文字列を返すこと |

---

## 6. テスト実装ガイド

### テストコード例

```csharp
// 正常系テスト
[Fact]
public void From_ValidCode_ReturnsInstance()
{
    var code = DepartmentCode.From("ABCD");
    Assert.Equal("ABCD", code.Value);
}

// 異常系テスト（From）
[Fact]
public void From_InvalidLength_ThrowsException()
{
    Assert.Throws<ArgumentException>(() => DepartmentCode.From("ABC"));
    Assert.Throws<ArgumentException>(() => DepartmentCode.From("ABCDE"));
}

// TryFrom テスト
[Fact]
public void TryFrom_ValidCode_ReturnsTrue()
{
    var result = DepartmentCode.TryFrom("TEST", out var code);
    Assert.True(result);
    Assert.Equal("TEST", code.Value);
}

// TryFrom 失敗テスト
[Fact]
public void TryFrom_InvalidCode_ReturnsFalse()
{
    var result = DepartmentCode.TryFrom("INVALID", out _);
    Assert.False(result);
}

// 等価性テスト
[Fact]
public void Equals_SameValue_ReturnsTrue()
{
    var code1 = DepartmentCode.From("ABCD");
    var code2 = DepartmentCode.From("ABCD");
    Assert.Equal(code1, code2);
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
- 実装：`tests/Contexts/Department.Domain.Tests/ValueObjects/DepartmentCodeTests.cs`
