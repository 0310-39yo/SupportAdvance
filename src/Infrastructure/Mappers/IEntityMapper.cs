using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.Infrastructure.Mappers;

/// <summary>
/// Domain Entity ↔ DbModel マッパー（汎用インターフェース）
///
/// 【責務】
/// - Domain Entity → DbModel（永続化前）
/// - DbModel → Domain Entity（読み取り時）
/// 【原則】純粋なマッピングのみ（ビジネスコンテキスト不問）
/// 【監査情報】createdBy, updatedBy は Repository 層で設定
///
/// 【型パラメータ】
/// - TEntity: Entity<TId>（ID型をサポート）
/// - TDbModel: データベースモデル
/// - TId: Entity の ID 型（ValueObject など）
/// </summary>
public interface IEntityMapper<TEntity, TDbModel, TId>
    where TEntity : Entity<TId>
    where TDbModel : class
    where TId : notnull
{
    /// <summary>
    /// Domain Entity → DbModel（保存用）
    /// </summary>
    TDbModel ToDbModel(TEntity entity);

    /// <summary>
    /// DbModel → Domain Entity（読み取り用）
    /// </summary>
    TEntity ToDomainEntity(TDbModel dbModel, IClock clock);
}
