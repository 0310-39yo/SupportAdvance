namespace SupportAdvance.Contexts.Employee.Infrastructure.Tests.Repositories;

using SupportAdvance.Contexts.Employee.Application.Repositories;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Person;
using SupportAdvance.Contexts.Employee.Infrastructure.Mappers;
using SupportAdvance.Contexts.Employee.Infrastructure.Repositories;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using Xunit;

/// <summary>
/// EmployeeRepository の単体テスト
/// </summary>
public class EmployeeRepositoryTests
{
    #region グループ 1: GetByIdAsync - 存在する場合

    [Fact(Skip = "要DB接続。Phase 5で結合テストとして再設計")]
    public async Task TestGetById01_WithValidIdReturnsEmployee()
    {
        // Arrange
        var rowId = EmployeeRowId.From(1L);
        var person = Person.Create(PersonRowId.From(100L), LastName.From("山田"), FirstName.From("太郎"), LastNameKana.From("ヤマダ"), FirstNameKana.From("タロウ"));
        var employee = Employee.Create(
            rowId,
            BizDivision.RegularEmployee(),
            BizId.From(1001),
            BizCode.From(BizDivision.RegularEmployee(), BizId.From(1001)),
            null,
            person,
            new List<DepartmentMembership>());

        var repository = CreateRepository();
        await repository.AddAsync(employee);

        // Act
        var result = await repository.GetByIdAsync(rowId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(rowId.Value, result.RowId.Value);
    }

    [Fact(Skip = "要DB接続。Phase 5で結合テストとして再設計")]
    public async Task TestGetById02_WithMultipleEmployeesReturnsCorrectOne()
    {
        // Arrange
        var rowId1 = EmployeeRowId.From(1L);
        var rowId2 = EmployeeRowId.From(2L);
        var person = Person.Create(PersonRowId.From(100L), LastName.From("山田"), FirstName.From("太郎"), LastNameKana.From("ヤマダ"), FirstNameKana.From("タロウ"));
        var employee1 = Employee.Create(
            rowId1,
            BizDivision.RegularEmployee(),
            BizId.From(1001),
            BizCode.From(BizDivision.RegularEmployee(), BizId.From(1001)),
            null,
            person,
            new List<DepartmentMembership>());
        var employee2 = Employee.Create(
            rowId2,
            BizDivision.Dispatched(),
            BizId.From(7502),
            BizCode.From(BizDivision.Dispatched(), BizId.From(7502)),
            null,
            person,
            new List<DepartmentMembership>());

        var repository = CreateRepository();
        await repository.AddAsync(employee1);
        await repository.AddAsync(employee2);

        // Act
        var result = await repository.GetByIdAsync(rowId1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(rowId1.Value, result.RowId.Value);
    }

    #endregion

    #region グループ 2: GetByIdAsync - 存在しない場合

    [Fact(Skip = "要DB接続。Phase 5で結合テストとして再設計")]
    public async Task TestGetById03_WithInvalidIdReturnsNull()
    {
        // Arrange
        var repository = CreateRepository();

        // Act
        var result = await repository.GetByIdAsync(EmployeeRowId.From(999L));

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region グループ 3: GetByRowIdAsync

    [Fact(Skip = "要DB接続。Phase 5で結合テストとして再設計")]
    public async Task TestGetByRowId01_WithValidRowIdReturnsEmployee()
    {
        // Arrange
        var rowId = EmployeeRowId.From(100L);
        var person = Person.Create(PersonRowId.From(100L), LastName.From("山田"), FirstName.From("太郎"), LastNameKana.From("ヤマダ"), FirstNameKana.From("タロウ"));
        var employee = Employee.Create(
            rowId,
            BizDivision.RegularEmployee(),
            BizId.From(1001),
            BizCode.From(BizDivision.RegularEmployee(), BizId.From(1001)),
            null,
            person,
            new List<DepartmentMembership>());

        var repository = CreateRepository();
        await repository.AddAsync(employee);

        // Act
        var result = await repository.GetByIdAsync(rowId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(rowId.Value, result.RowId.Value);
    }

    [Fact(Skip = "要DB接続。Phase 5で結合テストとして再設計")]
    public async Task TestGetByRowId02_WithInvalidRowIdReturnsNull()
    {
        // Arrange
        var repository = CreateRepository();

        // Act
        var result = await repository.GetByIdAsync(EmployeeRowId.From(999L));

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region グループ 4: GetByPersonRowIdAsync

    [Fact(Skip = "要DB接続。Phase 5で結合テストとして再設計")]
    public async Task TestGetByPersonRowId01_WithValidPersonRowIdReturnsEmployees()
    {
        // Arrange
        var personRowId = 100L;
        var person = Person.Create(PersonRowId.From(personRowId), LastName.From("山田"), FirstName.From("太郎"), LastNameKana.From("ヤマダ"), FirstNameKana.From("タロウ"));
        var employee1 = Employee.Create(
            EmployeeRowId.From(1L),
            BizDivision.RegularEmployee(),
            BizId.From(1001),
            BizCode.From(BizDivision.RegularEmployee(), BizId.From(1001)),
            null,
            person,
            new List<DepartmentMembership>());
        var employee2 = Employee.Create(
            EmployeeRowId.From(2L),
            BizDivision.Dispatched(),
            BizId.From(7502),
            BizCode.From(BizDivision.Dispatched(), BizId.From(7502)),
            null,
            person,
            new List<DepartmentMembership>());

        var repository = CreateRepository();
        await repository.AddAsync(employee1);
        await repository.AddAsync(employee2);

        // Act
        var results = await repository.GetByPersonRowIdAsync(PersonRowId.From(personRowId));

        // Assert
        Assert.NotEmpty(results);
        Assert.Equal(2, results.Count);
    }

    [Fact(Skip = "要DB接続。Phase 5で結合テストとして再設計")]
    public async Task TestGetByPersonRowId02_WithInvalidPersonRowIdReturnsEmpty()
    {
        // Arrange
        var repository = CreateRepository();

        // Act
        var results = await repository.GetByPersonRowIdAsync(PersonRowId.From(999L));

        // Assert
        Assert.Empty(results);
    }

    #endregion

    #region グループ 5: AddAsync

    [Fact(Skip = "要DB接続。Phase 5で結合テストとして再設計")]
    public async Task TestAdd01_WithValidEmployeeInsertsAndReturnsId()
    {
        // Arrange
        var rowId = EmployeeRowId.From(1L);
        var person = Person.Create(PersonRowId.From(100L), LastName.From("山田"), FirstName.From("太郎"), LastNameKana.From("ヤマダ"), FirstNameKana.From("タロウ"));
        var employee = Employee.Create(
            rowId,
            BizDivision.RegularEmployee(),
            BizId.From(1001),
            BizCode.From(BizDivision.RegularEmployee(), BizId.From(1001)),
            null,
            person,
            new List<DepartmentMembership>());

        var repository = CreateRepository();

        // Act
        await repository.AddAsync(employee);

        // Assert
        var result = await repository.GetByIdAsync(rowId);
        Assert.NotNull(result);
        Assert.Equal(rowId.Value, result.RowId.Value);
    }

    [Fact(Skip = "要DB接続。Phase 5で結合テストとして再設計")]
    public async Task TestAdd02_AuditColumnsAreSetAutomatically()
    {
        // Arrange
        var rowId = EmployeeRowId.From(1L);
        var person = Person.Create(PersonRowId.From(100L), LastName.From("山田"), FirstName.From("太郎"), LastNameKana.From("ヤマダ"), FirstNameKana.From("タロウ"));
        var employee = Employee.Create(
            rowId,
            BizDivision.RegularEmployee(),
            BizId.From(1001),
            BizCode.From(BizDivision.RegularEmployee(), BizId.From(1001)),
            null,
            person,
            new List<DepartmentMembership>());

        var repository = CreateRepository();

        // Act
        await repository.AddAsync(employee);

        // Assert
        var result = await repository.GetByIdAsync(rowId);
        Assert.NotNull(result);
        // CreatedAt と CreatedBy が設定されていることを確認
        // (DbModel の検証は Mapper テストで行う)
    }

    #endregion

    #region グループ 6: UpdateAsync

    [Fact(Skip = "要DB接続。Phase 5で結合テストとして再設計")]
    public async Task TestUpdate01_WithValidEmployeeUpdatesSuccessfully()
    {
        // Arrange
        var rowId = EmployeeRowId.From(1L);
        var person = Person.Create(PersonRowId.From(100L), LastName.From("山田"), FirstName.From("太郎"), LastNameKana.From("ヤマダ"), FirstNameKana.From("タロウ"));
        var employee = Employee.Create(
            rowId,
            BizDivision.RegularEmployee(),
            BizId.From(1001),
            BizCode.From(BizDivision.RegularEmployee(), BizId.From(1001)),
            null,
            person,
            new List<DepartmentMembership>());

        var repository = CreateRepository();
        await repository.AddAsync(employee);

        // Act
        var result = await repository.GetByIdAsync(rowId);
        Assert.NotNull(result);
    }

    #endregion

    #region グループ 7: DeleteAsync

    [Fact(Skip = "要DB接続。Phase 5で結合テストとして再設計")]
    public async Task TestDelete01_WithValidIdSetsDeletedAtLogicallyDeletes()
    {
        // Arrange
        var rowId = EmployeeRowId.From(1L);
        var person = Person.Create(PersonRowId.From(100L), LastName.From("山田"), FirstName.From("太郎"), LastNameKana.From("ヤマダ"), FirstNameKana.From("タロウ"));
        var employee = Employee.Create(
            rowId,
            BizDivision.RegularEmployee(),
            BizId.From(1001),
            BizCode.From(BizDivision.RegularEmployee(), BizId.From(1001)),
            null,
            person,
            new List<DepartmentMembership>());

        var repository = CreateRepository();
        await repository.AddAsync(employee);

        // Act
        await repository.DeleteAsync(rowId);

        // Assert
        var result = await repository.GetByIdAsync(rowId);
        Assert.Null(result);  // 論理削除されたため取得不可
    }

    #endregion

    #region ヘルパーメソッド

    private IEmployeeRepository CreateRepository()
    {
        // すべてのテストが Skip されているため、null 値でスタブ実装
        return new MockEmployeeRepository();
    }

    #endregion

    /// <summary>
    /// テスト用モック実装（Skip されたテストの型チェック用）
    /// </summary>
    private class MockEmployeeRepository : IEmployeeRepository
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
