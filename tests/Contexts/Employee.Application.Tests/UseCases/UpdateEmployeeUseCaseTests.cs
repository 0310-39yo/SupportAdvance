namespace SupportAdvance.Contexts.Employee.Application.Tests.UseCases;

using SupportAdvance.Contexts.Employee.Application.Dtos;
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
/// UpdateEmployeeUseCase の単体テスト
/// </summary>
public class UpdateEmployeeUseCaseTests
{
    #region グループ 1: 正常系

    [Fact(Skip = "要DB接続。Phase 5で結合テストとして再設計")]
    public async Task VO_EXEC_01_UpdateEmployee_WithValidRequest_Updates()
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
            new List<DepartmentMembership>()
        );
        await repository.AddAsync(employee);

        var request = new UpdateEmployeeRequest
        {
            EmployeeRowId = rowId.Value,
            DivisionCode = "T",
            EmployeeNumber = 7501  // 派遣社員は 7500-7999 の範囲
        };

        // Act
        var result = await useCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(rowId.Value, result.RowId);
    }

    #endregion

    #region グループ 2: 異常系

    [Fact(Skip = "要DB接続。Phase 5で結合テストとして再設計")]
    public async Task VO_ERROR_01_UpdateEmployee_WithNonExistentId_ThrowsException()
    {
        // Arrange
        var (useCase, _) = CreateUseCase();
        var request = new UpdateEmployeeRequest
        {
            EmployeeRowId = 99999L,
            DivisionCode = "T",
            EmployeeNumber = 7501
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.ExecuteAsync(request));
    }

    [Fact(Skip = "要DB接続。Phase 5で結合テストとして再設計")]
    public async Task VO_ERROR_02_UpdateEmployee_WithInvalidDivisionCode_ThrowsException()
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
            new List<DepartmentMembership>()
        );
        await repository.AddAsync(employee);

        var request = new UpdateEmployeeRequest
        {
            EmployeeRowId = rowId.Value,
            DivisionCode = "X",  // ← 無効
            EmployeeNumber = 1002
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(request));
    }

    [Fact(Skip = "要DB接続。Phase 5で結合テストとして再設計")]
    public async Task Test2_3_UpdateEmployee_WithInvalidEmployeeNumber_ThrowsException()
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
            new List<DepartmentMembership>()
        );
        await repository.AddAsync(employee);

        var request = new UpdateEmployeeRequest
        {
            EmployeeRowId = rowId.Value,
            DivisionCode = "M",
            EmployeeNumber = 1000  // ← 無効（1001以上）
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(request));
    }

    [Fact(Skip = "要DB接続。Phase 5で結合テストとして再設計")]
    public async Task Test2_4_UpdateEmployee_WithZeroId_ThrowsException()
    {
        // Arrange
        var (useCase, _) = CreateUseCase();
        var request = new UpdateEmployeeRequest
        {
            EmployeeRowId = 0L,
            DivisionCode = "T",
            EmployeeNumber = 2001
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(request));
    }

    #endregion

    #region ヘルパーメソッド

    private (UpdateEmployeeUseCase, SupportAdvance.Contexts.Employee.Application.Repositories.IEmployeeRepository) CreateUseCase()
    {
        var fixedDateTime = new DateTime(2026, 8, 11, 0, 0, 0, DateTimeKind.Unspecified);
        var clock = new MockClock(fixedDateTime);
        var mapper = new EmployeeMapper();
        var repository = new MockEmployeeRepository();
        var useCase = new UpdateEmployeeUseCase(repository);
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
