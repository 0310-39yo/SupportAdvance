using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Syncfusion.UI.Xaml.NavigationDrawer;

namespace SupportAdvance.Presentation.WpfTrial.Behaviors;

/// <summary>
/// <see cref="SfNavigationDrawer"/> の展開時の幅を、マウスのドラッグで変えられるようにする添付ビヘイビア（<see cref="Thumb"/> に指定）
///
/// 【背景】
/// SfNavigationDrawer は幅をドラッグで変える機能を持たない。ドロワーの右端に重ねて置いた <see cref="Thumb"/>（つかみ部分）のドラッグ量を、
/// ドロワーの <see cref="SfNavigationDrawer.ExpandedModeWidth"/> に反映する。
///
/// 【使い方】
/// &lt;Thumb HorizontalAlignment="Left" Width="6" Cursor="SizeWE"
///         behaviors:NavigationDrawerResizeBehavior.Target="{Binding ElementName=NavigationPane}"
///         behaviors:NavigationDrawerResizeBehavior.MinWidth="120"
///         behaviors:NavigationDrawerResizeBehavior.MaxWidth="500" /&gt;
///
/// 【動作】
/// - つかみ部分は、ドロワーの現在の右端に追従する（レイアウトの更新のたびに位置を合わせる）
/// - 折りたたみ表示（Compact）のときは、つかみ部分を隠して、幅を変えられないようにする
///
/// 【注意】
/// - 現在の幅は、ドロワーのテンプレートが作る <c>DrawerContentGrid</c> の幅から求める（テンプレートの構造に依存。見つからない場合は何もしない）
/// </summary>
public static class NavigationDrawerResizeBehavior
{
    // ドロワーのテンプレートが持つ、項目を並べる領域の要素の名前（この要素の幅が、現在のドロワーの幅になる）
    private const string DrawerContentGridName = "DrawerContentGrid";

    private static readonly ConditionalWeakTable<Thumb, EventHandler> LayoutHandlers = new();

    /// <summary>
    /// <c>Target</c> 添付プロパティの識別子（幅を変える対象のドロワー）
    /// </summary>
    public static readonly DependencyProperty TargetProperty =
        DependencyProperty.RegisterAttached(
            "Target",
            typeof(SfNavigationDrawer),
            typeof(NavigationDrawerResizeBehavior),
            new PropertyMetadata(null, OnTargetChanged));

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
    /// <c>Target</c> 添付プロパティの値の取得
    /// </summary>
    /// <param name="d">対象の要素（<see cref="Thumb"/>）</param>
    /// <returns>幅を変える対象のドロワー。未設定の場合は <see langword="null"/></returns>
    public static SfNavigationDrawer? GetTarget(DependencyObject d) => (SfNavigationDrawer?)d.GetValue(TargetProperty);

    /// <summary>
    /// <c>Target</c> 添付プロパティの値の設定
    /// </summary>
    /// <param name="d">対象の要素（<see cref="Thumb"/>）</param>
    /// <param name="value">幅を変える対象のドロワー</param>
    public static void SetTarget(DependencyObject d, SfNavigationDrawer? value) => d.SetValue(TargetProperty, value);

    /// <summary>
    /// <c>MinWidth</c> 添付プロパティの値の取得
    /// </summary>
    /// <param name="d">対象の要素（<see cref="Thumb"/>）</param>
    /// <returns>ドラッグで縮められる最小の幅</returns>
    public static double GetMinWidth(DependencyObject d) => (double)d.GetValue(MinWidthProperty);

    /// <summary>
    /// <c>MinWidth</c> 添付プロパティの値の設定
    /// </summary>
    /// <param name="d">対象の要素（<see cref="Thumb"/>）</param>
    /// <param name="value">ドラッグで縮められる最小の幅</param>
    public static void SetMinWidth(DependencyObject d, double value) => d.SetValue(MinWidthProperty, value);

    /// <summary>
    /// <c>MaxWidth</c> 添付プロパティの値の取得
    /// </summary>
    /// <param name="d">対象の要素（<see cref="Thumb"/>）</param>
    /// <returns>ドラッグで広げられる最大の幅</returns>
    public static double GetMaxWidth(DependencyObject d) => (double)d.GetValue(MaxWidthProperty);

    /// <summary>
    /// <c>MaxWidth</c> 添付プロパティの値の設定
    /// </summary>
    /// <param name="d">対象の要素（<see cref="Thumb"/>）</param>
    /// <param name="value">ドラッグで広げられる最大の幅</param>
    public static void SetMaxWidth(DependencyObject d, double value) => d.SetValue(MaxWidthProperty, value);

    private static void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Thumb thumb)
            return;

        thumb.DragDelta -= Thumb_DragDelta;

        if (e.OldValue is SfNavigationDrawer oldTarget && LayoutHandlers.TryGetValue(thumb, out var oldHandler))
        {
            oldTarget.LayoutUpdated -= oldHandler;
            LayoutHandlers.Remove(thumb);
        }

        if (e.NewValue is not SfNavigationDrawer target)
            return;

        thumb.DragDelta += Thumb_DragDelta;

        // LayoutUpdated の送信元はドロワーではないため、つかみ部分とドロワーをクロージャで保持する
        EventHandler handler = (_, _) => FollowDrawerEdge(thumb, target);
        LayoutHandlers.Add(thumb, handler);
        target.LayoutUpdated += handler;
    }

    private static void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        var thumb = (Thumb)sender;
        var target = GetTarget(thumb);
        if (target is null)
            return;

        var width = target.ExpandedModeWidth + e.HorizontalChange;
        target.ExpandedModeWidth = Math.Clamp(width, GetMinWidth(thumb), GetMaxWidth(thumb));
        e.Handled = true;
    }

    private static void FollowDrawerEdge(Thumb thumb, SfNavigationDrawer target)
    {
        var currentWidth = FindDrawerContentGrid(target)?.ActualWidth;
        if (currentWidth is null)
            return;

        // 折りたたみ表示（Compact）では幅を変えられないため、つかみ部分を隠す
        var isCompact = currentWidth.Value <= target.CompactModeWidth + 1;
        var visibility = isCompact ? Visibility.Collapsed : Visibility.Visible;
        if (thumb.Visibility != visibility)
        {
            thumb.Visibility = visibility;
        }

        var left = currentWidth.Value - thumb.Width / 2;
        if (!isCompact && Math.Abs(thumb.Margin.Left - left) > 0.01)
        {
            thumb.Margin = new Thickness(left, 0, 0, 0);
        }
    }

    private static FrameworkElement? FindDrawerContentGrid(DependencyObject parent)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is Grid { Name: DrawerContentGridName } grid)
                return grid;

            var found = FindDrawerContentGrid(child);
            if (found is not null)
                return found;
        }

        return null;
    }
}
