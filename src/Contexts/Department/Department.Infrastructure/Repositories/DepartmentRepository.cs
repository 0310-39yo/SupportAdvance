using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Department.Infrastructure.Repositories;

using System.Data;
using Dapper;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Department.Application.Repositories;
using SupportAdvance.Contexts.Department.Domain.Entities;
using SupportAdvance.Contexts.Department.Domain.ValueObjects;
using SupportAdvance.Contexts.Department.Infrastructure.DbModels;
using SupportAdvance.Contexts.Department.Infrastructure.Mappers;
using SupportAdvance.Infrastructure.Persistence;
using SupportAdvance.Infrastructure.Services;

/// <summary>
/// 部署リポジトリの実装
///
/// 【責務】
///   - Department 集約の永続化（保存・取得・削除）
///   - DbModel ↔ Entity のマッピング
///   - 監査フィールドの設定
/// 【実装】
///   - Dapper でジェネリック CRUD
///   - Mapper で型変換
///   - SQL で直接実行（単一テーブル集約）
/// </summary>
public class DepartmentRepository(
    DepartmentMapper mapper,
    IDbConnectionFactory connectionFactory,
    ICurrentUserService currentUser,
    IClock clock)
    : IDepartmentRepository
{
    private readonly DepartmentMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IDbConnectionFactory _connectionFactory =
        connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    private readonly ICurrentUserService _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    private readonly IClock _clock = clock ?? throw new ArgumentNullException(nameof(clock));

    /// <summary>
    /// 部署を行IDで取得する
    /// </summary>
    public async Task<Department?> GetByIdAsync(DepartmentRowId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        using var connection = _connectionFactory.CreateConnection();
        var dbModel = await connection.QuerySingleOrDefaultAsync<DepartmentDbModel>(
            "SELECT * FROM m_departments WHERE row_id = @rowId",
            new { rowId = id.Value });

        if (dbModel == null)
        {
            return null;
        }

        return _mapper.ToDomainEntity(dbModel);
    }

    /// <summary>
    /// 部署をコードで取得する
    /// </summary>
    public async Task<Department?> GetByCodeAsync(DepartmentCode code)
    {
        ArgumentNullException.ThrowIfNull(code);

        using var connection = _connectionFactory.CreateConnection();
        var dbModel = await connection.QuerySingleOrDefaultAsync<DepartmentDbModel>(
            "SELECT * FROM m_departments WHERE code = @code AND deleted_at IS NULL",
            new { code = code.Value });

        if (dbModel == null)
        {
            return null;
        }

        return _mapper.ToDomainEntity(dbModel);
    }

    /// <summary>
    /// すべての部署を取得する（廃止済みを含む）
    /// </summary>
    public async Task<IReadOnlyList<Department>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var dbModels = await connection.QueryAsync<DepartmentDbModel>(
            "SELECT * FROM m_departments ORDER BY row_id");

        return dbModels
            .Select(dbModel => _mapper.ToDomainEntity(dbModel))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// 部署を保存する（新規作成または更新）
    /// 【責務】UpdatedAt/UpdatedBy を設定
    /// </summary>
    public async Task SaveAsync(Department department)
    {
        ArgumentNullException.ThrowIfNull(department);

        var dbModel = _mapper.ToDbModel(department);

        // 監査フィールドを設定（Repository の責務）
        var now = _clock.JstNow.Value;
        var userId = _currentUser.EmployeeRowId;

        // 新規作成判定（CreatedAt が未設定の場合）
        if (dbModel.CreatedAt == default)
        {
            dbModel.CreatedAt = now;
            dbModel.CreatedBy = userId;
        }
        else
        {
            // 更新時
            dbModel.UpdatedAt = now;
            dbModel.UpdatedBy = userId;
        }

        using var connection = _connectionFactory.CreateConnection();
        if (dbModel.RowId == 0)
        {
            // 新規作成：INSERT
            await connection.ExecuteAsync(
                @"INSERT INTO m_departments (code, name, level, parent_department_row_id, manager_employee_row_id, abolished_on, created_at, created_by, updated_at, updated_by, deleted_at, deleted_by)
                  VALUES (@code, @name, @level, @parentDepartmentRowId, @managerEmployeeRowId, @abolishedOn, @createdAt, @createdBy, @updatedAt, @updatedBy, @deletedAt, @deletedBy)",
                dbModel);
        }
        else
        {
            // 更新：UPDATE
            await connection.ExecuteAsync(
                @"UPDATE m_departments SET code = @code, name = @name, level = @level, parent_department_row_id = @parentDepartmentRowId,
                  manager_employee_row_id = @managerEmployeeRowId, abolished_on = @abolishedOn, updated_at = @updatedAt, updated_by = @updatedBy
                  WHERE row_id = @rowId",
                dbModel);
        }
    }

    /// <summary>
    /// 部署を論理削除する
    /// 【責務】DeletedAt/DeletedBy を設定
    /// </summary>
    public async Task DeleteAsync(DepartmentRowId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        var now = _clock.JstNow.Value;
        var userId = _currentUser.EmployeeRowId;

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            @"UPDATE m_departments SET deleted_at = @deletedAt, deleted_by = @deletedBy, updated_at = @updatedAt, updated_by = @updatedBy
              WHERE row_id = @rowId",
            new { rowId = id.Value, deletedAt = now, deletedBy = userId, updatedAt = now, updatedBy = userId });
    }
}
