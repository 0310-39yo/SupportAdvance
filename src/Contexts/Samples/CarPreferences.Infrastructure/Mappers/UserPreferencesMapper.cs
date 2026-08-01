using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.Entities;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;
using SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure.DataAccess.Models;
using SupportAdvance.Infrastructure.Mappers;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure.Mappers;

/// <summary>
/// UserPreferences Domain Entity ↔ DbModel マッパー
///
/// 【責務】
/// - Domain Entity → DbModel（永続化前）
/// - DbModel → Domain Entity（読み取り時）
/// 【原則】純粋なマッピングのみ（ビジネスコンテキスト不問）
/// 【監査情報】createdBy, updatedBy は Repository 層で設定
/// </summary>
public class UserPreferencesMapper : IEntityMapper<UserPreferences, UserPreferencesDbModel>
{
    /// <summary>
    /// Domain Entity → DbModel（保存用）
    /// </summary>
    public UserPreferencesDbModel ToDbModel(UserPreferences entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new UserPreferencesDbModel
        {
            RowId = entity.Id,
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
    /// DbModel → Domain Entity（読み取り用）
    /// </summary>
    public UserPreferences ToDomainEntity(UserPreferencesDbModel dbModel, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(dbModel);
        ArgumentNullException.ThrowIfNull(clock);

        // ValueObject 変換（LocalDateTime → DateTime）
        var userId = RespondentPersonId.From(dbModel.UserId);
        var respondedAt = RespondentAt.From(dbModel.CreatedAt.Value, clock);

        // Entity 構築（rowId = dbModel.RowId）
        var entity = new UserPreferences(userId, respondedAt, clock, dbModel.RowId);

        // ビジネスロジックは設定しない（読み取り専用）
        // 必要に応じて別途メソッドで設定可能

        return entity;
    }
}
