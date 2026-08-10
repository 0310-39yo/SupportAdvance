using SupportAdvance.Application.UseCases;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Crosscutting.Logging;

namespace SupportAdvance.Presentation.Shared.Decorators;

/// <summary>
/// UseCase のパフォーマンス計測 デコレーター。
/// インナーの UseCase 実行時間を計測し、実行時間をログに記録する。
/// 実行時間が閾値を超える場合は Warning ログを出力。
/// 相関コンテキスト(CorrelationContext)を使用してパフォーマンスログを関連付ける。
///
/// 【単一責務】実行時間の計測とログ記録のみ。
/// </summary>
/// <typeparam name="TRequest">リクエストの型</typeparam>
/// <typeparam name="TResponse">レスポンスの型</typeparam>
public sealed class PerformanceDecorator<TRequest, TResponse> : IUseCase<TRequest, TResponse>
    where TRequest : IRequest
    where TResponse : IResponse
{
    private readonly IUseCase<TRequest, TResponse> _innerUseCase;
    private readonly IAppLogging<PerformanceDecorator<TRequest, TResponse>> _logger;
    private readonly ICorrelationContext _correlationContext;
    private readonly IClock _clock;
    private readonly int _warningThresholdMs;

    public PerformanceDecorator(
        IUseCase<TRequest, TResponse> innerUseCase,
        IAppLogging<PerformanceDecorator<TRequest, TResponse>> logger,
        ICorrelationContext correlationContext,
        IClock clock,
        int warningThresholdMs = 1000)
    {
        ArgumentNullException.ThrowIfNull(innerUseCase);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(correlationContext);
        ArgumentNullException.ThrowIfNull(clock);

        _innerUseCase = innerUseCase;
        _logger = logger;
        _correlationContext = correlationContext;
        _clock = clock;
        _warningThresholdMs = warningThresholdMs;
    }

    /// <summary>
    /// UseCase を実行し、実行時間を計測してログに記録する。
    ///
    /// 【単一責務】実行時間の計測とログ記録のみ
    /// </summary>
    /// <param name="request">リクエストの内容</param>
    /// <returns>レスポンスの内容</returns>
    public async Task<TResponse> ExecuteAsync(TRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var startTime = _clock.JstNow.Value;
        try
        {
            return await _innerUseCase.ExecuteAsync(request);
        }
        finally
        {
            var endTime = _clock.JstNow.Value;
            var elapsedMs = (int)(endTime - startTime).TotalMilliseconds;
            var correlationId = _correlationContext.GetOrCreate();

            if (elapsedMs > _warningThresholdMs)
            {
                _logger.LogWarning(
                    "UseCase実行時間が閾値を超過しました。実行時間: {0}ms (threshold: {1}ms), CorrelationId: {2}",
                    elapsedMs, _warningThresholdMs, correlationId);
            }
            else
            {
                _logger.LogInformation(
                    "UseCase実行完了。実行時間: {0}ms, CorrelationId: {1}",
                    elapsedMs, correlationId);
            }
        }
    }
}
