using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Syncfusion.UI.Xaml.NavigationDrawer;

namespace SupportAdvance.Presentation.WpfTrial.Behaviors;

/// <summary>
/// <see cref="SfNavigationDrawer"/> を置いた <see cref="Grid"/> の列の幅を、<see cref="GridSplitter"/> のドラッグで変えられるようにする添付ビヘイビア
///
/// 【背景】
/// SfNavigationDrawer は、展開時の幅（<see cref="SfNavigationDrawer.ExpandedModeWidth"/>）を、実行中に変えても表示に反映しない。
/// そのため、ドロワーの内側の幅は最大の幅に固定し、ドロワーを置いた列の幅（ドラッグで変える）で、見える範囲を切り取る方式にする。
///
/// 【使い方】
/// ドロワーを Grid の列 N に置き、列 N+1 に <see cref="GridSplitter"/> を置く。ドロワーに次を指定する。
/// &lt;navigationDrawer:SfNavigationDrawer ExpandedModeWidth="500"
///         behaviors:NavigationDrawerResizeBehavior.IsResizable="True"
///         behaviors:NavigationDrawerResizeBehavior.MinWidth="120"
///         behaviors:NavigationDrawerResizeBehavior.MaxWidth="500" /&gt;
/// （<c>ExpandedModeWidth</c> は <c>MaxWidth</c> 以上にする）
///
/// 【動作】
/// - 展開表示のとき: 列の幅を <c>MinWidth</c>〜<c>MaxWidth</c> の範囲でドラッグして変えられる
/// - 折りたたみ表示（Compact）になったとき: 列の幅を記憶したうえで <see cref="SfNavigationDrawer.CompactModeWidth"/> に固定し、境界を無効にする。
///   展開表示に戻ると、記憶した幅に戻す
///
/// 【注意】
/// - 展開／折りたたみの判定は、ドロワーのテンプレートが作る <c>ContentViewContentPresenter</c> の左の余白（折りたたみ時は <c>CompactModeWidth</c> になる）による。
///   テンプレートの構造に依存し、見つからない場合は何もしない
/// </summary>
public static class NavigationDrawerResizeBehavior
{
    // ドロワーのテンプレートが、メインコンテンツの左側に空ける余白（＝現在の展開／折りたたみの幅）を持つ要素の名前
    private const string ContentPresenterName = "ContentViewContentPresenter";

    // 列ごとの状態（展開時の幅の記憶など）と、LayoutUpdated ハンドラー（登録の解除と二重登録の防止のために保持）
    private sealed class State
    {
        public bool IsCompact;
        public GridLength ExpandedWidth = new(220);
        public EventHandler? Handler;
    }

    private static readonly ConditionalWeakTable<FrameworkElement, State> States = new();

    /// <summary>
    /// <c>IsResizable</c> 添付プロパティの識別子（<see langword="true"/> で、列の幅をドラッグで変えられるようにする）
    /// </summary>
    public static readonly DependencyProperty IsResizableProperty =
        DependencyProperty.RegisterAttached(
            "IsResizable",
            typeof(bool),
            typeof(NavigationDrawerResizeBehavior),
            new PropertyMetadata(false, OnIsResizableChanged));

    /// <summary>
    /// <c>MinWidth</c> 添付プロパティの識別子（ドラッグで縮められる最小の幅）
    /// </summary>
    public static readonly DependencyProperty MinWidthProperty =
        DependencyProperty.RegisterAttached("MinWidth", typeof(double), typeof(NavigationDrawerResizeBehavior), new PropertyMetadata(120.0));

    /// <summary>
    /// <c>MaxWidth</c> 添付プロパティの識別子（ドラッグで広げられる最大の幅）
    /// </summary>
    public static readonly DependencyProperty MaxWidthProperty =
        DependencyProperty.RegisterAttached("MaxWidth", typeof(double), typeof(NavigationDrawerResizeBehavior), new PropertyMetadata(500.0));

    /// <summary>
    /// <c>IsResizable</c> 添付プロパティの値の取得
    /// </summary>
    /// <param name="d">対象の要素（<see cref="SfNavigationDrawer"/>）</param>
    /// <returns>列の幅をドラッグで変えられる場合は <see langword="true"/></returns>
    public static bool GetIsResizable(DependencyObject d) => (bool)d.GetValue(IsResizableProperty);

