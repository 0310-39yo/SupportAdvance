using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Crosscutting.Logging;


namespace SupportAdvance.Crosscutting
{
    /// <summary>
    /// Crosscutting 層（ロギングなどの横断的関心事）のサービスを DI コンテナーに登録する拡張メソッド群
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Crosscutting 層のサービスの、DI コンテナーへの登録
        /// </summary>
        /// <param name="services">登録先のサービスコレクション</param>
        /// <param name="configuration">アプリケーションの構成（現在は未使用。将来のロギング設定用）</param>
        /// <returns>メソッドチェーン用の <paramref name="services"/> 自身</returns>
        /// <exception cref="ArgumentNullException"><paramref name="services"/> または <paramref name="configuration"/> が <see langword="null"/> の場合</exception>
        /// <remarks>
        /// <para>【登録内容】<see cref="IAppLogging{T}"/> → <see cref="FrameworkLoggingAdapter{T}"/>（Scoped）、
        /// <see cref="ICorrelationContext"/> → <see cref="CorrelationContext"/>（Scoped）</para>
        /// </remarks>
        public static IServiceCollection AddCrosscuttingModels(this IServiceCollection services,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            // ロギング実装の登録
            services.AddScoped(typeof(IAppLogging<>), typeof(FrameworkLoggingAdapter<>));

            // CorrelationContextの登録
            services.AddScoped<ICorrelationContext, CorrelationContext>();

            return services;
        }
    }
}
