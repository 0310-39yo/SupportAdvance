using SupportAdvance.Common.Clocks;
using SupportAdvance.Infrastructure.Mappers;
using SupportAdvance.SharedKernel.ValueObjects.Audit;
using Xunit;

namespace SupportAdvance.Infrastructure.Tests.Mappers;

/// <summary>
/// <see cref="DbDateTimeExtensions"/> の単体テスト
/// </summary>
public class DbDateTimeExtensionsTests
{
    private static readonly DateTime Sample = new(2026, 9, 26, 10, 30, 45, DateTimeKind.Unspecified);

    #region ToLocalDateTime

    /// <summary>
    /// 有効な日時の変換の検証
    /// </summary>
    [Fact]
    public void ToLocalDateTime_WithUnspecifiedKind_ReturnsSameDateTime()
    {
        var result = Sample.ToLocalDateTime();

        Assert.Equal(new LocalDateTime(Sample), result);
        Assert.Equal(Sample, result.Value);
    }

    /// <summary>
    /// 往復変換（DateTime → LocalDateTime → DateTime）で値が保たれることの検証
    /// </summary>
    [Fact]
    public void ToLocalDateTime_RoundTrip_PreservesValue()
    {
        var result = Sample.ToLocalDateTime().Value;

        Assert.Equal(Sample, result);
        Assert.Equal(DateTimeKind.Unspecified, result.Kind);
    }

    /// <summary>
    /// Kind が Unspecified でない場合の例外の検証
    /// </summary>
    [Theory]
    [InlineData(DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Local)]
    public void ToLocalDateTime_WithNonUnspecifiedKind_ThrowsArgumentException(DateTimeKind kind)
    {
        var value = new DateTime(2026, 9, 26, 10, 30, 45, kind);

        Assert.Throws<ArgumentException>(() => value.ToLocalDateTime());
    }

    #endregion

    #region ToLocalDateTimeOrNull

    /// <summary>
    /// 値がある場合の変換の検証
    /// </summary>
    [Fact]
    public void ToLocalDateTimeOrNull_WithValue_ReturnsSameDateTime()
    {
        DateTime? value = Sample;

        var result = value.ToLocalDateTimeOrNull();

        Assert.NotNull(result);
        Assert.Equal(Sample, result.Value.Value);
    }

    /// <summary>
    /// DB の NULL が null のまま返ることの検証
    /// </summary>
    [Fact]
    public void ToLocalDateTimeOrNull_WithNull_ReturnsNull()
    {
        DateTime? value = null;

        Assert.Null(value.ToLocalDateTimeOrNull());
    }

    /// <summary>
    /// Kind が Unspecified でない場合の例外の検証
    /// </summary>
    [Fact]
    public void ToLocalDateTimeOrNull_WithUtcKind_ThrowsArgumentException()
    {
        DateTime? value = new DateTime(2026, 9, 26, 10, 30, 45, DateTimeKind.Utc);

        Assert.Throws<ArgumentException>(() => value.ToLocalDateTimeOrNull());
    }

    #endregion

    #region 値オブジェクトの TryFrom との組み合わせ（DB の値 → Domain の型）

    /// <summary>
    /// DB の NULL が、任意型の値オブジェクトでは Unset になることの検証
    /// </summary>
    [Fact]
    public void DbNull_WithOptionalAuditValueObjects_BecomesUnset()
    {
        DateTime? dbNull = null;

        Assert.True(UpdatedAt.TryFrom(dbNull.ToLocalDateTimeOrNull(), out var updatedAt));
        Assert.False(updatedAt.HasUpdated);

        Assert.True(DeletedAt.TryFrom(dbNull.ToLocalDateTimeOrNull(), out var deletedAt));
        Assert.False(deletedAt.IsDeleted);
    }

    /// <summary>
    /// DB の値が、任意型の値オブジェクトに JST の日時として渡ることの検証
    /// </summary>
    [Fact]
    public void DbValue_WithOptionalAuditValueObjects_KeepsDateTime()
    {
        DateTime? dbValue = Sample;

        Assert.True(UpdatedAt.TryFrom(dbValue.ToLocalDateTimeOrNull(), out var updatedAt));
        Assert.True(updatedAt.HasUpdated);
        Assert.Equal(Sample, updatedAt.Value!.Value.Value);
    }

    /// <summary>
    /// 必須型の値オブジェクトでは、DB の値が保たれ、null は失敗になることの検証
    /// </summary>
    [Fact]
    public void DbValue_WithRequiredCreatedAt_KeepsDateTime_AndNullFails()
    {
        Assert.True(CreatedAt.TryFrom(Sample.ToLocalDateTime(), out var createdAt));
        Assert.Equal(Sample, createdAt.Value.Value);

        DateTime? dbNull = null;
        Assert.False(CreatedAt.TryFrom(dbNull.ToLocalDateTimeOrNull(), out _));
    }

    #endregion
}
