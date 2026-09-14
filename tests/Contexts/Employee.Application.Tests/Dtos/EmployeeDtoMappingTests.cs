namespace SupportAdvance.Contexts.Employee.Application.Tests.Dtos;

using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.Contexts.Employee.Application.Dtos;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using Xunit;

/// <summary>
/// EmployeeDto マッピングテスト
/// </summary>
public class EmployeeDtoMappingTests
{
    #region グループ 1: DTO マッピング

    [Fact]
    public void Test1_1_EmployeeDto_ToDto_MapsAllFields()
    {
        // Arrange
        var employee = Employee.Create(
            EmployeeRowId.From(100),
            EmployeeTypeDivision.RegularEmployee(),
            EmployeeBizId.From(1001),
            EmployeeBizCode.From(EmployeeTypeDivision.RegularEmployee(), EmployeeBizId.From(1001)),
            null,
            Person.Create(PersonRowId.From(50), PersonLastName.From("山田"), PersonFirstName.From("太郎"), PersonLastNameKana.From("ヤマダ"), PersonFirstNameKana.From("タロウ")),
            new List<DepartmentMembership>()
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
            EmployeeTypeDivision.RegularEmployee(),
            EmployeeBizId.From(1234),
            EmployeeBizCode.From(EmployeeTypeDivision.RegularEmployee(), EmployeeBizId.From(1234)),
            null,
            Person.Create(PersonRowId.From(1), PersonLastName.From("山田"), PersonFirstName.From("太郎"), PersonLastNameKana.From("ヤマダ"), PersonFirstNameKana.From("タロウ")),
            new List<DepartmentMembership>()
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
        var person = Person.Create(PersonRowId.From(1), PersonLastName.From("山田"), PersonFirstName.From("太郎"), PersonLastNameKana.From("ヤマダ"), PersonFirstNameKana.From("タロウ"));

        var employee1 = Employee.Create(
            EmployeeRowId.From(1),
            EmployeeTypeDivision.RegularEmployee(),
            EmployeeBizId.From(1001),
            EmployeeBizCode.From(EmployeeTypeDivision.RegularEmployee(), EmployeeBizId.From(1001)),
            null,
            person,
            new List<DepartmentMembership>()
        );
        var employee2 = Employee.Create(
            EmployeeRowId.From(2),
            EmployeeTypeDivision.Dispatched(),
            EmployeeBizId.From(7501),
            EmployeeBizCode.From(EmployeeTypeDivision.Dispatched(), EmployeeBizId.From(7501)),
            null,
            person,
            new List<DepartmentMembership>()
        );
        var employee3 = Employee.Create(
            EmployeeRowId.From(3),
            EmployeeTypeDivision.Contractor(),
            EmployeeBizId.From(8001),
            EmployeeBizCode.From(EmployeeTypeDivision.Contractor(), EmployeeBizId.From(8001)),
            null,
            person,
            new List<DepartmentMembership>()
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
