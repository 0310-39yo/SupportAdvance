namespace SupportAdvance.Contexts.Employee.Application.Tests.UseCases;

using SupportAdvance.Contexts.Employee.Application.UseCases;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.Contexts.Employee.Infrastructure.Mappers;
using SupportAdvance.Contexts.Employee.Infrastructure.Repositories;
using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using Xunit;

/// <summary>
/// GetEmployeeByIdUseCase の単体テスト
/// </summary>
public class GetEmployeeByIdUseCaseTests
{
    #region グループ 1: 正常系

    [Fact]
    public async Task Test1_1_GetEmployeeById_WithValidId_ReturnsEmployee()
    {
        // Arrange
        var (useCase, repository) = CreateUseCase();
        var rowId = EmployeeRowId.From(1L);
        var employee = Employee.Create(
            rowId,
            EmployeeTypeDivision.From("M"),
            EmployeeBizId.From(1001),
            EmployeeBizCode.From("EMP001"),
            PersonRowId.From(100L)
        );
        await repository.AddAsync(employee);

        // Act
        var result = await useCase.ExecuteAsync(rowId.Value);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(rowId.Value, result.RowId);
    }

    #endregion

    #region グループ 2: 異常系

    [Fact]
    public async Task Test2_1_GetEmployeeById_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var (useCase, _) = CreateUseCase();
        var nonExistentRowId = 99999L;

        // Act
        var result = await useCase.ExecuteAsync(nonExistentRowId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Test2_2_GetEmployeeById_WithZeroId_ThrowsException()
    {
        // Arrange
        var (useCase, _) = CreateUseCase();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => useCase.ExecuteAsync(0L));
    }

    #endregion

    #region ヘルパーメソッド

    private (GetEmployeeByIdUseCase, SupportAdvance.Contexts.Employee.Application.Repositories.IEmployeeRepository) CreateUseCase()
    {
        var fixedDateTime = new DateTime(2026, 8, 11, 0, 0, 0, DateTimeKind.Unspecified);
        var clock = new MockClock(fixedDateTime);
        var mapper = new EmployeeMapper();
        var repository = new EmployeeRepository(mapper, clock);
        var useCase = new GetEmployeeByIdUseCase(repository);
        return (useCase, repository);
    }

    #endregion
}
