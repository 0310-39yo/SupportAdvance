using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Infrastructure.Mappers;

/// <summary>
/// Domain の Entity と DB モデルの相互変換を行うマッパーの抽象
/// </summary>
/// <typeparam name="TEntity">変換対象のエンティティ（<see cref="Entity{TId}"/> の派生型）</typeparam>
/// <typeparam name="TDbModel">対応する DB モデル</typeparam>
/// <typeparam name="TId">エンティティの ID（<see cref="RowId"/> の派生型）</typeparam>
/// <remarks>
/// <para>【原則】純粋な型変換のみ。業務上の判断は対象外</para>
/// <para>【監査情報】監査列（<c>created_by</c>／<c>updated_by</c> など）の設定はリポジトリの担当で、マッパーでは設定なし</para>
/// </remarks>
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