    /// <summary>
    /// <c>IsResizable</c> 添付プロパティの値の設定
    /// </summary>
    /// <param name="d">対象の要素（<see cref="SfNavigationDrawer"/>）</param>
    /// <param name="value"><see langword="true"/> で、列の幅をドラッグで変えられるようにする</param>
    public static void SetIsResizable(DependencyObject d, bool value) => d.SetValue(IsResizableProperty, value);

    /// <summary>
    /// <c>MinWidth</c> 添付プロパティの値の取得
    /// </summary>
    /// <param name="d">対象の要素（<see cref="SfNavigationDrawer"/>）</param>
    /// <returns>ドラッグで縮められる最小の幅</returns>
    public static double GetMinWidth(DependencyObject d) => (double)d.GetValue(MinWidthProperty);

    /// <summary>
    /// <c>MinWidth</c> 添付プロパティの値の設定
    /// </summary>
    /// <param name="d">対象の要素（<see cref="SfNavigationDrawer"/>）</param>
    /// <param name="value">ドラッグで縮められる最小の幅</param>
    public static void SetMinWidth(DependencyObject d, double value) => d.SetValue(MinWidthProperty, value);

    /// <summary>
    /// <c>MaxWidth</c> 添付プロパティの値の取得
    /// </summary>
    /// <param name="d">対象の要素（<see cref="SfNavigationDrawer"/>）</param>
    /// <returns>ドラッグで広げられる最大の幅</returns>
    public static double GetMaxWidth(DependencyObject d) => (double)d.GetValue(MaxWidthProperty);

    /// <summary>
    /// <c>MaxWidth</c> 添付プロパティの値の設定
    /// </summary>
    /// <param name="d">対象の要素（<see cref="SfNavigationDrawer"/>）</param>
    /// <param name="value">ドラッグで広げられる最大の幅</param>
    public static void SetMaxWidth(DependencyObject d, double value) => d.SetValue(MaxWidthProperty, value);

    private static void OnIsResizableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not SfNavigationDrawer drawer)
            return;

        if (States.TryGetValue(drawer, out var existing) && existing.Handler is not null)
        {
            drawer.LayoutUpdated -= existing.Handler;
            States.Remove(drawer);
        }

        if (e.NewValue is not true)
            return;

        var state = new State();
        state.Handler = (_, _) => Update(drawer, state);
        States.Add(drawer, state);
        drawer.LayoutUpdated += state.Handler;
    }

    private static void Update(SfNavigationDrawer drawer, State state)
    {
        if (VisualTreeHelper.GetParent(drawer) is not Grid grid)
            return;

        var index = Grid.GetColumn(drawer);
        if (index < 0 || index >= grid.ColumnDefinitions.Count)
            return;

        var column = grid.ColumnDefinitions[index];
        var splitter = grid.Children.OfType<GridSplitter>().FirstOrDefault(s => Grid.GetColumn(s) == index + 1);

        var leftMargin = FindContentPresenter(drawer)?.Margin.Left;
        if (leftMargin is null)
            return;

        var isCompact = leftMargin.Value <= drawer.CompactModeWidth + 1;

        if (isCompact)
        {
            if (!state.IsCompact)
            {
                // 展開 → 折りたたみ: 列の幅を記憶して、折りたたみの幅に固定する
                state.IsCompact = true;
                state.ExpandedWidth = column.Width;
                column.MinWidth = drawer.CompactModeWidth;
                column.MaxWidth = drawer.CompactModeWidth;
                column.Width = new GridLength(drawer.CompactModeWidth);
                if (splitter is not null)
                {
                    splitter.IsEnabled = false;
                }
            }

            return;
        }

        if (state.IsCompact || column.MaxWidth != GetMaxWidth(drawer))
        {
            // 折りたたみ → 展開（または最初の表示）: 記憶した幅に戻す
            state.IsCompact = false;
            column.MinWidth = GetMinWidth(drawer);
            column.MaxWidth = GetMaxWidth(drawer);
            column.Width = state.ExpandedWidth;
            if (splitter is not null)
            {
                splitter.IsEnabled = true;
            }
        }
    }

    private static FrameworkElement? FindContentPresenter(DependencyObject parent)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is ContentPresenter { Name: ContentPresenterName } presenter)
                return presenter;

            var found = FindContentPresenter(child);
            if (found is not null)
                return found;
        }

        return null;
    }
}
