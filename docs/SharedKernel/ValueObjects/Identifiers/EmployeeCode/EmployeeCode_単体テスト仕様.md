# EmployeeCode - 単体テスト仕様書

**プロジェクト:** SupportAdvance  
**テスト対象:** EmployeeCode ValueObject  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-08-09

---

## 0. 本書の位置づけ

本書は、EmployeeCode ValueObject が、技術仕様書および詳細設計書で定義された以下の機能を満たすことを確認するテスト仕様書。

- **複合値の管理**: EmployeeDivision と EmployeeNumber の組み合わせを保持
- **範囲検証**: 区分ごとの有効範囲チェック
- **値の不変性**: 生成後、内部状態が変更されない
- **等価性比較**: 同じ値を持つ2つのオブジェクトは等価
- **文字列化**: ToString が "M1234" 形式を返す
- **文字列パース**: "M1234" から EmployeeCode を復元
- **ハッシュ整合性**: Equals=true のオブジェクトは同一ハッシュ値

---

## 1. テスト目的

EmployeeCode の以下を確認する：

1. **生成メソッドの正常系**: From が有効な組み合わせを受け入れ、生成成功
2. **範囲検証（正常系）**: 各区分の有効範囲（M:1001～6999・10000～、T:7500～7999・70000～、C:8000～8499・80000～）
3. **範囲検証（異常系）**: 無効な組み合わせで ArgumentException をスロー
4. **文字列パース**: "M1234" から EmployeeCode を復元
5. **等価性判定**: Equals, GetHashCode が ValueObject パターンを満たす
6. **表示形式**: ToString が "M1234" 形式を返す

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | EmployeeCode |
| **名前空間** | SupportAdvance.SharedKernel.ValueObjects.Identifiers |
| **依拠仕様** | EmployeeCode技術仕様書 v1.0 + 詳細設計書 v1.0 |
| **前提** | EmployeeDivision, EmployeeNumber が正常に機能していること |

---

## 3. テスト観点一覧

### 3.1 観点グループ GEN：生成メソッド（From/TryFrom）

| 観点ID | テスト項目 | 分類 | 検証方法 |
|--------|----------|------|---------|
| EC-GEN-01 | From(M, 1234) で M1234 コードを返す | 正常系 | `Assert.Equal("M1234", From(...).ToString());` |
| EC-GEN-02 | From(T, 7500) で T7500 コードを返す | 正常系 | `Assert.Equal("T7500", From(...).ToString());` |
| EC-GEN-03 | From(C, 8000) で C8000 コードを返す | 正常系 | `Assert.Equal("C8000", From(...).ToString());` |
| EC-GEN-04 | From(M, 10000) で M10000 コードを返す（使い切り後） | 正常系 | `Assert.Equal("M10000", From(...).ToString());` |
| EC-GEN-05 | From(T, 70000) で T70000 コードを返す（使い切り後） | 正常系 | `Assert.Equal("T70000", From(...).ToString());` |
| EC-GEN-06 | From(C, 80000) で C80000 コードを返す（使い切り後） | 正常系 | `Assert.Equal("C80000", From(...).ToString());` |
| EC-GEN-07 | From(M, 7500)（派遣範囲を従業員に） で ArgumentException | 異常系 | `Assert.Throws<ArgumentException>(() => From(...));` |
| EC-GEN-08 | From(T, 1234)（従業員範囲を派遣に） で ArgumentException | 異常系 | `Assert.Throws<ArgumentException>(() => From(...));` |
| EC-GEN-09 | From(C, 7500)（派遣範囲を請負に） で ArgumentException | 異常系 | `Assert.Throws<ArgumentException>(() => From(...));` |
| EC-GEN-10 | From(M, 999)（範囲下） で ArgumentException | 異常系 | `Assert.Throws<ArgumentException>(() => From(...));` |

### 3.2 観点グループ TRY：TryFrom

