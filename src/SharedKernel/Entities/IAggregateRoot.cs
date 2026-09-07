namespace SupportAdvance.SharedKernel.Entities;

/// <summary>
/// 集約ルートのマーカーインターフェース
///
/// 【責務】DDD パターンの集約ルートを型で表現
/// 【用途】
///   - Query Service パターンでのジェネリック制約
///   - Domain Event 発行・管理
///   - Transaction 境界の明示
///
/// 【実装】AggregateRoot<TId> が実装
/// </summary>
public interface IAggregateRoot
{
}
