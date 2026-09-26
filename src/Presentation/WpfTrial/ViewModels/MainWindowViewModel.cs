using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SupportAdvance.Common.Configuration;
using SupportAdvance.Crosscutting.Logging;
using SupportAdvance.Presentation.WpfTrial.ViewModels.Tabs;
using System.Collections.ObjectModel;

namespace SupportAdvance.Presentation.WpfTrial.ViewModels;

/// <summary>
/// メインウィンドウ ViewModel（MVVM Toolkit）
/// </summary>
/// <remarks>
/// <para>【責務】タブ管理（開いているタブのリスト、選択タブの追跡）、ナビゲーション選択からのタブ追加</para>
/// <para>【設計】View 型・UI コントロール型には依存しない。タブの画面は各 <see cref="TabItemViewModel.ContentViewModel"/> の型に対応する DataTemplate で決まる</para>
/// </remarks>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IAppLogging<MainWindowViewModel> _logger;

    /// <summary>
    /// 現在選択中のタブ
    /// </summary>
    [ObservableProperty]
    private TabItemViewModel? _selectedTab;

    /// <summary>
    /// <see cref="MainWindowViewModel"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="logger">ログの出力先</param>
    /// <param name="appSettings">アプリケーション設定</param>
    /// <exception cref="ArgumentNullException">いずれかの引数が <see langword="null"/> の場合</exception>
    public MainWindowViewModel(
        IAppLogging<MainWindowViewModel> logger,
        IAppSettings appSettings)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(appSettings);

        _logger = logger;

        _logger.LogInformation("MainWindowViewModel initialized.");
        _logger.LogInformation(appSettings.ApplicationBuildType);
    }

    /// <summary>
    /// 現在開いているタブのリスト
    /// </summary>
    public ObservableCollection<TabItemViewModel> OpenTabs { get; } = new();

    /// <summary>
    /// 指定した名前のタブを開く
    /// </summary>
    /// <param name="menuName">ナビゲーション項目の名前（タブのヘッダーにもなる）</param>
    /// <remarks>
    /// <para>【動作】同じ名前のタブが既に開いていればそのタブを選択。なければ新しいタブを追加して選択</para>
    /// <para>【注意】<paramref name="menuName"/> が <see langword="null"/> または空文字の場合は何もしない</para>
    /// </remarks>
    [RelayCommand]
    private void OpenTab(string? menuName)
    {
        if (string.IsNullOrEmpty(menuName))
        {
            return;
        }

        var existingTab = OpenTabs.FirstOrDefault(t => t.Header == menuName);
        if (existingTab is not null)
        {
            SelectedTab = existingTab;
            return;
        }

        // 画面ごとの ViewModel が用意できるまでは、未実装画面のプレースホルダーを表示
        var newTab = new TabItemViewModel(menuName, new EmptyTabContentViewModel());
        OpenTabs.Add(newTab);
        SelectedTab = newTab;

        _logger.LogInformation($"Tab opened: {menuName}");
    }

    /// <summary>
    /// 保存コマンド（リボンの Save ボタン用のプレースホルダー）
    /// </summary>
    /// <remarks>
    /// <para>【注意】保存対象の画面は未実装のため、ログ出力のみ</para>
    /// </remarks>
    [RelayCommand]
    private void Save()
    {
        _logger.LogInformation("Save is not implemented yet.");
    }

    /// <summary>
    /// 選択中のタブが変わった時のログ出力
    /// </summary>
    /// <param name="value">新しく選択されたタブ。選択なしの場合は <see langword="null"/></param>
    partial void OnSelectedTabChanged(TabItemViewModel? value)
    {
        _logger.LogInformation($"Tab selection changed to: {value?.Header ?? "(none)"}");
    }
}
