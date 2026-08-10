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
        var id = EmployeeId.NewId();
        var employee = Employee.Create(
            id,
            EmployeeRowId.From(1),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1001)),
            PersonRowId.From(1)
        );
        await repository.AddAsync(employee);

        // Act
        await useCase.ExecuteAsync(id.Value);

        // Act: 削除後に取得
        var retrieved = await repository.GetByIdAsync(id);

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
        var nonExistentId = EmployeeId.NewId();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.ExecuteAsync(nonExistentId.Value));
    }

    [Fact]
    public async Task Test2_2_DeleteEmployee_WithEmptyGuid_ThrowsException()
    {
        // Arrange
        var (useCase, _) = CreateUseCase();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(Guid.Empty));
    }

    #endregion

    #region ヘルパーメソッド

    private (DeleteEmployeeUseCase, SupportAdvance.Contexts.Employee.Application.Repositories.IEmployeeRepository) CreateUseCase()
    {
        var fixedDateTime = new DateTime(2026, 8, 11, 0, 0, 0, DateTimeKind.Unspecified);
        var clock = new MockClock(fixedDateTime);
        var mapper = new EmployeeMapper();
        var repository = new EmployeeRepository(mapper, clock);
        var useCase = new DeleteEmployeeUseCase(repository);
        return (useCase, repository);
    }

    #endregion
}
