using System.Windows.Controls;

namespace SupportAdvance.Presentation.WpfTrial.Views;

/// <summary>
/// Form1 の画面（顧客一覧のタブに仮として表示する UserControl）
/// </summary>
/// <remarks>
/// <para>【設計】DataContext の設定なし。タブの内容（<c>Form1ViewModel</c>）が DataContext として渡される（MainWindow.xaml の DataTemplate）</para>
/// </remarks>
public partial class Form1View : UserControl
{
    /// <summary>
    /// <see cref="Form1View"/> クラスの新しいインスタンスの初期化
    /// </summary>
    public Form1View()
    {
        InitializeComponent();
    }
}
