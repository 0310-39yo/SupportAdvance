using SupportAdvance.Common.Clocks;
using SupportAdvance.Infrastructure.Mappers;
using SupportAdvance.Infrastructure.Services;
using SupportAdvance.SharedKernel.Entities;

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
/// - TEntity: Entity<TId>（ID型をサポート）
/// - TDbModel: データベースモデル
/// - TId: Entity の ID 型（ValueObject など）
/// </summary>
public abstract class RepositoryBase<TEntity, TDbModel, TId>
    where TEntity : Entity<TId>
    where TDbModel : class
    where TId : notnull
{
    protected IEntityMapper<TEntity, TDbModel, TId> Mapper { get; }
    protected ICurrentUserService CurrentUser { get; }
    protected IClock Clock { get; }

    protected RepositoryBase(
        IEntityMapper<TEntity, TDbModel, TId> mapper,
        ICurrentUserService currentUser,
        IClock clock)
    {
        Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        CurrentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        Clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

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
    /// Entity を DbModel に変換し、updatedBy を設定
    /// </summary>
    protected TDbModel MapToDatabaseForUpdate(TEntity entity)
    {
        var dbModel = Mapper.ToDbModel(entity);
        SetUpdatedByAudit(dbModel);
        return dbModel;
    }

    /// <summary>
    /// DbModel を Domain Entity に変換
    /// </summary>
    protected TEntity MapToDomain(TDbModel dbModel)
    {
        return Mapper.ToDomainEntity(dbModel, Clock);
    }

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
