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
        var person = Person.Create(PersonRowId.From(personRowId), PersonLastName.From("山田"), PersonFirstName.From("太郎"), PersonLastNameKana.From("ヤマダ"), PersonFirstNameKana.From("タロウ"));
        var employee1 = Employee.Create(
            EmployeeRowId.From(1L),
            EmployeeTypeDivision.RegularEmployee(),
            EmployeeBizId.From(1001),
            EmployeeBizCode.From(EmployeeTypeDivision.RegularEmployee(), EmployeeBizId.From(1001)),
            null,
            person,
            new List<DepartmentMembership>()
        );
        var employee2 = Employee.Create(
            EmployeeRowId.From(2L),
            EmployeeTypeDivision.Dispatched(),
            EmployeeBizId.From(7502),
            EmployeeBizCode.From(EmployeeTypeDivision.Dispatched(), EmployeeBizId.From(7502)),
            null,
            person,
            new List<DepartmentMembership>()
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
        var mapper = new EmployeeMapper(clock);
        var repository = new EmployeeRepository(mapper, clock);
        var useCase = new GetEmployeesByPersonRowIdUseCase(repository);
        return (useCase, repository);
    }

    #endregion
}
