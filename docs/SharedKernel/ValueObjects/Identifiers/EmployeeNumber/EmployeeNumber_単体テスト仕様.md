# EmployeeNumber - 単体テスト仕様書

**プロジェクト:** SupportAdvance  
**テスト対象:** EmployeeNumber ValueObject  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-08-09

---

## 0. 本書の位置づけ

本書は、EmployeeNumber ValueObject が、技術仕様書および詳細設計書で定義された以下の機能を満たすことを確認するテスト仕様書。

- **値の不変性**: 生成後、内部状態が変更されない
- **等価性比較**: 同じ値を持つ2つのオブジェクトは等価
- **ハッシュ整合性**: Equals=true のオブジェクトは同一ハッシュ値
- **文字列化**: ToString が左0埋め5桁フォーマットを返す
- **範囲検証**: 1001～8499 のみ許可、1000は予約、範囲外は例外
- **生成メソッド**: From, TryFrom, FromDbValue, TryFromDbValue の動作

---

## 1. テスト目的

EmployeeNumber の以下を確認する：

1. **生成メソッドの正常系**: From, FromDbValue が有効な値を受け入れ、IsSet=true で返す
2. **生成メソッドの異常系**: TryFrom/TryFromDbValue が null/無効値を適切に処理
3. **値の検証**: 1000（予約）、範囲外で ArgumentOutOfRangeException をスロー
4. **等価性判定**: Equals, GetHashCode が ValueObject パターンを満たす
5. **表示形式**: ToString が左0埋め5桁を返す
6. **不変性**: 生成後、Value プロパティが変更不可

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | EmployeeNumber |
| **名前空間** | SupportAdvance.SharedKernel.ValueObjects.Identifiers |
| **依拠仕様** | EmployeeNumber技術仕様書 v1.0 + 詳細設計書 v1.0 |
| **前提** | PrimitiveValueObject<int> が正常に機能していること |

---

## 3. テスト観点一覧

### 3.1 観点グループ GEN：生成メソッド（From/FromDbValue）

| 観点ID | テスト項目 | 分類 | 検証方法 |
|--------|----------|------|---------|
| EN-GEN-01 | From(1001)（下限値）が IsSet=true で返る | 正常系 | `var obj = From(1001); Assert.True(obj.IsSet); Assert.Equal(1001, obj.Value);` |
| EN-GEN-02 | From(1234)（中間値）が IsSet=true で返る | 正常系 | `var obj = From(1234); Assert.Equal(1234, obj.Value);` |
| EN-GEN-03 | From(8499)（上限値）が IsSet=true で返る | 正常系 | `var obj = From(8499); Assert.Equal(8499, obj.Value);` |
| EN-GEN-04 | From(1000)（予約）で ArgumentOutOfRangeException | 異常系 | `Assert.Throws<ArgumentOutOfRangeException>(() => From(1000));` |
| EN-GEN-05 | From(999)（範囲下）で ArgumentOutOfRangeException | 異常系 | `Assert.Throws<ArgumentOutOfRangeException>(() => From(999));` |
| EN-GEN-06 | From(8500)（範囲上）で ArgumentOutOfRangeException | 異常系 | `Assert.Throws<ArgumentOutOfRangeException>(() => From(8500));` |
| EN-GEN-07 | From(0)（0以下）で ArgumentOutOfRangeException | 異常系 | `Assert.Throws<ArgumentOutOfRangeException>(() => From(0));` |
| EN-GEN-08 | From(-100)（負数）で ArgumentOutOfRangeException | 異常系 | `Assert.Throws<ArgumentOutOfRangeException>(() => From(-100));` |
| EN-GEN-09 | FromDbValue(1234) が IsSet=true で返る | 正常系 | `var obj = FromDbValue(1234); Assert.True(obj.IsSet);` |
| EN-GEN-10 | FromDbValue(1000) で ArgumentOutOfRangeException | 異常系 | `Assert.Throws<ArgumentOutOfRangeException>(() => FromDbValue(1000));` |

### 3.2 観点グループ TRY：TryFrom/TryFromDbValue

