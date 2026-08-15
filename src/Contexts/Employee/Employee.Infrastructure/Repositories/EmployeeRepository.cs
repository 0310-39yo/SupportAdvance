namespace SupportAdvance.Contexts.Employee.Infrastructure.Repositories;

using SupportAdvance.Contexts.Employee.Application.Repositories;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.Contexts.Employee.Infrastructure.Mappers;
using SupportAdvance.Contexts.Employee.Infrastructure.Models;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

/// <summary>
/// Employee 集約の Repository 実装
///
/// 【責務】
/// - Domain Entity ↔ DbModel の相互変換（Mapper 使用）
/// - メモリ内ストア（テスト用）での CRUD 操作
/// - 監査カラムの自動設定
/// - 論理削除の実装
/// </summary>
public class EmployeeRepository : IEmployeeRepository
{
    private readonly EmployeeMapper _mapper;
    private readonly IClock _clock;

    /// メモリ内ストア（テスト用）
    private readonly Dictionary<long, EmployeeDbModel> _employees = [];
    private readonly object _lock = new object();

    public EmployeeRepository(EmployeeMapper mapper, IClock clock)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>
    /// 集約根ID（EmployeeRowId）で Employee を検索する
    /// </summary>
    public async Task<Employee?> GetByIdAsync(EmployeeRowId id)
    {
        return await Task.Run(() =>
        {
            lock (_lock)
            {
                if (!_employees.TryGetValue(id.Value, out var dbModel))
                    return null;

                if (dbModel.DeletedAt != null)
                    return null;

                var person = CreatePersonFromDbModel(dbModel);
                return _mapper.ToDomainEntity(dbModel, person);
            }
        });
    }

    /// <summary>
    /// DB行ID で Employee を検索する
    /// </summary>
    public async Task<Employee?> GetByRowIdAsync(EmployeeRowId rowId)
    {
        return await Task.Run(() =>
        {
            lock (_lock)
            {
                var dbModel = _employees.Values.FirstOrDefault(e =>
                    e.RowId == rowId.Value && e.DeletedAt == null);

                if (dbModel == null)
                    return null;

                var person = CreatePersonFromDbModel(dbModel);
                return _mapper.ToDomainEntity(dbModel, person);
            }
        });
    }

    /// <summary>
    /// 人事マスタ行ID で Employee を検索する（複数結果想定）
    /// </summary>
    public async Task<IReadOnlyList<Employee>> GetByPersonRowIdAsync(PersonRowId personRowId)
    {
        return await Task.Run(() =>
        {
            lock (_lock)
            {
                var results = _employees.Values
                    .Where(e => e.PersonRowId == personRowId.Value && e.DeletedAt == null)
                    .Select(dbModel =>
                    {
                        var person = CreatePersonFromDbModel(dbModel);
                        return _mapper.ToDomainEntity(dbModel, person);
                    })
                    .ToList();

                return (IReadOnlyList<Employee>)results;
            }
        });
    }

    /// <summary>
    /// 新規 Employee を DB に登録する
    /// 【前提条件】RowId は Application層で事前採番済み
    /// </summary>
    public async Task AddAsync(Employee employee)
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                var dbModel = _mapper.ToDbModel(employee);

                // 【Assertion】RowId が Application層で事前採番されていることを確認
                if (dbModel.RowId <= 0)
                {
                    throw new InvalidOperationException(
                        $"RowId must be set by Application layer before AddAsync. Received: {dbModel.RowId}");
                }

                // 監査カラムの自動設定
                var now = _clock.JstNow;
                dbModel.CreatedAt = now.Value;
                dbModel.CreatedBy = 1L;  // テスト用デフォルト値
                dbModel.UpdatedAt = null;
                dbModel.UpdatedBy = null;
                dbModel.DeletedAt = null;
                dbModel.DeletedBy = null;
                dbModel.RowVersion = [];

                _employees[employee.RowId.Value] = dbModel;
            }
        });
    }

    /// <summary>
    /// 既存 Employee を DB で更新する
    /// </summary>
    public async Task UpdateAsync(Employee employee)
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                if (!_employees.TryGetValue(employee.RowId.Value, out var existingDbModel))
                    throw new InvalidOperationException($"Employee not found: {employee.RowId}");

                var updatedDbModel = _mapper.ToDbModel(employee);

                // 既存の監査情報を引き継ぐ
                updatedDbModel.RowId = existingDbModel.RowId;
                updatedDbModel.CreatedAt = existingDbModel.CreatedAt;
                updatedDbModel.CreatedBy = existingDbModel.CreatedBy;

                // 更新情報を設定
                var now = _clock.JstNow;
                updatedDbModel.UpdatedAt = now.Value;
                updatedDbModel.UpdatedBy = 1L;  // テスト用デフォルト値
                updatedDbModel.DeletedAt = existingDbModel.DeletedAt;
                updatedDbModel.DeletedBy = existingDbModel.DeletedBy;
                updatedDbModel.RowVersion = [];

                _employees[employee.RowId.Value] = updatedDbModel;
            }
        });
    }

    /// <summary>
    /// DbModel から Person Entity を生成するヘルパーメソッド
    /// 【注】テスト用の簡易実装。実装では PersonRepository から取得すべき
    /// </summary>
    private Person CreatePersonFromDbModel(EmployeeDbModel dbModel)
    {
        return Person.Create(
            PersonRowId.From(dbModel.PersonRowId),
            PersonLastName.From("山田"),  // プレースホルダー
            PersonFirstName.From("太郎"),  // プレースホルダー
            PersonLastNameKana.From("ヤマダ"),  // プレースホルダー
            PersonFirstNameKana.From("タロウ"));  // プレースホルダー
    }

    /// <summary>
    /// Employee を論理削除する
    /// </summary>
    public async Task DeleteAsync(EmployeeRowId id)
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                if (!_employees.TryGetValue(id.Value, out var dbModel))
                    throw new InvalidOperationException($"Employee not found: {id}");

                // 論理削除フラグを設定
                var now = _clock.JstNow;
                dbModel.DeletedAt = now.Value;
                dbModel.DeletedBy = 1L;  // テスト用デフォルト値
                dbModel.RowVersion = [];

                _employees[id.Value] = dbModel;
            }
        });
    }
}
