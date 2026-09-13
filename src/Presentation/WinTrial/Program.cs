using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SupportAdvance.Application;
using SupportAdvance.Common.Configuration;
using SupportAdvance.Contexts.Department.Application;
using SupportAdvance.Contexts.Department.Infrastructure;
using SupportAdvance.Contexts.Employee.Application;
using SupportAdvance.Contexts.Employee.Infrastructure;
using SupportAdvance.Contexts.Identity.Application;
using SupportAdvance.Contexts.Identity.Infrastructure;
using SupportAdvance.Contexts.IntegrationPrototype.Application;
using SupportAdvance.Crosscutting;
using SupportAdvance.Infrastructure;
using SupportAdvance.Infrastructure.Services;
using SupportAdvance.Presentation.Shared;
using SupportAdvance.Presentation.WinTrial.Forms;
using SupportAdvance.Presentation.WinTrial.Services;
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

        // Syncfusion ライセンスキーを環境変数から登録
        SyncfusionLicenseHelper.RegisterLicenseFromEnvironment();

        using var host = HostBuilderFactory.Create(null, (context, services) =>
            {
                services
                    .AddCrosscuttingModels(context.Configuration)
                    .AddInfrastructureModels(context.Configuration)
                    .AddEmployeeInfrastructureModels() // Employee Context Infrastructure を登録
                    .AddDepartmentInfrastructureModels() // Department Context Infrastructure を登録
                    .AddIdentityInfrastructureModels() // Identity Context Infrastructure を登録
                    .AddApplicationModels()
                    .AddEmployeeApplicationModels() // Employee Context Application を登録
                    .AddDepartmentApplicationModels() // Department Context Application を登録
                    .AddIdentityApplicationModels() // Identity Context Application を登録
                    .AddIntegrationPrototypeApplicationModels() // IntegrationPrototype Application を登録
                    .AddWinTrialModules()
                    // Identity BC 実装により RealCurrentUserService に切り替え
                    .AddScoped<ICurrentUserService, RealCurrentUserService>()
                    // LoginDialog と依存関係を DI 登録
                    .AddScoped<LoginDialog>()
                    ;
            })
            .Build();

        host.Start();

        using var scope = host.Services.CreateScope();
        var loginDialog = ServiceProviderServiceExtensions.GetRequiredService<LoginDialog>(scope.ServiceProvider);
        var dialogResult = loginDialog.ShowDialog();

        if (dialogResult == DialogResult.OK)
        {
            // ログイン成功時は Form1 を表示
            var mainForm = ServiceProviderServiceExtensions.GetRequiredService<Form1>(scope.ServiceProvider) ??
                           ActivatorUtilities.CreateInstance<Form1>(scope.ServiceProvider);
            System.Windows.Forms.Application.Run(mainForm);
        }
        // ログインキャンセル時はアプリを終了

        host.StopAsync().GetAwaiter().GetResult();
    }
}
