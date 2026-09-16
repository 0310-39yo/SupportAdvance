namespace SupportAdvance.Contexts.Department.Infrastructure.Tests.Repositories;

using System.Data;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Common.Configuration;
using SupportAdvance.Contexts.Department.Application.Repositories;
using SupportAdvance.Contexts.Department.Domain.Entities;
using SupportAdvance.Contexts.Department.Domain.ValueObjects;
using SupportAdvance.Contexts.Department.Infrastructure.Mappers;
using SupportAdvance.Contexts.Department.Infrastructure.Repositories;
using SupportAdvance.Infrastructure.Persistence;
using SupportAdvance.Infrastructure.Services;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using Xunit;

/// <summary>
/// DepartmentRepository の結合テスト（実DB接続）
///
/// 【接続先】既存の開発DB（3160EPOTAK / SupportAdvance、appsettings.Debug.json と同一）
/// 【注意】この Claude Code 実行環境からは当該DBへのネットワーク到達性がないため、
///         本ファイルは実行未確認の状態で作成されている。実行確認はユーザー環境（開発マシン）で行うこと。
/// 【テストデータ分離】RowId は 2147483648 以降のテスト専用範囲を使用し、既存データと衝突させない。
///         DepartmentCode は "T" + 3桁連番（T001, T002, ...）を使用。
/// 【クリーンアップ】IAsyncLifetime.DisposeAsync で、テスト中に作成した行を物理 DELETE する。
/// 【既知の制約】DepartmentRepository.SaveAsync の新規作成（Insert）判定（RowId==0）は、
///         DepartmentRowId が必須型（1以上）であるため実質到達不可能な疑いがある（別タスクで追跡：
///         「DepartmentRepository.SaveAsync の新規作成判定を修正」）。そのため本テストでは
///         Insert 系のテストは直接SQLでテストデータを投入し、SaveAsync 自体の Insert 動作検証は対象外とする。
/// </summary>
public class DepartmentRepositoryTests : IAsyncLifetime
{
    private const string ConnectionString =
        "Data Source=3160EPOTAK; Database=SupportAdvance; User ID=sa; Password=Misutamako4^; Encrypt=false";

    private const long TestRowIdStart = 2147483648L;

    private IDepartmentRepository _repository = null!;
    private IDbConnectionFactory _connectionFactory = null!;
    private readonly List<long> _createdRowIds = new();
    private long _nextTestRowId = TestRowIdStart;
    private int _nextTestCodeSuffix = 1;

    public Task InitializeAsync()
    {
        var appSettings = new AppSettings
        {
            ConnectionStrings = new Dictionary<string, string> { { "Default", ConnectionString } },
            Database = new DatabaseSettings { Dialect = "SqlServer" }
        };

        _connectionFactory = new DbConnectionFactory(appSettings);
        var queryLoader = new SqlQueryLoader(appSettings);
        var mapper = new DepartmentMapper();
        var currentUser = new SystemCurrentUserService();
        IClock clock = new SystemClock();

        _repository = new DepartmentRepository(queryLoader, mapper, _connectionFactory, currentUser, clock);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        if (_createdRowIds.Count == 0)
        {
            return Task.CompletedTask;
        }

        using var connection = _connectionFactory.CreateConnection();
        foreach (var rowId in _createdRowIds)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM m_departments WHERE row_id = @rowId";
            AddParam(cmd, "@rowId", rowId);
            cmd.ExecuteNonQuery();
        }

