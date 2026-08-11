using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

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
/// - TEntity: Entity<TId>（RowId型に限定）
/// - TDbModel: データベースモデル
/// - TId: Entity の ID 型（RowId を継承する型）
/// </summary>
public interface IEntityMapper<TEntity, TDbModel, TId>
    where TEntity : Entity<TId>
    where TDbModel : class
    where TId : notnull, RowId
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
