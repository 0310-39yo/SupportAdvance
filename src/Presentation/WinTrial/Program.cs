using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SupportAdvance.Application;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Common.Configuration;
using SupportAdvance.Contexts.Authentication.Application;
using SupportAdvance.Contexts.Authentication.Infrastructure;
using SupportAdvance.Contexts.Department.Application;
using SupportAdvance.Contexts.Department.Infrastructure;
using SupportAdvance.Contexts.Employee.Application;
using SupportAdvance.Contexts.Employee.Infrastructure;
using SupportAdvance.Contexts.IntegrationPrototype.Application;
using SupportAdvance.Crosscutting;
using SupportAdvance.Infrastructure;
using SupportAdvance.Application.Abstractions.Services;
using SupportAdvance.Presentation.Shared;
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
                    .AddAuthenticationInfrastructureModels() // Authentication Context Infrastructure を登録
                    .AddApplicationModels()
                    .AddEmployeeApplicationModels() // Employee Context Application を登録
                    .AddDepartmentApplicationModels() // Department Context Application を登録
                    .AddAuthenticationApplicationModels() // Authentication Context Application を登録
                    .AddIntegrationPrototypeApplicationModels() // IntegrationPrototype Application を登録
                    .AddWinTrialModules()
                    // Authentication BC 実装により RealCurrentUserService に切り替え
                    .AddScoped<ICurrentUserService, RealCurrentUserService>();
            })
            .Build();

        host.Start();

        // BusinessDayClockの場合のみ、起動を業務日の開始とする（それ以外は未登録で何もしない）
        // Syncfusion の System.ServiceExtensions と拡張メソッド名が衝突するため、静的メソッドとして明示的に呼び出す
        var businessDayClock =
            ServiceProviderServiceExtensions.GetService<IBusinessDayClockControl>(host.Services);
        businessDayClock?.TurnOn();

        using var scope = host.Services.CreateScope();
        var loginDialog = ServiceProviderServiceExtensions.GetRequiredService<LoginDialog>(scope.ServiceProvider);
        var dialogResult = loginDialog.ShowDialog();

        if (dialogResult == DialogResult.OK)
        {
            // ログイン成功時は MainWindow を表示
            var mainForm = ServiceProviderServiceExtensions.GetRequiredService<MainWindow>(scope.ServiceProvider);
            System.Windows.Forms.Application.Run(mainForm);
        }
        // ログインキャンセル時はアプリを終了

        // BusinessDayClockの場合のみ、終了を業務日の終了とする
        businessDayClock?.TurnOff();

        host.StopAsync().GetAwaiter().GetResult();
    }
}
