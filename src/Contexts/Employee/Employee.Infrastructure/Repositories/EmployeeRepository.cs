using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Employee.Infrastructure.Repositories;

using System.Data;
using System.Linq;
using Dapper;
using RepoDb;
using SupportAdvance.Contexts.Employee.Application.Repositories;
using Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using Domain.ValueObjects.Person;
using Mappers;
using Models;
using Crosscutting.Logging;
using SupportAdvance.Infrastructure.Persistence;
using SupportAdvance.Infrastructure.Repositories;
using SupportAdvance.Application.Abstractions.Services;
using SharedKernel.ValueObjects.Audit;

/// <summary>
/// Employee 集約の Repository 実装
///
/// 【責務】
/// - 複数テーブルからの Entity 構築（m_employees + m_persons + m_department_memberships）
/// - Domain Entity ↔ DbModel の相互変換（Mapper 使用）
/// - 実データベースでの CRUD 操作（Dapper + SQL）
/// 【基底クラス】MultiTableRepositoryBase（複数テーブル集約用）
/// 【実装状況】
/// - GetByBizIdAsync: 実装済み
/// - GetByIdAsync: 実装済み
/// - UpdateAsync: 実装済み
/// </summary>
public class EmployeeRepository(
    SqlQueryLoader queryLoader,
    EmployeeMapper mapper,
    IDbConnectionFactory connectionFactory,
    ICurrentUserService currentUser,
    Common.Clocks.IClock clock,
    IAppLogging<EmployeeRepository> logger)
    : MultiTableRepositoryBase<Employee, EmployeeDbModel, EmployeeRowId>(currentUser, clock),
        IEmployeeRepository
{
    private readonly SqlQueryLoader _queryLoader = queryLoader ?? throw new ArgumentNullException(nameof(queryLoader));

    private readonly EmployeeMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    private readonly IDbConnectionFactory _connectionFactory =
        connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

    private readonly IAppLogging<EmployeeRepository>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// BizId（従業員番号）で Employee を検索する
    /// 【責務】SQL ファイルを読み込んで Dapper で実行、DepartmentMembership も取得
    /// </summary>
    /// <param name="bizId">検索する従業員番号（1 以上）</param>
    /// <returns>見つかった従業員集約。見つからない場合は <see langword="null"/></returns>
    /// <exception cref="ArgumentException"><paramref name="bizId"/> が 0 以下の場合</exception>
    /// <exception cref="InvalidOperationException">対応する人物（<c>m_persons</c>）がない場合、または監査列の値が不正な場合</exception>
    public async Task<Employee?> GetByBizIdAsync(int bizId)
    {
        try
        {
            if (bizId <= 0)
            {
                throw new ArgumentException("Invalid BizId", nameof(bizId));
            }

            var sql = _queryLoader.LoadQuery("Employees.GetEmployeeByBizId", typeof(EmployeeRepository));

            using var connection = _connectionFactory.CreateConnection();
            var dbModel = await SqlMapper.QueryFirstOrDefaultAsync<EmployeeDbModel>(
                connection,
                sql,
                new { BizId = bizId });

            if (dbModel == null)
            {
                return null;
            }

            // Person を m_persons から読み込み（employee_row_id で JOIN）
            var personSql = _queryLoader.LoadQuery("Persons.GetPersonByEmployeeRowId", typeof(EmployeeRepository));
            _logger.LogInformation($"Person SQL loaded. EmployeeRowId={dbModel.RowId}");
            var personDbModel = await SqlMapper.QueryFirstOrDefaultAsync<PersonDbModel>(
                connection,
                personSql,
                new { EmployeeRowId = dbModel.RowId });

            if (personDbModel == null)
            {
                throw new InvalidOperationException($"Person not found for Employee RowId={dbModel.RowId}");
            }

            // DepartmentMembership を取得
            var departmentMembershipSql =
                _queryLoader.LoadQuery("Employees.GetEmployeeDepartmentMemberships", typeof(EmployeeRepository));
            _logger.LogInformation($"DepartmentMembership SQL loaded. RowId={dbModel.RowId}");
            var departmentMemberships = await SqlMapper.QueryAsync<DepartmentMembershipDbModel>(
                connection,
                departmentMembershipSql,
                new { EmployeeRowId = dbModel.RowId });

            // ============ 層間フィルター ============
            // DB の null を ValueObject の Unset() に変換（Infrastructure層の責務）
            if (!CreatedAt.TryFromDbValue(dbModel.CreatedAt, out var employeeCreatedAt))
            {
                throw new InvalidOperationException($"Invalid CreatedAt for Employee BizId={bizId}");
            }

            if (!UpdatedAt.TryFromDbValue(dbModel.UpdatedAt, out var employeeUpdatedAt))
            {
                throw new InvalidOperationException($"Invalid UpdatedAt for Employee BizId={bizId}");
            }

            if (!DeletedAt.TryFromDbValue(dbModel.DeletedAt, out var employeeDeletedAt))
            {
                throw new InvalidOperationException($"Invalid DeletedAt for Employee BizId={bizId}");
            }

            // Person の監査フィールドも変換
            if (!CreatedAt.TryFromDbValue(personDbModel.CreatedAt, out var personCreatedAt))
            {
                throw new InvalidOperationException($"Invalid CreatedAt for Person BizId={bizId}");
            }

            if (!UpdatedAt.TryFromDbValue(personDbModel.UpdatedAt, out var personUpdatedAt))
            {
                throw new InvalidOperationException($"Invalid UpdatedAt for Person BizId={bizId}");
            }

            if (!DeletedAt.TryFromDbValue(personDbModel.DeletedAt, out var personDeletedAt))
            {
                throw new InvalidOperationException($"Invalid DeletedAt for Person BizId={bizId}");
            }

            return _mapper.ToDomainEntity(dbModel, personDbModel, departmentMemberships.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError($"GetByBizIdAsync failed for BizId={bizId}", ex);
            throw;
        }
    }

    /// <summary>
    /// EmployeeRowId で Employee を検索する
    /// 【責務】複数テーブル（m_employees, m_persons, m_department_memberships）から統合データを読み込み
    /// </summary>
    /// <param name="id">検索する従業員の行ID</param>
    /// <returns>見つかった従業員集約。見つからない場合は <see langword="null"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="id"/> が <see langword="null"/> の場合</exception>
    /// <exception cref="InvalidOperationException">対応する人物（<c>m_persons</c>）がない場合、または監査列の値が不正な場合</exception>
    public async Task<Employee?> GetByIdAsync(EmployeeRowId id)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(id);

            using var connection = _connectionFactory.CreateConnection();

            // 1. m_employees テーブルを読み込み
            var employeeSql = _queryLoader.LoadQuery("Employees.GetEmployeeByRowId", typeof(EmployeeRepository));
            var employeeDbModel = await SqlMapper.QueryFirstOrDefaultAsync<EmployeeDbModel>(
                connection,
                employeeSql,
                new { RowId = id.Value });

            if (employeeDbModel == null)
            {
                return null;
            }

            // 2. m_persons テーブルを読み込み（employee_row_id で結合）
            var personSql = _queryLoader.LoadQuery("Persons.GetPersonByEmployeeRowId", typeof(EmployeeRepository));
            var personDbModel = await SqlMapper.QueryFirstOrDefaultAsync<PersonDbModel>(
                connection,
                personSql,
                new { EmployeeRowId = id.Value });

            if (personDbModel == null)
            {
                throw new InvalidOperationException($"Person not found for Employee RowId={id.Value}");
            }

            // 3. m_department_memberships テーブルを読み込み（複数行）
            var membershipSql =
                _queryLoader.LoadQuery("Employees.GetEmployeeDepartmentMemberships", typeof(EmployeeRepository));
            var membershipDbModels = await SqlMapper.QueryAsync<DepartmentMembershipDbModel>(
                connection,
                membershipSql,
                new { EmployeeRowId = id.Value });

            // ============ 層間フィルター ============
            // DB の null を ValueObject の Unset() に変換（Infrastructure層の責務）
            if (!CreatedAt.TryFromDbValue(employeeDbModel.CreatedAt, out var employeeCreatedAt))
            {
                throw new InvalidOperationException($"Invalid CreatedAt for Employee RowId={id.Value}");
            }

            if (!UpdatedAt.TryFromDbValue(employeeDbModel.UpdatedAt, out var employeeUpdatedAt))
            {
                throw new InvalidOperationException($"Invalid UpdatedAt for Employee RowId={id.Value}");
            }

            if (!DeletedAt.TryFromDbValue(employeeDbModel.DeletedAt, out var employeeDeletedAt))
            {
                throw new InvalidOperationException($"Invalid DeletedAt for Employee RowId={id.Value}");
            }

            // Person の監査フィールドも変換
            if (!CreatedAt.TryFromDbValue(personDbModel.CreatedAt, out var personCreatedAt))
            {
                throw new InvalidOperationException($"Invalid CreatedAt for Person EmployeeRowId={id.Value}");
            }

            if (!UpdatedAt.TryFromDbValue(personDbModel.UpdatedAt, out var personUpdatedAt))
            {
                throw new InvalidOperationException($"Invalid UpdatedAt for Person EmployeeRowId={id.Value}");
            }

            if (!DeletedAt.TryFromDbValue(personDbModel.DeletedAt, out var personDeletedAt))
            {
                throw new InvalidOperationException($"Invalid DeletedAt for Person EmployeeRowId={id.Value}");
            }

            _logger.LogInformation(
                $"Employee loaded: RowId={id.Value}, Person found, DepartmentMemberships={membershipDbModels.Count()}");

            return _mapper.ToDomainEntity(employeeDbModel, personDbModel, membershipDbModels.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError($"GetByIdAsync failed for RowId={id.Value}", ex);
            throw;
        }
    }

    /// <summary>
    /// GetByIdAsync の別名メソッド（互換性維持用）
    /// 【責務】EmployeeRowId で Employee を検索
    /// </summary>
    /// <param name="rowId">検索する従業員の行ID</param>
    /// <returns>見つかった従業員集約。見つからない場合は <see langword="null"/></returns>
    public Task<Employee?> GetByRowIdAsync(EmployeeRowId rowId) =>
        GetByIdAsync(rowId);

    /// <summary>
    /// PersonRowId で Employee を検索する（1:1 関係のため 0 件または 1 件）
    /// 【責務】m_persons.row_id で m_employees を JOIN 検索
    /// </summary>
    /// <param name="personRowId">検索する人物の行ID</param>
    /// <returns>該当する従業員集約の一覧（0 件または 1 件）</returns>
    /// <exception cref="ArgumentNullException"><paramref name="personRowId"/> が <see langword="null"/> の場合</exception>
    /// <exception cref="InvalidOperationException">対応する人物（<c>m_persons</c>）がない場合</exception>
    public async Task<IReadOnlyList<Employee>> GetByPersonRowIdAsync(PersonRowId personRowId)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(personRowId);

            using var connection = _connectionFactory.CreateConnection();

            var sql = _queryLoader.LoadQuery("Employees.GetEmployeeByPersonRowId", typeof(EmployeeRepository));
            var employeeDbModels = await SqlMapper.QueryAsync<EmployeeDbModel>(
                connection,
                sql,
                new { PersonRowId = personRowId.Value });

            var results = new List<Employee>();
            foreach (var employeeDbModel in employeeDbModels)
            {
                var personSql = _queryLoader.LoadQuery("Persons.GetPersonByEmployeeRowId", typeof(EmployeeRepository));
                var personDbModel = await SqlMapper.QueryFirstOrDefaultAsync<PersonDbModel>(
                    connection,
                    personSql,
                    new { EmployeeRowId = employeeDbModel.RowId });

                if (personDbModel == null)
                {
                    throw new InvalidOperationException($"Person not found for Employee RowId={employeeDbModel.RowId}");
                }

                var membershipSql =
                    _queryLoader.LoadQuery("Employees.GetEmployeeDepartmentMemberships", typeof(EmployeeRepository));
                var membershipDbModels = await SqlMapper.QueryAsync<DepartmentMembershipDbModel>(
                    connection,
                    membershipSql,
                    new { EmployeeRowId = employeeDbModel.RowId });

                results.Add(_mapper.ToDomainEntity(employeeDbModel, personDbModel, membershipDbModels.ToList()));
            }

            return results.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError($"GetByPersonRowIdAsync failed for PersonRowId={personRowId.Value}", ex);
            throw;
        }
    }

    /// <summary>
    /// row_version（timestamp列）を除いた RepoDb Field 一覧を取得する
    /// 【重要】SQL Server の timestamp は自動管理のため、明示的な値を INSERT/UPDATE に含められない。
    ///         RepoDb の fields パラメータで対象列を絞り込むことで除外する。
    /// </summary>
    private static IEnumerable<Field> FieldsExcludingRowVersion<T>() =>
        Field.Parse(typeof(T)).Where(f => f.Name != "row_version");

    /// <summary>
    /// UPDATE 対象から row_version・created_at・created_by を除いた RepoDb Field 一覧を取得する
    /// 【重要】Mapper の ToDbModel/ToPersonDbModel は CreatedAt/CreatedBy を設定しない（Mapper の責務外）ため、
    ///         UPDATE 時に DbModel の CreatedAt が既定値（0001-01-01）のまま SET 句に含まれると
    ///         SqlDateTime overflow が発生する。作成時刻は不変のため UPDATE 対象から除外する。
    /// </summary>
    private static IEnumerable<Field> FieldsExcludingRowVersionAndCreatedAudit<T>() =>
        Field.Parse(typeof(T)).Where(f => f.Name is not ("row_version" or "created_at" or "created_by"));

    /// <summary>
    /// Employee 集約を保存する（新規作成または更新）
    /// 【責務】RowVersion で Insert/Update を判定し、適切なメソッドへ委譲
    /// 【特徴】RowVersion が空（Create時）なら AddAsync、非空（Reconstruct時）なら UpdateAsync
    /// 【メリット】呼び出し側は SaveAsync 1つで統一でき、Repository が内部で最適な実装を選択
    /// </summary>
    /// <param name="employee">保存する従業員集約。<c>RowVersion</c> が空の場合は新規作成、それ以外は更新</param>
    /// <exception cref="ArgumentNullException"><paramref name="employee"/> が <see langword="null"/> の場合</exception>
    /// <exception cref="InvalidOperationException">他のユーザーによる更新・削除で <c>row_version</c> が一致しない場合（楽観ロックの競合）</exception>
    public async Task SaveAsync(Employee employee)
    {
        ArgumentNullException.ThrowIfNull(employee);

        // 新規作成判定：RowVersion が未設定（空配列）なら Insert
        // 【重要】RowVersion は Mapper.ToDomainEntity（Reconstruct）でのみ設定される。
        //         Employee.Create() による新規作成では空配列のまま。
        bool isInsert = employee.RowVersion.Length == 0;

        if (isInsert)
        {
            await AddAsync(employee);
        }
        else
        {
            await UpdateAsync(employee);
        }
    }

    /// <summary>
    /// Employee 集約を新規保存する
    /// 【責務】複数テーブル（m_employees, m_persons, m_department_memberships）をトランザクション内でINSERT
    /// 【特徴】CreatedAt/CreatedBy は Repository が設定（MultiTableRepositoryBase 経由）
    /// 【row_version 除外】RepoDb の fields パラメータで row_version 列を INSERT 対象から除外
    /// </summary>
    /// <param name="employee">新規保存する従業員集約</param>
    /// <exception cref="ArgumentNullException"><paramref name="employee"/> が <see langword="null"/> の場合</exception>
    /// <remarks>
    /// <para>【副作用】失敗した場合はトランザクションをロールバックし、エラーログを出力して例外を再送出</para>
    /// </remarks>
    public async Task AddAsync(Employee employee)
    {
        ArgumentNullException.ThrowIfNull(employee);

        using var connection = _connectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

        try
        {
            // 1. m_employees へ INSERT（row_version は fields で除外）
            var empDbModel = _mapper.ToDbModel(employee);
            SetCreatedAtAudit(empDbModel);
            SetCreatedByAudit(empDbModel);

            await connection.InsertAsync(
                empDbModel,
                fields: FieldsExcludingRowVersion<EmployeeDbModel>(),
                transaction: transaction);

            // 2. m_persons へ INSERT（row_version は fields で除外）
            var personDbModel = _mapper.ToPersonDbModel(employee.Person, employee.RowId.Value);
            SetAuditField(personDbModel, "CreatedAt", Clock.JstNow.Value);
            SetAuditField(personDbModel, "CreatedBy", CurrentUser.EmployeeRowId);

            await connection.InsertAsync(
                personDbModel,
                fields: FieldsExcludingRowVersion<PersonDbModel>(),
                transaction: transaction);

            // 3. m_department_memberships へ INSERT（row_version は fields で除外）
            foreach (var membership in employee.DepartmentMemberships)
            {
                var membershipDbModel = _mapper.ToDepartmentMembershipDbModel(membership);
                await connection.InsertAsync(
                    membershipDbModel,
                    fields: FieldsExcludingRowVersion<DepartmentMembershipDbModel>(),
                    transaction: transaction);
            }

            transaction.Commit();
            _logger.LogInformation($"Employee added: RowId={employee.RowId.Value}");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError($"AddAsync failed for RowId={employee.RowId.Value}", ex);
            throw;
        }
    }

    /// <summary>
    /// Employee 集約を保存（更新）する
    /// 【責務】複数テーブル（m_employees, m_persons, m_department_memberships）をトランザクション内で更新
    /// 【特徴】楽観ロック（row_version）による競合検出、自動タイムスタンプ管理（MultiTableRepositoryBase 経由）
    /// 【row_version 除外】RepoDb の fields パラメータで row_version 列を UPDATE 対象から除外
    /// </summary>
    /// <param name="employee">更新する従業員集約。<c>RowVersion</c> は読み込み時の値であること</param>
    /// <exception cref="ArgumentNullException"><paramref name="employee"/> が <see langword="null"/> の場合</exception>
    /// <exception cref="InvalidOperationException">他のユーザーによる更新・削除で <c>row_version</c> が一致しない場合（楽観ロックの競合）</exception>
    /// <remarks>
    /// <para>【注意】部署メンバーシップ（<c>m_department_memberships</c>）の追加・削除・更新は未実装。<c>m_employees</c> と <c>m_persons</c> のみ更新</para>
    /// </remarks>
    public async Task UpdateAsync(Employee employee)
    {
        ArgumentNullException.ThrowIfNull(employee);

        using var connection = _connectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

        try
        {
            // DbModel を生成し、監査フィールドを設定
            var empDbModel = _mapper.ToDbModel(employee);
            SetUpdatedAtAudit(empDbModel);
            SetUpdatedByAudit(empDbModel);

            // 1. m_employees を RepoDb UpdateAsync で UPDATE（楽観ロック付き、row_version・created_at・created_by は fields で除外）
            var oldRowVersion = employee.RowVersion;
            var empRowsAffected = await connection.UpdateAsync(
                empDbModel,
                new QueryGroup(new[]
                {
                    new QueryField("row_id", empDbModel.RowId),
                    new QueryField("row_version", oldRowVersion)
                }),
                fields: FieldsExcludingRowVersionAndCreatedAudit<EmployeeDbModel>(),
                transaction: transaction
            );

            if (empRowsAffected == 0)
            {
                throw new InvalidOperationException("Employee was updated by another user (concurrency conflict)");
            }

            // 2. m_persons を RepoDb UpdateAsync で UPDATE（楽観ロック付き、row_version・created_at・created_by は fields で除外）
            var personDbModel = _mapper.ToPersonDbModel(employee.Person, employee.RowId.Value);
            SetAuditField(personDbModel, "UpdatedAt", Clock.JstNow.Value);
            SetAuditField(personDbModel, "UpdatedBy", CurrentUser.EmployeeRowId);

            var oldPersonRowVersion = employee.Person.RowVersion;
            var perRowsAffected = await connection.UpdateAsync(
                personDbModel,
                new QueryGroup(new[]
                {
                    new QueryField("row_id", personDbModel.RowId),
                    new QueryField("row_version", oldPersonRowVersion)
                }),
                fields: FieldsExcludingRowVersionAndCreatedAudit<PersonDbModel>(),
                transaction: transaction
            );

            if (perRowsAffected == 0)
            {
                throw new InvalidOperationException("Person was updated by another user (concurrency conflict)");
            }

            // 3. m_department_memberships は複雑な更新ロジック（追加・削除・更新）
            // ← 実装計画では「既存ロジック継続」として、ここでは未実装

            transaction.Commit();
            _logger.LogInformation($"Employee updated: RowId={employee.RowId.Value}");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError($"UpdateAsync failed for RowId={employee.RowId.Value}", ex);
            throw;
        }
    }

    /// <summary>
    /// Employee を論理削除する
    /// 【責務】m_employees の deleted_at/deleted_by を設定（RepoDb UpdateAsync）
    /// 【注意】m_persons は他のEntityから参照される可能性があるため論理削除しない（Employee側のみ）
    /// </summary>
    /// <param name="id">削除する従業員の行ID</param>
    /// <exception cref="ArgumentNullException"><paramref name="id"/> が <see langword="null"/> の場合</exception>
    /// <exception cref="InvalidOperationException">従業員が見つからない場合、または楽観ロックの競合の場合</exception>
    public async Task DeleteAsync(EmployeeRowId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        using var connection = _connectionFactory.CreateConnection();

        try
        {
            var existing = await GetByIdAsync(id);
            if (existing == null)
            {
                throw new InvalidOperationException($"Employee with RowId={id.Value} not found");
            }

            var empDbModel = _mapper.ToDbModel(existing);
            SetDeletedAtAudit(empDbModel);
            SetDeletedByAudit(empDbModel);
            SetUpdatedAtAudit(empDbModel);
            SetUpdatedByAudit(empDbModel);

            var rowsAffected = await connection.UpdateAsync<EmployeeDbModel>(
                empDbModel,
                new QueryGroup(new[]
                {
                    new QueryField("row_id", empDbModel.RowId),
                    new QueryField("row_version", existing.RowVersion)
                }),
                fields: FieldsExcludingRowVersionAndCreatedAudit<EmployeeDbModel>()
            );

            if (rowsAffected == 0)
            {
                throw new InvalidOperationException("Employee was updated by another user (concurrency conflict)");
            }

            _logger.LogInformation($"Employee deleted (logical): RowId={id.Value}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"DeleteAsync failed for RowId={id.Value}", ex);
            throw;
        }
    }
}
