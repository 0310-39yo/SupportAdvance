namespace SupportAdvance.Contexts.Employee.Infrastructure.Repositories;

using Dapper;
using SupportAdvance.Contexts.Employee.Application.Repositories;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.Contexts.Employee.Infrastructure.Mappers;
using SupportAdvance.Contexts.Employee.Infrastructure.Models;
using SupportAdvance.Crosscutting.Logging;
using SupportAdvance.Infrastructure.Persistence;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

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
public class EmployeeRepository : IEmployeeRepository
{
    private readonly EmployeeMapper _mapper;
    private readonly IClock _clock;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IAppLogging<EmployeeRepository> _logger;

    public EmployeeRepository(EmployeeMapper mapper, IClock clock, IDbConnectionFactory connectionFactory, IAppLogging<EmployeeRepository> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// BizId（従業員番号）で Employee を検索する
    /// 【責務】SQL ファイルを読み込んで Dapper で実行
    /// </summary>
    public async Task<Employee?> GetByBizIdAsync(int bizId)
    {
        try
        {
            if (bizId <= 0)
                throw new ArgumentException("Invalid BizId", nameof(bizId));

            var sql = SqlQueryLoader.LoadQuery("Employee.GetEmployeeByBizId");

            using var connection = _connectionFactory.CreateConnection();
            var dbModel = await connection.QueryFirstOrDefaultAsync<EmployeeDbModel>(
                sql,
                new { BizId = bizId });

            if (dbModel == null)
                return null;

            var person = CreatePersonFromDbModel(dbModel);
            return _mapper.ToDomainEntity(dbModel, person);
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

    /// <summary>
    /// DbModel から Person Entity を生成するヘルパーメソッド
    /// 【責務】SQL JOINで取得した m_persons データから Person を生成
    /// </summary>
    private Person CreatePersonFromDbModel(EmployeeDbModel dbModel)
    {
        return Person.Create(
            PersonRowId.From(dbModel.PersonRowId),
            PersonLastName.From(dbModel.LastName),
            PersonFirstName.From(dbModel.FirstName),
            PersonLastNameKana.From(dbModel.LastNameKana),
            PersonFirstNameKana.From(dbModel.FirstNameKana));
    }
}
