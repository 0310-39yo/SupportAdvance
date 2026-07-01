using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SupportAdvance.Application;
using SupportAdvance.Common.Configuration;
using SupportAdvance.Contexts.Samples.CarPreferences.Application;
using SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure;
using SupportAdvance.Crosscutting;
using SupportAdvance.Infrastructure;
using SupportAdvance.Presentation.Shared;
using SupportAdvance.Presentation.WinTrial.Views;

namespace SupportAdvance.Presentation.WinTrial;

internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        DebugConsoleHelper.OpenConsoleForDebug();
        ApplicationConfiguration.Initialize();

        using var host = HostBuilderFactory.Create(null, (context, services) =>
            {
                services
                    .AddCrosscuttingModels(context.Configuration)
                    .AddInfrastructureModels(context.Configuration)
                    .AddCarPreferencesInfrastructureModels()
                    .AddApplicationModels()
                    .AddCarPreferencesApplicationModels()
                    .AddWinTrialModules()
                    ;
            })
            .Build();

        host.Start();

        using var scope = host.Services.CreateScope();
        var mainForm = scope.ServiceProvider.GetRequiredService<Form1>() ??
                       ActivatorUtilities.CreateInstance<Form1>(scope.ServiceProvider);
        System.Windows.Forms.Application.Run(mainForm);
        host.StopAsync().GetAwaiter().GetResult();
    }
}
