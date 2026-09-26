namespace SupportAdvance.Application.UseCases;

/// <summary>
/// UseCase の基本インターフェース
/// </summary>
/// <typeparam name="TRequest">IRequest を実装したリクエストの型</typeparam>
/// <typeparam name="TResponse">IResponse を実装したレスポンスの型</typeparam>
public interface IUseCase<in TRequest, TResponse>
    where TRequest : IRequest
    where TResponse : IResponse
{
    /// <summary>
    /// UseCase の実行
    /// </summary>
    /// <param name="request">リクエストのインスタンス</param>
    /// <returns>レスポンスのインスタンス</returns>
    Task<TResponse> ExecuteAsync(TRequest request);
}
