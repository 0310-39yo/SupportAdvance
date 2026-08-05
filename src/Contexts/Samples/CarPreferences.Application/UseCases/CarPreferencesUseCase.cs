using SupportAdvance.Application.UseCases;
using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Application.UseCases;

/// <summary>
/// 車の好みに関する処理を実行するUseCase
/// </summary>
public class CarPreferencesUseCase : IUseCase<CarPreferencesRequest, CarPreferencesResponse>
{
    private readonly IClock _clock;

    public CarPreferencesUseCase(IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        _clock = clock;
    }

    /// <summary>
    /// 指定されたリクエストに基づいて車の好みに関する処理を非同期で実行
    /// リクエストの Name と Details を受け取り、メッセージを返す
    /// </summary>
    /// <param name="request">車の好みに関するリクエスト情報</param>
    /// <returns>車の好みに関する処理結果</returns>
    public async Task<CarPreferencesResponse> ExecuteAsync(CarPreferencesRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // ビジネスロジック：リクエストを処理して結果を作成
        // 実際のアプリケーションではここで Domain ロジックを呼び出す
        await Task.Delay(100); // Simulate async work

        return new CarPreferencesResponse
        {
            ExecutedAt = _clock.JstNow,
            IsSuccess = true,
            Message = $"Processed: {request.Name} - {request.Details}"
        };
    }
}
