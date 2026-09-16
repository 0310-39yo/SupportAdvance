namespace SupportAdvance.Contexts.Employee.Infrastructure.Tests.Repositories;

using System.Data;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Common.Configuration;
using SupportAdvance.Contexts.Employee.Application.Repositories;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Person;
using SupportAdvance.Contexts.Employee.Infrastructure.Mappers;
using SupportAdvance.Contexts.Employee.Infrastructure.Repositories;
using SupportAdvance.Crosscutting.Logging;
using SupportAdvance.Infrastructure.Persistence;
using SupportAdvance.Infrastructure.Services;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using Xunit;

/// <summary>
/// テスト用の何もしないロガー実装
/// </summary>
internal sealed class NoOpAppLogging<T> : IAppLogging<T>
{
    public void LogInformation(string? message) { }
    public void LogInformation(string messageTemplate, params object[] args) { }
    public void LogWarning(string message) { }
    public void LogWarning(string messageTemplate, params object[] args) { }
    public void LogError(string message, Exception? exception = null) { }
    public void LogError(string messageTemplate, params object[] args) { }
}

/// <summary>
/// EmployeeRepository の結合テスト（実DB接続）
///
/// 【接続先】既存の開発DB（3160EPOTAK / SupportAdvance、appsettings.Debug.json と同一）
/// 【注意】この Claude Code 実行環境からは当該DBへのネットワーク到達性がないため、
///         本ファイルは実行未確認の状態で作成されている。実行確認はユーザー環境（開発マシン）で行うこと。
/// 【テストデータ分離】EmployeeRowId / PersonRowId / BizId はテスト専用範囲を使用し、既存データと衝突させない。
/// 【クリーンアップ】IAsyncLifetime.DisposeAsync で、テスト中に作成した行を
///         m_department_memberships → m_persons → m_employees の順に物理 DELETE する（FK制約順）。
/// 【前提】本テストは EmployeeRepository.AddAsync / DeleteAsync / GetByPersonRowIdAsync の実装
///         （本Phaseで新規実装）を含めて検証する。
/// </summary>
public class EmployeeRepositoryTests : IAsyncLifetime
{
    private const string ConnectionString =
        "Data Source=3160EPOTAK; Database=SupportAdvance; User ID=sa; Password=Misutamako4^; Encrypt=false";

    private const long TestRowIdStart = 2147483648L;
    private const int TestBizIdStart = 1001;

    private readonly IClock _clock = new SystemClock();
    private IEmployeeRepository _repository = null!;
    private IDbConnectionFactory _connectionFactory = null!;
    private readonly List<long> _createdEmployeeRowIds = new();
    private long _nextTestRowId = TestRowIdStart;
    private int _nextTestBizId = TestBizIdStart;

    public Task InitializeAsync()
    {
        var appSettings = new AppSettings
        {
            ConnectionStrings = new Dictionary<string, string> { { "Default", ConnectionString } },
            Database = new DatabaseSettings { Dialect = "SqlServer" }
        };

        _connectionFactory = new DbConnectionFactory(appSettings);
        var queryLoader = new SqlQueryLoader(appSettings);
        var mapper = new EmployeeMapper(_clock);
        var currentUser = new SystemCurrentUserService();
        var logger = new NoOpAppLogging<EmployeeRepository>();

        _repository = new EmployeeRepository(queryLoader, mapper, _connectionFactory, currentUser, _clock, logger);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        if (_createdEmployeeRowIds.Count == 0)
        {
            return Task.CompletedTask;
        }

        using var connection = _connectionFactory.CreateConnection();
        foreach (var rowId in _createdEmployeeRowIds)
        {
            ExecuteNonQuery(connection, "DELETE FROM m_department_memberships WHERE employee_row_id = @rowId", rowId);
            ExecuteNonQuery(connection, "DELETE FROM m_persons WHERE employee_row_id = @rowId", rowId);
            ExecuteNonQuery(connection, "DELETE FROM m_employees WHERE row_id = @rowId", rowId);
        }

        return Task.CompletedTask;
    }

    #region グループ 1: GetByIdAsync - 存在する場合

    [Fact]
    public async Task VO_CRUD_03_GetByIdAsync_WithValidId_WithValidIdReturnsEmployee()
    {
        // Arrange
        var repository = CreateRepository();
        var employee = BuildTestEmployee();
        await repository.AddAsync(employee);
        _createdEmployeeRowIds.Add(employee.RowId.Value);

        // Act
        var result = await repository.GetByIdAsync(employee.RowId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employee.RowId.Value, result.RowId.Value);
    }

