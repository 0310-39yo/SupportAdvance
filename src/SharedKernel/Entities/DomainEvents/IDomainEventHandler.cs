namespace SupportAdvance.SharedKernel.Entities.DomainEvents;

/// <summary>
/// ドメインイベントハンドラーのインターフェース
/// </summary>
/// <typeparam name="TEvent">処理するドメインイベント型。IDomainEvent を実装していること</typeparam>
/// <remarks>
/// <para>【責務】Application層でドメインイベントを消費・処理</para>
/// <para>【実行階層】Application層のみ</para>
/// <para>【型安全性】Generic で特定イベント型を指定</para>
/// <para>【使用方法】</para>
/// <list type="bullet">
/// <item><description>IDomainEventHandler を実装したクラスを作成</description></item>
/// <item><description>HandleAsync メソッドにログ出力・他処理を実装</description></item>
/// <item><description>DI コンテナに登録</description></item>
/// <item><description>EventDispatcher がイベント処理時に呼び出し</description></item>
/// </list>
/// <para>【非同期対応】</para>
/// <list type="bullet">
/// <item><description>すべてのハンドラーは async Task で実装</description></item>
/// <item><description>複数ハンドラーを並行実行可能（将来）</description></item>
/// </list>
/// </remarks>
public interface IDomainEventHandler<TEvent>
    where TEvent : IDomainEvent
{
    /// <summary>
    /// ドメインイベントの処理
    /// </summary>
    /// <param name="event">
    /// 処理するドメインイベント
    ///
    /// 【要件】
    /// - null ではない
    /// - OccurredAt に有効な LocalDateTime を持つ
    /// </param>
    /// <returns>処理完了タスク</returns>
    /// <remarks>
    /// <para>【責務】</para>
    /// <list type="bullet">
    /// <item><description>イベント情報からログ出力</description></item>
    /// <item><description>外部システムへの通知（メール送信など）</description></item>
    /// <item><description>関連データの更新（キャッシュクリアなど）</description></item>
    /// </list>
    /// <para>【実装時の注意】</para>
    /// <list type="bullet">
    /// <item><description>Domain層のビジネスロジックの記述なし</description></item>
    /// <item><description>イベント処理結果の Entity への反映なし</description></item>
    /// <item><description>例外が発生した場合は呼び出し元で処理</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// public async Task HandleAsync(PreferencesUpdatedEvent @event)
    /// {
    ///     _logger.LogInformation(
    ///         $"Preferences updated - UserId: {@event.UserId}");
    ///     await _notificationService.NotifyAsync(@event.UserId);
    /// }
    /// </code>
    /// </example>
    Task HandleAsync(TEvent @event);
}
