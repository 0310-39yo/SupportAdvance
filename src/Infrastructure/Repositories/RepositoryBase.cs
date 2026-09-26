using SupportAdvance.Common.Clocks;
using SupportAdvance.Infrastructure.Mappers;
using SupportAdvance.Application.Abstractions.Services;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Infrastructure.Repositories;

/// <summary>
/// エンティティと DB モデルの変換と、監査列の自動設定を行うリポジトリの基底クラス
/// </summary>
/// <typeparam name="TEntity">扱うエンティティ（<see cref="Entity{TId}"/> の派生型）</typeparam>
/// <typeparam name="TDbModel">対応する DB モデル</typeparam>
/// <typeparam name="TId">エンティティの ID（<see cref="RowId"/> の派生型）</typeparam>
/// <param name="mapper">エンティティと DB モデルの相互変換を行うマッパー</param>
/// <param name="currentUser">監査列（<c>*_by</c>）に記録する操作者の情報</param>
/// <param name="clock">監査列（<c>updated_at</c>）に記録する現在時刻（JST）の取得元</param>
/// <exception cref="ArgumentNullException">いずれかの引数が <see langword="null"/> の場合</exception>
/// <remarks>
/// <para>【使用方法】BC 固有のリポジトリが継承し、DB 操作を実装</para>
/// <para>【責務】監査列（<c>*_at</c>／<c>*_by</c>）の設定。マッパーの担当は業務データの変換のみ</para>
/// <para>【注意】監査列はプロパティ名によるリフレクションで検索。名前が異なる場合はエラーなしで設定されないまま</para>
/// </remarks>
public abstract class RepositoryBase<TEntity, TDbModel, TId>(
    IEntityMapper<TEntity, TDbModel, TId> mapper,
    ICurrentUserService currentUser,
    IClock clock)
    where TEntity : Entity<TId>
    where TDbModel : class
    where TId : notnull, RowId
{
    /// <summary>
    /// エンティティと DB モデルの相互変換を行うマッパー
    /// </summary>
    protected IEntityMapper<TEntity, TDbModel, TId> Mapper { get; } =
        mapper ?? throw new ArgumentNullException(nameof(mapper));

    /// <summary>
    /// 監査列（<c>*_by</c>）に記録する操作者の情報
    /// </summary>
    protected ICurrentUserService CurrentUser { get; } =
        currentUser ?? throw new ArgumentNullException(nameof(currentUser));

    /// <summary>
    /// 監査列（<c>*_at</c>）に記録する現在時刻（JST）の取得元
    /// </summary>
    protected IClock Clock { get; } = clock ?? throw new ArgumentNullException(nameof(clock));

    /// <summary>
    /// Entity を DbModel に変換し、createdBy を設定
    /// </summary>
    /// <param name="entity">保存するエンティティ</param>
    /// <returns>INSERT 用の DB モデル（<c>CreatedBy</c> 設定済み）</returns>
    /// <remarks>
    /// <para>【注意】<c>CreatedAt</c> はこのメソッドでは設定なし</para>
    /// </remarks>
    protected TDbModel MapToDatabaseForInsert(TEntity entity)
    {
        var dbModel = Mapper.ToDbModel(entity);
        SetCreatedByAudit(dbModel);
        return dbModel;
    }

    /// <summary>
    /// Entity を DbModel に変換し、updatedBy と updatedAt を設定
    /// </summary>
    /// <param name="entity">更新するエンティティ</param>
    /// <returns>UPDATE 用の DB モデル（<c>UpdatedAt</c>／<c>UpdatedBy</c> 設定済み）</returns>
    protected TDbModel MapToDatabaseForUpdate(TEntity entity)
    {
        var dbModel = Mapper.ToDbModel(entity);
        SetUpdatedAtAudit(dbModel);
        SetUpdatedByAudit(dbModel);
        return dbModel;
    }

    /// <summary>
    /// DbModel を Domain Entity に変換
    /// </summary>
    /// <param name="dbModel">DB から読み込んだ DB モデル</param>
    /// <returns>復元したエンティティ</returns>
    protected TEntity MapToDomain(TDbModel dbModel) => Mapper.ToDomainEntity(dbModel, Clock);

    /// <summary>
    /// createdBy（作成者従業員rowId）を設定
    /// </summary>
    private void SetCreatedByAudit(TDbModel dbModel)
    {
        var createdByProperty = typeof(TDbModel).GetProperty("CreatedBy");
        if (createdByProperty != null && createdByProperty.CanWrite)
        {
            createdByProperty.SetValue(dbModel, CurrentUser.EmployeeRowId);
        }
    }

    /// <summary>
    /// updatedAt（更新日時）を設定
    /// 【責務】Repository が保存時刻を管理（Mapper ではなく）
    /// </summary>
    private void SetUpdatedAtAudit(TDbModel dbModel)
    {
        var updatedAtProperty = typeof(TDbModel).GetProperty("UpdatedAt");
        if (updatedAtProperty != null && updatedAtProperty.CanWrite)
        {
            updatedAtProperty.SetValue(dbModel, Clock.JstNow.Value);
        }
    }

    /// <summary>
    /// updatedBy（更新者従業員rowId）を設定
    /// </summary>
    private void SetUpdatedByAudit(TDbModel dbModel)
    {
        var updatedByProperty = typeof(TDbModel).GetProperty("UpdatedBy");
        if (updatedByProperty != null && updatedByProperty.CanWrite)
        {
            updatedByProperty.SetValue(dbModel, CurrentUser.EmployeeRowId);
        }
    }

    /// <summary>
    /// deletedBy（削除者従業員rowId）を設定
    /// </summary>
    /// <param name="dbModel">設定先の DB モデル</param>
    protected void SetDeletedByAudit(TDbModel dbModel)
    {
        var deletedByProperty = typeof(TDbModel).GetProperty("DeletedBy");
        if (deletedByProperty != null && deletedByProperty.CanWrite)
        {
            deletedByProperty.SetValue(dbModel, CurrentUser.EmployeeRowId);
        }
    }
}
