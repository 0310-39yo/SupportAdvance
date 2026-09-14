namespace SupportAdvance.SharedKernel.Entities;

/// <summary>
/// Department 集約の Application層インターフェース
///
/// 【配置】SharedKernel に定義（すべての層から参照可能）
/// 【責務】他の Bounded Context に公開する Department の read-only API
/// 【特徴】Domain の Department Entity がこのインターフェースを実装
/// 【用途】Query Service を通じた Context間の Aggregate 参照
///
/// 【メリット】
///   - Domain が Application に依存しない（アーキテクチャ準拠）
///   - 他 Context は具体的な Department Entity を参照しない
///   - Application層のインターフェース経由で参照可能
///   - Domain層の実装詳細を隠蔽
/// </summary>
public interface IDepartment : IAggregateRoot
{
    // 現時点ではマーカーインターフェース
    // 将来的に public なプロパティを定義可能
}
