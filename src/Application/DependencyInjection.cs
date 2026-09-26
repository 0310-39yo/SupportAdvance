using Microsoft.Extensions.DependencyInjection;

namespace SupportAdvance.Application;

/// <summary>
/// Application層のサービス登録を管理するクラス。
/// 各コンテキストのUseCaseを登録する際にデコレーターパターンの活用
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Application層の共通サービスの登録
    /// </summary>
    /// <param name="services">登録先のサービスコレクション</param>
    /// <returns>メソッドチェーン用の <paramref name="services"/> 自身</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> が <see langword="null"/> の場合</exception>
    /// <remarks>
    /// <para>【注記】各 Context の Application 層との依存を避けるため、具体的な Context の登録は Program.cs での直接呼び出し</para>
    /// </remarks>
    public static IServiceCollection AddApplicationModels(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // 汎用 Application サービスをここで登録
        // 各 Context の具体的な登録は Program.cs で実施

        return services;
    }
}

/*
 * ==========================================
 * UseCase デコレーター登録ガイド
 * ==========================================
 * 
 * 【登録パターン】
 * 
 * 1. フルデコレーター（推奨）
 *    services.RegisterUseCaseWithDecorators<TRequest, TResponse, TImplementation>();
 *    
 *    チェーン順序: ErrorHandling → Performance → Logging → 実装
 *    責務:
 *    - ErrorHandling: 例外キャッチ、エラーログ、トレーサビリティ
 *    - Performance: 実行時間計測、警告ログ（閾値超過時）
 *    - Logging: リクエスト/レスポンスログ、CorrelationId付与
 * 
 * 
 * 2. ロギングのみ
 *    services.RegisterUseCaseWithLogging<TRequest, TResponse, TImplementation>();
 *    
 *    使用場面: 軽量な実装、副作用なし
 * 
 * 
 * 3. エラーハンドリングのみ
 *    services.RegisterUseCaseWithErrorHandling<TRequest, TResponse, TImplementation>();
 *    
 *    使用場面: パフォーマンス計測不要
 * 
 * 
 * 4. パフォーマンス計測（ロギング付き）
 *    services.RegisterUseCaseWithPerformance<TRequest, TResponse, TImplementation>(warningThresholdMs: 500);
 *    
 *    使用場面: パフォーマンス監視が重要、エラーハンドリング不要
 * 
 * 
 * 5. デコレーターなし（直接登録）
 *    services.RegisterUseCase<TRequest, TResponse, TImplementation>();
 *    
 *    使用場面: テスト、シンプルな実装
 * 
 * 
 * ==========================================
 * 実装例
 * ==========================================
 * 
 * 【CarPreferences コンテキスト】
 * 
 * namespace SupportAdvance.Contexts.Samples.CarPreferences.Application;
 * 
 * public static class DependencyInjection
 * {
 *     public static IServiceCollection AddCarPreferencesApplicationModels(this IServiceCollection services)
 *     {
 *         ArgumentNullException.ThrowIfNull(services);
 * 
 *         // フルデコレーター（最も推奨）
 *         services.RegisterUseCaseWithDecorators<
 *             GetUserCarPreferencesRequest,
 *             GetUserCarPreferencesResponse,
 *             GetUserCarPreferencesUseCase
 *         >(warningThresholdMs: 1000);
 * 
 *         return services;
 *     }
 * }
 * 
 * 
 * ==========================================
 * デコレーター順序の重要性
 * ==========================================
 * 
 * 【外側 → 内側】
 * ErrorHandling → Performance → Logging → 実装
 * 
 * ✓ 正しい順序の理由：
 * 1. ErrorHandling が最外側
 *    └─ 全ての実行をキャッチし、エラーを統一的にハンドル
 * 
 * 2. Performance が中間
 *    └─ 実行時間を正確に計測（エラー処理を含む）
 * 
 * 3. Logging が最内側
 *    └─ リクエスト/レスポンスの詳細をログ
 * 
 * 
 * ==========================================
 * CorrelationContext の活用
 * ==========================================
 * 
 * 各デコレーターは CorrelationContext を使用して
 * リクエスト全体を一意のIDで追跡します。
 * 
 * ログ出力例:
 * [Logging]    UseCase 実行開始。CorrelationId: abc-123
 * [Performance] UseCase 実行時間: 45ms, CorrelationId: abc-123
 * [Error]      UseCase 実行時にエラー。CorrelationId: abc-123
 * 
 * 
 * ==========================================
 * カスタムデコレーター追加例
 * ==========================================
 * 
 * 【キャッシングが必要な場合】
 * 
 * public sealed class CachingDecorator<TRequest, TResponse> : IUseCase<TRequest, TResponse>
 *     where TRequest : IRequest
 *     where TResponse : IResponse
 * {
 *     // 実装...
 * }
 * 
 * 登録時のチェーン:
 * Caching → ErrorHandling → Performance → Logging → 実装
 * 
 * services.AddScoped<IUseCase<TRequest, TResponse>>(provider =>
 * {
 *     IUseCase<TRequest, TResponse> useCase = provider.GetRequiredService<TImplementation>();
 *     // Logging デコレーター...
 *     // Performance デコレーター...
 *     // ErrorHandling デコレーター...
 *     var cache = provider.GetRequiredService<ICache>();
 *     useCase = new CachingDecorator<TRequest, TResponse>(useCase, cache);
 *     return useCase;
 * });
 * 
 * 
 * ==========================================
 * テスト時のデコレーター無効化
 * ==========================================
 * 
 * テストではシンプルな登録を使用:
 * 
 * services.RegisterUseCase<TRequest, TResponse, TImplementation>();
 * 
 * または実装にモックを直接注入（テストコンテキストで）
 * 
 */
