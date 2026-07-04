using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Crosscutting.Logging;


namespace SupportAdvance.Crosscutting
{
    public static class DependencyInjection
    {
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