| 観点ID | テスト項目 | 分類 | 検証方法 |
|--------|----------|------|---------|
| EC-TRY-01 | TryFrom(M, 1234) が true を返す | 正常系 | `Assert.True(TryFrom(..., out var result));` |
| EC-TRY-02 | TryFrom(T, 7500) が true を返す | 正常系 | `Assert.True(TryFrom(..., out _));` |
| EC-TRY-03 | TryFrom(C, 8000) が true を返す | 正常系 | `Assert.True(TryFrom(..., out _));` |
| EC-TRY-04 | TryFrom(M, 7500)（無効な組み合わせ） が false を返す | 異常系 | `Assert.False(TryFrom(..., out _));` |
| EC-TRY-05 | TryFrom(T, 1234)（無効な組み合わせ） が false を返す | 異常系 | `Assert.False(TryFrom(..., out _));` |
| EC-TRY-06 | TryFrom(C, 7500)（無効な組み合わせ） が false を返す | 異常系 | `Assert.False(TryFrom(..., out _));` |
| EC-TRY-07 | TryFrom(M, 999)（範囲外） が false を返す | 異常系 | `Assert.False(TryFrom(..., out _));` |
| EC-TRY-08 | TryFrom(M, 8000)（請負範囲） が false を返す | 異常系 | `Assert.False(TryFrom(..., out _));` |
| EC-TRY-09 | TryFrom(T, 70000) が true を返す（使い切り後） | 正常系 | `Assert.True(TryFrom(..., out _));` |
| EC-TRY-10 | TryFrom(C, 80000) が true を返す（使い切り後） | 正常系 | `Assert.True(TryFrom(..., out _));` |

### 3.3 観点グループ PARSE：文字列パース（TryParse）

| 観点ID | テスト項目 | 分類 | 検証方法 |
|--------|----------|------|---------|
| EC-PARSE-01 | TryParse("M1234") が true を返し、M1234 を復元 | 正常系 | `Assert.True(TryParse("M1234", out var c)); Assert.Equal("M1234", c.ToString());` |
| EC-PARSE-02 | TryParse("T7500") が true を返す | 正常系 | `Assert.True(TryParse("T7500", out _));` |
| EC-PARSE-03 | TryParse("C8000") が true を返す | 正常系 | `Assert.True(TryParse("C8000", out _));` |
| EC-PARSE-04 | TryParse("M10000") が true を返す（5桁） | 正常系 | `Assert.True(TryParse("M10000", out _));` |
| EC-PARSE-05 | TryParse("X1234")（無効な区分） が false を返す | 異常系 | `Assert.False(TryParse("X1234", out _));` |
| EC-PARSE-06 | TryParse("M999")（範囲外） が false を返す | 異常系 | `Assert.False(TryParse("M999", out _));` |
| EC-PARSE-07 | TryParse("T1234")（従業員範囲） が false を返す | 異常系 | `Assert.False(TryParse("T1234", out _));` |
| EC-PARSE-08 | TryParse(null) が false を返す | 異常系 | `Assert.False(TryParse(null, out _));` |
| EC-PARSE-09 | TryParse("") が false を返す | 異常系 | `Assert.False(TryParse("", out _));` |
| EC-PARSE-10 | TryParse("1234")（区分なし） が false を返す | 異常系 | `Assert.False(TryParse("1234", out _));` |

### 3.4 観点グループ DBVAL：DB値変換（TryFromDbValues）

| 観点ID | テスト項目 | 分類 | 検証方法 |
|--------|----------|------|---------|
| EC-DBVAL-01 | TryFromDbValues('M', 1234) が true を返す | 正常系 | `Assert.True(TryFromDbValues('M', 1234, out var c));` |
| EC-DBVAL-02 | TryFromDbValues('T', 7500) が true を返す | 正常系 | `Assert.True(TryFromDbValues('T', 7500, out _));` |
| EC-DBVAL-03 | TryFromDbValues('C', 8000) が true を返す | 正常系 | `Assert.True(TryFromDbValues('C', 8000, out _));` |
| EC-DBVAL-04 | TryFromDbValues('M', 7500)（無効な組み合わせ） が false を返す | 異常系 | `Assert.False(TryFromDbValues('M', 7500, out _));` |
| EC-DBVAL-05 | TryFromDbValues('T', 1234)（無効な組み合わせ） が false を返す | 異常系 | `Assert.False(TryFromDbValues('T', 1234, out _));` |
| EC-DBVAL-06 | TryFromDbValues('X', 1234)（無効な区分） が false を返す | 異常系 | `Assert.False(TryFromDbValues('X', 1234, out _));` |
| EC-DBVAL-07 | TryFromDbValues('M', 999)（範囲外） が false を返す | 異常系 | `Assert.False(TryFromDbValues('M', 999, out _));` |
| EC-DBVAL-08 | TryFromDbValues('M', 10000) が true を返す（使い切り後） | 正常系 | `Assert.True(TryFromDbValues('M', 10000, out _));` |
| EC-DBVAL-09 | TryFromDbValues('T', 70000) が true を返す（使い切り後） | 正常系 | `Assert.True(TryFromDbValues('T', 70000, out _));` |
| EC-DBVAL-10 | TryFromDbValues('C', 80000) が true を返す（使い切り後） | 正常系 | `Assert.True(TryFromDbValues('C', 80000, out _));` |

### 3.5 観点グループ DISP：表示形式（ToString）

