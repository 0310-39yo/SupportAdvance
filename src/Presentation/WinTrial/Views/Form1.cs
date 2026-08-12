using SupportAdvance.Common.Configuration;
using SupportAdvance.Crosscutting.Logging;
using SupportAdvance.Presentation.WinTrial.ViewModels;

namespace SupportAdvance.Presentation.WinTrial.Views;

public partial class Form1 : Form
{
    private readonly IAppSettings _appSettings;
    private readonly IAppLogging<Form1> _logger;
    private readonly Form1ViewModel _viewModel;

    public Form1(Form1ViewModel viewModel, IAppLogging<Form1> logger, IAppSettings settings)
    {
        InitializeComponent();

        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(settings);

        _logger = logger;
        _viewModel = viewModel;
        _appSettings = settings;

        // Syncfusion SfButton に ViewModel のコマンドをバインド
        sfButton1.Command = _viewModel.ExecuteSampleUseCaseCommand;

        _logger.LogInformation("Form1 initialized.");
        _logger.LogInformation("情報");
        _logger.LogWarning("警告");
        _logger.LogInformation(_appSettings?.ApplicationBuildType ?? "Unknown");
    }
}
