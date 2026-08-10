namespace SupportAdvance.SharedKernel.Tests.ValueObjects.Identifiers;

using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using Xunit;

/// <summary>
/// DepartmentCode（部署コード） 単体テスト
///
/// 必須 ValueObject パターン：null は失敗を返す
/// DB スキーマ: m_departments.department_code [char](4)
///
/// テスト観点：
/// - VO-01: From(4文字) は IsSet=true のインスタンスを生成
/// - VO-02: From(null/空文字列/不正文字) は ArgumentException を投げる
/// - VO-03: TryFrom で null は false を返す
/// - VO-06: 等価性判定（Equals/GetHashCode）
/// </summary>
public class DepartmentCodeTests
{
    /// <summary>From(4文字) は IsSet=true のインスタンスを生成</summary>
    [Theory]
    [InlineData("G100")]
    [InlineData("D200")]
    [InlineData("S050")]
    public void From_With4Characters_ReturnsInstanceWithIsSetTrue(string validCode)
    {
        // Act
        var deptCode = DepartmentCode.From(validCode);

        // Assert
        Assert.NotNull(deptCode);
        Assert.True(deptCode.IsSet);
        Assert.Equal(validCode, deptCode.Value);
    }

    /// <summary>From(null) は ArgumentException を投げる</summary>
    [Fact]
    public void From_WithNull_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => DepartmentCode.From(null!));
    }

    /// <summary>From(空文字列) は ArgumentException を投げる</summary>
    [Fact]
    public void From_WithEmptyString_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => DepartmentCode.From(""));
    }

    /// <summary>From(3文字) は ArgumentException を投げる</summary>
    [Fact]
    public void From_With3Characters_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => DepartmentCode.From("G10"));
    }

    /// <summary>From(5文字) は ArgumentException を投げる</summary>
    [Fact]
    public void From_With5Characters_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => DepartmentCode.From("G1000"));
    }

    /// <summary>From(特殊文字) は ArgumentException を投げる</summary>
    [Theory]
    [InlineData("G@00")]
    [InlineData("G!00")]
    [InlineData("G-00")]
    public void From_WithSpecialCharacters_ThrowsArgumentException(string invalidCode)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => DepartmentCode.From(invalidCode));
    }

    /// <summary>From(スペース含む) は ArgumentException を投げる</summary>
    [Fact]
    public void From_WithSpaces_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => DepartmentCode.From("G 00"));
    }

    /// <summary>From(小文字) は許可（大文字小文字区別）</summary>
    [Fact]
    public void From_WithLowercaseLetters_AcceptsAndStores()
    {
        // Act
        var deptCode = DepartmentCode.From("g100");

        // Assert
        Assert.Equal("g100", deptCode.Value);
    }

    /// <summary>TryFrom(4文字) は true と インスタンスを返す</summary>
    [Fact]
    public void TryFrom_With4Characters_ReturnsTrueAndInstance()
    {
        // Act
        var result = DepartmentCode.TryFrom("G100", out var deptCode);

        // Assert
        Assert.True(result);
        Assert.NotNull(deptCode);
        Assert.True(deptCode.IsSet);
        Assert.Equal("G100", deptCode.Value);
    }

    /// <summary>TryFrom(null) は false を返す</summary>
    [Fact]
    public void TryFrom_WithNull_ReturnsFalse()
    {
        // Act
        var result = DepartmentCode.TryFrom(null, out var deptCode);

        // Assert
        Assert.False(result);
    }

    /// <summary>TryFrom(空文字列) は false を返す</summary>
    [Fact]
    public void TryFrom_WithEmptyString_ReturnsFalse()
    {
        // Act
        var result = DepartmentCode.TryFrom("", out var deptCode);

        // Assert
        Assert.False(result);
    }

    /// <summary>TryFrom(不正な長さ) は false を返す</summary>
    [Theory]
    [InlineData("G10")]
    [InlineData("G1000")]
    public void TryFrom_WithInvalidLength_ReturnsFalse(string invalidCode)
    {
        // Act
        var result = DepartmentCode.TryFrom(invalidCode, out var deptCode);

        // Assert
        Assert.False(result);
    }

    /// <summary>TryFrom(特殊文字) は false を返す</summary>
    [Fact]
    public void TryFrom_WithSpecialCharacters_ReturnsFalse()
    {
        // Act
        var result = DepartmentCode.TryFrom("G@00", out var deptCode);

        // Assert
        Assert.False(result);
    }

    /// <summary>TryFromDbValue(4文字) は true を返す</summary>
    [Fact]
    public void TryFromDbValue_With4Characters_ReturnsTrue()
    {
        // Act
        var result = DepartmentCode.TryFromDbValue("G100", out var deptCode);

        // Assert
        Assert.True(result);
        Assert.Equal("G100", deptCode.Value);
    }

    /// <summary>TryFromDbValue(null) は false を返す</summary>
    [Fact]
    public void TryFromDbValue_WithNull_ReturnsFalse()
    {
        // Act
        var result = DepartmentCode.TryFromDbValue(null, out var deptCode);

        // Assert
        Assert.False(result);
    }

    /// <summary>同じコード同士は等価</summary>
    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        // Arrange
        var code1 = DepartmentCode.From("G100");
        var code2 = DepartmentCode.From("G100");

        // Act & Assert
        Assert.Equal(code1, code2);
        Assert.True(code1 == code2);
    }

    /// <summary>異なるコードは非等価</summary>
    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        // Arrange
        var code1 = DepartmentCode.From("G100");
        var code2 = DepartmentCode.From("D200");

        // Act & Assert
        Assert.NotEqual(code1, code2);
        Assert.False(code1 == code2);
    }

    /// <summary>大文字小文字区別：G100 と g100 は非等価</summary>
    [Fact]
    public void Equals_CaseSensitive_ReturnsFalse()
    {
        // Arrange
        var code1 = DepartmentCode.From("G100");
        var code2 = DepartmentCode.From("g100");

        // Act & Assert
        Assert.NotEqual(code1, code2);
    }

    /// <summary>ハッシュコード：同じ値は同じハッシュ</summary>
    [Fact]
    public void GetHashCode_SameValues_SameHash()
    {
        // Arrange
        var code1 = DepartmentCode.From("G100");
        var code2 = DepartmentCode.From("G100");

        // Act & Assert
        Assert.Equal(code1.GetHashCode(), code2.GetHashCode());
    }

    /// <summary>ハッシュコード：異なる値は異なるハッシュ</summary>
    [Fact]
    public void GetHashCode_DifferentValues_DifferentHash()
    {
        // Arrange
        var code1 = DepartmentCode.From("G100");
        var code2 = DepartmentCode.From("D200");

        // Act & Assert
        Assert.NotEqual(code1.GetHashCode(), code2.GetHashCode());
    }

    /// <summary>ToString は4文字の文字列表現を返す</summary>
    [Fact]
    public void ToString_Returns4CharacterString()
    {
        // Arrange
        var code = DepartmentCode.From("G100");

        // Act
        var result = code.ToString();

        // Assert
        Assert.Equal("G100", result);
    }
}
