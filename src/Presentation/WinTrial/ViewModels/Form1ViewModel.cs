using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Common.Configuration;
using SupportAdvance.Crosscutting.Logging;

namespace SupportAdvance.Presentation.WinTrial.ViewModels;

public partial class Form1ViewModel : ObservableObject
{
    private readonly AppSettings _appSettings;
    private readonly IClock _clock;
    private readonly IAppLogging<Form1ViewModel> _logger;

    public Form1ViewModel(IAppLogging<Form1ViewModel> logger,
        IAppSettings appSettings,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(appSettings);
        ArgumentNullException.ThrowIfNull(clock);

        _logger = logger;
        _appSettings = (AppSettings)appSettings;
        _clock = clock;
        _logger.LogInformation("Form1ViewModel initialized.");
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
