using SupportAdvance.Common.Configuration;
using SupportAdvance.Crosscutting.Logging;
using SupportAdvance.Presentation.WinTrial.ViewModels;

namespace SupportAdvance.Presentation.WinTrial.Views;

public partial class Form1 : Form
{
    private readonly IAppSettings _appSettings = null!;
    private readonly IAppLogging<Form1> _logger = null!;
    private readonly Form1ViewModel _viewModel = null!;

    public Form1(Form1ViewModel viewModel, IAppLogging<Form1> logger, IAppSettings settings)
    {
        InitializeComponent();

        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(settings);

        _logger = logger;
        _viewModel = viewModel;
        _appSettings = settings;

        // ViewModel を DataContext に設定
        this.DataContext = _viewModel;

        // Data Binding: textBoxExt1 ← BizIdSearchInput
        textBoxExt1.DataBindings.Add("Text", _viewModel, nameof(Form1ViewModel.BizIdSearchInput), true, DataSourceUpdateMode.OnPropertyChanged);

        // Data Binding: label1 ← EmployeeFullName
        label1.DataBindings.Add("Text", _viewModel, nameof(Form1ViewModel.EmployeeFullName));

        // Data Binding: label2 ← DepartmentNames
        label2.DataBindings.Add("Text", _viewModel, nameof(Form1ViewModel.DepartmentNames));

        // Command Binding: sfButton1 ← ExecuteSampleUseCaseCommand
        sfButton1.Command = _viewModel.ExecuteSampleUseCaseCommand;

        // Command Binding: sfButton2 ← SearchEmployeeByBizIdCommand
        sfButton2.Command = _viewModel.SearchEmployeeByBizIdCommand;

        _logger.LogInformation("Form1 initialized.");
        _logger.LogInformation("情報");
        _logger.LogWarning("警告");
        _logger.LogInformation(_appSettings?.ApplicationBuildType ?? "Unknown");
    }
}
