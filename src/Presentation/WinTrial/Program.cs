using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SupportAdvance.Common.Configuration;
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
        DapperTypeHandlerRegistration.Register();

        using var host = HostBuilderFactory.Create(null, (context, services) => { services.AddWinTrialModules(); })
            .Build();

        host.Start();

        using var scope = host.Services.CreateScope();
        var mainForm = scope.ServiceProvider.GetRequiredService<Form1>() ??
                       ActivatorUtilities.CreateInstance<Form1>(scope.ServiceProvider);
        Application.Run(mainForm);
        host.StopAsync().GetAwaiter().GetResult();
    }
}
