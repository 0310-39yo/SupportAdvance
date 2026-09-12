using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Employee.Infrastructure.Repositories;

using System.Data;
using Dapper;
using SupportAdvance.Contexts.Employee.Application.Repositories;
using Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using Domain.ValueObjects.Person;
using Mappers;
using Models;
using Crosscutting.Logging;
using SupportAdvance.Infrastructure.Persistence;
using SupportAdvance.Infrastructure.Repositories;
using SupportAdvance.Infrastructure.Services;
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
    EmployeeMapper mapper,
    IDbConnectionFactory connectionFactory,
    ICurrentUserService currentUser,
    Common.Clocks.IClock clock,
    IAppLogging<EmployeeRepository> logger)
    : MultiTableRepositoryBase<Employee, EmployeeDbModel, EmployeeRowId>(currentUser, clock),
        IEmployeeRepository
{
    private readonly EmployeeMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    private readonly IDbConnectionFactory _connectionFactory =
        connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

    private readonly IAppLogging<EmployeeRepository>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// BizId（従業員番号）で Employee を検索する
    /// 【責務】SQL ファイルを読み込んで Dapper で実行、DepartmentMembership も取得
    /// </summary>
    public async Task<Employee?> GetByBizIdAsync(int bizId)
    {
        try
        {
            if (bizId <= 0)
            {
                throw new ArgumentException("Invalid BizId", nameof(bizId));
            }

            var sql = SqlQueryLoader.LoadQuery("Employee.GetEmployeeByBizId");

            using var connection = _connectionFactory.CreateConnection();
            var dbModel = await connection.QueryFirstOrDefaultAsync<EmployeeDbModel>(
                sql,
                new { BizId = bizId });

            if (dbModel == null)
            {
                return null;
            }

            // Person を m_persons から読み込み（employee_row_id で JOIN）
            var personSql = SqlQueryLoader.LoadQuery("Employee.GetPersonByEmployeeRowId");
            _logger.LogInformation($"Person SQL loaded. EmployeeRowId={dbModel.RowId}");
            var personDbModel = await connection.QueryFirstOrDefaultAsync<PersonDbModel>(
                personSql,
                new { EmployeeRowId = dbModel.RowId });

            if (personDbModel == null)
            {
                throw new InvalidOperationException($"Person not found for Employee RowId={dbModel.RowId}");
            }

            // DepartmentMembership を取得
            var departmentMembershipSql = SqlQueryLoader.LoadQuery("Employee.GetEmployeeDepartmentMemberships");
            _logger.LogInformation($"DepartmentMembership SQL loaded. RowId={dbModel.RowId}");
            var departmentMemberships = await connection.QueryAsync<DepartmentMembershipDbModel>(
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
    public async Task<Employee?> GetByIdAsync(EmployeeRowId id)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(id);

            using var connection = _connectionFactory.CreateConnection();

            // 1. m_employees テーブルを読み込み
            var employeeSql = SqlQueryLoader.LoadQuery("Employee.GetEmployeeByRowId");
            var employeeDbModel = await connection.QueryFirstOrDefaultAsync<EmployeeDbModel>(
                employeeSql,
                new { RowId = id.Value });

            if (employeeDbModel == null)
            {
                return null;
            }

            // 2. m_persons テーブルを読み込み（employee_row_id で結合）
            var personSql = SqlQueryLoader.LoadQuery("Employee.GetPersonByEmployeeRowId");
            var personDbModel = await connection.QueryFirstOrDefaultAsync<PersonDbModel>(
                personSql,
                new { EmployeeRowId = id.Value });

            if (personDbModel == null)
            {
                throw new InvalidOperationException($"Person not found for Employee RowId={id.Value}");
            }

            // 3. m_department_memberships テーブルを読み込み（複数行）
            var membershipSql = SqlQueryLoader.LoadQuery("Employee.GetEmployeeDepartmentMemberships");
            var membershipDbModels = await connection.QueryAsync<DepartmentMembershipDbModel>(
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
    public Task<Employee?> GetByRowIdAsync(EmployeeRowId rowId) =>
        GetByIdAsync(rowId);

    public Task<IReadOnlyList<Employee>> GetByPersonRowIdAsync(PersonRowId personRowId)
        => throw new NotImplementedException("GetByPersonRowIdAsync is not yet implemented");

    public Task AddAsync(Employee employee)
        => throw new NotImplementedException("AddAsync is not yet implemented");

    /// <summary>
    /// Employee 集約を保存（更新）する
    /// 【責務】複数テーブル（m_employees, m_persons, m_department_memberships）をトランザクション内で更新
    /// 【特徴】楽観ロック（row_version）による競合検出、自動タイムスタンプ管理（MultiTableRepositoryBase 経由）
    /// </summary>
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

            // 1. m_employees を UPDATE
            var empRowsAffected = await connection.ExecuteAsync(
                @"UPDATE [dbo].[m_employees]
                  SET [biz_division] = @BizDivision,
                      [biz_id] = @BizId,
                      [biz_code] = @BizCode,
                      [retired_on] = @RetiredOn,
                      [updated_at] = @UpdatedAt,
                      [updated_by] = @UpdatedBy,
                      [row_version] = DEFAULT
                  WHERE [row_id] = @RowId
                  AND [row_version] = @OldRowVersion",
                new
                {
                    RowId = empDbModel.RowId,
                    BizDivision = empDbModel.BizDivision,
                    BizId = empDbModel.BizId,
                    BizCode = employee.BizCode.ToString(),
                    RetiredOn = empDbModel.RetiredOn,
                    UpdatedAt = empDbModel.UpdatedAt,
                    UpdatedBy = empDbModel.UpdatedBy,
                    OldRowVersion = employee.RowVersion
                },
                transaction
            );

            if (empRowsAffected == 0)
            {
                throw new InvalidOperationException("Employee was updated by another user (concurrency conflict)");
            }

            // 2. m_persons を UPDATE
            var personDbModel = _mapper.ToPersonDbModel(employee.Person, employee.RowId.Value);
            SetAuditField(personDbModel, "UpdatedAt", Clock.JstNow.Value);
            SetAuditField(personDbModel, "UpdatedBy", CurrentUser.EmployeeRowId);

            var perRowsAffected = await connection.ExecuteAsync(
                @"UPDATE [dbo].[m_persons]
                  SET [last_name] = @LastName,
                      [first_name] = @FirstName,
                      [last_name_kana] = @LastNameKana,
                      [first_name_kana] = @FirstNameKana,
                      [updated_at] = @UpdatedAt,
                      [updated_by] = @UpdatedBy,
                      [row_version] = DEFAULT
                  WHERE [row_id] = @RowId
                  AND [row_version] = @OldRowVersion",
                new
                {
                    RowId = personDbModel.RowId,
                    LastName = personDbModel.LastName,
                    FirstName = personDbModel.FirstName,
                    LastNameKana = personDbModel.LastNameKana,
                    FirstNameKana = personDbModel.FirstNameKana,
                    UpdatedAt = personDbModel.UpdatedAt,
                    UpdatedBy = personDbModel.UpdatedBy,
                    OldRowVersion = employee.Person.RowVersion
                },
                transaction
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

    public Task DeleteAsync(EmployeeRowId id)
        => throw new NotImplementedException("DeleteAsync is not yet implemented");
}
