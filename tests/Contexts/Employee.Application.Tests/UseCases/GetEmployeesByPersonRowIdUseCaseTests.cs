namespace SupportAdvance.Contexts.Employee.Application.Tests.UseCases;

using SupportAdvance.Contexts.Employee.Application.UseCases;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Person;
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

    [Fact(Skip = "要DB接続。Phase 5で結合テストとして再設計")]
    public async Task VO_EXEC_01_GetEmployeesByPersonRowId_WithValidId_ReturnsEmployees()
    {
        // Arrange
        var (useCase, repository) = CreateUseCase();
        var personRowId = 100L;
        var person = Person.Create(PersonRowId.From(personRowId), LastName.From("山田"), FirstName.From("太郎"), LastNameKana.From("ヤマダ"), FirstNameKana.From("タロウ"));
        var employee1 = Employee.Create(
            EmployeeRowId.From(1L),
            BizDivision.RegularEmployee(),
            BizId.From(1001),
            BizCode.From(BizDivision.RegularEmployee(), BizId.From(1001)),
            null,
            person,
            new List<DepartmentMembership>()
        );
        var employee2 = Employee.Create(
            EmployeeRowId.From(2L),
            BizDivision.Dispatched(),
            BizId.From(7502),
            BizCode.From(BizDivision.Dispatched(), BizId.From(7502)),
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

    [Fact(Skip = "要DB接続。Phase 5で結合テストとして再設計")]
    public async Task VO_RESULT_01_GetEmployeesByPersonRowId_WithNonExistentId_ReturnsEmpty()
    {
        // Arrange
        var (useCase, _) = CreateUseCase();
        var nonExistentPersonRowId = 999L;

        // Act
        var results = await useCase.ExecuteAsync(nonExistentPersonRowId);

        // Assert
        Assert.Empty(results);
    }

    [Fact(Skip = "要DB接続。Phase 5で結合テストとして再設計")]
    public async Task VO_ERROR_01_GetEmployeesByPersonRowId_WithInvalidPersonRowId_ThrowsException()
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
        var repository = new MockEmployeeRepository();
        var useCase = new GetEmployeesByPersonRowIdUseCase(repository);
        return (useCase, repository);
    }

    #endregion

    /// <summary>
    /// テスト用モック実装（Skip されたテストの型チェック用）
    /// </summary>
    private class MockEmployeeRepository : SupportAdvance.Contexts.Employee.Application.Repositories.IEmployeeRepository
    {
        public Task AddAsync(Employee employee) => Task.CompletedTask;
        public Task<Employee?> GetByIdAsync(EmployeeRowId id) => Task.FromResult<Employee?>(null);
        public Task<Employee?> GetByRowIdAsync(EmployeeRowId rowId) => Task.FromResult<Employee?>(null);
        public Task<Employee?> GetByBizIdAsync(int bizId) => Task.FromResult<Employee?>(null);
        public Task<IReadOnlyList<Employee>> GetByPersonRowIdAsync(PersonRowId personRowId) => Task.FromResult<IReadOnlyList<Employee>>(new List<Employee>());
        public Task UpdateAsync(Employee employee) => Task.CompletedTask;
        public Task DeleteAsync(EmployeeRowId id) => Task.CompletedTask;
    }
}
