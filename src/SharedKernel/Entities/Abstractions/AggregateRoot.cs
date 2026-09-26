using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.SharedKernel.Entities;

/// <summary>
/// AggregateRoot（集約ルート）の基底クラス
/// </summary>
/// <typeparam name="TId">集約ルート ID の型（RowId ベース）</typeparam>
/// <remarks>
/// <para>【意味論】</para>
/// <list type="bullet">
/// <item><description>AggregateRoot はトランザクション境界を表現</description></item>
/// <item><description>一度に保存・削除される複数の Entity をグループ化</description></item>
/// <item><description>DDD の集約パターンの実装</description></item>
/// </list>
/// <para>【Entity との違い】</para>
/// <list type="bullet">
/// <item><description>機能的には完全に同じ</description></item>
/// <item><description>意味論的に「これは集約ルートである」ことを表現</description></item>
/// </list>
/// <para>【マーカーインターフェース】</para>
/// <list type="bullet">
/// <item><description>IAggregateRoot を実装（Query Service パターンでのジェネリック制約用）</description></item>
/// </list>
/// <para>【将来拡張】</para>
/// <list type="bullet">
/// <item><description>AggregateRoot 固有の機能（例：子Entity管理）は将来追加予定</description></item>
/// </list>
/// </remarks>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    where TId : notnull, RowId
{
    // Entity<TId> を継承
    // 現在は固有の実装なし
}
