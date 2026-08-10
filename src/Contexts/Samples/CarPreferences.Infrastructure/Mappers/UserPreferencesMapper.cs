using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.Entities;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;
using SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure.DataAccess.Models;
using SupportAdvance.Infrastructure.Mappers;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using SupportAdvance.SharedKernel.ValueObjects.Audit;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure.Mappers;

/// <summary>
/// UserPreferences Domain Entity ↔ DbModel マッパー
///
/// 【責務】
/// - Domain Entity → DbModel（永続化前、ValueObject → プリミティブ型 変換）
/// - DbModel → Domain Entity（読み取り時、プリミティブ型 → ValueObject 変換）
/// 【原則】純粋なマッピングのみ（ビジネスコンテキスト不問）
/// 【監査情報】createdBy, updatedBy は Repository 層で設定
/// 【型変換】
///   - UserPreferencesId（Guid） ↔ DbModel.UserPreferencesId
///   - RowId（long） ↔ DbModel.RowId
///   - LocalDateTime ↔ DateTime
/// </summary>
public class UserPreferencesMapper : IEntityMapper<UserPreferences, UserPreferencesDbModel, UserPreferencesId>
{
    /// <summary>
    /// Domain Entity → DbModel（保存用、ValueObject → プリミティブ型 変換、LocalDateTime → DateTime 変換）
    /// 【責務】Entity の ID/ValueObject を DbModel のプリミティブ型に変換して DB保存用に
    /// </summary>
    public UserPreferencesDbModel ToDbModel(UserPreferences entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new UserPreferencesDbModel
        {
            UserPreferencesId = entity.Id.Value,  // UserPreferencesId（Guid） → Guid
            RowId = entity.RowId.Value,  // RowId（long） → long
            UserId = entity.UserId.Value ?? 0,  // int? → int
            CreatedAt = entity.CreatedAt.ToDbValue(),  // LocalDateTime → DateTime
            CreatedBy = 0,  // Repository で設定される
            UpdatedAt = entity.UpdatedAt.HasUpdated ? entity.UpdatedAt.ToDbValue() : null,  // LocalDateTime → DateTime?
            UpdatedBy = null,  // Repository で設定される
            DeletedAt = entity.DeletedAt.IsDeleted ? entity.DeletedAt.ToDbValue() : null,  // LocalDateTime → DateTime?
            DeletedBy = null,
            PreferredModel = entity.PreferredModel?.Value,
            PreferredBodyType = entity.PreferredBodyType?.ToString(),
            PrefersAutomatic = entity.PrefersAutomatic,
            BudgetFrom = entity.BudgetFrom?.Amount,
            BudgetTo = entity.BudgetTo?.Amount
        };
    }

    /// <summary>
    /// DbModel → Domain Entity（読 み込み用、long → RowId ValueObject 変換、DateTime → LocalDateTime 変換）
    /// 【責務】DB値を Domain Entity に復元、型変換は Audit ValueObject で処理
    /// </summary>
    public UserPreferences ToDomainEntity(UserPreferencesDbModel dbModel, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(dbModel);
        ArgumentNullException.ThrowIfNull(clock);

        // ① ビジネス ValueObject 変換
        if (!RespondentPersonId.TryFrom(dbModel.UserId, out var userId))
        {
            throw new InvalidOperationException(
                $"Failed to convert UserId: {dbModel.UserId}");
        }

        // ② Audit ValueObject の変換（DateTime → LocalDateTime、TryFromDbValue で失敗時は例外）
        if (!CreatedAt.TryFromDbValue(dbModel.CreatedAt, out var createdAt))
        {
            throw new InvalidOperationException(
                $"Failed to convert CreatedAt ValueObject: {dbModel.CreatedAt}");
        }

        if (!UpdatedAt.TryFromDbValue(dbModel.UpdatedAt, out var updatedAt))
        {
            throw new InvalidOperationException(
                $"Failed to convert UpdatedAt ValueObject: {dbModel.UpdatedAt}");
        }

        if (!DeletedAt.TryFromDbValue(dbModel.DeletedAt, out var deletedAt))
        {
            throw new InvalidOperationException(
                $"Failed to convert DeletedAt ValueObject: {dbModel.DeletedAt}");
        }

        // ③ ビジネスロジック検証が必要な ValueObject（clock パラメータ必須）
        // RespondentAt: 未来日チェック等の検証が必要、createdAt（LocalDateTime）から取得
        if (!RespondentAt.TryFrom(
            createdAt.Value,
            clock,
            out var respondedAt))
        {
            throw new InvalidOperationException(
                $"Failed to convert CreatedAt to RespondentAt: {dbModel.CreatedAt}");
        }

        // ④ Reconstruct Factory メソッド呼び出し（DB値で復元）
        var entity = UserPreferences.Reconstruct(
            UserPreferencesId.From(dbModel.UserPreferencesId),  // ビジネスID を復元
            userId,
            respondedAt,
            createdAt,
            updatedAt,
            deletedAt,
            RowId.From(dbModel.RowId),  // 物理キーを復元
            dbModel.PreferredModel.HasValue
                ? CarModel.From(dbModel.PreferredModel.Value)
                : null,
            dbModel.PreferredBodyType != null
                ? BodyType.From(dbModel.PreferredBodyType)
                : null,
            dbModel.PrefersAutomatic,
            dbModel.BudgetFrom.HasValue
                ? Money.From(dbModel.BudgetFrom.Value)
                : null,
            dbModel.BudgetTo.HasValue
                ? Money.From(dbModel.BudgetTo.Value)
                : null);

        return entity;
    }
}

