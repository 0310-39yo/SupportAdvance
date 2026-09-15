namespace SupportAdvance.SharedKernel.Entities;

/// <summary>
/// 集約ルートのマーカーインターフェース。
/// DDD パターンの集約ルートを型で表現し、Query Service パターンでのジェネリック制約、
/// Domain Event 発行・管理、Transaction 境界を明示する。
/// AggregateRoot&lt;TId&gt; が実装する。
/// </summary>
public interface IAggregateRoot
{
}