| 観点ID | テスト項目 | 分類 | 検証方法 |
|--------|----------|------|---------|
| EN-TRY-01 | TryFrom(1234) が true を返し result に値を設定 | 正常系 | `Assert.True(TryFrom(1234, out var r)); Assert.Equal(1234, r.Value);` |
| EN-TRY-02 | TryFrom(1001)（下限）が true を返す | 正常系 | `Assert.True(TryFrom(1001, out _));` |
| EN-TRY-03 | TryFrom(8499)（上限）が true を返す | 正常系 | `Assert.True(TryFrom(8499, out _));` |
| EN-TRY-04 | TryFrom(1000)（予約）が false を返す | 異常系 | `Assert.False(TryFrom(1000, out _));` |
| EN-TRY-05 | TryFrom(999) が false を返す | 異常系 | `Assert.False(TryFrom(999, out _));` |
| EN-TRY-06 | TryFrom(8500) が false を返す | 異常系 | `Assert.False(TryFrom(8500, out _));` |
| EN-TRY-07 | TryFrom(null) が true を返す（null許容） | 正常系 | `Assert.True(TryFrom((int?)null, out _));` |
| EN-TRY-08 | TryFromDbValue(1234) が true を返す | 正常系 | `Assert.True(TryFromDbValue(1234, out var r)); Assert.Equal(1234, r.Value);` |
| EN-TRY-09 | TryFromDbValue(1000) が false を返す | 異常系 | `Assert.False(TryFromDbValue(1000, out _));` |
| EN-TRY-10 | TryFromDbValue(null) が false を返す | 異常系 | `Assert.False(TryFromDbValue((int?)null, out _));` |

### 3.3 観点グループ DISP：表示形式（ToString）

| 観点ID | テスト項目 | 分類 | 検証方法 |
|--------|----------|------|---------|
| EN-DISP-01 | From(1).ToString() が "00001"（5桁左0埋め） | 正常系 | `Assert.Equal("00001", From(1).ToString());` |
| EN-DISP-02 | From(1234).ToString() が "01234" | 正常系 | `Assert.Equal("01234", From(1234).ToString());` |
| EN-DISP-03 | From(8499).ToString() が "08499" | 正常系 | `Assert.Equal("08499", From(8499).ToString());` |
| EN-DISP-04 | From(1001).ToString() が "01001" | 正常系 | `Assert.Equal("01001", From(1001).ToString());` |
| EN-DISP-05 | From(10000).ToString() が "10000" | 正常系 | `Assert.Equal("10000", From(10000).ToString());` |

### 3.4 観点グループ EQ：等価性（Equals/GetHashCode）

| 観点ID | テスト項目 | 分類 | 検証方法 |
|--------|----------|------|---------|
| EN-EQ-01 | 同じ値 (1234) の2つのオブジェクトは等価 | 正常系 | `Assert.Equal(From(1234), From(1234));` |
| EN-EQ-02 | 同一参照は等価 | 正常系 | `var x = From(1234); Assert.Equal(x, x);` |
| EN-EQ-03 | 異なる値 (1234 vs 5678) は非等価 | 正常系 | `Assert.NotEqual(From(1234), From(5678));` |
| EN-EQ-04 | Equals(null) が false を返す | 異常系 | `Assert.False(From(1234).Equals((EmployeeNumber?)null));` |
| EN-EQ-05 | GetHashCode が同じ値で同一ハッシュ | 正常系 | `Assert.Equal(From(1234).GetHashCode(), From(1234).GetHashCode());` |
| EN-EQ-06 | GetHashCode が異なる値で異なるハッシュ（高確率） | 正常系 | `Assert.NotEqual(From(1234).GetHashCode(), From(5678).GetHashCode());` |
| EN-EQ-07 | Dictionary<EmployeeNumber, T> で使用可能 | 正常系 | `var dict = new Dictionary<EmployeeNumber, string> { { From(1234), "test" } }; Assert.Equal("test", dict[From(1234)]);` |

### 3.5 観点グループ IMM：不変性

