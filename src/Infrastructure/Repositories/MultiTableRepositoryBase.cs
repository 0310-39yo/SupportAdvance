using SupportAdvance.Common.Clocks;
using SupportAdvance.Infrastructure.Services;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Infrastructure.Repositories;

/// <summary>
/// 複数テーブル集約用の Repository 基底クラス
///
/// 【責務】
/// - 監査情報（createdBy, updatedBy, deletedBy）の自動設定
/// - 認証コンテキストからユーザー情報を取得
/// - クロックの注入
///
/// 【対象】
/// 単一テーブルではなく、複数の関連テーブルから構成される集約
/// （例：Employee（m_employees + m_persons + m_department_memberships））
///
/// 【使用方法】
/// 複数テーブル集約の Repository が継承して、複雑な読み込み・保存ロジックを実装
/// Mapper の複雑性を避けることができる
///
/// 【型パラメータ】
/// - TEntity: Entity<TId>（RowId型に限定）
/// - TDbModel: 主テーブルのデータベースモデル
/// - TId: Entity の ID 型（RowId を継承する型）
/// </summary>
public abstract class MultiTableRepositoryBase<TEntity, TDbModel, TId>(
    ICurrentUserService currentUser,
    IClock clock)
    where TEntity : Entity<TId>
    where TDbModel : class
    where TId : notnull, RowId
{
    protected ICurrentUserService CurrentUser { get; } =
        currentUser ?? throw new ArgumentNullException(nameof(currentUser));

    protected IClock Clock { get; } = clock ?? throw new ArgumentNullException(nameof(clock));

    /// <summary>
    /// DbModel に監査フィールド（createdBy）を設定
    /// 【用途】Insert/Create 時
    /// </summary>
    protected void SetCreatedByAudit(TDbModel dbModel)
    {
        var createdByProperty = typeof(TDbModel).GetProperty("CreatedBy");
        if (createdByProperty != null && createdByProperty.CanWrite)
        {
            createdByProperty.SetValue(dbModel, CurrentUser.EmployeeRowId);
        }
    }

    /// <summary>
    /// DbModel に監査フィールド（createdAt）を設定
    /// 【用途】Insert/Create 時
    /// 【責務】Repository が保存時刻を管理
    /// </summary>
    protected void SetCreatedAtAudit(TDbModel dbModel)
    {
        var createdAtProperty = typeof(TDbModel).GetProperty("CreatedAt");
        if (createdAtProperty != null && createdAtProperty.CanWrite)
        {
            createdAtProperty.SetValue(dbModel, Clock.JstNow.Value);
        }
    }

    /// <summary>
    /// DbModel に監査フィールド（updatedAt）を設定
    /// 【用途】Update 時
    /// 【責務】Repository が保存時刻を管理（Mapper ではなく）
    /// 【注意】複数テーブル集約の場合、どの DbModel 型にも対応
    /// </summary>
    protected void SetUpdatedAtAudit(TDbModel dbModel)
    {
        SetAuditField(dbModel, "UpdatedAt", Clock.JstNow.Value);
    }

    /// <summary>
    /// DbModel に監査フィールド（updatedBy）を設定
    /// 【用途】Update 時
    /// 【注意】複数テーブル集約の場合、どの DbModel 型にも対応
    /// </summary>
    protected void SetUpdatedByAudit(TDbModel dbModel)
    {
        SetAuditField(dbModel, "UpdatedBy", CurrentUser.EmployeeRowId);
    }

    /// <summary>
    /// 任意の DbModel（複数テーブル集約対応）に監査フィールドを設定
    /// 【用途】複数の DbModel 型に同じ監査ロジックを適用
    /// </summary>
    protected void SetAuditField<T>(T dbModel, string fieldName, object value) where T : class
    {
        if (dbModel == null) return;
        var property = typeof(T).GetProperty(fieldName);
        if (property != null && property.CanWrite)
        {
            property.SetValue(dbModel, value);
        }
    }

    /// <summary>
    /// DbModel に監査フィールド（deletedBy）を設定
    /// 【用途】Delete/論理削除 時
    /// </summary>
    protected void SetDeletedByAudit(TDbModel dbModel)
    {
        var deletedByProperty = typeof(TDbModel).GetProperty("DeletedBy");
        if (deletedByProperty != null && deletedByProperty.CanWrite)
        {
            deletedByProperty.SetValue(dbModel, CurrentUser.EmployeeRowId);
        }
    }

    /// <summary>
    /// DbModel に監査フィールド（deletedAt）を設定
    /// 【用途】Delete/論理削除 時
    /// 【責務】Repository が削除時刻を管理
    /// </summary>
    protected void SetDeletedAtAudit(TDbModel dbModel)
    {
        var deletedAtProperty = typeof(TDbModel).GetProperty("DeletedAt");
        if (deletedAtProperty != null && deletedAtProperty.CanWrite)
        {
            deletedAtProperty.SetValue(dbModel, Clock.JstNow.Value);
        }
    }
}
