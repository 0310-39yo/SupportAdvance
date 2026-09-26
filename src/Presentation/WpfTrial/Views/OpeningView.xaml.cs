using System.Windows.Controls;

namespace SupportAdvance.Presentation.WpfTrial.Views;

/// <summary>
/// オープニング画面（起動後、ナビゲーションが選ばれるまで表示する UserControl）
/// </summary>
/// <remarks>
/// <para>【設計】DataContext は設定しない。<c>OpeningViewModel</c> が DataContext として渡される（MainWindow.xaml の DataTemplate）</para>
/// </remarks>
public partial class OpeningView : UserControl
{
    /// <summary>
    /// <see cref="OpeningView"/> クラスの新しいインスタンスの初期化
    /// </summary>
    public OpeningView()
    {
        InitializeComponent();
    }
}
