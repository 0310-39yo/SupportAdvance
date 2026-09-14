using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Common.Configuration;
using SupportAdvance.Contexts.IntegrationPrototype.Application.UseCases;
using SupportAdvance.Crosscutting.Logging;

namespace SupportAdvance.Presentation.WinTrial.ViewModels;

public partial class Form1ViewModel : ObservableObject
{
    private readonly AppSettings _appSettings;
    private readonly IClock _clock;
    private readonly IAppLogging<Form1ViewModel> _logger;
    private readonly GetEmployeeByBizIdIntegrationUseCase _getEmployeeByBizIdUseCase;

    [ObservableProperty]
    private string _bizIdSearchInput = string.Empty;

    [ObservableProperty]
    private string _employeeFullName = string.Empty;

    [ObservableProperty]
    private string _departmentNames = string.Empty;

    public Form1ViewModel(IAppLogging<Form1ViewModel> logger,
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
        _logger.LogInformation("Form1ViewModel initialized.");
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

    public void Dispose()
    {
        // リソース解放があれば記載
    }
}
