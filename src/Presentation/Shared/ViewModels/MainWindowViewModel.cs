using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SupportAdvance.Common.Configuration;
using SupportAdvance.Crosscutting.Logging;
using SupportAdvance.Presentation.Shared.ViewModels.Tabs;
using System.Collections.ObjectModel;

namespace SupportAdvance.Presentation.Shared.ViewModels;

/// <summary>
/// メインウィンドウ ViewModel（MVVM Toolkit）
/// </summary>
/// <remarks>
/// <para>【責務】タブ管理（開いているタブのリスト、選択タブの追跡）、ナビゲーション選択からのタブ追加、タブが無いときのオープニング画面の表示の切り替え</para>
/// <para>【設計】View 型・UI コントロール型への依存なし。タブの画面は各 <see cref="TabItemViewModel.ContentViewModel"/> の型に対応する DataTemplate で決定</para>
/// </remarks>
public partial class MainWindowViewModel : ObservableObject
{
    /// <summary>
    /// 顧客一覧のナビゲーション項目の名前（仮として <see cref="Form1ViewModel"/> の画面を表示する項目）
    /// </summary>
    public const string CustomerListMenuName = "顧客一覧";

    private readonly IAppLogging<MainWindowViewModel> _logger;
    private readonly Form1ViewModel _form1ViewModel;

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
    /// <param name="form1ViewModel">顧客一覧のタブ（仮）に表示する画面の ViewModel</param>
    /// <param name="appIdentity">起動しているアプリケーションの識別情報（オープニング画面に表示する名前）</param>
    /// <exception cref="ArgumentNullException">いずれかの引数が <see langword="null"/> の場合</exception>
    public MainWindowViewModel(
        IAppLogging<MainWindowViewModel> logger,
        IAppSettings appSettings,
        Form1ViewModel form1ViewModel,
        AppIdentity appIdentity)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(appSettings);
        ArgumentNullException.ThrowIfNull(form1ViewModel);
        ArgumentNullException.ThrowIfNull(appIdentity);

        _logger = logger;
        _form1ViewModel = form1ViewModel;
        Opening = new OpeningViewModel(appIdentity);

        // タブの有無が変わったら、タブ領域とオープニング画面の表示の切り替えを通知する
        OpenTabs.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasOpenTabs));
            OnPropertyChanged(nameof(IsOpeningViewVisible));
        };

        _logger.LogInformation("MainWindowViewModel initialized.");
        _logger.LogInformation(appSettings.ApplicationBuildType);
    }

    /// <summary>
    /// 現在開いているタブのリスト
    /// </summary>
    public ObservableCollection<TabItemViewModel> OpenTabs { get; } = new();

    /// <summary>
    /// オープニング画面の ViewModel
    /// </summary>
    public OpeningViewModel Opening { get; }

    /// <summary>
    /// タブが 1 つ以上開いているかどうかを示す値（タブ領域の表示に使う）
    /// </summary>
    public bool HasOpenTabs => OpenTabs.Count > 0;

    /// <summary>
    /// オープニング画面を表示するかどうかを示す値
    /// </summary>
    /// <value>タブが 1 つも開いていない場合（起動直後、または最後のタブを閉じた後）は <see langword="true"/></value>
    public bool IsOpeningViewVisible => !HasOpenTabs;

    /// <summary>
    /// 指定した名前のタブを開く
    /// </summary>
    /// <param name="menuName">ナビゲーション項目の名前（タブのヘッダーにもなる）</param>
    /// <remarks>
    /// <para>【動作】同じ名前のタブが既に開いていればそのタブを選択。なければ新しいタブを追加して選択</para>
    /// <para>【注意】顧客一覧のタブは <see cref="Form1ViewModel"/> を共有する（同名のタブは 1 つしか開かないため）</para>
    /// <para>【注意】<paramref name="menuName"/> が <see langword="null"/> または空文字の場合は処理なし</para>
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

        var newTab = new TabItemViewModel(menuName, CreateContentViewModel(menuName));
        OpenTabs.Add(newTab);
        SelectedTab = newTab;

        _logger.LogInformation($"Tab opened: {menuName}");
    }

    /// <summary>
    /// ナビゲーション項目に対応するタブ内容の ViewModel の生成
    /// </summary>
    /// <param name="menuName">ナビゲーション項目の名前</param>
    /// <returns>顧客一覧は <see cref="Form1ViewModel"/>（仮）。それ以外は、画面ごとの ViewModel が用意できるまでのプレースホルダー</returns>
    private object CreateContentViewModel(string menuName)
        => menuName == CustomerListMenuName ? _form1ViewModel : new EmptyTabContentViewModel();

    /// <summary>
    /// 指定したタブを閉じる
    /// </summary>
    /// <param name="tab">閉じるタブ</param>
    /// <remarks>
    /// <para>【動作】閉じたタブが選択中だった場合は、残っている最後のタブを選択（タブが無くなれば選択なし）</para>
    /// <para>【注意】<paramref name="tab"/> が <see langword="null"/> または開いていないタブの場合は処理なし</para>
    /// <para>【用途】ItemsSource／SelectedItem を自動同期できない UI（WinForms）が、タブの終了を ViewModel に反映するために使用</para>
    /// </remarks>
    [RelayCommand]
    private void CloseTab(TabItemViewModel? tab)
    {
        if (tab is null || !OpenTabs.Remove(tab))
        {
            return;
        }

        if (ReferenceEquals(SelectedTab, tab))
        {
            SelectedTab = OpenTabs.LastOrDefault();
        }

        _logger.LogInformation($"Tab closed: {tab.Header}");
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
