using System;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SupportAdvance.Application.UseCases;
using SupportAdvance.Crosscutting.Logging;

namespace SupportAdvance.Presentation.Shared
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSharedPresentationModels(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            // ここで、Shared Presentation に関連するサービスを登録します。
            // 例: services.AddScoped<ISharedService, SharedService>();
            return services;
        }

        private static void RegisterUseCaseWithLogging<TRequest, TResponse, TImplementation>(IServiceCollection services)
            where TRequest : IRequest
            where TResponse : IResponse
            where TImplementation : class, IUseCase<TRequest, TResponse>
        {
            services.AddScoped<TImplementation>();

            services.AddScoped<IUseCase<TRequest, TResponse>>(provider =>
            {
                var implementation = provider.GetRequiredService<TImplementation>();
               // var logger = provider.GetRequiredService<IAppLogging<LoggingDe>.ILogger<TImplementation>>();
                // ここで、ロギングやその他のデコレーションを追加することができます。
                return implementation;
            });

            // ここで、Shared Presentation に関連するサービスを登録します。
            // 例: services.AddScoped<ISharedService, SharedService>();
        }
    }
}
