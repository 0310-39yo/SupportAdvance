# EmployeeRowId - 単体テスト仕様書

**対象者**: テスト実装者（EmployeeRowId の単体テストを実装）

**版**: 1.0  
**作成日**: 2026-08-09

---

## 📖 概要

このドキュメントは EmployeeRowId ValueObject の単体テストの仕様を定義します。

各テストは以下の観点をカバーします：
- **生成メソッド（From）**: 正常系・異常系
- **安全な生成（TryFrom）**: null安全、異常系
- **DB値変換（TryFromDbValue）**: DB値からの復元
- **等価性（Equality）**: ValueObject の等価性
- **表示形式（Display）**: ToString の正確性
- **不変性（Immutability）**: 値の変更不可

---

## 🧪 テストケース一覧

### グループ 1: 生成メソッド（From）

#### ER-GEN-01: 正常値で生成成功

```gherkin
When: EmployeeRowId.From(1L) を呼び出す
Then: EmployeeRowId が生成される
And:  Value は 1L である
```

#### ER-GEN-02: 大きな正の値で生成成功

```gherkin
When: EmployeeRowId.From(9223372036854775807L) を呼び出す（long.MaxValue）
Then: EmployeeRowId が生成される
And:  Value は 9223372036854775807L である
```

#### ER-GEN-03: 典型値で生成成功

```gherkin
When: EmployeeRowId.From(12345L) を呼び出す
Then: EmployeeRowId が生成される
And:  Value は 12345L である
```

#### ER-GEN-04: ゼロで生成失敗

```gherkin
When: EmployeeRowId.From(0L) を呼び出す
Then: ArgumentOutOfRangeException が発生する
```

#### ER-GEN-05: 負の値で生成失敗

```gherkin
When: EmployeeRowId.From(-1L) を呼び出す
Then: ArgumentOutOfRangeException が発生する
```

#### ER-GEN-06: 大きな負の値で生成失敗

```gherkin
When: EmployeeRowId.From(long.MinValue) を呼び出す
Then: ArgumentOutOfRangeException が発生する
```

#### ER-GEN-07: 複数の有効値で生成成功（Data-driven）

```gherkin
When: 以下の値で EmployeeRowId.From を呼び出す:
      1, 100, 1000, 10000, 100000, 1000000
Then: 各値について EmployeeRowId が生成される
And:  Value はそれぞれの入力値である
```

#### ER-GEN-08: 複数の無効値で生成失敗（Data-driven）

```gherkin
When: 以下の値で EmployeeRowId.From を呼び出す:
      0, -1, -100, -1000
Then: 各値について ArgumentOutOfRangeException が発生する
```

### グループ 2: 安全な生成（TryFrom）

#### ER-TRY-01: 正常値で成功

```gherkin
When: EmployeeRowId.TryFrom(1L, out var result) を呼び出す
Then: 戻り値は true である
And:  result.Value は 1L である
```

#### ER-TRY-02: 大きな正の値で成功

```gherkin
When: EmployeeRowId.TryFrom(long.MaxValue, out var result) を呼び出す
Then: 戻り値は true である
And:  result.Value は long.MaxValue である
```

#### ER-TRY-03: ゼロで失敗

```gherkin
When: EmployeeRowId.TryFrom(0L, out var result) を呼び出す
Then: 戻り値は false である
```

#### ER-TRY-04: 負の値で失敗

```gherkin
When: EmployeeRowId.TryFrom(-1L, out var result) を呼び出す
Then: 戻り値は false である
```

#### ER-TRY-05: 複数の有効値で成功（Data-driven）

```gherkin
When: 以下の値で TryFrom を呼び出す:
      1, 100, 1000, 10000
Then: 各値について true が返される
And:  result.Value はそれぞれの入力値である
```

#### ER-TRY-06: 複数の無効値で失敗（Data-driven）

```gherkin
When: 以下の値で TryFrom を呼び出す:
      0, -1, -100
Then: 各値について false が返される
```

### グループ 3: DB値変換（TryFromDbValue）

#### ER-DBVAL-01: DB値で成功

```gherkin
When: EmployeeRowId.TryFromDbValue(12345L, out var result) を呼び出す
Then: 戻り値は true である
And:  result.Value は 12345L である
```

#### ER-DBVAL-02: 最小有効値で成功

```gherkin
When: EmployeeRowId.TryFromDbValue(1L, out var result) を呼び出す
Then: 戻り値は true である
And:  result.Value は 1L である
```

#### ER-DBVAL-03: 最大値で成功

```gherkin
When: EmployeeRowId.TryFromDbValue(long.MaxValue, out var result) を呼び出す
Then: 戻り値は true である
And:  result.Value は long.MaxValue である
```

#### ER-DBVAL-04: ゼロで失敗

```gherkin
When: EmployeeRowId.TryFromDbValue(0L, out var result) を呼び出す
Then: 戻り値は false である
```

#### ER-DBVAL-05: 負の値で失敗

```gherkin
When: EmployeeRowId.TryFromDbValue(-1L, out var result) を呼び出す
Then: 戻り値は false である
```

### グループ 4: 等価性（Equality）

#### ER-EQ-01: 同じ値で等価

