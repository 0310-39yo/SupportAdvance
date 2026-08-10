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
        var id = EmployeeId.NewId();
        var employee = Employee.Create(
            id,
            EmployeeRowId.From(100),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1001)),
            PersonRowId.From(50)
        );

        // Act
        var dto = employee.ToDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(id.Value, dto.Id);
        Assert.Equal(100, dto.RowId);
        Assert.Equal(50, dto.PersonRowId);
        Assert.NotEmpty(dto.Code);
    }

    [Fact]
    public void Test1_2_EmployeeDto_ToDto_CodeFormatIsCorrect()
    {
        // Arrange
        var employee = Employee.Create(
            EmployeeId.NewId(),
            EmployeeRowId.From(1),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1234)),
            PersonRowId.From(1)
        );

        // Act
        var dto = employee.ToDto();

        // Assert
        Assert.Contains("M", dto.Code);
        Assert.Contains("1234", dto.Code);
    }

    [Fact]
    public void Test1_3_EmployeeDto_ToDto_WithDifferentDivisions()
    {
        // Arrange
        var employee1 = Employee.Create(
            EmployeeId.NewId(),
            EmployeeRowId.From(1),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1001)),
            PersonRowId.From(1)
        );
        var employee2 = Employee.Create(
            EmployeeId.NewId(),
            EmployeeRowId.From(2),
            EmployeeCode.From(EmployeeDivision.Dispatched(), EmployeeNumber.From(7501)),
            PersonRowId.From(1)
        );
        var employee3 = Employee.Create(
            EmployeeId.NewId(),
            EmployeeRowId.From(3),
            EmployeeCode.From(EmployeeDivision.Contractor(), EmployeeNumber.From(8001)),
            PersonRowId.From(1)
        );

        // Act
        var dto1 = employee1.ToDto();
        var dto2 = employee2.ToDto();
        var dto3 = employee3.ToDto();

        // Assert
        Assert.Contains("M", dto1.Code);
        Assert.Contains("T", dto2.Code);
        Assert.Contains("C", dto3.Code);
    }

    #endregion
}
