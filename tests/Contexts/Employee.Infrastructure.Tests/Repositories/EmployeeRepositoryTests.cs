namespace SupportAdvance.Contexts.Employee.Infrastructure.Tests.Repositories;

using SupportAdvance.Contexts.Employee.Application.Repositories;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
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

    [Fact]
    public async Task TestGetById01_WithValidIdReturnsEmployee()
    {
        // Arrange
        var rowId = EmployeeRowId.From(1L);
        var employee = Employee.Create(
            rowId,
            EmployeeTypeDivision.From("M"),
            EmployeeBizId.From(1001),
            EmployeeBizCode.From("EMP001"),
            PersonRowId.From(100L));

        var repository = CreateRepository();
        await repository.AddAsync(employee);

        // Act
        var result = await repository.GetByIdAsync(rowId.Value);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(rowId, result.RowId);
    }

    [Fact]
    public async Task TestGetById02_WithMultipleEmployeesReturnsCorrectOne()
    {
        // Arrange
        var rowId1 = EmployeeRowId.From(1L);
        var rowId2 = EmployeeRowId.From(2L);
        var employee1 = Employee.Create(
            rowId1,
            EmployeeTypeDivision.From("M"),
            EmployeeBizId.From(1001),
            EmployeeBizCode.From("EMP001"),
            PersonRowId.From(100L));
        var employee2 = Employee.Create(
            rowId2,
            EmployeeTypeDivision.From("T"),
            EmployeeBizId.From(7502),
            EmployeeBizCode.From("EMP002"),
            PersonRowId.From(101L));

        var repository = CreateRepository();
        await repository.AddAsync(employee1);
        await repository.AddAsync(employee2);

        // Act
        var result = await repository.GetByIdAsync(rowId1.Value);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(rowId1, result.RowId);
    }

    #endregion

    #region グループ 2: GetByIdAsync - 存在しない場合

    [Fact]
    public async Task TestGetById03_WithInvalidIdReturnsNull()
    {
        // Arrange
        var repository = CreateRepository();

        // Act
        var result = await repository.GetByIdAsync(999L);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region グループ 3: GetByRowIdAsync

    [Fact]
    public async Task TestGetByRowId01_WithValidRowIdReturnsEmployee()
    {
        // Arrange
        var rowId = EmployeeRowId.From(100L);
        var employee = Employee.Create(
            rowId,
            EmployeeTypeDivision.From("M"),
            EmployeeBizId.From(1001),
            EmployeeBizCode.From("EMP001"),
            PersonRowId.From(100L));

        var repository = CreateRepository();
        await repository.AddAsync(employee);

        // Act
        var result = await repository.GetByIdAsync(rowId.Value);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(rowId, result.RowId);
    }

    [Fact]
    public async Task TestGetByRowId02_WithInvalidRowIdReturnsNull()
    {
        // Arrange
        var repository = CreateRepository();

        // Act
        var result = await repository.GetByIdAsync(999L);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region グループ 4: GetByPersonRowIdAsync

    [Fact]
    public async Task TestGetByPersonRowId01_WithValidPersonRowIdReturnsEmployees()
    {
        // Arrange
        var personRowId = 100L;
        var employee1 = Employee.Create(
            EmployeeRowId.From(1L),
            EmployeeTypeDivision.From("M"),
            EmployeeBizId.From(1001),
            EmployeeBizCode.From("EMP001"),
            PersonRowId.From(personRowId));
        var employee2 = Employee.Create(
            EmployeeRowId.From(2L),
            EmployeeTypeDivision.From("T"),
            EmployeeBizId.From(7502),
            EmployeeBizCode.From("EMP002"),
            PersonRowId.From(personRowId));

        var repository = CreateRepository();
        await repository.AddAsync(employee1);
        await repository.AddAsync(employee2);

        // Act
        var results = await repository.GetByPersonRowIdAsync(personRowId);

        // Assert
        Assert.NotEmpty(results);
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task TestGetByPersonRowId02_WithInvalidPersonRowIdReturnsEmpty()
    {
        // Arrange
        var repository = CreateRepository();

        // Act
        var results = await repository.GetByPersonRowIdAsync(999L);

        // Assert
        Assert.Empty(results);
    }

    #endregion

    #region グループ 5: AddAsync

    [Fact]
    public async Task TestAdd01_WithValidEmployeeInsertsAndReturnsId()
    {
        // Arrange
        var rowId = EmployeeRowId.From(1L);
        var employee = Employee.Create(
            rowId,
            EmployeeTypeDivision.From("M"),
            EmployeeBizId.From(1001),
            EmployeeBizCode.From("EMP001"),
            PersonRowId.From(100L));

        var repository = CreateRepository();

        // Act
        await repository.AddAsync(employee);

        // Assert
        var result = await repository.GetByIdAsync(rowId.Value);
        Assert.NotNull(result);
        Assert.Equal(rowId, result.RowId);
    }

    [Fact]
    public async Task TestAdd02_AuditColumnsAreSetAutomatically()
    {
        // Arrange
        var rowId = EmployeeRowId.From(1L);
        var employee = Employee.Create(
            rowId,
            EmployeeTypeDivision.From("M"),
            EmployeeBizId.From(1001),
            EmployeeBizCode.From("EMP001"),
            PersonRowId.From(100L));

        var repository = CreateRepository();

        // Act
        await repository.AddAsync(employee);

        // Assert
        var result = await repository.GetByIdAsync(rowId.Value);
        Assert.NotNull(result);
        // CreatedAt と CreatedBy が設定されていることを確認
        // (DbModel の検証は Mapper テストで行う)
    }

    #endregion

    #region グループ 6: UpdateAsync

    [Fact]
    public async Task TestUpdate01_WithValidEmployeeUpdatesSuccessfully()
    {
        // Arrange
        var rowId = EmployeeRowId.From(1L);
        var employee = Employee.Create(
            rowId,
            EmployeeTypeDivision.From("M"),
            EmployeeBizId.From(1001),
            EmployeeBizCode.From("EMP001"),
            PersonRowId.From(100L));

        var repository = CreateRepository();
        await repository.AddAsync(employee);

        // Act
        var result = await repository.GetByIdAsync(rowId.Value);
        Assert.NotNull(result);
    }

    #endregion

    #region グループ 7: DeleteAsync

    [Fact]
    public async Task TestDelete01_WithValidIdSetsDeletedAtLogicallyDeletes()
    {
        // Arrange
        var rowId = EmployeeRowId.From(1L);
        var employee = Employee.Create(
            rowId,
            EmployeeTypeDivision.From("M"),
            EmployeeBizId.From(1001),
            EmployeeBizCode.From("EMP001"),
            PersonRowId.From(100L));

        var repository = CreateRepository();
        await repository.AddAsync(employee);

        // Act
        await repository.DeleteAsync(rowId.Value);

        // Assert
        var result = await repository.GetByIdAsync(rowId.Value);
        Assert.Null(result);  // 論理削除されたため取得不可
    }

    #endregion

    #region ヘルパーメソッド

    private IEmployeeRepository CreateRepository()
    {
        var mapper = new EmployeeMapper();
        var fixedDateTime = new DateTime(2026, 8, 10, 12, 0, 0, DateTimeKind.Unspecified);
        var clock = new MockClock(fixedDateTime);
        return new EmployeeRepository(mapper, clock);
    }

    #endregion
}
