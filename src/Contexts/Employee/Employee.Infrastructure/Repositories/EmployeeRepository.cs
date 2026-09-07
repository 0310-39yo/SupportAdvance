namespace SupportAdvance.Contexts.Employee.Infrastructure.Repositories;

using Dapper;
using Common.Clocks;
using SupportAdvance.Contexts.Employee.Application.Repositories;
using Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using Domain.ValueObjects.Person;
using Mappers;
using Models;
using Crosscutting.Logging;
using SupportAdvance.Infrastructure.Persistence;

/// <summary>
/// Employee 集約の Repository 実装
///
/// 【責務】
/// - Domain Entity ↔ DbModel の相互変換（Mapper 使用）
/// - 実データベースでの CRUD 操作（Dapper + SQL）
/// 【実装状況】
/// - GetByBizIdAsync: 実装済み（Dapper + SQL）
/// - その他メソッド: 未実装（必要に応じて追加予定）
/// </summary>
public class EmployeeRepository(
    EmployeeMapper mapper,
    IClock clock,
    IDbConnectionFactory connectionFactory,
    IAppLogging<EmployeeRepository> logger)
    : IEmployeeRepository
{
    private readonly EmployeeMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IClock _clock = clock ?? throw new ArgumentNullException(nameof(clock));

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

            return _mapper.ToDomainEntity(dbModel, personDbModel, departmentMemberships.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError($"GetByBizIdAsync failed for BizId={bizId}", ex);
            throw;
        }
    }

    // 未実装メソッド（スタブ）
    public Task<Employee?> GetByIdAsync(EmployeeRowId id)
        => throw new NotImplementedException("GetByIdAsync is not yet implemented");

    public Task<Employee?> GetByRowIdAsync(EmployeeRowId rowId)
        => throw new NotImplementedException("GetByRowIdAsync is not yet implemented");

    public Task<IReadOnlyList<Employee>> GetByPersonRowIdAsync(PersonRowId personRowId)
        => throw new NotImplementedException("GetByPersonRowIdAsync is not yet implemented");

    public Task AddAsync(Employee employee)
        => throw new NotImplementedException("AddAsync is not yet implemented");

    public Task UpdateAsync(Employee employee)
        => throw new NotImplementedException("UpdateAsync is not yet implemented");

    public Task DeleteAsync(EmployeeRowId id)
        => throw new NotImplementedException("DeleteAsync is not yet implemented");
}