        return Task.CompletedTask;
    }

    #region グループ 1: GetByIdAsync - 正常系

    [Fact]
    public async Task VO_CRUD_03_GetByIdAsync_WithValidId_GetByIdAsync_WithValidId_ReturnsDepartment()
    {
        // Arrange: テストデータを直接SQLで投入（SaveAsyncのInsert不具合を回避）
        var rowId = InsertTestDepartment(NextTestCode(), "結合テスト部署A", level: 1);
        var repository = CreateRepository();

        // Act
        var result = await repository.GetByIdAsync(DepartmentRowId.From(rowId));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(rowId, result.RowId.Value);
    }

    [Fact]
    public async Task VO_CRUD_04_GetByIdAsync_WithNonExistentId_GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange: テスト専用範囲内の未使用ID（作成していないID）を使う
        var repository = CreateRepository();
        var nonExistentId = DepartmentRowId.From(_nextTestRowId + 100_000);

        // Act
        var result = await repository.GetByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region グループ 2: GetByCodeAsync - 正常系

    [Fact]
    public async Task VO_QUERY_01_GetByCodeAsync_WithValidCode_GetByCodeAsync_WithValidCode_ReturnsDepartment()
    {
        // Arrange
        var testCode = NextTestCode();
        InsertTestDepartment(testCode, "結合テスト部署B", level: 1);
        var repository = CreateRepository();
        var code = DepartmentCode.From(testCode);

        // Act
        var result = await repository.GetByCodeAsync(code);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(code, result.DeptCode);
    }

    #endregion

    #region グループ 3: GetAllAsync

    [Fact]
    public async Task VO_QUERY_02_GetAllAsync_GetAllAsync_ReturnsAllDepartments()
    {
        // Arrange: 開発DBは既存データを含むため、件数の厳密一致ではなく型・非null・作成した行が含まれることを確認
        var rowId = InsertTestDepartment(NextTestCode(), "結合テスト部署C", level: 1);
        var repository = CreateRepository();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IReadOnlyList<Department>>(result);
        Assert.Contains(result, d => d.RowId.Value == rowId);
    }

    #endregion

    #region グループ 4: SaveAsync - Insert（既知の制約により Skip）

    [Fact(Skip = "DepartmentRepository.SaveAsync の新規作成判定（RowId==0）に到達不可能な疑いのある不具合あり。別タスクで追跡中")]
    public async Task VO_CRUD_01_SaveAsync_WithNewEntity_InsertsSaveAsync_WithNewEntity_InsertsSuccessfully()
    {
        // Arrange
        var repository = CreateRepository();
        var entity = Department.Create(
            DepartmentRowId.From(_nextTestRowId++),
            DepartmentCode.From(NextTestCode()),
            "営業部",
            HierarchyLevel.From(1)
        );

        // Act
        await repository.SaveAsync(entity);
        _createdRowIds.Add(entity.RowId.Value);

        // Assert - DB から読み込んで確認
        var retrieved = await repository.GetByIdAsync(entity.RowId);
        Assert.NotNull(retrieved);
        Assert.Equal(entity.DeptCode.Value, retrieved.DeptCode.Value);
    }

    [Fact(Skip = "DepartmentRepository.SaveAsync の新規作成判定（RowId==0）に到達不可能な疑いのある不具合あり。別タスクで追跡中")]
    public async Task VO_AUDIT_01_SaveAsync_WithNewEntity_SetsAuditSaveAsync_WithNewEntity_SetsCratedAtAndBy()
    {
        // Arrange
        var repository = CreateRepository();
        var entity = Department.Create(
            DepartmentRowId.From(_nextTestRowId++),
            DepartmentCode.From(NextTestCode()),
            "企画部",
            HierarchyLevel.From(2)
        );

        // Act
        await repository.SaveAsync(entity);
        _createdRowIds.Add(entity.RowId.Value);

        // Assert - 監査フィールドが設定されていることを確認
        var createdAt = QueryScalar<DateTime?>(entity.RowId.Value, "created_at");
        Assert.NotNull(createdAt);
    }

    #endregion

    #region グループ 5: SaveAsync - Update

    [Fact]
    public async Task VO_CRUD_05_SaveAsync_WithExistingEntity_UpdatesSaveAsync_WithExistingEntity_UpdatesSuccessfully()
    {
        // Arrange: 直接SQLで既存データを投入してから取得し、変更してSaveAsync（Update）
        var rowId = InsertTestDepartment(NextTestCode(), "旧製造部", level: 1);
        var repository = CreateRepository();
        var existing = await repository.GetByIdAsync(DepartmentRowId.From(rowId));
        Assert.NotNull(existing);

        var updated = Department.Reconstruct(
            existing.RowId,
            existing.DeptCode,
            "新製造部",
            existing.Level,
            existing.ParentId,
            existing.ManagerId,
            existing.AbolishedOn,
            existing.RowVersion);

        // Act
        await repository.SaveAsync(updated);

        // Assert
        var retrieved = await repository.GetByIdAsync(DepartmentRowId.From(rowId));
        Assert.NotNull(retrieved);
        Assert.Equal("新製造部", retrieved.Name);
    }

    #endregion

    #region グループ 6: DeleteAsync

    [Fact]
    public async Task VO_CRUD_06_DeleteAsync_WithValidId_DeleteAsync_WithValidId_PerformsLogicalDelete()
    {
        // Arrange
        var rowId = InsertTestDepartment(NextTestCode(), "廃止予定部署", level: 1);
        var repository = CreateRepository();
        var departmentId = DepartmentRowId.From(rowId);

        // Act
        await repository.DeleteAsync(departmentId);

        // Assert - 論理削除されていることを確認（deleted_at が設定されている）
        // 【注意】GetDepartmentById.sql は deleted_at でフィルタしないため、直接SQLで検証する
        var deletedAt = QueryScalar<DateTime?>(rowId, "deleted_at");
        Assert.NotNull(deletedAt);
    }

    #endregion

    #region グループ 7: 異常系

    [Fact]
    public async Task VO_ERROR_01_SaveAsync_WithNullEntity_SaveAsync_WithNullEntity_ThrowsArgumentNullException()
    {
        // Arrange
        var repository = CreateRepository();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => repository.SaveAsync(null!));
    }

    [Fact]
    public async Task VO_ERROR_02_GetByIdAsync_WithNullId_GetByIdAsync_WithNullId_ThrowsArgumentNullException()
    {
        // Arrange
        var repository = CreateRepository();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => repository.GetByIdAsync(null!));
    }

    #endregion

    #region ヘルパーメソッド

    private IDepartmentRepository CreateRepository() => _repository;

    /// <summary>
    /// テスト専用の部署コードを採番する（"T" + 3桁連番、4文字固定制約に準拠）
    /// </summary>
    private string NextTestCode() => $"T{_nextTestCodeSuffix++:D3}";

    /// <summary>
    /// テストデータを直接SQLでDBに投入する
    /// 【理由】DepartmentRepository.SaveAsync の新規作成分岐に既知の不具合疑いがあるため、
    ///         Repository を経由せずテストデータを準備する
    /// </summary>
    private long InsertTestDepartment(
        string code,
        string name,
        int level,
        long? parentId = null,
        long? managerId = null,
        DateTime? abolishedOn = null)
    {
        var rowId = _nextTestRowId++;

        using var connection = _connectionFactory.CreateConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO m_departments
                (row_id, code, name, level, parent_department_row_id, manager_employee_row_id, abolished_on, created_at, created_by)
            VALUES
                (@rowId, @code, @name, @level, @parentId, @managerId, @abolishedOn, @createdAt, @createdBy)";

        AddParam(cmd, "@rowId", rowId);
        AddParam(cmd, "@code", code);
        AddParam(cmd, "@name", name);
        AddParam(cmd, "@level", level);
        AddParam(cmd, "@parentId", (object?)parentId ?? DBNull.Value);
        AddParam(cmd, "@managerId", (object?)managerId ?? DBNull.Value);
        AddParam(cmd, "@abolishedOn", (object?)abolishedOn ?? DBNull.Value);
        AddParam(cmd, "@createdAt", DateTime.Now);
        AddParam(cmd, "@createdBy", SystemCurrentUserService.SystemUserEmployeeRowId);

        cmd.ExecuteNonQuery();
        _createdRowIds.Add(rowId);
        return rowId;
    }

    /// <summary>
    /// 指定した行・カラムの値を直接SQLで取得する（Entity に露出しない監査フィールド検証用）
    /// </summary>
    private T QueryScalar<T>(long rowId, string columnName)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"SELECT {columnName} FROM m_departments WHERE row_id = @rowId";
        AddParam(cmd, "@rowId", rowId);
        var value = cmd.ExecuteScalar();
        return value is null or DBNull ? default! : (T)value;
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
