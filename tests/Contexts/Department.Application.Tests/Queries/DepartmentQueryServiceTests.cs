namespace SupportAdvance.Contexts.Department.Application.Tests.Queries;

using SupportAdvance.Contexts.Department.Application.Queries;
using SupportAdvance.Contexts.Department.Application.Repositories;
using SupportAdvance.Contexts.Department.Domain.Entities;
using SupportAdvance.Contexts.Department.Domain.ValueObjects;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using Xunit;

/// <summary>
/// DepartmentQueryService の単体テスト
/// </summary>
public class DepartmentQueryServiceTests
{
    #region グループ 1: 正常系

    [Fact]
    public async Task Test1_1_GetByIdAsync_WithValidId_DelegateToRepository()
    {
        // Arrange
        var (queryService, repository) = CreateQueryService();
        var departmentId = DepartmentRowId.From(1L);
        var expectedDepartment = Department.Create(
            departmentId,
            DepartmentCode.From("D001"),
            "営業部",
            HierarchyLevel.From(1)
        );
        ((MockDepartmentRepository)repository).SetupGetByIdAsync(departmentId, expectedDepartment);

        // Act
        var result = await queryService.GetByIdAsync(departmentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDepartment.RowId, result.RowId);
    }

    [Fact]
    public async Task Test1_2_GetByIdAsync_WithValidId_ReturnsRepositoryResult()
    {
        // Arrange
        var (queryService, repository) = CreateQueryService();
        var departmentId = DepartmentRowId.From(2L);
        ((MockDepartmentRepository)repository).SetupGetByIdAsync(departmentId, null);

        // Act
        var result = await queryService.GetByIdAsync(departmentId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region グループ 2: 異常系

    [Fact]
    public async Task Test2_1_GetByIdAsync_WithNullId_ThrowsArgumentNullException()
    {
        // Arrange
        var (queryService, _) = CreateQueryService();

        // Act & Assert
        // IDentifierは value objectなので null チェックが必要
        await Assert.ThrowsAsync<ArgumentNullException>(() => queryService.GetByIdAsync(null!));
    }

    #endregion

    #region ヘルパーメソッド

    private (DepartmentQueryService, IDepartmentRepository) CreateQueryService()
    {
        var repository = new MockDepartmentRepository();
        var queryService = new DepartmentQueryService(repository);
        return (queryService, repository);
    }

    #endregion

    /// <summary>
    /// テスト用モック実装（Skip されたテストの型チェック用）
    /// </summary>
    private class MockDepartmentRepository : IDepartmentRepository
    {
        private Dictionary<long, Department?> _store = new();

        public void SetupGetByIdAsync(DepartmentRowId id, Department? department)
        {
            _store[id.Value] = department;
        }

        public Task<Department?> GetByIdAsync(DepartmentRowId id)
        {
            if (_store.TryGetValue(id.Value, out var department))
            {
                return Task.FromResult(department);
            }
            return Task.FromResult<Department?>(null);
        }

        public Task<Department?> GetByCodeAsync(DepartmentCode code) => Task.FromResult<Department?>(null);

        public Task<IReadOnlyList<Department>> GetAllAsync() => Task.FromResult<IReadOnlyList<Department>>(new List<Department>());

        public Task SaveAsync(Department entity) => Task.CompletedTask;

        public Task DeleteAsync(DepartmentRowId id) => Task.CompletedTask;
    }
}
