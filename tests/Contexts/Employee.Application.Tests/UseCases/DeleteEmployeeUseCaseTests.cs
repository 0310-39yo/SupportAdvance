namespace SupportAdvance.Contexts.Employee.Application.Tests.UseCases;

using SupportAdvance.Contexts.Employee.Application.UseCases;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Person;
using SupportAdvance.Contexts.Employee.Infrastructure.Repositories;
using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using Xunit;

/// <summary>
/// DeleteEmployeeUseCase の単体テスト
/// </summary>
public class DeleteEmployeeUseCaseTests
{
    private readonly IClock _clock = new SystemClock();

    #region グループ 1: 正常系

    [Fact]
    public async Task VO_EXEC_01_DeleteEmployee_WithValidId_LogicallyDeletes()
    {
        // Arrange
        var (useCase, repository) = CreateUseCase();
        var rowId = EmployeeRowId.From(1L);
        var person = Person.Create(PersonRowId.From(100L), LastName.From("山田"), FirstName.From("太郎"), LastNameKana.From("ヤマダ"), FirstNameKana.From("タロウ"));
        var employee = Employee.Create(
            rowId,
            BizDivision.RegularEmployee(),
            BizId.From(1001),
            BizCode.From(BizDivision.RegularEmployee(), BizId.From(1001)),
            null,
            person,
            new List<DepartmentMembership>(),
            _clock
        );
        await repository.AddAsync(employee);

        // Act
        await useCase.ExecuteAsync(rowId.Value);

        // Act: 削除後に取得
        var retrieved = await repository.GetByIdAsync(rowId);

        // Assert
        Assert.Null(retrieved);  // 論理削除されたため null
    }

    #endregion

    #region グループ 2: 異常系

    [Fact]
    public async Task VO_ERROR_01_DeleteEmployee_WithNonExistentId_ThrowsException()
    {
        // Arrange
        var (useCase, _) = CreateUseCase();
        var nonExistentRowId = 99999L;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.ExecuteAsync(nonExistentRowId));
    }

    [Fact]
    public async Task VO_ERROR_02_DeleteEmployee_WithZeroId_ThrowsException()
    {
        // Arrange
        var (useCase, _) = CreateUseCase();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(0L));
    }

    #endregion

    #region ヘルパーメソッド

    private (DeleteEmployeeUseCase, SupportAdvance.Contexts.Employee.Application.Repositories.IEmployeeRepository) CreateUseCase()
    {
        var repository = new MockEmployeeRepository();
        var useCase = new DeleteEmployeeUseCase(repository);
        return (useCase, repository);
    }

    #endregion

    /// <summary>
    /// テスト用インメモリ Repository 実装（Application層の単体テスト用、DB接続不要）
    /// </summary>
    private class MockEmployeeRepository : SupportAdvance.Contexts.Employee.Application.Repositories.IEmployeeRepository
    {
        private readonly Dictionary<long, Employee> _store = new();

        public Task AddAsync(Employee employee)
        {
            _store[employee.RowId.Value] = employee;
            return Task.CompletedTask;
        }

        public Task SaveAsync(Employee employee)
        {
            _store[employee.RowId.Value] = employee;
            return Task.CompletedTask;
        }

        public Task<Employee?> GetByIdAsync(EmployeeRowId id) =>
            Task.FromResult(_store.GetValueOrDefault(id.Value));

        public Task<Employee?> GetByRowIdAsync(EmployeeRowId rowId) => GetByIdAsync(rowId);

        public Task<Employee?> GetByBizIdAsync(int bizId) =>
            Task.FromResult(_store.Values.FirstOrDefault(e => e.BizId.Value == bizId));

        public Task<IReadOnlyList<Employee>> GetByPersonRowIdAsync(PersonRowId personRowId) =>
            Task.FromResult<IReadOnlyList<Employee>>(
                _store.Values.Where(e => e.Person.RowId == personRowId).ToList());

        public Task UpdateAsync(Employee employee)
        {
            _store[employee.RowId.Value] = employee;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(EmployeeRowId id)
        {
            _store.Remove(id.Value);
            return Task.CompletedTask;
        }
    }
}