    [Fact]
    public async Task VO_CRUD_04_GetByIdAsync_WithMultipleEmployees_WithMultipleEmployeesReturnsCorrectOne()
    {
        // Arrange
        var repository = CreateRepository();
        var employee1 = BuildTestEmployee();
        var employee2 = BuildTestEmployee();
        await repository.AddAsync(employee1);
        _createdEmployeeRowIds.Add(employee1.RowId.Value);
        await repository.AddAsync(employee2);
        _createdEmployeeRowIds.Add(employee2.RowId.Value);

        // Act
        var result = await repository.GetByIdAsync(employee1.RowId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employee1.RowId.Value, result.RowId.Value);
    }

    #endregion

    #region グループ 2: GetByIdAsync - 存在しない場合

    [Fact]
    public async Task VO_CRUD_04_GetByIdAsync_WithInvalidId_WithInvalidIdReturnsNull()
    {
        // Arrange
        var repository = CreateRepository();
        var nonExistentId = EmployeeRowId.From(_nextTestRowId + 100_000);

        // Act
        var result = await repository.GetByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region グループ 3: GetByRowIdAsync

    [Fact]
    public async Task VO_CRUD_03_GetByRowIdAsync_WithValidRowId_WithValidRowIdReturnsEmployee()
    {
        // Arrange
        var repository = CreateRepository();
        var employee = BuildTestEmployee();
        await repository.AddAsync(employee);
        _createdEmployeeRowIds.Add(employee.RowId.Value);

        // Act
        var result = await repository.GetByRowIdAsync(employee.RowId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employee.RowId.Value, result.RowId.Value);
    }

    [Fact]
    public async Task VO_CRUD_04_GetByRowIdAsync_WithInvalidRowId_WithInvalidRowIdReturnsNull()
    {
        // Arrange
        var repository = CreateRepository();
        var nonExistentId = EmployeeRowId.From(_nextTestRowId + 100_000);

        // Act
        var result = await repository.GetByRowIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region グループ 4: GetByPersonRowIdAsync

    [Fact]
    public async Task VO_EXEC_01_GetByPersonRowIdAsync_WithValidPersonRowId_WithValidPersonRowIdReturnsEmployees()
    {
        // Arrange: 同一 PersonRowId を持つ Employee を2件作成
        var repository = CreateRepository();
        var personRowId = NextPersonRowId();
        var employee1 = BuildTestEmployee(personRowId: personRowId);
        var employee2 = BuildTestEmployee(personRowId: personRowId);
        await repository.AddAsync(employee1);
        _createdEmployeeRowIds.Add(employee1.RowId.Value);
        await repository.AddAsync(employee2);
        _createdEmployeeRowIds.Add(employee2.RowId.Value);

        // Act
        var results = await repository.GetByPersonRowIdAsync(PersonRowId.From(personRowId));

        // Assert
        Assert.NotEmpty(results);
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task VO_EXEC_01_GetByPersonRowIdAsync_WithInvalidPersonRowId_WithInvalidPersonRowIdReturnsEmpty()
    {
        // Arrange
        var repository = CreateRepository();
        var nonExistentPersonRowId = NextPersonRowId() + 100_000;

        // Act
        var results = await repository.GetByPersonRowIdAsync(PersonRowId.From(nonExistentPersonRowId));

        // Assert
        Assert.Empty(results);
    }

    #endregion

    #region グループ 5: AddAsync

    [Fact]
    public async Task VO_CRUD_01_AddAsync_WithValidEntity_WithValidEmployeeInsertsAndReturnsId()
    {
        // Arrange
        var repository = CreateRepository();
        var employee = BuildTestEmployee();

        // Act
        await repository.AddAsync(employee);
        _createdEmployeeRowIds.Add(employee.RowId.Value);

        // Assert
        var result = await repository.GetByIdAsync(employee.RowId);
        Assert.NotNull(result);
        Assert.Equal(employee.RowId.Value, result.RowId.Value);
    }

    [Fact]
    public async Task VO_AUDIT_01_AddAsync_AuditColumns_AuditColumnsAreSetAutomatically()
    {
        // Arrange
        var repository = CreateRepository();
        var employee = BuildTestEmployee();

        // Act
        await repository.AddAsync(employee);
        _createdEmployeeRowIds.Add(employee.RowId.Value);

        // Assert - CreatedAt/CreatedBy が設定されていることを直接SQLで確認（Entity には CreatedAt が露出しないため）
        var createdAt = QueryScalar<DateTime?>("m_employees", employee.RowId.Value, "created_at");
        var createdBy = QueryScalar<long?>("m_employees", employee.RowId.Value, "created_by");
        Assert.NotNull(createdAt);
        Assert.NotNull(createdBy);
    }

    #endregion

    #region グループ 6: UpdateAsync

    [Fact]
    public async Task VO_CRUD_05_UpdateAsync_WithValidEntity_WithValidEmployeeUpdatesSuccessfully()
    {
        // Arrange
        var repository = CreateRepository();
        var employee = BuildTestEmployee();
        await repository.AddAsync(employee);
        _createdEmployeeRowIds.Add(employee.RowId.Value);

        var loaded = await repository.GetByIdAsync(employee.RowId);
        Assert.NotNull(loaded);

        var retiredOn = new LocalDateTime(DateTime.Now);
        var updated = Employee.Reconstruct(
            loaded.RowId,
            loaded.TypeDivision,
            loaded.BizId,
            loaded.BizCode,
            RetiredOn.From(retiredOn),
            loaded.Person,
            loaded.DepartmentMemberships,
            _clock,
            loaded.RowVersion);

        // Act
        await repository.UpdateAsync(updated);

        // Assert
        var retrieved = await repository.GetByIdAsync(employee.RowId);
        Assert.NotNull(retrieved);
        Assert.True(retrieved.RetiredOn.HasRetired);
    }

    #endregion

    #region グループ 7: DeleteAsync

    [Fact]
    public async Task VO_CRUD_06_DeleteAsync_WithValidId_WithValidIdSetsDeletedAtLogicallyDeletes()
    {
        // Arrange
        var repository = CreateRepository();
        var employee = BuildTestEmployee();
        await repository.AddAsync(employee);
        _createdEmployeeRowIds.Add(employee.RowId.Value);

        // Act
        await repository.DeleteAsync(employee.RowId);

        // Assert - GetEmployeeByRowId.sql は deleted_at IS NULL でフィルタするため、論理削除後は取得不可
        var result = await repository.GetByIdAsync(employee.RowId);
        Assert.Null(result);

        var deletedAt = QueryScalar<DateTime?>("m_employees", employee.RowId.Value, "deleted_at");
        Assert.NotNull(deletedAt);
    }

    #endregion

    #region ヘルパーメソッド

    private IEmployeeRepository CreateRepository() => _repository;

    private long NextPersonRowId() => _nextTestRowId + 500_000;

    /// <summary>
    /// テスト用 Employee エンティティを構築する（RowId/BizId はテスト専用範囲を自動採番）
    /// </summary>
    private Employee BuildTestEmployee(long? personRowId = null)
    {
        var rowId = _nextTestRowId++;
        var bizId = _nextTestBizId++;
        var resolvedPersonRowId = personRowId ?? NextPersonRowId();

        var person = Person.Create(
            PersonRowId.From(resolvedPersonRowId),
            LastName.From("結合テスト"),
            FirstName.From("太郎"),
            LastNameKana.From("ケツゴウテスト"),
            FirstNameKana.From("タロウ"));

        return Employee.Create(
            EmployeeRowId.From(rowId),
            BizDivision.RegularEmployee(),
            BizId.From(bizId),
            BizCode.From(BizDivision.RegularEmployee(), BizId.From(bizId)),
            null,
            person,
            new List<DepartmentMembership>(),
            _clock);
    }

    private T QueryScalar<T>(string tableName, long rowId, string columnName)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"SELECT {columnName} FROM {tableName} WHERE row_id = @rowId";
        AddParam(cmd, "@rowId", rowId);
        var value = cmd.ExecuteScalar();
        return value is null or DBNull ? default! : (T)value;
    }

    private static void ExecuteNonQuery(IDbConnection connection, string sql, long rowId)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        AddParam(cmd, "@rowId", rowId);
        cmd.ExecuteNonQuery();
    }

    private static void AddParam(IDbCommand cmd, string name, object value)
    {
        var param = cmd.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        cmd.Parameters.Add(param);
    }

    #endregion
}