| 観点ID | テスト項目 | 分類 | 検証方法 |
|--------|----------|------|---------|
| EC-DISP-01 | M1234 の ToString が "M1234" を返す | 正常系 | `Assert.Equal("M1234", From(M, 1234).ToString());` |
| EC-DISP-02 | T7500 の ToString が "T7500" を返す | 正常系 | `Assert.Equal("T7500", From(T, 7500).ToString());` |
| EC-DISP-03 | C8000 の ToString が "C8000" を返す | 正常系 | `Assert.Equal("C8000", From(C, 8000).ToString());` |
| EC-DISP-04 | M10000 の ToString が "M10000" を返す（5桁） | 正常系 | `Assert.Equal("M10000", From(M, 10000).ToString());` |
| EC-DISP-05 | M1001 の ToString が "M1001" を返す（最小） | 正常系 | `Assert.Equal("M1001", From(M, 1001).ToString());` |

### 3.6 観点グループ EQ：等価性（Equals/GetHashCode）

| 観点ID | テスト項目 | 分類 | 検証方法 |
|--------|----------|------|---------|
| EC-EQ-01 | 同じ M1234 の2つのコードは等価 | 正常系 | `Assert.Equal(From(M, 1234), From(M, 1234));` |
| EC-EQ-02 | 同一参照は等価 | 正常系 | `var c = From(...); Assert.Equal(c, c);` |
| EC-EQ-03 | 異なる M1234 と T7500 は非等価 | 正常系 | `Assert.NotEqual(From(M, 1234), From(T, 7500));` |
| EC-EQ-04 | Equals(null) が false を返す | 異常系 | `Assert.False(From(...).Equals((EmployeeCode?)null));` |
| EC-EQ-05 | GetHashCode が同じ値で同一ハッシュ | 正常系 | `Assert.Equal(From(M, 1234).GetHashCode(), From(M, 1234).GetHashCode());` |
| EC-EQ-06 | GetHashCode が異なる値で異なるハッシュ（高確率） | 正常系 | `Assert.NotEqual(From(M, 1234).GetHashCode(), From(T, 7500).GetHashCode());` |
| EC-EQ-07 | Dictionary<EmployeeCode, T> で使用可能 | 正常系 | `var dict = new Dictionary<...> { { From(...), "val" } }; Assert.Equal("val", dict[From(...)]);` |

### 3.7 観点グループ PROP：プロパティ

| 観点ID | テスト項目 | 分類 | 検証方法 |
|--------|----------|------|---------|
| EC-PROP-01 | Division が EmployeeDivision.RegularEmployee を返す | 正常系 | `var c = From(EmployeeDivision.RegularEmployee(), ...); Assert.True(c.Division.IsRegularEmployee);` |
| EC-PROP-02 | Number が EmployeeNumber.From(1234) と等価 | 正常系 | `var c = From(..., EmployeeNumber.From(1234)); Assert.Equal(1234, c.Number.Value);` |
| EC-PROP-03 | Division と Number は読み取り専用 | 正常系 | `// Division, Number に set メンバーがないことを確認` |

### 3.8 観点グループ IMM：不変性

| 観点ID | テスト項目 | 分類 | 検証方法 |
|--------|----------|------|---------|
| EC-IMM-01 | 生成後、Division が変更不可 | 正常系 | `var c = From(...); // Division に set がない` |
| EC-IMM-02 | 生成後、Number が変更不可 | 正常系 | `var c = From(...); // Number に set がない` |

### 3.9 観点グループ RANGE：範囲検証（詳細）

