namespace SupportAdvance.Contexts.Employee.Application.Tests.UseCases;

using SupportAdvance.Contexts.Employee.Application.Repositories;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Application.Dtos;
using SupportAdvance.Contexts.Employee.Application.UseCases;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.Contexts.Employee.Infrastructure.Mappers;
using SupportAdvance.Contexts.Employee.Infrastructure.Repositories;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using Xunit;

/// <summary>
/// CreateEmployeeUseCase の単体テスト
/// </summary>
public class CreateEmployeeUseCaseTests
{
    #region グループ 1: 正常系 - 従業員作成成功

    [Fact]
    public async Task Test1_1_CreateEmployee_WithValidRequest_ReturnsEmployeeDto()
    {
        // Arrange
        var (useCase, _) = CreateUseCase();
        var request = new CreateEmployeeRequest
        {
            PersonRowId = 1,
            DivisionCode = "M",
            EmployeeNumber = 1001
        };

        // Act
        var result = await useCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Contains("M", result.Code);
        Assert.Contains("1001", result.Code);
        Assert.Equal(1, result.PersonRowId);
    }

    [Fact]
    public async Task Test1_2_CreateEmployee_WithMultipleRequests_AllCreated()
    {
        // Arrange
        var (useCase, _) = CreateUseCase();
        var request1 = new CreateEmployeeRequest { PersonRowId = 1, DivisionCode = "M", EmployeeNumber = 1001 };
        var request2 = new CreateEmployeeRequest { PersonRowId = 2, DivisionCode = "T", EmployeeNumber = 7501 };

        // Act
        var result1 = await useCase.ExecuteAsync(request1);
        var result2 = await useCase.ExecuteAsync(request2);

        // Assert
        Assert.NotEqual(result1.Id, result2.Id);
        Assert.Contains("M", result1.Code);
        Assert.Contains("T", result2.Code);
    }

    #endregion

    #region グループ 2: 異常系 - 入力値検証

    [Fact]
    public async Task Test2_1_CreateEmployee_WithInvalidPersonRowId_ThrowsException()
    {
        // Arrange
        var (useCase, _) = CreateUseCase();
        var request = new CreateEmployeeRequest
        {
            PersonRowId = 0,  // ← 無効
            DivisionCode = "M",
            EmployeeNumber = 1001
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(request));
    }

    [Fact]
    public async Task Test2_2_CreateEmployee_WithInvalidDivisionCode_ThrowsException()
    {
        // Arrange
        var (useCase, _) = CreateUseCase();
        var request = new CreateEmployeeRequest
        {
            PersonRowId = 1,
            DivisionCode = "X",  // ← 無効
            EmployeeNumber = 1001
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(request));
    }

    [Fact]
    public async Task Test2_3_CreateEmployee_WithInvalidEmployeeNumber_ThrowsException()
    {
        // Arrange
        var (useCase, _) = CreateUseCase();
        var request = new CreateEmployeeRequest
        {
            PersonRowId = 1,
            DivisionCode = "M",
            EmployeeNumber = 1000  // ← 無効（1001以上必須）
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(request));
    }

    [Fact]
    public async Task Test2_4_CreateEmployee_WithEmployeeNumberTooHigh_ThrowsException()
    {
        // Arrange
        var (useCase, _) = CreateUseCase();
        var request = new CreateEmployeeRequest
        {
            PersonRowId = 1,
            DivisionCode = "M",
            EmployeeNumber = 10000  // ← 無効（9999以下必須）
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(request));
    }

    #endregion

    #region グループ 3: 統合 - Repository との連携

    [Fact]
    public async Task Test3_1_CreateEmployee_PersistsToRepository_CanBeRetrieved()
    {
        // Arrange
        var (createUseCase, repository) = CreateUseCase();
        var request = new CreateEmployeeRequest
        {
            PersonRowId = 1,
            DivisionCode = "M",
            EmployeeNumber = 1001
        };

        // Act: 作成
        var created = await createUseCase.ExecuteAsync(request);

        // Act: Repository で確認
        var retrieved = await repository.GetByIdAsync(SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee.EmployeeId.From(created.Id));

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id.Value);
    }

    #endregion

    #region ヘルパーメソッド

    private (CreateEmployeeUseCase, IEmployeeRepository) CreateUseCase()
    {
        var fixedDateTime = new DateTime(2026, 8, 11, 0, 0, 0, DateTimeKind.Unspecified);
        var clock = new MockClock(fixedDateTime);
        var mapper = new EmployeeMapper();
        var repository = new EmployeeRepository(mapper, clock);
        var useCase = new CreateEmployeeUseCase(repository, clock);
        return (useCase, repository);
    }

    #endregion
}
