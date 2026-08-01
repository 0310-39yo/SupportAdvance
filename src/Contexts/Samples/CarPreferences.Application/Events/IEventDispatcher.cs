using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Application.Events;

/// <summary>
/// ドメインイベント ディスパッチャー インターフェース
///
/// 【責務】ドメインイベントを適切なハンドラーにルーティング
/// 【実装】Application層で実装
/// </summary>
public interface IEventDispatcher
{
    /// <summary>
    /// ドメインイベントをディスパッチ
    ///
    /// 【処理フロー】
    /// 1. イベント型から対応するハンドラーを特定
    /// 2. DI から該当ハンドラー（複数可）を取得
    /// 3. 全ハンドラーの HandleAsync() を順序に実行
    ///
    /// 【パラメータ】@event - ディスパッチするイベント
    /// 【例外】ハンドラー実行時に例外が発生した場合は再スロー
    /// </summary>
    Task DispatchAsync(IDomainEvent @event);
}