| 観点ID | テスト項目 | 分類 | 検証方法 |
|--------|----------|------|---------|
| EN-IMM-01 | 生成後、Value プロパティが変更不可 | 正常系 | `var x = From(1234); // Value に set メンバーがないことを確認` |
| EN-IMM-02 | 複数参照が同じ値を共有（不変） | 正常系 | `var x = From(1234); var y = x; Assert.Equal(x.Value, y.Value);` |

### 3.6 観点グループ EDGE：エッジケース

| 観点ID | テスト項目 | 分類 | 検証方法 |
|--------|----------|------|---------|
| EN-EDGE-01 | From(1001)（最小有効値）と From(1000)（予約） | 正常系・異常系 | `From(1001) OK, From(1000) NG` |
| EN-EDGE-02 | From(8499)（最大有効値）と From(8500)（範囲外） | 正常系・異常系 | `From(8499) OK, From(8500) NG` |
| EN-EDGE-03 | 大量の生成と等価性チェック（パフォーマンス確認） | パフォーマンス | `1000 個の EmployeeNumber を生成、重複なし確認` |

---

## 4. テストケース実装ガイド

### 4.1 テストクラス構成

```csharp
namespace SupportAdvance.Tests.SharedKernel.ValueObjects.Identifiers;

using Xunit;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

public class EmployeeNumberTests
{
    // 各観点IDごとにテストメソッドを実装
    
    [Fact]
    public void TestENGEN01_From1001ReturnsValidObject()
    {
        // Arrange
        int input = 1001;
        
        // Act
        var result = EmployeeNumber.From(input);
        
        // Assert
        Assert.Equal(1001, result.Value);
        Assert.True(result.IsSet);
    }
    
    [Fact]
    public void TestENGEN04_From1000ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        int input = 1000;  // 予約番号
        
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => EmployeeNumber.From(input));
    }
    
    // ... 他のテストメソッド
}
```

### 4.2 データドリブンテスト例

```csharp
[Theory]
[InlineData(1001, "01001", true)]    // 下限
[InlineData(1234, "01234", true)]    // 中間
[InlineData(8499, "08499", true)]    // 上限
[InlineData(1000, null, false)]      // 予約（例外）
[InlineData(999, null, false)]       // 範囲下（例外）
[InlineData(8500, null, false)]      // 範囲上（例外）
public void TestFromVariousValues(int input, string? expectedDisplay, bool shouldSucceed)
{
    if (shouldSucceed)
    {
        var result = EmployeeNumber.From(input);
        Assert.Equal(expectedDisplay, result.ToString());
    }
    else
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => EmployeeNumber.From(input));
    }
}
```

### 4.3 ToString フォーマット確認テスト

```csharp
[Theory]
[InlineData(1, "00001")]
[InlineData(10, "00010")]
[InlineData(100, "00100")]
[InlineData(1000, "01000")]
[InlineData(1001, "01001")]
[InlineData(1234, "01234")]
[InlineData(8499, "08499")]
[InlineData(10000, "10000")]
public void TestToStringFormat(int value, string expected)
{
    var number = EmployeeNumber.From(value);
    Assert.Equal(expected, number.ToString());
}
```

---

## 5. テスト実行環境

| 項目 | 内容 |
|------|------|
| テストフレームワーク | xUnit |
| 対象フレームワーク | .NET 10.0 |
| テストプロジェクト | `tests/SharedKernel.Tests` |
| テストファイル | `ValueObjects/Identifiers/EmployeeNumberTests.cs` |

---

## 6. テスト実行コマンド

```bash
# すべてのテストを実行
dotnet test tests/SharedKernel.Tests

# EmployeeNumber テストのみ実行
dotnet test tests/SharedKernel.Tests --filter "EmployeeNumberTests"

# 観点IDで絞り込み
dotnet test tests/SharedKernel.Tests --filter "EmployeeNumberTests AND (ENGEN OR ENTRY)"
```

---

## 参考資料

- 技術仕様書: `EmployeeNumber_技術仕様書.md`
- 詳細設計書: `EmployeeNumber_詳細設計書.md`
- PrimitiveValueObject: `src/SharedKernel/ValueObjects/Abstractions/PrimitiveValueObject.cs`
