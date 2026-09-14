using SupportAdvance.Common.Clocks;
using SupportAdvance.Infrastructure.Mappers;
using SupportAdvance.Infrastructure.Services;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Infrastructure.Repositories;

/// <summary>
/// Repository の基底クラス
///
/// 【責務】
/// - Entity ↔ DbModel の変換
/// - 監査情報（createdBy, updatedBy, deletedBy）の自動設定
/// - 認証コンテキストからユーザー情報を取得
///
/// 【使用方法】BC固有の Repository が継承して、DB操作を実装
///
/// 【型パラメータ】
/// - TEntity: Entity<TId>（RowId型に限定）
/// - TDbModel: データベースモデル
/// - TId: Entity の ID 型（RowId を継承する型）
/// </summary>
public abstract class RepositoryBase<TEntity, TDbModel, TId>(
    IEntityMapper<TEntity, TDbModel, TId> mapper,
    ICurrentUserService currentUser,
    IClock clock)
    where TEntity : Entity<TId>
    where TDbModel : class
    where TId : notnull, RowId
{
    protected IEntityMapper<TEntity, TDbModel, TId> Mapper { get; } =
        mapper ?? throw new ArgumentNullException(nameof(mapper));

    protected ICurrentUserService CurrentUser { get; } =
        currentUser ?? throw new ArgumentNullException(nameof(currentUser));

    protected IClock Clock { get; } = clock ?? throw new ArgumentNullException(nameof(clock));

    /// <summary>
    /// Entity を DbModel に変換し、createdBy を設定
    /// </summary>
    protected TDbModel MapToDatabaseForInsert(TEntity entity)
    {
        var dbModel = Mapper.ToDbModel(entity);
        SetCreatedByAudit(dbModel);
        return dbModel;
    }

    /// <summary>
    /// Entity を DbModel に変換し、updatedBy と updatedAt を設定
    /// </summary>
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
    protected void SetDeletedByAudit(TDbModel dbModel)
    {
        var deletedByProperty = typeof(TDbModel).GetProperty("DeletedBy");
        if (deletedByProperty != null && deletedByProperty.CanWrite)
        {
            deletedByProperty.SetValue(dbModel, CurrentUser.EmployeeRowId);
        }
    }
}
