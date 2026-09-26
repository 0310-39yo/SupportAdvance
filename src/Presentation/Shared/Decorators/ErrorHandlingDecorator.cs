using SupportAdvance.Application.UseCases;
using SupportAdvance.Crosscutting.Logging;

namespace SupportAdvance.Presentation.Shared.Decorators;

/// <summary>
/// UseCase のエラーハンドリング デコレーター
/// </summary>
/// <typeparam name="TRequest">リクエストの型</typeparam>
/// <typeparam name="TResponse">レスポンスの型</typeparam>
/// <remarks>
/// <para>インナーの UseCase 実行時に発生した例外をキャッチし、詳細なエラーログの記録。相関コンテキスト(CorrelationContext)を使用してエラーログを関連付け、トレーサビリティを向上させる</para>
/// <para>【単一責務】例外のキャッチとエラーログ記録。例外の再スロー処理により、呼び出し元での適切なハンドリングの実現</para>
/// </remarks>
public sealed class ErrorHandlingDecorator<TRequest, TResponse> : IUseCase<TRequest, TResponse>
    where TRequest : IRequest
    where TResponse : IResponse
{
    private readonly IUseCase<TRequest, TResponse> _innerUseCase;
    private readonly IAppLogging<ErrorHandlingDecorator<TRequest, TResponse>> _logger;
    private readonly ICorrelationContext _correlationContext;

    /// <summary>
    /// <see cref="ErrorHandlingDecorator{TRequest, TResponse}"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="innerUseCase">装飾対象のユースケース（次に実行されるデコレーターまたは本体）</param>
    /// <param name="logger">ログの出力先</param>
    /// <param name="correlationContext">ログに付与する CorrelationId の取得元</param>
    /// <exception cref="ArgumentNullException">いずれかの引数が <see langword="null"/> の場合</exception>
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
    /// UseCase を実行し、例外が発生した場合はエラーログの記録
    /// </summary>
    /// <param name="request">リクエストの内容</param>
    /// <returns>レスポンスの内容</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> が <see langword="null"/> の場合</exception>
    /// <remarks>
    /// <para>【単一責務】例外のキャッチとエラーログ記録</para>
    /// <para>【注意】例外はエラーログの記録後、そのまま再送出（握りつぶさない）</para>
    /// </remarks>
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
