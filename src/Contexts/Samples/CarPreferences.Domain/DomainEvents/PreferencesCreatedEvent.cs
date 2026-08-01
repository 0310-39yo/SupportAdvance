using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Domain.DomainEvents;

/// <summary>
/// ユーザーの初期好み設定が作成されたイベント
///
/// 【発行】Application層（CreateUserPreferencesUseCase）
/// 【用途】ウェルカムメール、初期推奨、ログ
/// 【識別子】RowId（システム基本ID、マイナンバー相当）
/// </summary>
public sealed class PreferencesCreatedEvent : IDomainEvent
{
    /// <summary>row_id（システム基本ID）</summary>
    public long RowId { get; }

    /// <summary>イベント発生時刻（JST）</summary>
    public LocalDateTime OccurredAt { get; }

    public PreferencesCreatedEvent(
        long rowId,
        LocalDateTime occurredAt)
    {
        RowId = rowId;
        OccurredAt = occurredAt;
    }
}
