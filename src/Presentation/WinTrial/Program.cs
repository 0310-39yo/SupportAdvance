using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SupportAdvance.Application;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Common.Configuration;
using SupportAdvance.Contexts.Authentication.Application;
using SupportAdvance.Contexts.Authentication.Application.Repositories;
using SupportAdvance.Contexts.Authentication.Application.UseCases;
using SupportAdvance.Contexts.Authentication.Domain.ValueObjects;
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
using SupportAdvance.Presentation.Shared.Services;
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
                    // 【重要】Singleton とする。ログイン画面（Scope A）で設定した情報を、
                    // アプリケーション終了時（別の Scope B）から参照する必要があるため。
                    // Scoped にすると Scope ごとに別インスタンスとなり、ログイン情報が引き継がれない
                    .AddSingleton<ICurrentUserService, RealCurrentUserService>();
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

            // ステップ1: ログアウト処理（セッションのログアウト日時を記録）
            using (var logoutScope = host.Services.CreateScope())
            {
                try
                {
                    var currentUser = ServiceProviderServiceExtensions.GetService<ICurrentUserService>(logoutScope.ServiceProvider);

                    if (currentUser?.IsAuthenticated == true)
                    {
                        try
                        {
                            var sessionRowId = UserAuthSessionRowId.From(currentUser.CurrentUserSessionRowId);
                            var logoutUseCase = ServiceProviderServiceExtensions.GetService<LogoutUseCase>(logoutScope.ServiceProvider);
                            if (logoutUseCase != null)
                            {
                                // 【重要】UI スレッドから直接 .GetAwaiter().GetResult() すると、
                                // ExecuteAsync 内部の await が UI スレッドの SynchronizationContext に
                                // 戻ろうとしてデッドロックする可能性がある。Task.Run で回避
                                Task.Run(() => logoutUseCase.ExecuteAsync(sessionRowId)).GetAwaiter().GetResult();
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Logout failed: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Logout initialization failed: {ex.Message}");
                }
            }
        }
        // ログインキャンセル時はアプリを終了

        try
        {
            // ステップ2: BusinessDayClockの場合のみ、終了を業務日の終了とする
            businessDayClock?.TurnOff();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"BusinessDayClock TurnOff failed: {ex.Message}");
        }

        try
        {
            // ステップ3: DI ホストの停止
            host.StopAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Host cleanup failed: {ex.Message}");
        }
    }
}
