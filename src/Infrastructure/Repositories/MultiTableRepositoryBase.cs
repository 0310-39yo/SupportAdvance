using SupportAdvance.Common.Clocks;
using SupportAdvance.Infrastructure.Services;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Infrastructure.Repositories;

/// <summary>
/// 複数テーブルで構成される集約のための、リポジトリの基底クラス
/// </summary>
/// <typeparam name="TEntity">扱うエンティティ（<see cref="Entity{TId}"/> の派生型）</typeparam>
/// <typeparam name="TDbModel">主テーブルの DB モデル</typeparam>
/// <typeparam name="TId">エンティティの ID（<see cref="RowId"/> の派生型）</typeparam>
/// <param name="currentUser">監査列（<c>*_by</c>）に記録する操作者の情報</param>
/// <param name="clock">監査列（<c>*_at</c>）に記録する現在時刻（JST）の取得元</param>
/// <exception cref="ArgumentNullException">いずれかの引数が <see langword="null"/> の場合</exception>
/// <remarks>
/// <para>【対象】複数の関連テーブルから構成される集約（例: Employee = <c>m_employees</c> + <c>m_persons</c> + <c>m_department_memberships</c>）</para>
/// <para>【責務】監査列（<c>*_at</c>／<c>*_by</c>）の設定。読み込み・保存の処理は派生クラスの担当</para>
/// <para>【設計】マッパーを使わず派生クラスで直接組み立てることによる、マッパーの複雑化の回避</para>
/// <para>【注意】監査列はプロパティ名によるリフレクションで検索。名前が異なる場合はエラーなしで設定されないまま</para>
/// </remarks>
public abstract class MultiTableRepositoryBase<TEntity, TDbModel, TId>(
    ICurrentUserService currentUser,
    IClock clock)
    where TEntity : Entity<TId>
    where TDbModel : class
    where TId : notnull, RowId
{
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
    /// DbModel に監査フィールド（createdBy）を設定
    /// 【用途】Insert/Create 時
    /// </summary>
    /// <param name="dbModel">設定先の DB モデル</param>
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
    /// <param name="dbModel">設定先の DB モデル</param>
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
    /// <param name="dbModel">設定先の DB モデル</param>
    protected void SetUpdatedAtAudit(TDbModel dbModel)
    {
        SetAuditField(dbModel, "UpdatedAt", Clock.JstNow.Value);
    }

    /// <summary>
    /// DbModel に監査フィールド（updatedBy）を設定
    /// 【用途】Update 時
    /// 【注意】複数テーブル集約の場合、どの DbModel 型にも対応
    /// </summary>
    /// <param name="dbModel">設定先の DB モデル</param>
    protected void SetUpdatedByAudit(TDbModel dbModel)
    {
        SetAuditField(dbModel, "UpdatedBy", CurrentUser.EmployeeRowId);
    }

    /// <summary>
    /// 任意の DbModel（複数テーブル集約対応）に監査フィールドを設定
    /// 【用途】複数の DbModel 型に同じ監査ロジックを適用
    /// </summary>
    /// <typeparam name="T">設定先の DB モデルの型（主テーブル以外の DB モデルも可）</typeparam>
    /// <param name="dbModel">設定先の DB モデル。<see langword="null"/> の場合は何もしない</param>
    /// <param name="fieldName">設定するプロパティ名（例: <c>UpdatedAt</c>）。存在しない・書き込み不可の場合は何もしない</param>
    /// <param name="value">設定する値</param>
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
    /// <param name="dbModel">設定先の DB モデル</param>
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
    /// <param name="dbModel">設定先の DB モデル</param>
    protected void SetDeletedAtAudit(TDbModel dbModel)
    {
        var deletedAtProperty = typeof(TDbModel).GetProperty("DeletedAt");
        if (deletedAtProperty != null && deletedAtProperty.CanWrite)
        {
            deletedAtProperty.SetValue(dbModel, Clock.JstNow.Value);
        }
    }
}
