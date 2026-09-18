using SupportAdvance.Application.UseCases;
using SupportAdvance.Crosscutting.Logging;

namespace SupportAdvance.Presentation.Shared.Decorators;

/// <summary>
/// UseCase のロギング デコレーター。
/// インナーの UseCase 実行時に、リクエストとレスポンスの内容をログに記録する。
/// 相関コンテキスト(CorrelationContext)を使用してログを関連付け、分散トレース対応を実現する。
///
/// 【単一責務】リクエスト/レスポンスのログ記録のみ。
/// 他の責務（パフォーマンス計測、エラーハンドリング）は別デコレーターに委譲。
/// </summary>
/// <typeparam name="TRequest">リクエストの型</typeparam>
/// <typeparam name="TResponse">レスポンスの型</typeparam>
public sealed class LoggingDecorator<TRequest, TResponse> : IUseCase<TRequest, TResponse>
    where TRequest : IRequest
    where TResponse : IResponse
{
    private readonly ICorrelationContext _correlationContext;
    private readonly IUseCase<TRequest, TResponse> _innerUseCase;
    private readonly IAppLogging<LoggingDecorator<TRequest, TResponse>> _logger;

    /// <summary>
    /// <see cref="LoggingDecorator{TRequest, TResponse}"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="innerUseCase">装飾対象のユースケース（次に実行されるデコレーターまたは本体）</param>
    /// <param name="logger">ログの出力先</param>
    /// <param name="correlationContext">ログに付与する CorrelationId の取得元</param>
    /// <exception cref="ArgumentNullException">いずれかの引数が <see langword="null"/> の場合</exception>
    public LoggingDecorator(
        IUseCase<TRequest, TResponse> innerUseCase,
        IAppLogging<LoggingDecorator<TRequest, TResponse>> logger,
        ICorrelationContext correlationContext)
    {
        ArgumentNullException.ThrowIfNull(innerUseCase);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(correlationContext);

        _innerUseCase = innerUseCase;
        _logger = logger;
        _correlationContext = correlationContext;
    }

    /// <summary>
    /// UseCase を実行し、リクエストとレスポンスの内容をログに記録する。
    ///
    /// 【単一責務】リクエスト/レスポンスのログ記録のみ
    /// </summary>
    /// <param name="request">リクエストの内容</param>
    /// <returns>レスポンスの内容</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> が <see langword="null"/> の場合</exception>
    public async Task<TResponse> ExecuteAsync(TRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var correlationId = _correlationContext.GetOrCreate();
        _logger.LogInformation($"UseCase 実行開始。CorrelationId: {correlationId}");

        try
        {
            return await _innerUseCase.ExecuteAsync(request);
        }
        finally
        {
            _logger.LogInformation($"UseCase 実行完了。CorrelationId: {correlationId}");
        }
    }
}
