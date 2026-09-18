using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Common.Configuration;
using SupportAdvance.Contexts.IntegrationPrototype.Application.UseCases;
using SupportAdvance.Crosscutting.Logging;

namespace SupportAdvance.Presentation.WpfTrial.ViewModels;

/// <summary>
/// メインウィンドウ ViewModel（MVVM Toolkit）
///
/// 【責務】
/// - BizId 検索入力の状態管理
/// - GetEmployeeByBizIdIntegrationUseCase の実行
/// - 検索結果（従業員氏名・所属部署）の保持
///
/// 【UI バインディング】
/// - BizIdSearchInput（ObservableProperty）
/// - EmployeeFullName（ObservableProperty）
/// - DepartmentNames（ObservableProperty）
/// - SearchEmployeeByBizIdCommand（RelayCommand）
/// - ExecuteSampleUseCaseCommand（RelayCommand）
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly AppSettings _appSettings;
    private readonly IClock _clock;
    private readonly IAppLogging<MainWindowViewModel> _logger;
    private readonly GetEmployeeByBizIdIntegrationUseCase _getEmployeeByBizIdUseCase;

    /// <summary>
    /// BizId 検索欄の入力値（数値以外の入力は検索時にエラー表示）
    /// </summary>
    [ObservableProperty]
    private string _bizIdSearchInput = string.Empty;

    /// <summary>
    /// 検索結果の従業員氏名（「姓 名」形式）。見つからない場合やエラー時は、その旨のメッセージ
    /// </summary>
    [ObservableProperty]
    private string _employeeFullName = string.Empty;

    /// <summary>
    /// 検索結果の従業員の所属部署名
    /// </summary>
    [ObservableProperty]
    private string _departmentNames = string.Empty;

    /// <summary>
    /// <see cref="MainWindowViewModel"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="logger">ログの出力先</param>
    /// <param name="appSettings">アプリケーション設定（<see cref="AppSettings"/> であること）</param>
    /// <param name="clock">現在日時（JST）の取得元</param>
    /// <param name="getEmployeeByBizIdUseCase">BizId による従業員検索のユースケース</param>
    /// <exception cref="ArgumentNullException">いずれかの引数が <see langword="null"/> の場合</exception>
    /// <exception cref="InvalidCastException"><paramref name="appSettings"/> が <see cref="AppSettings"/> 以外の実装の場合</exception>
    public MainWindowViewModel(IAppLogging<MainWindowViewModel> logger,
        IAppSettings appSettings,
        IClock clock,
        GetEmployeeByBizIdIntegrationUseCase getEmployeeByBizIdUseCase)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(appSettings);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(getEmployeeByBizIdUseCase);

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
    /// サンプル実行コマンド（ボタンクリック時に実行）
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
}
