using SupportAdvance.Presentation.Shared.ViewModels;

namespace SupportAdvance.Presentation.WinTrial.Views;

/// <summary>
/// BizId による従業員検索を行うメインフォーム（WinForms + MVVM Toolkit）
/// </summary>
/// <remarks>
/// <para>【責務】<see cref="Form1ViewModel"/> とのデータバインディングとコマンドの配線のみ</para>
/// </remarks>
public partial class Form1View : Form
{
    private readonly Form1ViewModel _viewModel;

    /// <summary>
    /// <see cref="Form1View"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="viewModel">バインドする ViewModel</param>
    /// <exception cref="ArgumentNullException"><paramref name="viewModel"/> が <see langword="null"/> の場合</exception>
    public Form1View(Form1ViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();

        _viewModel = viewModel;

        // ViewModel を DataContext に設定
        DataContext = _viewModel;

        textBoxExt1.DataBindings.Add("Text", _viewModel, nameof(Form1ViewModel.BizIdSearchInput), true,
            DataSourceUpdateMode.OnPropertyChanged);

        label1.DataBindings.Add("Text", _viewModel, nameof(Form1ViewModel.EmployeeFullName));
        label2.DataBindings.Add("Text", _viewModel, nameof(Form1ViewModel.DepartmentNames));

        buttonAdv1.Command = _viewModel.ExecuteSampleUseCaseCommand;
        buttonAdv2.Command = _viewModel.SearchEmployeeByBizIdCommand;
    }
}