| 観点ID | テスト項目 | 分類 | 検証方法 |
|--------|----------|------|---------|
| EC-RANGE-01 | M: 1001（下限）OK | 正常系 | `Assert.True(TryFrom(M, 1001, out _));` |
| EC-RANGE-02 | M: 1000（下限-1）NG | 異常系 | `Assert.False(TryFrom(M, 1000, out _));` |
| EC-RANGE-03 | M: 6999（初期上限）OK | 正常系 | `Assert.True(TryFrom(M, 6999, out _));` |
| EC-RANGE-04 | M: 7000（初期上限+1）NG | 異常系 | `Assert.False(TryFrom(M, 7000, out _));` |
| EC-RANGE-05 | M: 9999（ギャップ）NG | 異常系 | `Assert.False(TryFrom(M, 9999, out _));` |
| EC-RANGE-06 | M: 10000（使い切り後下限）OK | 正常系 | `Assert.True(TryFrom(M, 10000, out _));` |
| EC-RANGE-07 | T: 7500（下限）OK | 正常系 | `Assert.True(TryFrom(T, 7500, out _));` |
| EC-RANGE-08 | T: 7499（下限-1）NG | 異常系 | `Assert.False(TryFrom(T, 7499, out _));` |
| EC-RANGE-09 | T: 7999（初期上限）OK | 正常系 | `Assert.True(TryFrom(T, 7999, out _));` |
| EC-RANGE-10 | T: 8000（初期上限+1、請負範囲）NG | 異常系 | `Assert.False(TryFrom(T, 8000, out _));` |
| EC-RANGE-11 | T: 69999（ギャップ）NG | 異常系 | `Assert.False(TryFrom(T, 69999, out _));` |
| EC-RANGE-12 | T: 70000（使い切り後下限）OK | 正常系 | `Assert.True(TryFrom(T, 70000, out _));` |
| EC-RANGE-13 | C: 8000（下限）OK | 正常系 | `Assert.True(TryFrom(C, 8000, out _));` |
| EC-RANGE-14 | C: 7999（下限-1）NG | 異常系 | `Assert.False(TryFrom(C, 7999, out _));` |
| EC-RANGE-15 | C: 8499（初期上限）OK | 正常系 | `Assert.True(TryFrom(C, 8499, out _));` |
| EC-RANGE-16 | C: 8500（初期上限+1）NG | 異常系 | `Assert.False(TryFrom(C, 8500, out _));` |
| EC-RANGE-17 | C: 79999（ギャップ）NG | 異常系 | `Assert.False(TryFrom(C, 79999, out _));` |
| EC-RANGE-18 | C: 80000（使い切り後下限）OK | 正常系 | `Assert.True(TryFrom(C, 80000, out _));` |

---

## 4. テストケース実装ガイド

### 4.1 テストクラス構成

```csharp
namespace SupportAdvance.Tests.SharedKernel.ValueObjects.Identifiers;

using Xunit;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

public class EmployeeCodeTests
{
    // 各観点IDごとにテストメソッドを実装
    // メソッド名: Test + 観点ID + 説明
    
    [Fact]
    public void TestECGEN01_FromM1234ReturnsValidCode()
    {
        // Arrange
        var division = EmployeeDivision.RegularEmployee();
        var number = EmployeeNumber.From(1234);
        
        // Act
        var result = EmployeeCode.From(division, number);
        
        // Assert
        Assert.Equal("M1234", result.ToString());
    }
    
    // ... 他のテストメソッド
}
```

### 4.2 データドリブンテスト例

```csharp
[Theory]
[InlineData("M", 1234, true)]   // 従業員
[InlineData("M", 10000, true)]  // 従業員使い切り後
[InlineData("T", 7500, true)]   // 派遣社員
[InlineData("T", 70000, true)]  // 派遣使い切り後
[InlineData("C", 8000, true)]   // 請負者
[InlineData("C", 80000, true)]  // 請負使い切り後
[InlineData("M", 7500, false)]  // 無効な組み合わせ
[InlineData("T", 1234, false)]  // 無効な組み合わせ
public void TestTryFromVariousCombinations(char divChar, int number, bool shouldSucceed)
{
    if (shouldSucceed)
    {
        bool success = EmployeeCode.TryFrom(
            EmployeeDivision.FromDbValue(divChar.ToString()),
            EmployeeNumber.From(number),
            out var result);
        Assert.True(success);
    }
    else
    {
        bool success = EmployeeCode.TryFrom(
            EmployeeDivision.FromDbValue(divChar.ToString()),
            EmployeeNumber.From(number),
            out _);
        Assert.False(success);
    }
}
```

---

## 5. テスト実行環境

| 項目 | 内容 |
|------|------|
| テストフレームワーク | xUnit |
| 対象フレームワーク | .NET 10.0 |
| テストプロジェクト | `tests/SharedKernel.Tests` |
| テストファイル | `ValueObjects/Identifiers/EmployeeCodeTests.cs` |

---

## 6. テスト実行コマンド

```bash
# すべてのテストを実行
dotnet test tests/SharedKernel.Tests

# EmployeeCode テストのみ実行
dotnet test tests/SharedKernel.Tests --filter "EmployeeCodeTests"

# 観点IDで絞り込み
dotnet test tests/SharedKernel.Tests --filter "EmployeeCodeTests AND (ECGEN OR ECPARSE)"
```

---

## 参考資料

- 技術仕様書: `EmployeeCode_技術仕様書.md`
- 詳細設計書: `EmployeeCode_詳細設計書.md`
- EmployeeDivision テスト: `../EmployeeDivision/EmployeeDivision_単体テスト仕様.md`
- EmployeeNumber テスト: `../EmployeeNumber/EmployeeNumber_単体テスト仕様.md`
