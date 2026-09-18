using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Department.Infrastructure.Repositories;

using System.Data;
using Dapper;
using RepoDb;
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
///   - SQL ファイルで実行（単一テーブル集約）
/// </summary>
public class DepartmentRepository(
    SqlQueryLoader queryLoader,
    DepartmentMapper mapper,
    IDbConnectionFactory connectionFactory,
    ICurrentUserService currentUser,
    IClock clock)
    : IDepartmentRepository
{
    private readonly SqlQueryLoader _queryLoader = queryLoader ?? throw new ArgumentNullException(nameof(queryLoader));
    private readonly DepartmentMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IDbConnectionFactory _connectionFactory =
        connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    private readonly ICurrentUserService _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    private readonly IClock _clock = clock ?? throw new ArgumentNullException(nameof(clock));

    /// <summary>
    /// 部署を行IDで取得する
    /// </summary>
    /// <param name="id">取得する部署の行ID</param>
    /// <returns>見つかった部署。見つからない場合は <see langword="null"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="id"/> が <see langword="null"/> の場合</exception>
    public async Task<Department?> GetByIdAsync(DepartmentRowId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        var sql = _queryLoader.LoadQuery("Departments.GetDepartmentById", typeof(DepartmentRepository));

        using var connection = _connectionFactory.CreateConnection();
        var dbModel = await connection.QuerySingleOrDefaultAsync<DepartmentDbModel>(
            sql,
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
    /// <param name="code">取得する部署のコード</param>
    /// <returns>見つかった部署。見つからない場合は <see langword="null"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="code"/> が <see langword="null"/> の場合</exception>
    public async Task<Department?> GetByCodeAsync(DepartmentCode code)
    {
        ArgumentNullException.ThrowIfNull(code);

        var sql = _queryLoader.LoadQuery("Departments.GetDepartmentByCode", typeof(DepartmentRepository));

        using var connection = _connectionFactory.CreateConnection();
        var dbModel = await connection.QuerySingleOrDefaultAsync<DepartmentDbModel>(
            sql,
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
    /// <returns>すべての部署（廃止済みを含む）。部署なしの場合は空の一覧</returns>
    public async Task<IReadOnlyList<Department>> GetAllAsync()
    {
        var sql = _queryLoader.LoadQuery("Departments.GetAllDepartments", typeof(DepartmentRepository));

        using var connection = _connectionFactory.CreateConnection();
        var dbModels = await connection.QueryAsync<DepartmentDbModel>(sql);

        return dbModels
            .Select(dbModel => _mapper.ToDomainEntity(dbModel))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// row_version（timestamp列）を除いた RepoDb Field 一覧を取得する
    /// 【重要】SQL Server の timestamp は自動管理のため、明示的な値を INSERT/UPDATE に含められない。
    ///         RepoDb の fields パラメータで対象列を絞り込むことで除外する。
    /// </summary>
    private static IEnumerable<Field> FieldsExcludingRowVersion() =>
        Field.Parse(typeof(DepartmentDbModel)).Where(f => f.Name != "row_version");

    /// <summary>
    /// UPDATE 対象から row_version・created_at・created_by を除いた RepoDb Field 一覧を取得する
    /// 【重要】_mapper.ToDbModel() は CreatedAt/CreatedBy を設定しない（Mapper の責務外）ため、
    ///         UPDATE 時に DbModel の CreatedAt が既定値（0001-01-01）のまま SET 句に含まれると
    ///         SqlDateTime overflow が発生する。作成時刻は不変のため UPDATE 対象から除外する。
    /// </summary>
    private static IEnumerable<Field> FieldsExcludingRowVersionAndCreatedAudit() =>
        Field.Parse(typeof(DepartmentDbModel))
            .Where(f => f.Name is not ("row_version" or "created_at" or "created_by"));

    /// <summary>
    /// 部署を保存する（新規作成または更新）
    /// 【責務】UpdatedAt/UpdatedBy を設定、RepoDb でDB操作（row_version は fields で除外）
    /// </summary>
    /// <param name="department">保存する部署。<c>RowVersion</c> が空の場合は新規作成、それ以外は更新</param>
    /// <exception cref="ArgumentNullException"><paramref name="department"/> が <see langword="null"/> の場合</exception>
    /// <remarks>
    /// <para>【注意】更新時の楽観ロック（<c>row_version</c> の照合）なし</para>
    /// </remarks>
    public async Task SaveAsync(Department department)
    {
        ArgumentNullException.ThrowIfNull(department);

        var dbModel = _mapper.ToDbModel(department);

        // 監査フィールドを設定（Repository の責務）
        var now = _clock.JstNow.Value;
        var userId = _currentUser.EmployeeRowId;

        // 新規作成判定：RowVersion が未設定（空配列）なら Insert
        // 【重要】RowVersion は GetByIdAsync 経由（Mapper.ToDomainEntity → Reconstruct）でのみ設定される。
        //         Department.Create() による新規作成では空配列のまま。
        //         この性質を利用することで、GetByIdAsync によるDB再読込を避けられる
        //         （呼び出し元が Update 前に GetByIdAsync 済みであることが多く、二重読込を防止）。
        bool isInsert = department.RowVersion.Length == 0;

        if (isInsert)
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

        if (isInsert)
        {
            // 新規作成：RepoDb InsertAsync（row_version は fields で除外）
            await connection.InsertAsync(dbModel, fields: FieldsExcludingRowVersion());
        }
        else
        {
            // 更新：RepoDb UpdateAsync（row_version・created_at・created_by は fields で除外）
            await connection.UpdateAsync(dbModel, fields: FieldsExcludingRowVersionAndCreatedAudit());
        }
    }

    /// <summary>
    /// 部署を論理削除する
    /// 【責務】DeletedAt/DeletedBy を設定、RepoDb で更新（row_version は fields で除外）
    /// </summary>
    /// <param name="id">削除する部署の行ID</param>
    /// <exception cref="ArgumentNullException"><paramref name="id"/> が <see langword="null"/> の場合</exception>
    /// <exception cref="InvalidOperationException">部署が見つからない場合</exception>
    public async Task DeleteAsync(DepartmentRowId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        var now = _clock.JstNow.Value;
        var userId = _currentUser.EmployeeRowId;

        // 削除対象の部署を取得
        var department = await GetByIdAsync(id);
        if (department == null)
            throw new InvalidOperationException($"Department with RowId={id.Value} not found");

        // DbModel を作成して論理削除フィールドを設定
        var dbModel = _mapper.ToDbModel(department);
        dbModel.DeletedAt = now;
        dbModel.DeletedBy = userId;
        dbModel.UpdatedAt = now;
        dbModel.UpdatedBy = userId;

        using var connection = _connectionFactory.CreateConnection();
        // RepoDb UpdateAsync で更新（row_version・created_at・created_by は fields で除外）
        await connection.UpdateAsync(dbModel, fields: FieldsExcludingRowVersionAndCreatedAudit());
    }
}
