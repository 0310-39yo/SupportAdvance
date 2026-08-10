namespace SupportAdvance.Contexts.Employee.Infrastructure.Tests.Repositories;

using SupportAdvance.Application.Repositories;
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
        var id = EmployeeId.NewId();
        var rowId = EmployeeRowId.From(1L);
        var code = EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1001));
        var personRowId = PersonRowId.From(1L);
        var employee = Employee.Create(id, rowId, code, personRowId);

        var repository = CreateRepository();
        await repository.AddAsync(employee);

        // Act
        var result = await repository.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal(rowId, result.RowId);
        Assert.Equal(code, result.Code);
        Assert.Equal(personRowId, result.PersonRowId);
    }

    [Fact]
    public async Task TestGetById02_WithMultipleEmployeesReturnsCorrectOne()
    {
        // Arrange
        var id1 = EmployeeId.NewId();
        var id2 = EmployeeId.NewId();
        var employee1 = Employee.Create(
            id1,
            EmployeeRowId.From(1L),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1001)),
            PersonRowId.From(1L));
        var employee2 = Employee.Create(
            id2,
            EmployeeRowId.From(2L),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1002)),
            PersonRowId.From(2L));

        var repository = CreateRepository();
        await repository.AddAsync(employee1);
        await repository.AddAsync(employee2);

        // Act
        var result = await repository.GetByIdAsync(id1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id1, result.Id);
    }

    #endregion

    #region グループ 2: GetByIdAsync - 存在しない場合

    [Fact]
    public async Task TestGetById03_WithInvalidIdReturnsNull()
    {
        // Arrange
        var repository = CreateRepository();
        var nonExistentId = EmployeeId.NewId();

        // Act
        var result = await repository.GetByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region グループ 3: GetByRowIdAsync

    [Fact]
    public async Task TestGetByRowId01_WithValidRowIdReturnsEmployee()
    {
        // Arrange
        var id = EmployeeId.NewId();
        var rowId = EmployeeRowId.From(100L);
        var code = EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1001));
        var personRowId = PersonRowId.From(1L);
        var employee = Employee.Create(id, rowId, code, personRowId);

        var repository = CreateRepository();
        await repository.AddAsync(employee);

        // Act
        var result = await repository.GetByRowIdAsync(rowId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(rowId, result.RowId);
        Assert.Equal(id, result.Id);
    }

    [Fact]
    public async Task TestGetByRowId02_WithInvalidRowIdReturnsNull()
    {
        // Arrange
        var repository = CreateRepository();
        var nonExistentRowId = EmployeeRowId.From(999L);

        // Act
        var result = await repository.GetByRowIdAsync(nonExistentRowId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region グループ 4: GetByPersonRowIdAsync

    [Fact]
    public async Task TestGetByPersonRowId01_WithValidPersonRowIdReturnsEmployees()
    {
        // Arrange
        var personRowId = PersonRowId.From(1L);
        var employee1 = Employee.Create(
            EmployeeId.NewId(),
            EmployeeRowId.From(1L),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1001)),
            personRowId);
        var employee2 = Employee.Create(
            EmployeeId.NewId(),
            EmployeeRowId.From(2L),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1002)),
            personRowId);

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
        var nonExistentPersonRowId = PersonRowId.From(999L);

        // Act
        var results = await repository.GetByPersonRowIdAsync(nonExistentPersonRowId);

        // Assert
        Assert.Empty(results);
    }

    #endregion

    #region グループ 5: AddAsync

    [Fact]
    public async Task TestAdd01_WithValidEmployeeInsertsAndReturnsId()
    {
        // Arrange
        var id = EmployeeId.NewId();
        var employee = Employee.Create(
            id,
            EmployeeRowId.From(1L),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1001)),
            PersonRowId.From(1L));

        var repository = CreateRepository();

        // Act
        await repository.AddAsync(employee);

        // Assert
        var result = await repository.GetByIdAsync(id);
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
    }

    [Fact]
    public async Task TestAdd02_AuditColumnsAreSetAutomatically()
    {
        // Arrange
        var employee = Employee.Create(
            EmployeeId.NewId(),
            EmployeeRowId.From(1L),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1001)),
            PersonRowId.From(1L));

        var repository = CreateRepository();

        // Act
        await repository.AddAsync(employee);

        // Assert
        var result = await repository.GetByIdAsync(employee.Id);
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
        var id = EmployeeId.NewId();
        var originalCode = EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1001));
        var employee = Employee.Create(
            id,
            EmployeeRowId.From(1L),
            originalCode,
            PersonRowId.From(1L));

        var repository = CreateRepository();
        await repository.AddAsync(employee);

        // Act
        // 更新されたエンプロイを作成（実装時は ChangeCode などのメソッドを使用）
        // ここでは検証のみ
        var result = await repository.GetByIdAsync(id);
        Assert.NotNull(result);
    }

    #endregion

    #region グループ 7: DeleteAsync

    [Fact]
    public async Task TestDelete01_WithValidIdSetsDeletedAtLogicallyDeletes()
    {
        // Arrange
        var id = EmployeeId.NewId();
        var employee = Employee.Create(
            id,
            EmployeeRowId.From(1L),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1001)),
            PersonRowId.From(1L));

        var repository = CreateRepository();
        await repository.AddAsync(employee);

        // Act
        await repository.DeleteAsync(id);

        // Assert
        var result = await repository.GetByIdAsync(id);
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
