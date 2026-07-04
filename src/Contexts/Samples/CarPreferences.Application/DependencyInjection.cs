using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Application.UseCases;
using SupportAdvance.Contexts.Samples.CarPreferences.Application.UseCases;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Application;

/// <summary>
/// CarPreferences.Application に関連する依存関係の注入を設定するための拡張メソッドを提供するクラス
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddCarPreferencesApplicationModels(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // ===== UseCase 登録 =====
        // 追加方法：
        // 1. UseCase インターフェースをスコープで登録
        // 2. Validator をスコープで登録（必要な場合）
        // 3. Application Service をスコープで登録（必要な場合）
        services.AddScoped<IUseCase<CarPreferencesRequest, CarPreferencesResponse>, CarPreferencesUseCase>();

        return services;
    }
}
