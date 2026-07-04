using SupportAdvance.Application.UseCases;
using SupportAdvance.Crosscutting.Logging;

namespace SupportAdvance.Presentation.Shared.Decorators;

/// <summary>
/// UseCase のエラーハンドリング デコレーター。
/// インナーの UseCase 実行時に発生した例外をキャッチし、詳細なエラーログを記録する。
/// 相関コンテキスト(CorrelationContext)を使用してエラーログを関連付け、トレーサビリティを向上させる。
///
/// 【単一責務】例外のキャッチとエラーログ記録。
/// 例外の再スロー処理により、呼び出し元での適切なハンドリングを可能にする。
/// </summary>
/// <typeparam name="TRequest">リクエストの型</typeparam>
/// <typeparam name="TResponse">レスポンスの型</typeparam>
public sealed class ErrorHandlingDecorator<TRequest, TResponse> : IUseCase<TRequest, TResponse>
    where TRequest : IRequest
    where TResponse : IResponse
{
    private readonly IUseCase<TRequest, TResponse> _innerUseCase;
    private readonly IAppLogging<ErrorHandlingDecorator<TRequest, TResponse>> _logger;
    private readonly ICorrelationContext _correlationContext;

    public ErrorHandlingDecorator(
        IUseCase<TRequest, TResponse> innerUseCase,
        IAppLogging<ErrorHandlingDecorator<TRequest, TResponse>> logger,
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
    /// UseCase を実行し、例外が発生した場合はエラーログを記録する。
    ///
    /// 【単一責務】例外のキャッチとエラーログ記録
    /// </summary>
    /// <param name="request">リクエストの内容</param>
    /// <returns>レスポンスの内容</returns>
    public async Task<TResponse> ExecuteAsync(TRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _innerUseCase.ExecuteAsync(request);
        }
        catch (Exception ex)
        {
            var correlationId = _correlationContext.GetOrCreate();
            _logger.LogError(
                $"UseCase実行時にエラーが発生しました。CorrelationId: {correlationId}",
                ex);
            throw;
        }
    }
}