```gherkin
When: 2つの EmployeeRowId を生成:
      var rowId1 = EmployeeRowId.From(12345L)
      var rowId2 = EmployeeRowId.From(12345L)
Then: rowId1.Equals(rowId2) は true である
And:  rowId1 == rowId2 は true である
```

#### ER-EQ-02: 異なる値で非等価

```gherkin
When: 2つの異なる EmployeeRowId を生成:
      var rowId1 = EmployeeRowId.From(12345L)
      var rowId2 = EmployeeRowId.From(67890L)
Then: rowId1.Equals(rowId2) は false である
And:  rowId1 != rowId2 は true である
```

#### ER-EQ-03: オブジェクト等価（object）

```gherkin
When: 2つの同じ値の EmployeeRowId を object.Equals で比較:
      var rowId1 = EmployeeRowId.From(12345L)
      object obj = EmployeeRowId.From(12345L)
Then: rowId1.Equals(obj) は true である
```

#### ER-EQ-04: ハッシュコードが一致

```gherkin
When: 同じ値の 2つの EmployeeRowId を生成:
      var rowId1 = EmployeeRowId.From(12345L)
      var rowId2 = EmployeeRowId.From(12345L)
Then: rowId1.GetHashCode() == rowId2.GetHashCode() は true である
```

#### ER-EQ-05: ディクショナリで使用可能

```gherkin
When: EmployeeRowId をディクショナリのキーとして使用:
      var dict = new Dictionary<EmployeeRowId, string>()
      dict.Add(EmployeeRowId.From(12345L), "Employee123")
      dict.Add(EmployeeRowId.From(12345L), "Employee124")
Then: ディクショナリは 1 つのキーのみ保持する
And:  最後の値 "Employee124" が取得される
```

#### ER-EQ-06: null との比較

```gherkin
When: EmployeeRowId と null を Equals で比較:
      var rowId = EmployeeRowId.From(12345L)
Then: rowId.Equals(null) は false である
```

#### ER-EQ-07: 異なる型との比較

```gherkin
When: EmployeeRowId と long を比較:
      var rowId = EmployeeRowId.From(12345L)
Then: rowId.Equals(12345L) は false である
```

### グループ 5: 表示形式（Display）

#### ER-DISP-01: ToString は数値文字列

```gherkin
When: EmployeeRowId.From(12345L).ToString() を呼び出す
Then: "12345" が返される
```

#### ER-DISP-02: ToString 最小値

```gherkin
When: EmployeeRowId.From(1L).ToString() を呼び出す
Then: "1" が返される
```

#### ER-DISP-03: ToString 大きな値

```gherkin
When: EmployeeRowId.From(9223372036854775807L).ToString() を呼び出す
Then: "9223372036854775807" が返される
```

#### ER-DISP-04: ToString Data-driven

```gherkin
When: 以下の値で ToString を呼び出す:
      (1, "1"), (100, "100"), (12345, "12345")
Then: 各値について対応する文字列が返される
```

### グループ 6: 不変性（Immutability）

#### ER-IMM-01: Value は読み取り専用

```gherkin
When: EmployeeRowId.From(12345L) を生成
Then: Value プロパティを読み取り可能
And:  Value プロパティに書き込むことはできない
```

#### ER-IMM-02: 生成後の値は変更不可

```gherkin
When: 同一の EmployeeRowId を複数回アクセス
Then: Value は常に初期値である
```

---

## 📊 テスト概要

### テスト数

| グループ | テスト数 | 備考 |
|---------|--------|------|
| 生成メソッド（ER-GEN） | 8 | 正常系3、異常系3、データ駆動2 |
| 安全な生成（ER-TRY） | 6 | 正常系2、異常系2、データ駆動2 |
| DB値変換（ER-DBVAL） | 5 | 正常系3、異常系2 |
| 等価性（ER-EQ） | 7 | 等価性判定、ハッシュ、ディクショナリ、null |
| 表示形式（ER-DISP） | 4 | ToString のバリエーション |
| 不変性（ER-IMM） | 2 | 読み取り専用、値の永続性 |
| **合計** | **32** | |

---

## ✅ テスト実装ガイド

### 命名規則

```
Test{グループ}{シーケンス}_{説明}

例：
- TestERGEN01_FromMin1ReturnsValidRowId
- TestERTRY03_TryFromZeroReturnsFalse
- TestERDISP01_ToStringReturns12345
```

### Fact vs Theory

- **Fact**: 1つの値で検証するテスト
- **Theory**: 複数の値を検証するデータ駆動テスト

```csharp
// Fact（1つの値）
[Fact]
public void TestERGEN01_FromMin1ReturnsValidRowId()
{
    var result = EmployeeRowId.From(1L);
    Assert.Equal(1L, result.Value);
}

// Theory（複数の値）
[Theory]
[InlineData(1L)]
[InlineData(100L)]
[InlineData(12345L)]
public void TestERGENDataDriven(long value)
{
    var result = EmployeeRowId.From(value);
    Assert.Equal(value, result.Value);
}
```

---

## 参考資料

- 技術仕様書: `EmployeeRowId_技術仕様書.md`
- 詳細設計書: `EmployeeRowId_詳細設計書.md`
- テスト実装例: Employee Domain の他の ValueObject テスト
