using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Application.UseCases;
using SupportAdvance.Crosscutting.Logging;
using SupportAdvance.Presentation.Shared.Decorators;
using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Presentation.Shared;

/// <summary>
/// Presentation層の共通サービスを登録するクラス。
/// デコレーターパターンを使用したUseCaseの登録の提供
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Shared Presentation の共通サービスの登録
    /// </summary>
    /// <param name="services">登録先のサービスコレクション</param>
    /// <returns>メソッドチェーン用の <paramref name="services"/> 自身</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> が <see langword="null"/> の場合</exception>
    /// <remarks>
    /// <para>【注意】現在は登録するサービスなし</para>
    /// </remarks>
    public static IServiceCollection AddSharedPresentationModels(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services;
    }

    /// <summary>
    /// デコレーターパターンを使用してUseCaseの登録。
    /// チェーン順序（外側から内側）: ErrorHandlingDecorator → PerformanceDecorator → LoggingDecorator → 実装
    /// </summary>
    /// <typeparam name="TRequest">リクエストの型</typeparam>
    /// <typeparam name="TResponse">レスポンスの型</typeparam>
    /// <typeparam name="TImplementation">UseCase実装の型</typeparam>
    /// <param name="services">サービスコレクション</param>
    /// <param name="warningThresholdMs">PerformanceDecoratorの警告閾値（ミリ秒）</param>
    /// <returns>メソッドチェーン用の <paramref name="services"/> 自身</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> が <see langword="null"/> の場合</exception>
    /// <remarks>
    /// <para>【登録内容】<typeparamref name="TImplementation"/> 自体と、デコレーターで包んだ <see cref="IUseCase{TRequest, TResponse}"/>（いずれも Scoped）</para>
    /// </remarks>
    public static IServiceCollection RegisterUseCaseWithDecorators<TRequest, TResponse, TImplementation>(
        this IServiceCollection services,
        int warningThresholdMs = 1000)
        where TRequest : IRequest
        where TResponse : IResponse
        where TImplementation : class, IUseCase<TRequest, TResponse>
    {
        ArgumentNullException.ThrowIfNull(services);

        // 1. 実装を登録
        services.AddScoped<TImplementation>();

        // 2. デコレーターをチェーン化してIUseCase<TRequest, TResponse>として登録
        services.AddScoped<IUseCase<TRequest, TResponse>>(provider =>
        {
            // 内側から外側へデコレーターをチェーン化
            // 1. 実装のインスタンス
            IUseCase<TRequest, TResponse> useCase = provider.GetRequiredService<TImplementation>();

            // 2. LoggingDecorator を追加
            var loggingLogger = provider.GetRequiredService<IAppLogging<LoggingDecorator<TRequest, TResponse>>>();
            var correlationContext = provider.GetRequiredService<ICorrelationContext>();
            useCase = new LoggingDecorator<TRequest, TResponse>(useCase, loggingLogger, correlationContext);

            // 3. PerformanceDecorator を追加
            var performanceLogger = provider.GetRequiredService<IAppLogging<PerformanceDecorator<TRequest, TResponse>>>();
            var clock = provider.GetRequiredService<IClock>();
            useCase = new PerformanceDecorator<TRequest, TResponse>(useCase, performanceLogger, correlationContext, clock, warningThresholdMs);

            // 4. ErrorHandlingDecorator を追加（最外側）
            var errorLogger = provider.GetRequiredService<IAppLogging<ErrorHandlingDecorator<TRequest, TResponse>>>();
            useCase = new ErrorHandlingDecorator<TRequest, TResponse>(useCase, errorLogger, correlationContext);

            return useCase;
        });

        return services;
    }

    /// <summary>
    /// ロギング機能のみを持つUseCaseの登録。
    /// PerformanceDecoratorやErrorHandlingDecoratorは不要な場合の使用
    /// </summary>
    /// <typeparam name="TRequest">ユースケースのリクエストの型</typeparam>
    /// <typeparam name="TResponse">ユースケースのレスポンスの型</typeparam>
    /// <typeparam name="TImplementation">ユースケースの実装の型</typeparam>
    /// <param name="services">登録先のサービスコレクション</param>
    /// <returns>メソッドチェーン用の <paramref name="services"/> 自身</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> が <see langword="null"/> の場合</exception>
    public static IServiceCollection RegisterUseCaseWithLogging<TRequest, TResponse, TImplementation>(
        this IServiceCollection services)
        where TRequest : IRequest
        where TResponse : IResponse
        where TImplementation : class, IUseCase<TRequest, TResponse>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<TImplementation>();

        services.AddScoped<IUseCase<TRequest, TResponse>>(provider =>
        {
            IUseCase<TRequest, TResponse> useCase = provider.GetRequiredService<TImplementation>();

            var loggingLogger = provider.GetRequiredService<IAppLogging<LoggingDecorator<TRequest, TResponse>>>();
            var correlationContext = provider.GetRequiredService<ICorrelationContext>();
            useCase = new LoggingDecorator<TRequest, TResponse>(useCase, loggingLogger, correlationContext);

            return useCase;
        });

        return services;
    }

    /// <summary>
    /// エラーハンドリング機能のみを持つUseCaseの登録。
    /// 軽量な登録が必要な場合の使用
    /// </summary>
    /// <typeparam name="TRequest">ユースケースのリクエストの型</typeparam>
    /// <typeparam name="TResponse">ユースケースのレスポンスの型</typeparam>
    /// <typeparam name="TImplementation">ユースケースの実装の型</typeparam>
    /// <param name="services">登録先のサービスコレクション</param>
    /// <returns>メソッドチェーン用の <paramref name="services"/> 自身</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> が <see langword="null"/> の場合</exception>
    public static IServiceCollection RegisterUseCaseWithErrorHandling<TRequest, TResponse, TImplementation>(
        this IServiceCollection services)
        where TRequest : IRequest
        where TResponse : IResponse
        where TImplementation : class, IUseCase<TRequest, TResponse>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<TImplementation>();

        services.AddScoped<IUseCase<TRequest, TResponse>>(provider =>
        {
            IUseCase<TRequest, TResponse> useCase = provider.GetRequiredService<TImplementation>();

            var errorLogger = provider.GetRequiredService<IAppLogging<ErrorHandlingDecorator<TRequest, TResponse>>>();
            var correlationContext = provider.GetRequiredService<ICorrelationContext>();
            useCase = new ErrorHandlingDecorator<TRequest, TResponse>(useCase, errorLogger, correlationContext);

            return useCase;
        });

        return services;
    }

    /// <summary>
    /// ロギングとパフォーマンス計測機能を持つUseCaseの登録。
    /// エラーハンドリングは不要な場合の使用
    /// </summary>
    /// <typeparam name="TRequest">ユースケースのリクエストの型</typeparam>
    /// <typeparam name="TResponse">ユースケースのレスポンスの型</typeparam>
    /// <typeparam name="TImplementation">ユースケースの実装の型</typeparam>
    /// <param name="services">登録先のサービスコレクション</param>
    /// <param name="warningThresholdMs"><see cref="PerformanceDecorator{TRequest, TResponse}"/> が Warning ログを出力する実行時間のしきい値（ミリ秒）</param>
    /// <returns>メソッドチェーン用の <paramref name="services"/> 自身</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> が <see langword="null"/> の場合</exception>
    /// <remarks>
    /// <para>【チェーン順序】（外側から）PerformanceDecorator → LoggingDecorator → 実装</para>
    /// </remarks>
    public static IServiceCollection RegisterUseCaseWithPerformance<TRequest, TResponse, TImplementation>(
        this IServiceCollection services,
        int warningThresholdMs = 1000)
        where TRequest : IRequest
        where TResponse : IResponse
        where TImplementation : class, IUseCase<TRequest, TResponse>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<TImplementation>();

        services.AddScoped<IUseCase<TRequest, TResponse>>(provider =>
        {
            IUseCase<TRequest, TResponse> useCase = provider.GetRequiredService<TImplementation>();

            // LoggingDecorator を追加
            var loggingLogger = provider.GetRequiredService<IAppLogging<LoggingDecorator<TRequest, TResponse>>>();
            var correlationContext = provider.GetRequiredService<ICorrelationContext>();
            useCase = new LoggingDecorator<TRequest, TResponse>(useCase, loggingLogger, correlationContext);

            // PerformanceDecorator を追加
            var performanceLogger = provider.GetRequiredService<IAppLogging<PerformanceDecorator<TRequest, TResponse>>>();
            var clock = provider.GetRequiredService<IClock>();
            useCase = new PerformanceDecorator<TRequest, TResponse>(useCase, performanceLogger, correlationContext, clock, warningThresholdMs);

            return useCase;
        });

        return services;
    }

    /// <summary>
    /// デコレーターなしでUseCaseの直接登録。
    /// テストやシンプルな実装で不要な場合の使用
    /// </summary>
    /// <typeparam name="TRequest">ユースケースのリクエストの型</typeparam>
    /// <typeparam name="TResponse">ユースケースのレスポンスの型</typeparam>
    /// <typeparam name="TImplementation">ユースケースの実装の型</typeparam>
    /// <param name="services">登録先のサービスコレクション</param>
    /// <returns>メソッドチェーン用の <paramref name="services"/> 自身</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> が <see langword="null"/> の場合</exception>
    public static IServiceCollection RegisterUseCase<TRequest, TResponse, TImplementation>(
        this IServiceCollection services)
        where TRequest : IRequest
        where TResponse : IResponse
        where TImplementation : class, IUseCase<TRequest, TResponse>
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<TImplementation>();
        services.AddScoped<IUseCase<TRequest, TResponse>, TImplementation>();

        return services;
    }
}
