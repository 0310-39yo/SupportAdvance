using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Common.Configuration;
using SupportAdvance.Contexts.IntegrationPrototype.Application.UseCases;
using SupportAdvance.Crosscutting.Logging;
using SupportAdvance.Presentation.Shared.ViewModels;

namespace SupportAdvance.Presentation.WinTrial.ViewModels;

/// <summary>
/// メインフォーム（<c>Form1</c>）の ViewModel（MVVM Toolkit）
/// </summary>
/// <remarks>
/// <para>【責務】BizId 検索入力の状態管理、<see cref="GetEmployeeByBizIdIntegrationUseCase"/> の実行、検索結果の保持</para>
/// </remarks>
public partial class Form1ViewModel : ObservableObject
{
    private readonly AppSettings _appSettings;
    private readonly IClock _clock;
    private readonly IAppLogging<Form1ViewModel> _logger;
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
    /// <see cref="Form1ViewModel"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="logger">ログの出力先</param>
    /// <param name="appSettings">アプリケーション設定（<see cref="AppSettings"/> であること）</param>
    /// <param name="clock">現在日時（JST）の取得元</param>
    /// <param name="getEmployeeByBizIdUseCase">BizId による従業員検索のユースケース</param>
    /// <param name="businessDayClock">BusinessDayClockの操作パネルの ViewModel</param>
    /// <exception cref="ArgumentNullException">いずれかの引数が <see langword="null"/> の場合</exception>
    /// <exception cref="InvalidCastException"><paramref name="appSettings"/> が <see cref="AppSettings"/> 以外の実装の場合</exception>
    public Form1ViewModel(IAppLogging<Form1ViewModel> logger,
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
        _logger.LogInformation("Form1ViewModel initialized.");
    }

    /// <summary>
    /// BusinessDayClockの操作パネルの ViewModel
    /// </summary>
    /// <value>ClockType が BusinessDay 以外の場合、<see cref="BusinessDayClockViewModel.IsAvailable"/> は <see langword="false"/>（パネルは非表示）</value>
    public BusinessDayClockViewModel BusinessDayClock { get; }

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

    /// <summary>
    /// リソースの解放（現在は解放対象なし）
    /// </summary>
    /// <remarks>
    /// <para>【注意】<see cref="IDisposable"/> は未実装のため、DI コンテナーからの自動呼び出しなし</para>
    /// </remarks>
    public void Dispose()
    {
        // リソース解放があれば記載
    }
}
