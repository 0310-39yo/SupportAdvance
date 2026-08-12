using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SupportAdvance.Application.UseCases;
using SupportAdvance.Common.Clocks;
using SupportAdvance.Common.Configuration;
using SupportAdvance.Contexts.Samples.CarPreferences.Application.UseCases;
using SupportAdvance.Crosscutting.Logging;

namespace SupportAdvance.Presentation.WinTrial.ViewModels;

public partial class Form1ViewModel : ObservableObject
{
    private readonly AppSettings _appSettings;
    private readonly IUseCase<CarPreferencesRequest, CarPreferencesResponse> _carPreferencesUseCase;
    private readonly IClock _clock;
    private readonly IAppLogging<Form1ViewModel> _logger;

    public Form1ViewModel(IAppLogging<Form1ViewModel> logger,
        IAppSettings appSettings,
        IClock clock,
        IUseCase<CarPreferencesRequest, CarPreferencesResponse> carPreferencesUseCase)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(appSettings);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(carPreferencesUseCase);

        _logger = logger;
        _appSettings = (AppSettings)appSettings;
        _clock = clock;
        _carPreferencesUseCase = carPreferencesUseCase;
        _logger.LogInformation("Form1ViewModel initialized.");
    }

    /// <summary>
    /// CarPreferencesUseCaseを実行するコマンド
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    private async Task ExecuteSampleUseCase()
    {
        try
        {
            var request = new CarPreferencesRequest
            {
                Name = "WinTrial Demo",
                Details = $"Executed at {_clock.JstNow:yyyy-MM-dd HH:mm:ss}"
            };

            // UseCase を実行
            // ロギングは Decorator で自動的に適用される
            var response = await _carPreferencesUseCase.ExecuteAsync(request);

            _logger.LogInformation($"SampleUseCase result: {response.Message}");
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
