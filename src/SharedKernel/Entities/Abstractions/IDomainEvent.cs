using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.ValueObjects.Audit;

namespace SupportAdvance.SharedKernel.Entities;

/// <summary>
/// ドメインイベントの基本インターフェース
///
/// Domain層で発生した重要な事象を表現し、Application層への通知を可能にする。
/// 各ドメインイベント型はこのインターフェースを実装する必要がある。
///
/// 【タイムゾーン】すべてのイベント発生時刻は JST（日本標準時）
/// 【不変性】イベントは発行後に変更されない前提
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// イベント一意識別子（GUID ValueObject）
    ///
    /// 【用途】各ドメインイベントを一意に識別
    /// 【型】DomainEventId（GUID ValueObject）
    /// 【必須】すべてのイベント実装で必ず値を持つこと
    /// </summary>
    DomainEventId EventId { get; }

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
