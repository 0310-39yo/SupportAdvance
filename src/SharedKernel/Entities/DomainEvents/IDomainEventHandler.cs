namespace SupportAdvance.SharedKernel.Entities.DomainEvents;

/// <summary>
/// ドメインイベントハンドラーのインターフェース
///
/// 【責務】Application層でドメインイベントを消費・処理
/// 【実行階層】Application層のみ
/// 【型安全性】Generic で特定イベント型を指定
///
/// 【使用方法】
/// 1. IDomainEventHandler を実装したクラスを作成
/// 2. HandleAsync メソッドにログ出力・他処理を実装
/// 3. DI コンテナに登録
/// 4. EventDispatcher がイベント処理時に呼び出し
///
/// 【非同期対応】
/// - すべてのハンドラーは async Task で実装
/// - 複数ハンドラーを並行実行可能（将来）
/// </summary>
/// <typeparam name="TEvent">処理するドメインイベント型。IDomainEvent を実装していること</typeparam>
public interface IDomainEventHandler<TEvent>
    where TEvent : IDomainEvent
{
    /// <summary>
    /// ドメインイベントの処理
    ///
    /// 【責務】
    /// - イベント情報からログ出力
    /// - 外部システムへの通知（メール送信など）
    /// - 関連データの更新（キャッシュクリアなど）
    ///
    /// 【実装時の注意】
    /// - Domain層のビジネスロジックを記述しない
    /// - イベント処理結果を Entity に反映しない
    /// - 例外が発生した場合は呼び出し元で処理
    ///
    /// 【使用例】
    /// public async Task HandleAsync(PreferencesUpdatedEvent @event)
    /// {
    ///     _logger.LogInformation(
    ///         $"Preferences updated - UserId: {@event.UserId}");
    ///     await _notificationService.NotifyAsync(@event.UserId);
    /// }
    /// </summary>
    /// <param name="event">
    /// 処理するドメインイベント
    ///
    /// 【要件】
    /// - null ではない
    /// - OccurredAt に有効な LocalDateTime を持つ
    /// </param>
    /// <returns>処理完了タスク</returns>
    Task HandleAsync(TEvent @event);
}
