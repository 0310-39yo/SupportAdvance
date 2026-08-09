# EmployeeDivision - 単体テスト仕様書

**プロジェクト:** SupportAdvance  
**テスト対象:** EmployeeDivision ValueObject  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-08-09

---

## 0. 本書の位置づけ

本書は、EmployeeDivision ValueObject が、技術仕様書および詳細設計書で定義された以下の機能を満たすことを確認するテスト仕様書。

- **値の不変性**: 生成後、内部状態が変更されない
- **等価性比較**: 同じ値を持つ2つのオブジェクトは等価
- **ハッシュ整合性**: Equals=true のオブジェクトは同一ハッシュ値
- **文字列化**: ToString が仕様通りの日本語名を返す
- **固定値管理**: M/T/C の3つの区分値のみ許可
- **生成メソッド**: From, TryFrom, FromDbValue, TryFromDbValue の動作

---

## 1. テスト目的

EmployeeDivision の以下を確認する：

1. **ファクトリメソッドの正常系**: RegularEmployee(), Dispatched(), Contractor() が正しい値を返す
2. **生成メソッドの正常系**: From(char), FromDbValue(string) が有効な値を受け入れ、IsSet=true で返す
3. **生成メソッドの異常系**: TryFrom/TryFromDbValue が null/無効値を適切に処理（false を返す）
4. **等価性判定**: Equals, GetHashCode が ValueObject パターンを満たす
5. **値の検証**: M/T/C 以外の値で ArgumentOutOfRangeException がスロー
6. **日本語名**: ToString が char 値に対応する日本語名を返す

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | EmployeeDivision |
| **名前空間** | SupportAdvance.SharedKernel.ValueObjects.Identifiers |
| **依拠仕様** | EmployeeDivision技術仕様書 v1.0 + 詳細設計書 v1.0 |
| **前提** | EnumValueObject<char> が正常に機能していること |

---

## 3. テスト観点一覧

### 3.1 観点グループ FM：ファクトリメソッド（RegularEmployee/Dispatched/Contractor）

| 観点ID | テスト項目 | 分類 | 検証方法 |
|--------|----------|------|---------|
| EM-FM-01 | RegularEmployee() が 'M' で IsSet=true を返す | 正常系 | `Assert.Equal('M', RegularEmployee().Value)` + `Assert.True(IsSet)` |
| EM-FM-02 | Dispatched() が 'T' で IsSet=true を返す | 正常系 | `Assert.Equal('T', Dispatched().Value)` + `Assert.True(IsSet)` |
| EM-FM-03 | Contractor() が 'C' で IsSet=true を返す | 正常系 | `Assert.Equal('C', Contractor().Value)` + `Assert.True(IsSet)` |
| EM-FM-04 | 複数呼び出しで毎回新しいインスタンスが生成される | 正常系 | `Assert.NotSame(RegularEmployee(), RegularEmployee())` |

### 3.2 観点グループ GEN：生成メソッド（From/FromDbValue）

| 観点ID | テスト項目 | 分類 | 検証方法 |
|--------|----------|------|---------|
| EM-GEN-01 | From('M') が IsSet=true で返る | 正常系 | `var obj = From('M'); Assert.True(obj.IsSet);` |
| EM-GEN-02 | From('T'), From('C') も同様に正常系 | 正常系 | 同上（複数値） |
| EM-GEN-03 | From('X')（無効値）で ArgumentOutOfRangeException | 異常系 | `Assert.Throws<ArgumentOutOfRangeException>(() => From('X'));` |
| EM-GEN-04 | FromDbValue("M") が IsSet=true で返る | 正常系 | `var obj = FromDbValue("M"); Assert.True(obj.IsSet);` |
| EM-GEN-05 | FromDbValue("") で ArgumentException | 異常系 | `Assert.Throws<ArgumentException>(() => FromDbValue(""));` |
| EM-GEN-06 | FromDbValue(null) で ArgumentException | 異常系 | `Assert.Throws<ArgumentException>(() => FromDbValue(null));` |

### 3.3 観点グループ TRY：TryFrom/TryFromDbValue

| 観点ID | テスト項目 | 分類 | 検証方法 |
|--------|----------|------|---------|
| EM-TRY-01 | TryFrom('M') が true を返し result に値を設定 | 正常系 | `Assert.True(TryFrom('M', out var r)); Assert.Equal('M', r.Value);` |
| EM-TRY-02 | TryFrom(null) が true を返す（null許容） | 正常系 | `Assert.True(TryFrom(null, out _));` |
| EM-TRY-03 | TryFrom('X') が false を返す（無効値） | 異常系 | `Assert.False(TryFrom('X', out _));` |
| EM-TRY-04 | TryFromDbValue("M") が true を返す | 正常系 | `Assert.True(TryFromDbValue("M", out var r));` |
| EM-TRY-05 | TryFromDbValue(null) が false を返す | 異常系 | `Assert.False(TryFromDbValue(null, out _));` |
| EM-TRY-06 | TryFromDbValue("") が false を返す | 異常系 | `Assert.False(TryFromDbValue("", out _));` |

### 3.4 観点グループ JUDGE：判定プロパティ

| 観点ID | テスト項目 | 分類 | 検証方法 |
|--------|----------|------|---------|
| EM-JUDGE-01 | IsRegularEmployee が 'M' で true | 正常系 | `Assert.True(RegularEmployee().IsRegularEmployee);` |
| EM-JUDGE-02 | IsRegularEmployee が 'T' で false | 正常系 | `Assert.False(Dispatched().IsRegularEmployee);` |
| EM-JUDGE-03 | IsDispatched が 'T' で true | 正常系 | `Assert.True(Dispatched().IsDispatched);` |
| EM-JUDGE-04 | IsDispatched が 'M' で false | 正常系 | `Assert.False(RegularEmployee().IsDispatched);` |
| EM-JUDGE-05 | IsContractor が 'C' で true | 正常系 | `Assert.True(Contractor().IsContractor);` |
| EM-JUDGE-06 | IsContractor が 'M' で false | 正常系 | `Assert.False(RegularEmployee().IsContractor);` |

