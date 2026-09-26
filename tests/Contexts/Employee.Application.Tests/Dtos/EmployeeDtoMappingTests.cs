namespace SupportAdvance.Contexts.Employee.Application.Tests.Dtos;

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Person;
using SupportAdvance.Contexts.Employee.Application.Dtos;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using Xunit;

/// <summary>
/// EmployeeDto マッピングテスト
/// </summary>
public class EmployeeDtoMappingTests
{
    private readonly IClock _clock = new SystemClock();
    #region グループ 1: DTO マッピング

    [Fact]
    public void Test1_1_EmployeeDto_ToDto_MapsAllFields()
    {
        // Arrange
        var employee = Employee.Create(
            EmployeeRowId.From(100),
            BizDivision.RegularEmployee(),
            BizId.From(1001),
            BizCode.From(BizDivision.RegularEmployee(), BizId.From(1001)),
            null,
            Person.Create(PersonRowId.From(50), LastName.From("山田"), FirstName.From("太郎"), LastNameKana.From("ヤマダ"), FirstNameKana.From("タロウ")),
            new List<DepartmentMembership>(),
            _clock
        );

        // Act
        var dto = employee.ToDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(100, dto.RowId);
        Assert.Equal(50, dto.PersonRowId);
        Assert.NotEmpty(dto.BizCode);
    }

    [Fact]
    public void Test1_2_EmployeeDto_ToDto_CodeFormatIsCorrect()
    {
        // Arrange
        var employee = Employee.Create(
            EmployeeRowId.From(1),
            BizDivision.RegularEmployee(),
            BizId.From(1234),
            BizCode.From(BizDivision.RegularEmployee(), BizId.From(1234)),
            null,
            Person.Create(PersonRowId.From(1), LastName.From("山田"), FirstName.From("太郎"), LastNameKana.From("ヤマダ"), FirstNameKana.From("タロウ")),
            new List<DepartmentMembership>(),
            _clock
        );

        // Act
        var dto = employee.ToDto();

        // Assert
        Assert.Contains("M", dto.BizCode);
        Assert.Contains("1234", dto.BizCode);
    }

    [Fact]
    public void Test1_3_EmployeeDto_ToDto_WithDifferentDivisions()
    {
        // Arrange
        var person = Person.Create(PersonRowId.From(1), LastName.From("山田"), FirstName.From("太郎"), LastNameKana.From("ヤマダ"), FirstNameKana.From("タロウ"));

        var employee1 = Employee.Create(
            EmployeeRowId.From(1),
            BizDivision.RegularEmployee(),
            BizId.From(1001),
            BizCode.From(BizDivision.RegularEmployee(), BizId.From(1001)),
            null,
            person,
            new List<DepartmentMembership>(),
            _clock
        );
        var employee2 = Employee.Create(
            EmployeeRowId.From(2),
            BizDivision.Dispatched(),
            BizId.From(7501),
            BizCode.From(BizDivision.Dispatched(), BizId.From(7501)),
            null,
            person,
            new List<DepartmentMembership>(),
            _clock
        );
        var employee3 = Employee.Create(
            EmployeeRowId.From(3),
            BizDivision.Contractor(),
            BizId.From(8001),
            BizCode.From(BizDivision.Contractor(), BizId.From(8001)),
            null,
            person,
            new List<DepartmentMembership>(),
            _clock
        );

        // Act
        var dto1 = employee1.ToDto();
        var dto2 = employee2.ToDto();
        var dto3 = employee3.ToDto();

        // Assert
        Assert.Contains("M", dto1.BizCode);
        Assert.Contains("T", dto2.BizCode);
        Assert.Contains("C", dto3.BizCode);
    }

    #endregion
}
