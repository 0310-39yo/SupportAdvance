using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using Xunit;

namespace SupportAdvance.Contexts.Employee.Domain.Tests.Entities;

/// <summary>
/// <see cref="DepartmentMembership"/> の単体テスト
/// </summary>
public class DepartmentMembershipTests
{
    private static readonly LocalDateTime EndDate = new(new DateTime(2026, 12, 31, 0, 0, 0));

    private static DepartmentMembership Create(EndOn? endOn = null, DepartmentDisplayName? name = null, bool primary = true) =>
        DepartmentMembership.Create(
            DepartmentMembershipRowId.From(1),
            EmployeeRowId.From(2),
            DepartmentRowId.From(3),
            IsPrimary.From(primary),
            endOn,
            name);

    /// <summary>
    /// 終了日・部署名を省略した場合、いずれも Unset になることの検証
    /// </summary>
    [Fact]
    public void Create_WithoutOptionalValues_EndOnAndDisplayNameAreUnset()
    {
        var membership = Create();

        Assert.False(membership.EndOn.HasEnded);
        Assert.False(membership.DepartmentDisplayName.HasName);
        Assert.Equal(1, membership.RowId.Value);
        Assert.True(membership.IsPrimary.Value);
    }

    /// <summary>
    /// 終了日・部署名を指定した場合、その値が保持されることの検証
    /// </summary>
    [Fact]
    public void Create_WithOptionalValues_KeepsValues()
    {
        var membership = Create(EndOn.From(EndDate), DepartmentDisplayName.From("営業部"), primary: false);

        Assert.Equal(EndDate, membership.EndOn.Value);
        Assert.Equal("営業部", membership.DepartmentDisplayName.Value);
        Assert.False(membership.IsPrimary.Value);
    }

    /// <summary>
    /// Reconstruct で全項目が復元されることの検証
    /// </summary>
    [Fact]
    public void Reconstruct_RestoresAllValues()
    {
        var membership = DepartmentMembership.Reconstruct(
            DepartmentMembershipRowId.From(9),
            EmployeeRowId.From(8),
            DepartmentRowId.From(7),
            IsPrimary.Secondary(),
            EndOn.From(EndDate),
            DepartmentDisplayName.From("開発部"));

        Assert.Equal(9, membership.RowId.Value);
        Assert.Equal(EmployeeRowId.From(8), membership.EmployeeRowId);
        Assert.Equal(DepartmentRowId.From(7), membership.DepartmentRowId);
        Assert.Equal(EndDate, membership.EndOn.Value);
        Assert.Equal("開発部", membership.DepartmentDisplayName.Value);
    }

    /// <summary>
    /// Reconstruct で部署名を省略すると Unset になることの検証
    /// </summary>
    [Fact]
    public void Reconstruct_WithoutDisplayName_DisplayNameIsUnset()
    {
        var membership = DepartmentMembership.Reconstruct(
            DepartmentMembershipRowId.From(9), EmployeeRowId.From(8), DepartmentRowId.From(7),
            IsPrimary.Primary(), EndOn.Unset());

        Assert.False(membership.DepartmentDisplayName.HasName);
    }

    /// <summary>
    /// 終了日が未設定なら、いつ時点でも有効であることの検証
    /// </summary>
    [Fact]
    public void IsActive_WithoutEndOn_AlwaysTrue()
    {
        Assert.True(Create().IsActive(LocalDateTime.MaxValue));
    }

    /// <summary>
    /// 終了日の前は有効、終了日当日以降は無効であることの検証（境界）
    /// </summary>
    [Fact]
    public void IsActive_WithEndOn_ActiveOnlyBeforeEndOn()
    {
        var membership = Create(EndOn.From(EndDate));

        Assert.True(membership.IsActive(EndDate - TimeSpan.FromSeconds(1)));
        Assert.False(membership.IsActive(EndDate));
        Assert.False(membership.IsActive(EndDate + TimeSpan.FromDays(1)));
    }

    /// <summary>
    /// ToString に行ID・主部署区分が含まれることの検証
    /// </summary>
    [Fact]
    public void ToString_ContainsRowIdAndPrimary()
    {
        var text = Create().ToString();

        Assert.Contains("RowId=1", text);
        Assert.Contains("主部署", text);
    }
}