### 3.5 観点グループ DISP：表示名（ToString）

| 観点ID | テスト項目 | 分類 | 検証方法 |
|--------|----------|------|---------|
| EM-DISP-01 | RegularEmployee().ToString() が "従業員" | 正常系 | `Assert.Equal("従業員", RegularEmployee().ToString());` |
| EM-DISP-02 | Dispatched().ToString() が "派遣社員" | 正常系 | `Assert.Equal("派遣社員", Dispatched().ToString());` |
| EM-DISP-03 | Contractor().ToString() が "請負者" | 正常系 | `Assert.Equal("請負者", Contractor().ToString());` |

### 3.6 観点グループ EQ：等価性（Equals/GetHashCode）

| 観点ID | テスト項目 | 分類 | 検証方法 |
|--------|----------|------|---------|
| EM-EQ-01 | 同じ値 ('M') の2つのオブジェクトは等価 | 正常系 | `Assert.Equal(From('M'), From('M'));` |
| EM-EQ-02 | 同一参照は等価 | 正常系 | `var x = From('M'); Assert.Equal(x, x);` |
| EM-EQ-03 | 異なる値 ('M' vs 'T') は非等価 | 正常系 | `Assert.NotEqual(From('M'), From('T'));` |
| EM-EQ-04 | Equals(null) が false を返す | 異常系 | `Assert.False(From('M').Equals((EmployeeDivision?)null));` |
| EM-EQ-05 | GetHashCode が同じ値で同一ハッシュ | 正常系 | `Assert.Equal(From('M').GetHashCode(), From('M').GetHashCode());` |
| EM-EQ-06 | GetHashCode が異なる値で異なるハッシュ（高確率） | 正常系 | `Assert.NotEqual(From('M').GetHashCode(), From('T').GetHashCode());` |

### 3.7 観点グループ IMM：不変性

| 観点ID | テスト項目 | 分類 | 検証方法 |
|--------|----------|------|---------|
| EM-IMM-01 | 生成後、Value プロパティが変更不可 | 正常系 | `var x = From('M'); // Value に set メンバーがないことを確認` |
| EM-IMM-02 | IsSet プロパティが変更不可 | 正常系 | `var x = From('M'); // IsSet に set メンバーがないことを確認` |

### 3.8 観点グループ CONST：定数

| 観点ID | テスト項目 | 分類 | 検証方法 |
|--------|----------|------|---------|
| EM-CONST-01 | RegularEmployeeValue が 'M' | 正常系 | `Assert.Equal('M', EmployeeDivision.RegularEmployeeValue);` |
| EM-CONST-02 | DispatchedValue が 'T' | 正常系 | `Assert.Equal('T', EmployeeDivision.DispatchedValue);` |
| EM-CONST-03 | ContractorValue が 'C' | 正常系 | `Assert.Equal('C', EmployeeDivision.ContractorValue);` |

---

## 4. テストケース実装ガイド

### 4.1 テストクラス構成

```csharp
namespace SupportAdvance.Tests.SharedKernel.ValueObjects.Identifiers;

using Xunit;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

public class EmployeeDivisionTests
{
    // 各観点IDごとにテストメソッドを実装
    // メソッド名: Test + 観点ID + 説明
    
    [Fact]
    public void TestEMFM01_RegularEmployeeReturnsM()
    {
        // Arrange
        // Act
        var result = EmployeeDivision.RegularEmployee();
        
        // Assert
        Assert.Equal('M', result.Value);
        Assert.True(result.IsSet);
    }
    
    // ... 他のテストメソッド
}
```

### 4.2 テスト用ヘルパーメソッド（不要な場合は省略）

```csharp
private void AssertEmployeeDivision(
    EmployeeDivision division,
    char expectedValue,
    string expectedDisplayName)
{
    Assert.Equal(expectedValue, division.Value);
    Assert.Equal(expectedDisplayName, division.ToString());
}
```

### 4.3 データドリブンテスト例

```csharp
[Theory]
[InlineData('M', "従業員", true)]
[InlineData('T', "派遣社員", true)]
[InlineData('C', "請負者", true)]
[InlineData('X', null, false)]  // 無効値は例外
public void TestFromVariousValues(char input, string? expectedDisplay, bool shouldSucceed)
{
    if (shouldSucceed)
    {
        var result = EmployeeDivision.From(input);
        Assert.Equal(expectedDisplay, result.ToString());
    }
    else
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EmployeeDivision.From(input));
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
| テストファイル | `ValueObjects/Identifiers/EmployeeDivisionTests.cs` |

---

## 6. テスト実行コマンド

```bash
# すべてのテストを実行
dotnet test tests/SharedKernel.Tests

# EmployeeDivision テストのみ実行
dotnet test tests/SharedKernel.Tests --filter "EmployeeDivisionTests"

# 観点IDで絞り込み
dotnet test tests/SharedKernel.Tests --filter "EmployeeDivisionTests AND (EMFM OR EMGEN)"
```

---

## 参考資料

- 技術仕様書: `EmployeeDivision_技術仕様書.md`
- 詳細設計書: `EmployeeDivision_詳細設計.md`
- EnumValueObject: `src/SharedKernel/ValueObjects/Abstractions/EnumValueObject.cs`
