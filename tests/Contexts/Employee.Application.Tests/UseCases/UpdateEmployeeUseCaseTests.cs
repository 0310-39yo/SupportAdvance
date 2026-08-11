namespace SupportAdvance.Contexts.Employee.Application.Tests.UseCases;

using SupportAdvance.Contexts.Employee.Application.Dtos;
using SupportAdvance.Contexts.Employee.Application.UseCases;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
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

    [Fact]
    public async Task Test1_1_UpdateEmployee_WithValidRequest_Updates()
    {
        // Arrange
        var (useCase, repository) = CreateUseCase();
        var rowId = EmployeeRowId.From(1L);
        var employee = Employee.Create(
            rowId,
            EmployeeTypeDivision.From("M"),
            EmployeeBizId.From(1001),
            EmployeeBizCode.From("EMP001"),
            PersonRowId.From(100L)
        );
        await repository.AddAsync(employee);

        var request = new UpdateEmployeeRequest
        {
            EmployeeId = rowId.Value,
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

    [Fact]
    public async Task Test2_1_UpdateEmployee_WithNonExistentId_ThrowsException()
    {
        // Arrange
        var (useCase, _) = CreateUseCase();
        var request = new UpdateEmployeeRequest
        {
            EmployeeId = 99999L,
            DivisionCode = "T",
            EmployeeNumber = 7501
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.ExecuteAsync(request));
    }

    [Fact]
    public async Task Test2_2_UpdateEmployee_WithInvalidDivisionCode_ThrowsException()
    {
        // Arrange
        var (useCase, repository) = CreateUseCase();
        var rowId = EmployeeRowId.From(1L);
        var employee = Employee.Create(
            rowId,
            EmployeeTypeDivision.From("M"),
            EmployeeBizId.From(1001),
            EmployeeBizCode.From("EMP001"),
            PersonRowId.From(100L)
        );
        await repository.AddAsync(employee);

        var request = new UpdateEmployeeRequest
        {
            EmployeeId = rowId.Value,
            DivisionCode = "X",  // ← 無効
            EmployeeNumber = 1002
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(request));
    }

    [Fact]
    public async Task Test2_3_UpdateEmployee_WithInvalidEmployeeNumber_ThrowsException()
    {
        // Arrange
        var (useCase, repository) = CreateUseCase();
        var rowId = EmployeeRowId.From(1L);
        var employee = Employee.Create(
            rowId,
            EmployeeTypeDivision.From("M"),
            EmployeeBizId.From(1001),
            EmployeeBizCode.From("EMP001"),
            PersonRowId.From(100L)
        );
        await repository.AddAsync(employee);

        var request = new UpdateEmployeeRequest
        {
            EmployeeId = rowId.Value,
            DivisionCode = "M",
            EmployeeNumber = 1000  // ← 無効（1001以上）
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(request));
    }

    [Fact]
    public async Task Test2_4_UpdateEmployee_WithZeroId_ThrowsException()
    {
        // Arrange
        var (useCase, _) = CreateUseCase();
        var request = new UpdateEmployeeRequest
        {
            EmployeeId = 0L,
            DivisionCode = "T",
            EmployeeNumber = 2001
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => useCase.ExecuteAsync(request));
    }

    #endregion

    #region ヘルパーメソッド

    private (UpdateEmployeeUseCase, SupportAdvance.Contexts.Employee.Application.Repositories.IEmployeeRepository) CreateUseCase()
    {
        var fixedDateTime = new DateTime(2026, 8, 11, 0, 0, 0, DateTimeKind.Unspecified);
        var clock = new MockClock(fixedDateTime);
        var mapper = new EmployeeMapper();
        var repository = new EmployeeRepository(mapper, clock);
        var useCase = new UpdateEmployeeUseCase(repository);
        return (useCase, repository);
    }

    #endregion
}
