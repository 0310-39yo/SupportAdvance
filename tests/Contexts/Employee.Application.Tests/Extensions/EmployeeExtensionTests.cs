using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Application.Extensions;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Person;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using Xunit;
using EmployeeEntity = SupportAdvance.Contexts.Employee.Domain.Entities.Employee;

namespace SupportAdvance.Contexts.Employee.Application.Tests.Extensions;

/// <summary>
/// <see cref="EmployeeExtensions"/> の単体テスト
/// </summary>
/// <remarks>
/// <para>【観点ID】docs/Contexts/Employee/Application/Application_単体テスト仕様書.md の VO-EXT に対応</para>
/// </remarks>
public class EmployeeExtensionTests
{
    private readonly IClock _clock = new SystemClock();

    /// <summary>
    /// 全項目が DTO に写されることの検証
    /// </summary>
    [Fact]
    public void VO_EXT_01_ToDto_ValidEmployee_MapsAllFields()
    {
        var employee = CreateEmployee(new List<DepartmentMembership>());

        var dto = employee.ToDto();

        Assert.Equal(100, dto.RowId);
        Assert.Equal(50, dto.PersonRowId);
        Assert.Equal("山田", dto.PersonLastName);
        Assert.Equal("太郎", dto.PersonFirstName);
        Assert.Equal("ヤマダ", dto.PersonLastNameKana);
        Assert.Equal("タロウ", dto.PersonFirstNameKana);
        Assert.Equal(employee.TypeDivision.ToString(), dto.TypeDivision);
    }

    /// <summary>
    /// 従業員コードが区分と業務ID を含む形式で出力されることの検証
    /// </summary>
    [Fact]
    public void VO_EXT_02_ToDto_ValidEmployee_BizCodeContainsDivisionAndBizId()
    {
        var dto = CreateEmployee(new List<DepartmentMembership>()).ToDto();

        Assert.Contains("M", dto.BizCode);
        Assert.Contains("1234", dto.BizCode);
        Assert.Equal("1234", dto.BizId);
    }

    /// <summary>
    /// 所属がない場合、部署名が空文字になることの検証
    /// </summary>
    [Fact]
    public void VO_EXT_03_ToDto_NoMemberships_DepartmentNamesIsEmpty()
    {
        var dto = CreateEmployee(new List<DepartmentMembership>()).ToDto();

        Assert.Equal(string.Empty, dto.DepartmentNames);
    }

    /// <summary>
    /// 複数所属の場合、主部署が先頭になり「, 」で連結されることの検証
    /// </summary>
    [Fact]
    public void VO_EXT_04_ToDto_MultipleMemberships_PrimaryDepartmentFirstJoinedByComma()
    {
        var memberships = new List<DepartmentMembership>
        {
            Membership(1, 20, isPrimary: false, "開発部"),
            Membership(2, 10, isPrimary: true, "営業部")
        };

        var dto = CreateEmployee(memberships).ToDto();

        Assert.Equal("営業部, 開発部", dto.DepartmentNames);
    }

    /// <summary>
    /// 部署名が未設定（Unset）の所属は、空文字として扱われ例外にならないことの検証
    /// </summary>
    [Fact]
    public void VO_EXT_05_ToDto_UnsetDepartmentDisplayName_DepartmentNamesIsEmpty()
    {
        var memberships = new List<DepartmentMembership> { Membership(1, 10, isPrimary: true, null) };

        var dto = CreateEmployee(memberships).ToDto();

        Assert.Equal(string.Empty, dto.DepartmentNames);
    }

    private EmployeeEntity CreateEmployee(List<DepartmentMembership> memberships) =>
        EmployeeEntity.Create(
            EmployeeRowId.From(100),
            BizDivision.RegularEmployee(),
            BizId.From(1234),
            BizCode.From(BizDivision.RegularEmployee(), BizId.From(1234)),
            null,
            Person.Create(PersonRowId.From(50), LastName.From("山田"), FirstName.From("太郎"), LastNameKana.From("ヤマダ"), FirstNameKana.From("タロウ")),
            memberships,
            _clock);

    private static DepartmentMembership Membership(long rowId, long departmentRowId, bool isPrimary, string? name) =>
        DepartmentMembership.Create(
            DepartmentMembershipRowId.From(rowId),
            EmployeeRowId.From(100),
            DepartmentRowId.From(departmentRowId),
            IsPrimary.From(isPrimary),
            departmentDisplayName: name is null ? null : DepartmentDisplayName.From(name));
}
