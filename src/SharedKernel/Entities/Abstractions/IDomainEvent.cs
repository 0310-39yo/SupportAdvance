using SupportAdvance.Common.Clocks;

namespace SupportAdvance.SharedKernel.Entities;

/// <summary>
/// ドメインイベントの基本インターフェース
/// </summary>
/// <remarks>
/// <para>Domain層で発生した重要な事象を表現し、Application層への通知の実現。各ドメインイベント型はこのインターフェースの実装が必要</para>
/// <para>【識別方法】RowId事前採番により、イベント発行時にAggregateRootId（RowId）が確定</para>
/// <para>【タイムゾーン】すべてのイベント発生時刻は JST（日本標準時）</para>
/// <para>【不変性】イベントは発行後に変更されない前提</para>
/// </remarks>
public interface IDomainEvent
{
    /// <summary>
    /// 集約ルートの行ID（RowId）
    /// </summary>
    /// <remarks>
    /// <para>【用途】ドメインイベントを発行した集約を一意に識別</para>
    /// <para>【型】long（RowId の値）</para>
    /// <para>【重要】RowId事前採番により、Entity生成時点でこの値は確定</para>
    /// <para>【必須】すべてのイベント実装で必ず値を持つこと</para>
    /// </remarks>
    long AggregateRootId { get; }

    /// <summary>
    /// イベント発生時刻（JST）
    /// </summary>
    /// <remarks>
    /// <para>【型】LocalDateTime（JST専用型）</para>
    /// <para>【タイムゾーン】常に JST（Tokyo Standard Time）</para>
    /// <para>【用途】監査ログ、イベント順序付けなど</para>
    /// <para>【必須】すべてのイベント実装で必ず値を持つこと</para>
    /// </remarks>
    LocalDateTime OccurredAt { get; }
}
