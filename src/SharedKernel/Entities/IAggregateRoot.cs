namespace SupportAdvance.SharedKernel.Entities;

/// <summary>
/// 集約ルートのマーカーインターフェース。
/// DDD パターンの集約ルートを型で表現し、Query Service パターンでのジェネリック制約、
/// Domain Event 発行・管理、Transaction 境界の明示。
/// AggregateRoot&lt;TId&gt; による実装
/// </summary>
public interface IAggregateRoot
{
}
