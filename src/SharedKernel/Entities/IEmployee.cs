namespace SupportAdvance.SharedKernel.Entities;

/// <summary>
/// Employee 集約の Application層インターフェース
/// </summary>
/// <remarks>
/// <para>【配置】SharedKernel に定義（すべての層から参照可能）</para>
/// <para>【責務】他の Bounded Context に公開する Employee の read-only API</para>
/// <para>【特徴】Domain の Employee Entity がこのインターフェースを実装</para>
/// <para>【用途】Query Service を通じた Context間の Aggregate 参照</para>
/// <para>【メリット】</para>
/// <list type="bullet">
/// <item><description>Domain が Application に依存しない（アーキテクチャ準拠）</description></item>
/// <item><description>他 Context からの具体的な Employee Entity の参照なし</description></item>
/// <item><description>Application層のインターフェース経由で参照可能</description></item>
/// <item><description>Domain層の実装詳細を隠蔽</description></item>
/// </list>
/// </remarks>
public interface IEmployee : IAggregateRoot
{
    // 現時点ではマーカーインターフェース
    // 将来的に public なプロパティを定義可能
}
