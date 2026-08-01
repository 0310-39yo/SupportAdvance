using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.Entities;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;
using SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure.DataAccess.Models;
using SupportAdvance.Infrastructure.Mappers;
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
    /// Domain Entity → DbModel（保存用、RowId → long 変換）
    /// </summary>
    public UserPreferencesDbModel ToDbModel(UserPreferences entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new UserPreferencesDbModel
        {
            RowId = entity.Id.Value,  // RowId → long 変換
            UserId = entity.UserId.Value ?? 0,  // int? → int
            CreatedAt = entity.CreatedAt,  // LocalDateTime をそのまま使用
            CreatedBy = 0,  // Repository で設定される
            UpdatedAt = entity.UpdatedAt == entity.CreatedAt ? null : entity.UpdatedAt,
            UpdatedBy = null,  // Repository で設定される
            DeletedAt = null,
            DeletedBy = null,
            PreferredModel = entity.PreferredModel?.Value,
            PreferredBodyType = entity.PreferredBodyType?.ToString(),
            PrefersAutomatic = entity.PrefersAutomatic,
            BudgetFrom = entity.BudgetFrom?.Amount,
            BudgetTo = entity.BudgetTo?.Amount
        };
    }

    /// <summary>
    /// DbModel → Domain Entity（読み取り用、long → RowId ValueObject 変換）
    /// </summary>
    public UserPreferences ToDomainEntity(UserPreferencesDbModel dbModel, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(dbModel);
        ArgumentNullException.ThrowIfNull(clock);

        // ValueObject 変換（LocalDateTime → DateTime）
        var userId = RespondentPersonId.From(dbModel.UserId);
        var respondedAt = RespondentAt.From(dbModel.CreatedAt.Value, clock);

        // Entity 構築（dbModel.RowId (long) → RowId ValueObject 変換）
        var entity = new UserPreferences(userId, respondedAt, clock, RowId.From(dbModel.RowId));

        // ビジネスロジックは設定しない（読み取り専用）
        // 必要に応じて別途メソッドで設定可能

        return entity;
    }
}
