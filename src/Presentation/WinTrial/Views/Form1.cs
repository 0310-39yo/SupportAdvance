using SupportAdvance.Common.Configuration;
using SupportAdvance.Crosscutting.Logging;
using SupportAdvance.Presentation.Shared.ViewModels;
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

        AddBusinessDayClockPanel(_viewModel.BusinessDayClock);

        _logger.LogInformation("Form1 initialized.");
        _logger.LogInformation("情報");
        _logger.LogWarning("警告");
        _logger.LogInformation(_appSettings?.ApplicationBuildType ?? "Unknown");
    }

    /// <summary>
    /// 業務日クロックの操作パネルの配置（ClockType が BusinessDay の場合のみ）
    /// </summary>
    /// <param name="clockViewModel">パネルにバインドする ViewModel</param>
    /// <remarks>
    /// <para>【設計】デザイナーのファイル（Form1.Designer.cs）を変更しないよう、コードで配置。フォームの高さはパネルの分だけ拡張</para>
    /// </remarks>
    private void AddBusinessDayClockPanel(BusinessDayClockViewModel clockViewModel)
    {
        if (!clockViewModel.IsAvailable)
        {
            return;
        }

        var dateLabel = new Label { AutoSize = true, Margin = new Padding(0, 6, 0, 0) };
        dateLabel.DataBindings.Add("Text", clockViewModel, nameof(BusinessDayClockViewModel.CurrentBusinessDateText));

        var statusLabel = new Label { AutoSize = true, Margin = new Padding(4, 6, 0, 0) };
        statusLabel.DataBindings.Add("Text", clockViewModel, nameof(BusinessDayClockViewModel.StatusText));

        var infoRow = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
        infoRow.Controls.Add(new Label { Text = "業務日:", AutoSize = true, Margin = new Padding(0, 6, 0, 0) });
        infoRow.Controls.Add(dateLabel);
        infoRow.Controls.Add(statusLabel);

        var buttonRow = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
        buttonRow.Controls.Add(new Button { Text = "業務日を開始", Width = 100, Command = clockViewModel.TurnOnCommand });
        buttonRow.Controls.Add(new Button { Text = "業務日を終了", Width = 100, Command = clockViewModel.TurnOffCommand });
        buttonRow.Controls.Add(new Button { Text = "最初からやり直す", Width = 120, Command = clockViewModel.ResetCommand });

        var errorLabel = new Label { AutoSize = true, ForeColor = Color.Red };
        errorLabel.DataBindings.Add("Text", clockViewModel, nameof(BusinessDayClockViewModel.ErrorMessage));

        var content = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };
        content.Controls.Add(infoRow);
        content.Controls.Add(buttonRow);
        content.Controls.Add(errorLabel);

        const int panelHeight = 110;
        var group = new GroupBox
        {
            Text = "業務日クロック（検証用）",
            Location = new Point(12, ClientSize.Height),
            Size = new Size(ClientSize.Width - 24, panelHeight - 10),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };
        group.Controls.Add(content);

        ClientSize = new Size(ClientSize.Width, ClientSize.Height + panelHeight);
        Controls.Add(group);
    }
}
