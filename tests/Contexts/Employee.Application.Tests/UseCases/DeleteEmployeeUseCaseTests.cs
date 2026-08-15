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
/// DeleteEmployeeUseCase の単体テスト
/// </summary>
public class DeleteEmployeeUseCaseTests
{
    #region グループ 1: 正常系

    [Fact]
    public async Task Test1_1_DeleteEmployee_WithValidId_LogicallyDeletes()
    {
        // Arrange
        var (useCase, repository) = CreateUseCase();
        var rowId = EmployeeRowId.From(1L);
        var person = Person.Create(PersonRowId.From(100L), PersonLastName.From("山田"), PersonFirstName.From("太郎"), PersonLastNameKana.From("ヤマダ"), PersonFirstNameKana.From("タロウ"));
        var employee = Employee.Create(
            rowId,
            EmployeeTypeDivision.RegularEmployee(),
            EmployeeBizId.From(1001),
            EmployeeBizCode.From(EmployeeTypeDivision.RegularEmployee(), EmployeeBizId.From(1001)),
            null,
            person,
            new List<DepartmentMembership>()
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
    public async Task Test2_1_DeleteEmployee_WithNonExistentId_ThrowsException()
    {
        // Arrange
        var (useCase, _) = CreateUseCase();
        var nonExistentRowId = 99999L;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.ExecuteAsync(nonExistentRowId));
    }

    [Fact]
    public async Task Test2_2_DeleteEmployee_WithZeroId_ThrowsException()
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
        var fixedDateTime = new DateTime(2026, 8, 11, 0, 0, 0, DateTimeKind.Unspecified);
        var clock = new MockClock(fixedDateTime);
        var mapper = new EmployeeMapper(clock);
        var repository = new EmployeeRepository(mapper, clock);
        var useCase = new DeleteEmployeeUseCase(repository);
        return (useCase, repository);
    }

    #endregion
}
