using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.Entities;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;
using SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure.DataAccess.Models;
using SupportAdvance.Infrastructure.Mappers;
using SupportAdvance.SharedKernel.ValueObjects.Audit;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure.Mappers;

/// <summary>
/// UserPreferences Domain Entity ↔ DbModel マッパー
///
/// 【責務】
/// - Domain Entity → DbModel（永続化前、RowId ValueObject → long 変換）
/// - DbModel → Domain Entity（読み取り時、long → RowId ValueObject 変換）
/// 【原則】純粋なマッピングのみ（ビジネスコンテキスト不問）
/// 【監査情報】createdBy, updatedBy は Repository 層で設定
/// 【型変換】DbModel: long(row_id) ↔ Entity: RowId ValueObject
/// </summary>
public class UserPreferencesMapper : IEntityMapper<UserPreferences, UserPreferencesDbModel, RowId>
{
    /// <summary>
    /// Domain Entity → DbModel（保存用、RowId → long 変換、ValueObject → DateTime 変換）
    /// </summary>
    public UserPreferencesDbModel ToDbModel(UserPreferences entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new UserPreferencesDbModel
        {
            RowId = entity.Id.Value,  // RowId → long 変換
            UserId = entity.UserId.Value ?? 0,  // int? → int
            CreatedAt = new LocalDateTime(entity.CreatedAt.Value),  // DateTime → LocalDateTime
            CreatedBy = 0,  // Repository で設定される
            UpdatedAt = entity.UpdatedAt.HasUpdated ? new LocalDateTime(entity.UpdatedAt.Value!.Value) : null,  // DateTime? → LocalDateTime?
            UpdatedBy = null,  // Repository で設定される
            DeletedAt = entity.DeletedAt.IsDeleted ? new LocalDateTime(entity.DeletedAt.Value!.Value) : null,  // DateTime? → LocalDateTime?
            DeletedBy = null,
            PreferredModel = entity.PreferredModel?.Value,
            PreferredBodyType = entity.PreferredBodyType?.ToString(),
            PrefersAutomatic = entity.PrefersAutomatic,
            BudgetFrom = entity.BudgetFrom?.Amount,
            BudgetTo = entity.BudgetTo?.Amount
        };
    }

    /// <summary>
    /// DbModel → Domain Entity（読み取り用、long → RowId ValueObject 変換、TryFrom で型安全化）
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

        if (!RespondentAt.TryFrom(
            dbModel.CreatedAt,
            clock, out var respondedAt))
        {
            throw new InvalidOperationException(
                $"Failed to convert CreatedAt to RespondentAt: {dbModel.CreatedAt}");
        }

        // ② 監査 ValueObject の変換（TryFrom で失敗時は例外）
        if (!CreatedAt.TryFrom(
            dbModel.CreatedAt,
            out var createdAt))
        {
            throw new InvalidOperationException(
                $"Failed to convert CreatedAt ValueObject: {dbModel.CreatedAt}");
        }

        if (!UpdatedAt.TryFrom(
            dbModel.UpdatedAt,
            out var updatedAt))
        {
            throw new InvalidOperationException(
                $"Failed to convert UpdatedAt ValueObject: {dbModel.UpdatedAt}");
        }

        if (!DeletedAt.TryFrom(
            dbModel.DeletedAt,
            out var deletedAt))
        {
            throw new InvalidOperationException(
                $"Failed to convert DeletedAt ValueObject: {dbModel.DeletedAt}");
        }

        // ③ Reconstruct Factory メソッド呼び出し（DB値で復元）
        var entity = UserPreferences.Reconstruct(
            userId,
            respondedAt,
            createdAt,
            updatedAt,
            deletedAt,
            RowId.From(dbModel.RowId),
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
