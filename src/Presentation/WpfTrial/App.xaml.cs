using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SupportAdvance.Application;
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
using SupportAdvance.Infrastructure.Services;
using SupportAdvance.Presentation.Shared;
using SupportAdvance.Presentation.WpfTrial.Services;
using SupportAdvance.Presentation.WpfTrial.Views;

namespace SupportAdvance.Presentation.WpfTrial;

/// <summary>
/// Interaction logic for App.xaml
///
/// 【責務】Composition Root
/// - ホスト（DI コンテナ）の構築
/// - ログインウィンドウ → メインウィンドウの起動フロー制御
/// - Infrastructure への参照はこのファイル（Composition Root）内でのみ許可
/// </summary>
public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        DebugConsoleHelper.OpenConsoleForDebug();

        // Syncfusion ライセンスキーを環境変数から登録
        SyncfusionLicenseHelper.RegisterLicenseFromEnvironment();

        _host = HostBuilderFactory.Create(null, (context, services) =>
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
                    .AddWpfTrialModules()
                    // Authentication BC 実装により RealCurrentUserService に切り替え
                    .AddScoped<ICurrentUserService, RealCurrentUserService>();
            })
            .Build();

        _host.Start();

        // LoginWindow は ShowDialog() 中は唯一のウィンドウになるため、
        // 既定の ShutdownMode（OnLastWindowClose）のままだとログイン成功時に
        // LoginWindow を閉じた瞬間（MainWindow.Show() より前）にアプリが終了してしまう。
        // MainWindow 表示まではアプリ終了を抑止する。
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var scope = _host.Services.CreateScope();
        var loginWindow = scope.ServiceProvider.GetRequiredService<LoginWindow>();
        var loginResult = loginWindow.ShowDialog();

        if (loginResult == true)
        {
            // ログイン成功時は MainWindow を表示
            var mainWindow = scope.ServiceProvider.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            mainWindow.Show();
        }
        else
        {
            // ログインキャンセル時はアプリを終了
            Shutdown();
            return;
        }

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.StopAsync().GetAwaiter().GetResult();
        _host?.Dispose();
        base.OnExit(e);
    }
}
