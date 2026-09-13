using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Identity.Domain.Entities;
using SupportAdvance.Contexts.Identity.Domain.ValueObjects;
using SupportAdvance.Contexts.Identity.Infrastructure.DbModels;

namespace SupportAdvance.Contexts.Identity.Infrastructure.Mappers;

/// <summary>
/// UserAuthSession マッパー
///
/// 【責務】
/// - DbModel ↔ Domain Entity の双方向変換
/// - LocalDateTime ↔ DateTime 変換（ビジネスフィールドのみ）
///
/// 【監査情報について】
/// - Mapper では CreatedAt/CreatedBy/UpdatedAt/UpdatedBy/DeletedAt/DeletedBy を **設定しない**
/// - Repository が保存・更新時に設定する責務を持つ
/// - Reconstruct() では DB の監査情報をそのまま渡す
/// </summary>
public sealed class UserAuthSessionMapper
{
    /// <summary>
    /// DbModel を Domain Entity に変換（DB から復元）
    /// </summary>
    /// <param name="dbModel">DB モデル</param>
    /// <returns>Domain Entity（Reconstruct パターン）</returns>
    public UserAuthSession ToDomainEntity(UserAuthSessionDbModel dbModel)
    {
        // ValueObjects の生成
        var sessionRowId = UserAuthSessionRowId.From(dbModel.RowId);
        var authorityRowId = AuthorityRowId.From(dbModel.CurrentUserRowId);

        var loginCredentialsRowId = dbModel.LoginCredentialsRowId.HasValue
            ? LoginCredentialsRowId.From(dbModel.LoginCredentialsRowId.Value)
            : null;

        var loggedInAt = new LocalDateTime(dbModel.LoggedInAt);
        LocalDateTime? loggedOutAt = dbModel.LoggedOutAt.HasValue
            ? new LocalDateTime(dbModel.LoggedOutAt.Value)
            : null;

        // DB から復元（全フィールド指定）
        return UserAuthSession.Reconstruct(
            sessionRowId,
            authorityRowId,
            dbModel.IsAdAuthenticated,
            dbModel.LoginSuccess,
            loggedInAt,
            loggedOutAt,
            loginCredentialsRowId);
    }

    /// <summary>
    /// Domain Entity を DbModel に変換（ビジネスフィールドのみ）
    ///
    /// 【注意】
    /// - 監査フィールド（CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy）は設定しない
    /// - Repository が責務を持つ
    /// </summary>
    /// <param name="entity">Domain Entity</param>
    /// <returns>DbModel（ビジネスフィールドのみ設定）</returns>
    public UserAuthSessionDbModel ToDbModel(UserAuthSession entity)
    {
        return new UserAuthSessionDbModel
        {
            RowId = entity.RowId.Value,
            CurrentUserRowId = entity.AuthorityRowId.Value,
            IsAdAuthenticated = entity.IsAdAuthenticated,
            LoginSuccess = entity.LoginSuccess,
            LoggedInAt = entity.LoggedInAt.Value,
            LoggedOutAt = entity.LoggedOutAt?.Value,
            LoginCredentialsRowId = entity.LoginCredentialsRowId?.Value,
            RowVersion = entity.RowVersion,

            // ❌ 監査フィールドは設定しない
            // CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy は省略
        };
    }
}
