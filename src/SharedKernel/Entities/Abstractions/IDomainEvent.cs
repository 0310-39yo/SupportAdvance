using SupportAdvance.Common.Clocks;

namespace SupportAdvance.SharedKernel.Entities;

/// <summary>
/// ドメインイベントの基本インターフェース
///
/// Domain層で発生した重要な事象を表現し、Application層への通知を可能にする。
/// 各ドメインイベント型はこのインターフェースを実装する必要がある。
///
/// 【識別方法】RowId事前採番により、イベント発行時にAggregateRootId（RowId）が確定
/// 【タイムゾーン】すべてのイベント発生時刻は JST（日本標準時）
/// 【不変性】イベントは発行後に変更されない前提
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// 集約ルートの行ID（RowId）
    ///
    /// 【用途】ドメインイベントを発行した集約を一意に識別
    /// 【型】long（RowId の値）
    /// 【重要】RowId事前採番により、Entity生成時点でこの値は確定
    /// 【必須】すべてのイベント実装で必ず値を持つこと
    /// </summary>
    long AggregateRootId { get; }

    /// <summary>
    /// イベント発生時刻（JST）
    ///
    /// 【型】LocalDateTime（JST専用型）
    /// 【タイムゾーン】常に JST（Tokyo Standard Time）
    /// 【用途】監査ログ、イベント順序付けなど
    /// 【必須】すべてのイベント実装で必ず値を持つこと
    /// </summary>
    LocalDateTime OccurredAt { get; }
}
