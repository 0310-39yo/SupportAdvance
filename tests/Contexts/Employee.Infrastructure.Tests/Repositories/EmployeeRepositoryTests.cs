namespace SupportAdvance.Contexts.Employee.Infrastructure.Tests.Repositories;

using System.Data;
using RepoDb;
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
using SupportAdvance.Infrastructure.Providers;
using SupportAdvance.Application.Abstractions.Services;
using SupportAdvance.Infrastructure.Tests.Utilities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using SupportAdvance.Application.Abstractions.Identifiers;
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

internal sealed class TestCurrentUserService : ICurrentUserService
{
    private const long TestUserEmployeeRowId = 999999999L;
    public long EmployeeRowId => TestUserEmployeeRowId;
    public bool IsAuthenticated => true;
    public void SetLoggedInUser(long employeeRowId, string loginId) { }
    public void SetLoggedOut() { }
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
public class EmployeeRepositoryTests : RepositoryTestBase
{
    private const string ConnectionString =
        "Data Source=3160EPOTAK; Database=SupportAdvance; User ID=sa; Password=Misutamako4^; Encrypt=false";

    private const int TestBizIdStart = 1001;

    private readonly IClock _clock = new SystemClock();
    private IEmployeeRepository _repository = null!;
    private TestSequenceProvider _testSequenceProvider = null!;
    private int _nextTestBizId = TestBizIdStart;

