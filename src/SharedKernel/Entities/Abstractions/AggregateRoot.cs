namespace SupportAdvance.SharedKernel.Entities;

/// <summary>
/// AggregateRoot（集約ルート）の基底クラス
///
/// 【意味論】
/// - AggregateRoot はトランザクション境界を表現
/// - 一度に保存・削除される複数の Entity をグループ化
/// - DDD の集約パターンの実装
///
/// 【Entity との違い】
/// - 機能的には完全に同じ
/// - 意味論的に「これは集約ルートである」ことを表現
///
/// 【将来拡張】
/// - AggregateRoot 固有の機能（例：子Entity管理）は将来追加予定
/// </summary>
/// <typeparam name="TId">集約ルート ID の型</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    // Entity<TId> を継承
    // 現在は固有の実装なし
}
