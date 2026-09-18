namespace SupportAdvance.Contexts.Department.Application.Queries;

using SupportAdvance.Application.Queries;
using SupportAdvance.Contexts.Department.Application.Repositories;
using SupportAdvance.Contexts.Department.Domain.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using Department = SupportAdvance.Contexts.Department.Domain.Entities.Department;

/// <summary>
/// 部署照会サービス（ジェネリック Query Service パターン）
///
/// 【責務】
///   - IQueryService&lt;Department, DepartmentRowId&gt; を実装
///   - 他の Bounded Context から部署情報の照会を提供（読み取り専用）
/// 【パターン】
///   - Employee BC 等が DI で注入され、部署情報をリアルタイムに照会
///   - Repository 経由で DB アクセス（トランザクション外）
/// </summary>
public class DepartmentQueryService : IQueryService<Department, DepartmentRowId>
{
    private readonly IDepartmentRepository _repository;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="repository">部署リポジトリ</param>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> が <see langword="null"/> の場合</exception>
    public DepartmentQueryService(IDepartmentRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    /// <summary>
    /// 部署を行IDで照会する
    /// </summary>
    /// <param name="id">部署行ID</param>
    /// <returns>部署（存在しない場合は null）</returns>
    /// <exception cref="ArgumentNullException"><paramref name="id"/> が <see langword="null"/> の場合</exception>
    public async Task<Department?> GetByIdAsync(DepartmentRowId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return await _repository.GetByIdAsync(id);
    }
}
