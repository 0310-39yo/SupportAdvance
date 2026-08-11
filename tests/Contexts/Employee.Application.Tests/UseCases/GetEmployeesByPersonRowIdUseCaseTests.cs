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
/// GetEmployeesByPersonRowIdUseCase の単体テスト
/// </summary>
public class GetEmployeesByPersonRowIdUseCaseTests
{
    #region グループ 1: 正常系

    [Fact]
    public async Task Test1_1_GetEmployeesByPersonRowId_WithValidId_ReturnsEmployees()
    {
        // Arrange
        var (useCase, repository) = CreateUseCase();
        var personRowId = 100L;
        var employee1 = Employee.Create(
            EmployeeRowId.From(1L),
            EmployeeTypeDivision.From("M"),
            EmployeeBizId.From(1001),
            EmployeeBizCode.From("EMP001"),
            PersonRowId.From(personRowId)
        );
        var employee2 = Employee.Create(
            EmployeeRowId.From(2L),
            EmployeeTypeDivision.From("T"),
            EmployeeBizId.From(7502),
            EmployeeBizCode.From("EMP002"),
            PersonRowId.From(personRowId)
        );
        await repository.AddAsync(employee1);
        await repository.AddAsync(employee2);

        // Act
        var results = await useCase.ExecuteAsync(personRowId);

        // Assert
        Assert.NotEmpty(results);
        Assert.Equal(2, results.Count);
    }

    #endregion

    #region グループ 2: 異常系

    [Fact]
    public async Task Test2_1_GetEmployeesByPersonRowId_WithNonExistentId_ReturnsEmpty()
    {
        // Arrange
        var (useCase, _) = CreateUseCase();
        var nonExistentPersonRowId = 999L;

        // Act
        var results = await useCase.ExecuteAsync(nonExistentPersonRowId);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task Test2_2_GetEmployeesByPersonRowId_WithInvalidPersonRowId_ThrowsException()
    {
        // Arrange
        var (useCase, _) = CreateUseCase();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(0));
    }

    #endregion

    #region ヘルパーメソッド

    private (GetEmployeesByPersonRowIdUseCase, SupportAdvance.Contexts.Employee.Application.Repositories.IEmployeeRepository) CreateUseCase()
    {
        var fixedDateTime = new DateTime(2026, 8, 11, 0, 0, 0, DateTimeKind.Unspecified);
        var clock = new MockClock(fixedDateTime);
        var mapper = new EmployeeMapper();
        var repository = new EmployeeRepository(mapper, clock);
        var useCase = new GetEmployeesByPersonRowIdUseCase(repository);
        return (useCase, repository);
    }

    #endregion
}
