using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Common.Configuration;
using SupportAdvance.Contexts.IntegrationPrototype.Application.UseCases;
using SupportAdvance.Crosscutting.Logging;
using SupportAdvance.Presentation.Shared.ViewModels;
using SupportAdvance.Presentation.WpfTrial.ViewModels.Models;
using SupportAdvance.Presentation.WpfTrial.Views;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace SupportAdvance.Presentation.WpfTrial.ViewModels;

/// <summary>
/// メインウィンドウ ViewModel（MVVM Toolkit）
/// 
/// 責務:
/// - BizId 検索入力の状態管理
/// - GetEmployeeByBizIdIntegrationUseCase の実行
/// - 検索結果（従業員氏名・所属部署）の保持
/// - タブ管理（開いているタブのリスト、選択タブの追跡）
/// - ナビゲーション選択→タブ追加の処理
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly AppSettings _appSettings;
    private readonly IClock _clock;
    private readonly IAppLogging<MainWindowViewModel> _logger;
    private readonly GetEmployeeByBizIdIntegrationUseCase _getEmployeeByBizIdUseCase;

    /// <summary>
    /// BizId 検索欄の入力値
    /// </summary>
    [ObservableProperty]
    private string _bizIdSearchInput = string.Empty;

    /// <summary>
    /// 検索結果の従業員氏名
    /// </summary>
    [ObservableProperty]
    private string _employeeFullName = string.Empty;

    /// <summary>
    /// 検索結果の従業員の所属部署名
    /// </summary>
    [ObservableProperty]
    private string _departmentNames = string.Empty;

    /// <summary>
    /// 現在開いているタブのリスト
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<TabItemData> _openTabs = new();

    /// <summary>
    /// 現在選択中のタブ
    /// </summary>
    [ObservableProperty]
    private TabItemData? _selectedTab;

    /// <summary>
    /// BusinessDayClockの操作パネルの ViewModel
    /// </summary>
    public BusinessDayClockViewModel BusinessDayClock { get; }

    /// <summary>
    /// MainWindowViewModel の新しいインスタンスの初期化
    /// </summary>
    public MainWindowViewModel(
        IAppLogging<MainWindowViewModel> logger,
        IAppSettings appSettings,
        IClock clock,
        GetEmployeeByBizIdIntegrationUseCase getEmployeeByBizIdUseCase,
        BusinessDayClockViewModel businessDayClock)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(appSettings);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(getEmployeeByBizIdUseCase);
        ArgumentNullException.ThrowIfNull(businessDayClock);

        BusinessDayClock = businessDayClock;
        _logger = logger;
        _appSettings = (AppSettings)appSettings;
        _clock = clock;
        _getEmployeeByBizIdUseCase = getEmployeeByBizIdUseCase;

        _logger.LogInformation("MainWindowViewModel initialized.");
        _logger.LogInformation(_appSettings.ApplicationBuildType ?? "Unknown");
    }

    /// <summary>
    /// BizId で従業員を検索
    /// </summary>
    [RelayCommand]
    public async Task SearchEmployeeByBizId()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(BizIdSearchInput))
            {
                EmployeeFullName = string.Empty;
                _logger.LogInformation("BizId search input is empty.");
                return;
            }

            if (!int.TryParse(BizIdSearchInput, out var bizId))
            {
                EmployeeFullName = "入力エラー：BizId は数値で入力してください";
                _logger.LogWarning($"Invalid BizId format: {BizIdSearchInput}");
                return;
            }

            _logger.LogInformation($"Searching employee by BizId: {bizId}");
            var employee = await _getEmployeeByBizIdUseCase.ExecuteAsync(bizId);

            if (employee == null)
            {
                EmployeeFullName = "従業員が見つかりません";
                DepartmentNames = string.Empty;
                _logger.LogInformation($"No employee found with BizId: {bizId}");
                return;
            }

            EmployeeFullName = $"{employee.PersonLastName} {employee.PersonFirstName}";
            DepartmentNames = employee.DepartmentNames;
            _logger.LogInformation($"Employee found: {EmployeeFullName}, Departments: {DepartmentNames}");
        }
        catch (Exception ex)
        {
            EmployeeFullName = "エラーが発生しました";
            DepartmentNames = string.Empty;
            _logger.LogError("SearchEmployeeByBizId execution failed", ex);
        }
    }

    /// <summary>
    /// サンプル実行コマンド
    /// </summary>
    [RelayCommand]
    public void ExecuteSampleUseCase()
    {
        try
        {
            _logger.LogInformation($"[テストボタンクリック] 現在時刻: {_clock.JstNow}");
        }
        catch (Exception ex)
        {
            _logger.LogError("SampleUseCase execution failed", ex);
            throw;
        }
    }

    /// <summary>
    /// ナビゲーション項目が選択された時の処理
    /// </summary>
    [RelayCommand]
    public void NavigationItemSelected(object? parameter)
    {
        if (parameter is not Syncfusion.Windows.Tools.Controls.GroupViewItem groupViewItem)
        {
            return;
        }

        var itemText = groupViewItem.Text;
        if (string.IsNullOrEmpty(itemText))
        {
            return;
        }

        // 既に開いているタブがあればそれを選択
        var existingTab = _openTabs.FirstOrDefault(t => t.Header == itemText);
        if (existingTab != null)
        {
            _selectedTab = existingTab;
            OnPropertyChanged("SelectedTab");
            return;
        }

        // 新規にタブを作成
        var newTab = new TabItemData
        {
            Header = itemText,
            Content = new EmptyView()
        };

        _openTabs.Add(newTab);
        _selectedTab = newTab;
        OnPropertyChanged("SelectedTab");

        _logger.LogInformation($"Tab opened: {itemText}");
    }

    /// <summary>
    /// タブ選択が変更された時の処理
    /// </summary>
    [RelayCommand]
    public void TabSelectionChanged(object? parameter)
    {
        var selectedTabText = _selectedTab?.Header ?? "(none)";
        _logger.LogInformation($"Tab selection changed to: {selectedTabText}");
    }
}
