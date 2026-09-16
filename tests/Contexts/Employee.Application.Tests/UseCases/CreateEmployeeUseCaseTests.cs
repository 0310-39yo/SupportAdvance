namespace SupportAdvance.Contexts.Employee.Application.Tests.UseCases;

using SupportAdvance.Application.Abstractions.Identifiers;
using SupportAdvance.Contexts.Employee.Application.Repositories;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Application.Dtos;
using SupportAdvance.Contexts.Employee.Application.UseCases;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Person;
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
    public async Task VO_EXEC_01_CreateEmployee_WithValidRequest_ReturnsEmployeeDto()
    {
        // Arrange
        var (useCase, _) = CreateUseCase();
        var request = new CreateEmployeeRequest
        {
            PersonRowId = 1,
            DivisionCode = "M",
            EmployeeNumber = 1001,
            PersonLastName = "山田",
            PersonFirstName = "太郎",
            PersonLastNameKana = "ヤマダ",
            PersonFirstNameKana = "タロウ"
        };

        // Act
        var result = await useCase.ExecuteAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.RowId > 0);
        Assert.Contains("M", result.BizCode);
        Assert.Contains("1001", result.BizCode);
        Assert.Equal(1, result.PersonRowId);
    }

    [Fact]
    public async Task VO_EXEC_02_CreateEmployee_WithMultipleRequests_AllCreated()
    {
        // Arrange
        var (useCase, _) = CreateUseCase();
        var request1 = new CreateEmployeeRequest
        {
            PersonRowId = 1,
            DivisionCode = "M",
            EmployeeNumber = 1001,
            PersonLastName = "山田",
            PersonFirstName = "太郎",
            PersonLastNameKana = "ヤマダ",
            PersonFirstNameKana = "タロウ"
        };
        var request2 = new CreateEmployeeRequest
        {
            PersonRowId = 2,
            DivisionCode = "T",
            EmployeeNumber = 7501,
            PersonLastName = "佐藤",
            PersonFirstName = "花子",
            PersonLastNameKana = "サトウ",
            PersonFirstNameKana = "ハナコ"
        };

        // Act
        var result1 = await useCase.ExecuteAsync(request1);
        var result2 = await useCase.ExecuteAsync(request2);

        // Assert
        Assert.NotEqual(result1.RowId, result2.RowId);
        Assert.Contains("M", result1.BizCode);
        Assert.Contains("T", result2.BizCode);
    }

    #endregion

    #region グループ 2: 異常系 - 入力値検証

    [Fact]
    public async Task VO_ERROR_01_CreateEmployee_WithInvalidPersonRowId_ThrowsException()
    {
        // Arrange
        var (useCase, _) = CreateUseCase();
        var request = new CreateEmployeeRequest
        {
            PersonRowId = 0,  // ← 無効
            DivisionCode = "M",
            EmployeeNumber = 1001,
            PersonLastName = "山田",
            PersonFirstName = "太郎",
            PersonLastNameKana = "ヤマダ",
            PersonFirstNameKana = "タロウ"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(request));
    }

    [Fact]
    public async Task VO_ERROR_02_CreateEmployee_WithInvalidDivisionCode_ThrowsException()
    {
        // Arrange
        var (useCase, _) = CreateUseCase();
        var request = new CreateEmployeeRequest
        {
            PersonRowId = 1,
            DivisionCode = "X",  // ← 無効
            EmployeeNumber = 1001,
            PersonLastName = "山田",
            PersonFirstName = "太郎",
            PersonLastNameKana = "ヤマダ",
            PersonFirstNameKana = "タロウ"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(request));
    }

    [Fact]
    public async Task VO_ERROR_03_CreateEmployee_WithInvalidEmployeeNumber_ThrowsException()
    {
        // Arrange
        var (useCase, _) = CreateUseCase();
        var request = new CreateEmployeeRequest
        {
            PersonRowId = 1,
            DivisionCode = "M",
            EmployeeNumber = 1000,  // ← 無効（1001以上必須）
            PersonLastName = "山田",
            PersonFirstName = "太郎",
            PersonLastNameKana = "ヤマダ",
            PersonFirstNameKana = "タロウ"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(request));
    }

    [Fact]
    public async Task VO_ERROR_04_CreateEmployee_WithEmployeeNumberTooHigh_ThrowsException()
    {
        // Arrange
        var (useCase, _) = CreateUseCase();
        var request = new CreateEmployeeRequest
        {
            PersonRowId = 1,
            DivisionCode = "M",
            EmployeeNumber = 10000,  // ← 無効（9999以下必須）
            PersonLastName = "山田",
            PersonFirstName = "太郎",
            PersonLastNameKana = "ヤマダ",
            PersonFirstNameKana = "タロウ"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(request));
    }

    #endregion

    #region グループ 3: 統合 - Repository との連携

    [Fact]
    public async Task VO_SIDE_01_CreateEmployee_PersistsToRepository_CanBeRetrieved()
    {
        // Arrange
        var (createUseCase, repository) = CreateUseCase();
        var request = new CreateEmployeeRequest
        {
            PersonRowId = 1,
            DivisionCode = "M",
            EmployeeNumber = 1001,
            PersonLastName = "山田",
            PersonFirstName = "太郎",
            PersonLastNameKana = "ヤマダ",
            PersonFirstNameKana = "タロウ"
        };

        // Act: 作成
        var created = await createUseCase.ExecuteAsync(request);

        // Act: Repository で確認
        var retrieved = await repository.GetByIdAsync(EmployeeRowId.From(created.RowId));

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(created.RowId, retrieved.RowId.Value);
    }

    #endregion

    #region ヘルパーメソッド

    /// <summary>
    /// テスト用 MockSequenceProvider
    /// 連続した RowId を返す（テスト用開始値 2147483648）
    /// </summary>
    private class MockSequenceProvider : ISequenceProvider
    {
        private long _counter = 2147483648;  // テスト用開始値

        public async Task<long> GetNextValueAsync()
        {
            return await Task.FromResult(_counter++);
        }

        public async Task<IReadOnlyList<long>> GetNextValuesAsync(int count = 1)
        {
            if (count <= 0)
                throw new ArgumentException("Count must be greater than 0.", nameof(count));

            var result = new List<long>(capacity: count);
            for (int i = 0; i < count; i++)
            {
                result.Add(_counter++);
            }

            return await Task.FromResult(result.AsReadOnly());
        }
    }

    private (CreateEmployeeUseCase, IEmployeeRepository) CreateUseCase()
    {
        var fixedDateTime = new DateTime(2026, 8, 11, 0, 0, 0, DateTimeKind.Unspecified);
        var clock = new MockClock(fixedDateTime);
        var sequenceProvider = new MockSequenceProvider();
        var repository = new MockEmployeeRepository();
        var useCase = new CreateEmployeeUseCase(repository, clock, sequenceProvider);
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
