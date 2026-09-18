using SupportAdvance.Common.Configuration;
using SupportAdvance.Crosscutting.Logging;
using SupportAdvance.Presentation.WinTrial.ViewModels;

namespace SupportAdvance.Presentation.WinTrial.Views;

/// <summary>
/// BizId による従業員検索を行うメインフォーム（WinForms + MVVM Toolkit）
/// </summary>
/// <remarks>
/// <para>【責務】<see cref="Form1ViewModel"/> とのデータバインディングとコマンドの配線のみ</para>
/// </remarks>
public partial class Form1 : Form
{
    private readonly IAppSettings _appSettings = null!;
    private readonly IAppLogging<Form1> _logger = null!;
    private readonly Form1ViewModel _viewModel = null!;

    /// <summary>
    /// <see cref="Form1"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="viewModel">バインドする ViewModel</param>
    /// <param name="logger">ログの出力先</param>
    /// <param name="settings">アプリケーション設定（ビルド種別のログ出力用）</param>
    /// <exception cref="ArgumentNullException">いずれかの引数が <see langword="null"/> の場合</exception>
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
        DataContext = _viewModel;

        textBoxExt1.DataBindings.Add("Text", _viewModel, nameof(Form1ViewModel.BizIdSearchInput), true,
            DataSourceUpdateMode.OnPropertyChanged);

        label1.DataBindings.Add("Text", _viewModel, nameof(Form1ViewModel.EmployeeFullName));
        label2.DataBindings.Add("Text", _viewModel, nameof(Form1ViewModel.DepartmentNames));

        sfButton1.Command = _viewModel.ExecuteSampleUseCaseCommand;
        sfButton2.Command = _viewModel.SearchEmployeeByBizIdCommand;

        _logger.LogInformation("Form1 initialized.");
        _logger.LogInformation("情報");
        _logger.LogWarning("警告");
        _logger.LogInformation(_appSettings?.ApplicationBuildType ?? "Unknown");
    }
}
