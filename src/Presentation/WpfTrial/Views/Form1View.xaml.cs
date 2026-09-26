using System.Windows;
using SupportAdvance.Presentation.Shared.ViewModels;

namespace SupportAdvance.Presentation.WpfTrial.Views;

/// <summary>
/// Form1View ウィンドウ
/// </summary>
public partial class Form1View : Window
{
    /// <summary>
    /// <see cref="Form1View"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="viewModel">バインドする ViewModel</param>
    /// <exception cref="ArgumentNullException"><paramref name="viewModel"/> が <see langword="null"/> の場合</exception>
    public Form1View(Form1ViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();
        DataContext = viewModel;
    }
}
