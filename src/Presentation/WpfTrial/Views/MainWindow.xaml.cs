using System.Windows;
using SupportAdvance.Common.Configuration;
using SupportAdvance.Crosscutting.Logging;
using SupportAdvance.Presentation.WpfTrial.ViewModels;

namespace SupportAdvance.Presentation.WpfTrial.Views;

/// <summary>
/// メインウィンドウ（WPF + MVVM Toolkit）
///
/// 【責務】
/// - ViewModel（MainWindowViewModel）を DataContext に設定するのみ
/// - UI ロジックは XAML の Binding / Command に委譲（コードビハインドは配線のみ）
/// </summary>
public partial class MainWindow : Window
{
    private readonly IAppLogging<MainWindow> _logger;

    /// <summary>
    /// <see cref="MainWindow"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="viewModel">DataContext に設定する ViewModel</param>
    /// <param name="logger">ログの出力先</param>
    /// <param name="settings">アプリケーション設定（ビルド種別のログ出力用）</param>
    /// <exception cref="ArgumentNullException">いずれかの引数が <see langword="null"/> の場合</exception>
    public MainWindow(MainWindowViewModel viewModel, IAppLogging<MainWindow> logger, IAppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(settings);

        InitializeComponent();

        _logger = logger;
        DataContext = viewModel;

        _logger.LogInformation("MainWindow initialized.");
        _logger.LogInformation("情報");
        _logger.LogWarning("警告");
        _logger.LogInformation(settings.ApplicationBuildType ?? "Unknown");
    }
}