    public override Task InitializeAsync()
    {
        // RepoDb GlobalConfiguration 設定（SQL Server用）
        GlobalConfiguration
            .Setup()
            .UseSqlServer();

        // Dapper グローバル型マッピング設定（snake_case カラム ↔ PascalCase プロパティ変換に必須）
        SupportAdvance.Infrastructure.ORM.Dapper.DapperTypeHandlerRegistration.Register();

        var appSettings = new AppSettings
        {
            ConnectionStrings = new Dictionary<string, string> { { "SupportAdvance", ConnectionString } },
            Database = new DatabaseSettings { Dialect = "SqlServer" }
        };

        _connectionFactory = new DbConnectionFactory(appSettings);
        _testSequenceProvider = new TestSequenceProvider(appSettings);
        var queryLoader = new SqlQueryLoader(appSettings);
        var mapper = new EmployeeMapper(_clock);
        var currentUser = new TestCurrentUserService();
        var logger = new NoOpAppLogging<EmployeeRepository>();

        _repository = new EmployeeRepository(queryLoader, mapper, _connectionFactory, currentUser, _clock, logger);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Employee・Person・DepartmentMembership テーブルのクリーンアップ
    /// 【重要】m_persons から person_row_id を取得してから削除（FK制約順）
    /// Employee-Person は 1対1のため、m_persons.employee_row_id で参照
    /// </summary>
    protected override async Task CleanupAsync(IDbConnection connection, long employeeRowId)
    {
        await Task.Run(() =>
        {
            // Step 1: m_persons から person_row_id を取得（employee_row_id で検索）
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT row_id FROM m_persons WHERE employee_row_id = @employeeRowId";
            AddParam(cmd, "@employeeRowId", employeeRowId);
            var personRowId = (long?)cmd.ExecuteScalar();

            // Step 2: FK制約逆順で削除
            // 【注意】ExecuteNonQuery ヘルパーは内部で常に "@rowId" という名前でパラメータをバインドするため、
            //         SQL文中のプレースホルダーも "@rowId" に統一する
            ExecuteNonQuery(connection, "DELETE FROM m_department_memberships WHERE employee_row_id = @rowId", employeeRowId);

            if (personRowId.HasValue)
            {
                ExecuteNonQuery(connection, "DELETE FROM m_persons WHERE row_id = @rowId", personRowId.Value);
            }

            ExecuteNonQuery(connection, "DELETE FROM m_employees WHERE row_id = @rowId", employeeRowId);
        });
    }

    #region グループ 1: GetByIdAsync - 存在する場合

    [Fact]
    public async Task VO_CRUD_03_GetByIdAsync_WithValidId_WithValidIdReturnsEmployee()
    {
        try
        {
            // Arrange
            var repository = CreateRepository();
            var employee = await BuildTestEmployeeAsync();
            await repository.AddAsync(employee);
            _createdRowIds.Add(employee.RowId.Value);

            // Act
            var result = await repository.GetByIdAsync(employee.RowId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(employee.RowId.Value, result.RowId.Value);
        }
        finally
        {
            await DisposeAsync();
        }
    }

    [Fact]
    public async Task VO_CRUD_04_GetByIdAsync_WithMultipleEmployees_WithMultipleEmployeesReturnsCorrectOne()
    {
        try
        {
            // Arrange
            var repository = CreateRepository();
            var employee1 = await BuildTestEmployeeAsync();
            var employee2 = await BuildTestEmployeeAsync();
            await repository.AddAsync(employee1);
            _createdRowIds.Add(employee1.RowId.Value);
            await repository.AddAsync(employee2);
            _createdRowIds.Add(employee2.RowId.Value);

            // Act
            var result = await repository.GetByIdAsync(employee1.RowId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(employee1.RowId.Value, result.RowId.Value);
        }
        finally
        {
            await DisposeAsync();
        }
    }

    #endregion

    #region グループ 2: GetByIdAsync - 存在しない場合

    [Fact]
    public async Task VO_CRUD_04_GetByIdAsync_WithInvalidId_WithInvalidIdReturnsNull()
    {
        try
        {
            // Arrange
            var repository = CreateRepository();
            var nonExistentId = EmployeeRowId.From(9999999999);

            // Act
            var result = await repository.GetByIdAsync(nonExistentId);

            // Assert
            Assert.Null(result);
        }
        finally
        {
            await DisposeAsync();
        }
    }

    #endregion

    #region グループ 3: GetByRowIdAsync

    [Fact]
    public async Task VO_CRUD_03_GetByRowIdAsync_WithValidRowId_WithValidRowIdReturnsEmployee()
    {
        try
        {
            // Arrange
            var repository = CreateRepository();
            var employee = await BuildTestEmployeeAsync();
            await repository.AddAsync(employee);
            _createdRowIds.Add(employee.RowId.Value);

            // Act
            var result = await repository.GetByRowIdAsync(employee.RowId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(employee.RowId.Value, result.RowId.Value);
        }
        finally
        {
            await DisposeAsync();
        }
    }

    [Fact]
    public async Task VO_CRUD_04_GetByRowIdAsync_WithInvalidRowId_WithInvalidRowIdReturnsNull()
    {
        try
        {
            // Arrange
            var repository = CreateRepository();
            var nonExistentId = EmployeeRowId.From(9999999999);

            // Act
            var result = await repository.GetByRowIdAsync(nonExistentId);

            // Assert
            Assert.Null(result);
        }
        finally
        {
            await DisposeAsync();
        }
    }

    #endregion

    #region グループ 4: GetByPersonRowIdAsync

    [Fact]
    public async Task VO_EXEC_01_GetByPersonRowIdAsync_WithValidPersonRowId_WithValidPersonRowIdReturnsEmployees()
    {
        try
        {
            // Arrange: Employee と Person は1対1のため、1件のみ作成
            var repository = CreateRepository();
            var employee = await BuildTestEmployeeAsync();
            await repository.AddAsync(employee);
            _createdRowIds.Add(employee.RowId.Value);

            // Act
            var results = await repository.GetByPersonRowIdAsync(employee.Person.RowId);

            // Assert
            Assert.NotEmpty(results);
            Assert.Single(results);
            Assert.Equal(employee.RowId.Value, results[0].RowId.Value);
        }
        finally
        {
            await DisposeAsync();
        }
    }

    [Fact]
    public async Task VO_EXEC_01_GetByPersonRowIdAsync_WithInvalidPersonRowId_WithInvalidPersonRowIdReturnsEmpty()
    {
        try
        {
            // Arrange
            var repository = CreateRepository();
            var nonExistentPersonRowId = await _testSequenceProvider.GetNextValueAsync() + 100_000;

            // Act
            var results = await repository.GetByPersonRowIdAsync(PersonRowId.From(nonExistentPersonRowId));

            // Assert
            Assert.Empty(results);
        }
        finally
        {
            await DisposeAsync();
        }
    }

    #endregion

    #region グループ 5: AddAsync

    [Fact]
    public async Task VO_CRUD_01_SaveAsync_WithNewEmployee_InsertsSaveAsync_WithNewEmployeeInsertsSuccessfully()
    {
        try
        {
            // Arrange
            var repository = CreateRepository();
            var employee = await BuildTestEmployeeAsync();

            // Act
            await repository.SaveAsync(employee);
            _createdRowIds.Add(employee.RowId.Value);

            // Assert
            var result = await repository.GetByIdAsync(employee.RowId);
            Assert.NotNull(result);
            Assert.Equal(employee.RowId.Value, result.RowId.Value);
        }
        finally
        {
            await DisposeAsync();
        }
    }

    [Fact]
    public async Task VO_AUDIT_01_SaveAsync_WithNewEmployee_SetsCratedAtAndBy()
    {
        try
        {
            // Arrange
            var repository = CreateRepository();
            var employee = await BuildTestEmployeeAsync();

            // Act
            await repository.SaveAsync(employee);
            _createdRowIds.Add(employee.RowId.Value);

            // Assert - CreatedAt/CreatedBy が設定されていることを直接SQLで確認（Entity には CreatedAt が露出しないため）
            var createdAt = QueryScalar<DateTime?>("m_employees", employee.RowId.Value, "created_at");
            var createdBy = QueryScalar<long?>("m_employees", employee.RowId.Value, "created_by");
            Assert.NotNull(createdAt);
            Assert.NotNull(createdBy);
        }
        finally
        {
            await DisposeAsync();
        }
    }

    #endregion

    #region グループ 6: SaveAsync（更新時）

    [Fact]
    public async Task VO_CRUD_05_SaveAsync_WithExistingEmployee_UpdatesSaveAsync_WithExistingEmployeeUpdatesSuccessfully()
    {
        try
        {
            // Arrange
            var repository = CreateRepository();
            var employee = await BuildTestEmployeeAsync();
            await repository.SaveAsync(employee);
            _createdRowIds.Add(employee.RowId.Value);

            var loaded = await repository.GetByIdAsync(employee.RowId);
            Assert.NotNull(loaded);

            var retiredOn = new LocalDateTime(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified));
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
            await repository.SaveAsync(updated);

            // Assert
            var retrieved = await repository.GetByIdAsync(employee.RowId);
            Assert.NotNull(retrieved);
            Assert.True(retrieved.RetiredOn.HasRetired);
        }
        finally
        {
            await DisposeAsync();
        }
    }

    #endregion

    #region グループ 7: DeleteAsync

    [Fact]
    public async Task VO_CRUD_06_DeleteAsync_WithValidId_WithValidIdSetsDeletedAtLogicallyDeletes()
    {
        try
        {
            // Arrange
            var repository = CreateRepository();
            var employee = await BuildTestEmployeeAsync();
            await repository.AddAsync(employee);
            _createdRowIds.Add(employee.RowId.Value);

            // Act
            await repository.DeleteAsync(employee.RowId);

            // Assert - GetEmployeeByRowId.sql は deleted_at IS NULL でフィルタするため、論理削除後は取得不可
            var result = await repository.GetByIdAsync(employee.RowId);
            Assert.Null(result);

            var deletedAt = QueryScalar<DateTime?>("m_employees", employee.RowId.Value, "deleted_at");
            Assert.NotNull(deletedAt);
        }
        finally
        {
            await DisposeAsync();
        }
    }

    #endregion

    #region ヘルパーメソッド

    private IEmployeeRepository CreateRepository() => _repository;

    /// <summary>
    /// テスト用 Employee エンティティを構築する（RowId/BizId をテスト用シーケンスから採番）
    /// 【重要】RowId・PersonRowId は TestSequenceProvider.GetNextValueAsync() で採番
    /// テストと本番データが ID 範囲で重複しないことを保証
    /// </summary>
    private async Task<Employee> BuildTestEmployeeAsync(long? personRowId = null)
    {
        var rowId = await _testSequenceProvider.GetNextValueAsync();
        var bizId = _nextTestBizId++;
        var resolvedPersonRowId = personRowId ?? await _testSequenceProvider.GetNextValueAsync();

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

    #endregion
}
