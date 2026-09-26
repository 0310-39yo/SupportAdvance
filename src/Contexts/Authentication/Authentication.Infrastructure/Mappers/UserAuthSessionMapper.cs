using SupportAdvance.Contexts.Authentication.Domain.Entities;
using SupportAdvance.Contexts.Authentication.Domain.ValueObjects;
using SupportAdvance.Contexts.Authentication.Infrastructure.DbModels;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Infrastructure.Mappers;

namespace SupportAdvance.Contexts.Authentication.Infrastructure.Mappers;

/// <summary>
/// 認証セッション（<see cref="UserAuthSession"/>）と DB モデル（<see cref="UserAuthSessionDbModel"/>）の相互変換を行うマッパー
/// </summary>
/// <remarks>
/// <para>【責務】DB モデル ↔ Domain Entity の双方向変換。<c>DateTime</c> ↔ <see cref="LocalDateTime"/> の変換を含む（業務項目のみ）</para>
/// <para>【注意】監査情報（CreatedAt／CreatedBy／UpdatedAt／UpdatedBy／DeletedAt／DeletedBy）は設定しない。保存・更新時の設定は Repository の担当</para>
/// <para>【null契約】DB の NULL（ログアウト日時、認証情報の行ID）は、Unset の値オブジェクトに変換して Domain に渡す</para>
/// </remarks>
public sealed class UserAuthSessionMapper
{
    /// <summary>
    /// DB モデルからの認証セッションの復元
    /// </summary>
    /// <param name="dbModel">DB モデル</param>
    /// <returns>復元したセッション</returns>
    /// <exception cref="InvalidOperationException">DB の値を値オブジェクトに変換できない場合（DB の整合性エラー）</exception>
    public UserAuthSession ToDomainEntity(UserAuthSessionDbModel dbModel)
    {
        // ValueObjects の生成
        var sessionRowId = UserAuthSessionRowId.From(dbModel.RowId);
        var authorityRowId = AuthorityRowId.From(dbModel.CurrentUserRowId);

        if (!UsedLoginCredentialsRowId.TryFrom(dbModel.LoginCredentialsRowId, out var loginCredentialsRowId))
        {
            throw new InvalidOperationException(
                $"Failed to convert LoginCredentialsRowId from DB value: {dbModel.LoginCredentialsRowId}");
        }

        var loggedInAt = dbModel.LoggedInAt.ToLocalDateTime();

        // DB の DateTime? を LocalDateTime? に変換してから TryFrom に渡す（null は Unset に変換）
        if (!LoggedOutAt.TryFrom(dbModel.LoggedOutAt.ToLocalDateTimeOrNull(), out var loggedOutAt))
        {
            throw new InvalidOperationException(
                $"Failed to convert LoggedOutAt from DB value: {dbModel.LoggedOutAt}");
        }

        return UserAuthSession.Reconstruct(
            sessionRowId,
            authorityRowId,
            dbModel.IsAdAuthenticated,
            dbModel.LoginSuccess,
            loggedInAt,
            loggedOutAt,
            loginCredentialsRowId,
            dbModel.RowVersion);
    }

    /// <summary>
    /// 認証セッションの DB モデルへの変換
    /// </summary>
    /// <param name="entity">変換するセッション</param>
    /// <returns>業務項目のみ設定した DB モデル（監査列は未設定）</returns>
    public UserAuthSessionDbModel ToDbModel(UserAuthSession entity)
    {
        return new UserAuthSessionDbModel
        {
            RowId = entity.RowId.Value,
            CurrentUserRowId = entity.AuthorityRowId.Value,
            IsAdAuthenticated = entity.IsAdAuthenticated,
            LoginSuccess = entity.LoginSuccess,
            LoggedInAt = entity.LoggedInAt.Value,
            LoggedOutAt = entity.LoggedOutAt.HasLoggedOut ? entity.LoggedOutAt.Value.Value : null,
            LoginCredentialsRowId = entity.LoginCredentialsRowId.HasCredentials ? entity.LoginCredentialsRowId.Value : null,
            RowVersion = entity.RowVersion,

            // 監査フィールド（CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy）は設定しない
        };
    }
}
