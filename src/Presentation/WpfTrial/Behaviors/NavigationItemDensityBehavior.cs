using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Syncfusion.UI.Xaml.NavigationDrawer;

namespace SupportAdvance.Presentation.WpfTrial.Behaviors;

/// <summary>
/// <see cref="SfNavigationDrawer"/> の項目の上下の間隔を詰める添付ビヘイビア
///
/// 【背景】
/// 項目の高さは、テーマのテンプレートが決めている。項目の見出しの行は、アイコン欄（高さ 25 に上下の余白 5 ずつ、合計 35）で高さが決まり、
/// 項目のスタイル（MinHeight など）では 35 より小さくできない。テンプレート内の要素に直接指定された値は、
/// スタイルよりも優先されるため、表示後にビジュアルツリーのアイコン欄へ、より優先度の高い値（ローカル値）を指定して詰める。
///
/// 【使い方】
/// &lt;navigationDrawer:SfNavigationDrawer behaviors:NavigationItemDensityBehavior.IconRowHeight="20" /&gt;
///
/// 【注意】
/// - テンプレートの構造（<c>NavigationItem</c> の中の、高さ 25・上下の余白 5 のアイコン欄）に依存する。Syncfusion の更新で構造が変わると、何も変えずに終わる（壊れはしない）
/// - 子項目は展開したときに作られるため、レイアウトが更新されるたびに、未処理のアイコン欄を探して詰める
/// </summary>
public static class NavigationItemDensityBehavior
{
    // テンプレートが指定しているアイコン欄の高さ（これと一致する要素だけを対象にする）
    private const double TemplateIconRowHeight = 25;

    // テンプレートが指定しているアイコン欄の上下の余白
    private const double TemplateRowVerticalMargin = 5;

    // 詰めた後のアイコン欄の上下の余白
    private const double RowVerticalMargin = 2;

    // ドロワーごとの LayoutUpdated ハンドラー（登録の解除と二重登録の防止のために保持）
    private static readonly ConditionalWeakTable<FrameworkElement, EventHandler> Handlers = new();

    /// <summary>
    /// <c>IconRowHeight</c> 添付プロパティの識別子（アイコン欄の高さ。項目の高さは、この値に上下の余白（合計 4）を足した大きさになる）
    /// </summary>
    /// <remarks>
    /// <para>【既定値】<see cref="double.NaN"/>（何もしない。テーマの既定の間隔のまま）</para>
    /// </remarks>
    public static readonly DependencyProperty IconRowHeightProperty =
        DependencyProperty.RegisterAttached(
            "IconRowHeight",
            typeof(double),
            typeof(NavigationItemDensityBehavior),
            new PropertyMetadata(double.NaN, OnIconRowHeightChanged));

    /// <summary>
    /// <c>IconRowHeight</c> 添付プロパティの値の取得
    /// </summary>
    /// <param name="d">対象の要素（通常は <see cref="SfNavigationDrawer"/>）</param>
    /// <returns>アイコン欄の高さ。未設定の場合は <see cref="double.NaN"/></returns>
    public static double GetIconRowHeight(DependencyObject d) => (double)d.GetValue(IconRowHeightProperty);

    /// <summary>
    /// <c>IconRowHeight</c> 添付プロパティの値の設定
    /// </summary>
    /// <param name="d">対象の要素（通常は <see cref="SfNavigationDrawer"/>）</param>
    /// <param name="value">アイコン欄の高さ</param>
    public static void SetIconRowHeight(DependencyObject d, double value) => d.SetValue(IconRowHeightProperty, value);

    private static void OnIconRowHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement drawer)
            return;

        if (Handlers.TryGetValue(drawer, out var existing))
        {
            drawer.LayoutUpdated -= existing;
            Handlers.Remove(drawer);
        }

        if (double.IsNaN((double)e.NewValue))
            return;

        // LayoutUpdated の送信元はドロワーではないため、ドロワーをクロージャで保持する
        EventHandler handler = (_, _) => Apply(drawer);
        Handlers.Add(drawer, handler);
        drawer.LayoutUpdated += handler;

        // 最初の表示でも詰める（テンプレートの適用後に実行）
        drawer.Loaded += (_, _) => drawer.Dispatcher.BeginInvoke(() => Apply(drawer), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private static void Apply(FrameworkElement drawer)
    {
        var rowHeight = GetIconRowHeight(drawer);
        if (double.IsNaN(rowHeight))
            return;

        Walk(drawer, rowHeight);
    }

    private static void Walk(DependencyObject parent, double rowHeight)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            // メインコンテンツ（タブなど）の中には項目が無いため、探索しない
            if (child is ContentPresenter { Name: "ContentViewContentPresenter" })
                continue;

            if (child is ContentPresenter presenter && IsTemplateIconRow(presenter))
            {
                presenter.Height = rowHeight;
                presenter.Margin = new Thickness(presenter.Margin.Left, RowVerticalMargin, presenter.Margin.Right, RowVerticalMargin);
                continue;
            }

            Walk(child, rowHeight);
        }
    }

    // テンプレートが指定した値（高さ 25、上下の余白 5）のままの、項目の見出しの行のアイコン欄だけを対象にする（詰めた後は一致しなくなり、再処理されない）
    private static bool IsTemplateIconRow(ContentPresenter presenter)
        => presenter.Height == TemplateIconRowHeight
           && presenter.Margin.Top == TemplateRowVerticalMargin
           && presenter.Margin.Bottom == TemplateRowVerticalMargin
           && HasNavigationItemAncestor(presenter);

    private static bool HasNavigationItemAncestor(DependencyObject element)
    {
        for (var parent = VisualTreeHelper.GetParent(element); parent is not null; parent = VisualTreeHelper.GetParent(parent))
        {
            if (parent is NavigationItem)
                return true;
        }

        return false;
    }
}
