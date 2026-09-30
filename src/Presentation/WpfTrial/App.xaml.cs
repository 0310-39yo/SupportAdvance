using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SupportAdvance.Application;
using SupportAdvance.Application.Abstractions.Services;
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
using SupportAdvance.Presentation.Shared;
using SupportAdvance.Presentation.Shared.Services;
using SupportAdvance.Presentation.WpfTrial.Views;
using Syncfusion.SfSkinManager;
using System.Windows;

namespace SupportAdvance.Presentation.WpfTrial;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
/// <remarks>
/// <para>【責務】Composition Root</para>
/// <list type="bullet">
/// <item><description>ホスト（DI コンテナ）の構築</description></item>
/// <item><description>ログインウィンドウ → メインウィンドウの起動フロー制御</description></item>
/// <item><description>Infrastructure への参照はこのファイル（Composition Root）内でのみ許可</description></item>
/// </list>
/// </remarks>
public partial class App : System.Windows.Application
{
    private IHost? _host;

    /// <summary>
    /// DI ホストの構築と、ログインウィンドウ → メインウィンドウの起動
    /// </summary>
    /// <param name="e">起動イベントの引数</param>
    /// <remarks>
    /// <para>【処理の流れ】</para>
    /// <list type="number">
    /// <item><description>DI ホストの構築と開始（Composition Root）</description></item>
    /// <item><description>BusinessDayClockの場合は業務日の開始（ON）</description></item>
    /// <item><description><see cref="LoginWindow"/> のモーダル表示</description></item>
    /// <item><description>ログイン成功の場合は <see cref="MainWindow"/> の表示、キャンセルの場合はアプリケーションの終了</description></item>
    /// </list>
    /// <para>【注意】ログイン中は <see cref="ShutdownMode.OnExplicitShutdown"/>。ログインウィンドウを閉じた時点でのアプリケーション終了の防止</para>
    /// </remarks>
    protected override void OnStartup(StartupEventArgs e)
    {
        DebugConsoleHelper.OpenConsoleForDebug();

        // Syncfusion ライセンスキーを環境変数から登録
        SyncfusionLicenseHelper.RegisterLicenseFromEnvironment();

        // アプリケーション全体（すべてのウィンドウ）に Office2019Colorful テーマを適用
        SfSkinManager.ApplicationTheme = new Theme("Office2019Colorful");

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
                    // 【重要】Singleton とする。ログイン画面（Scope A）で設定した情報を、
                    // アプリケーション終了時（別の Scope B）から参照する必要があるため。
                    // Scoped にすると Scope ごとに別インスタンスとなり、ログイン情報が引き継がれない
                    .AddSingleton<ICurrentUserService, RealCurrentUserService>();
            })
            .Build();

        _host.Start();

        // BusinessDayClockの場合のみ、起動を業務日の開始とする（それ以外は未登録で何もしない）
        _host.Services.GetService<IBusinessDayClockControl>()?.TurnOn();

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

    /// <summary>
    /// ログアウト処理、BusinessDayClockの終了（OFF）、DI ホストの停止と破棄
    /// </summary>
    /// <param name="e">終了イベントの引数</param>
    /// <remarks>
    /// <para>【処理の流れ】</para>
    /// <list type="number">
    /// <item><description>ログイン中のセッションをログアウト（LogoutUseCase で logged_out_at を DB に記録）</description></item>
    /// <item><description>BusinessDayClockの場合は業務日の終了（OFF）</description></item>
    /// <item><description>DI ホストを停止</description></item>
    /// <item><description>リソースを解放</description></item>
    /// </list>
    /// </remarks>
    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            // ステップ1: ログアウト処理（セッションのログアウト日時を記録）
            using (var scope = _host?.Services.CreateScope())
            {
                var currentUser = scope?.ServiceProvider.GetService<ICurrentUserService>();

                if (currentUser?.IsAuthenticated == true)
                {
                    try
                    {
                        var sessionRowId = UserAuthSessionRowId.From(currentUser.CurrentUserSessionRowId);
                        var logoutUseCase = scope?.ServiceProvider.GetService<LogoutUseCase>();
                        if (logoutUseCase != null)
                        {
                            // 【重要】UI スレッドから直接 .GetAwaiter().GetResult() すると、
                            // ExecuteAsync 内部の await が UI スレッドの SynchronizationContext に
                            // 戻ろうとしてデッドロックする。Task.Run でスレッドプール上で実行し回避
                            Task.Run(() => logoutUseCase.ExecuteAsync(sessionRowId)).GetAwaiter().GetResult();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Logout failed: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Logout initialization failed: {ex.Message}");
        }

        try
        {
            // ステップ2: BusinessDayClockの場合のみ、終了を業務日の終了とする
            _host?.Services.GetService<IBusinessDayClockControl>()?.TurnOff();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"BusinessDayClock TurnOff failed: {ex.Message}");
        }

        try
        {
            // ステップ3: DI ホストの停止と破棄
            _host?.StopAsync().GetAwaiter().GetResult();
            _host?.Dispose();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Host cleanup failed: {ex.Message}");
        }

        base.OnExit(e);
    }
}
