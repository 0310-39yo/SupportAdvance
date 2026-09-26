using System.Windows;
using System.Windows.Input;
using Syncfusion.UI.Xaml.NavigationDrawer;

namespace SupportAdvance.Presentation.WpfTrial.Behaviors;

/// <summary>
/// <see cref="SfNavigationDrawer"/> の項目クリックをコマンドに橋渡しする添付ビヘイビア
/// </summary>
/// <remarks>
/// <para>【背景】<c>SfNavigationDrawer</c> は項目クリックのコマンドを持たず、<see cref="SfNavigationDrawer.ItemClicked"/> イベントのみを公開。コードビハインドを使わずに ViewModel のコマンドを呼び出すため、イベントを添付プロパティ経由でコマンドに変換</para>
/// </remarks>
/// <example>
/// <code>
/// &lt;navigationDrawer:SfNavigationDrawer behaviors:NavigationDrawerBehavior.ItemClickedCommand="{Binding OpenTabCommand}" /&gt;
/// </code>
/// </example>
public static class NavigationDrawerBehavior
{
    /// <summary>
    /// <c>ItemClickedCommand</c> 添付プロパティの識別子（子項目を持たない項目がクリックされた時に実行するコマンド）
    /// </summary>
    /// <remarks>
    /// <para>【コマンドパラメーター】クリックされた項目のヘッダー文字列（<see cref="string"/>）</para>
    /// <para>【注意】子項目を持つ親項目のクリックでは、コマンドは実行されない（展開／折りたたみのみ）</para>
    /// </remarks>
    public static readonly DependencyProperty ItemClickedCommandProperty =
        DependencyProperty.RegisterAttached(
            "ItemClickedCommand",
            typeof(ICommand),
            typeof(NavigationDrawerBehavior),
            new PropertyMetadata(null, OnItemClickedCommandChanged));

    /// <summary>
    /// <c>ItemClickedCommand</c> 添付プロパティの値の取得
    /// </summary>
    /// <param name="d">対象の要素（通常は <see cref="SfNavigationDrawer"/>）</param>
    /// <returns>設定されているコマンド。未設定の場合は <see langword="null"/></returns>
    public static ICommand? GetItemClickedCommand(DependencyObject d) => (ICommand?)d.GetValue(ItemClickedCommandProperty);

    /// <summary>
    /// <c>ItemClickedCommand</c> 添付プロパティの値の設定
    /// </summary>
    /// <param name="d">対象の要素（通常は <see cref="SfNavigationDrawer"/>）</param>
    /// <param name="value">項目クリック時に実行するコマンド</param>
    public static void SetItemClickedCommand(DependencyObject d, ICommand? value) => d.SetValue(ItemClickedCommandProperty, value);

    private static void OnItemClickedCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not SfNavigationDrawer drawer)
            return;

        // 購読の二重登録を避けるため、いったん解除してから、コマンドがある場合のみ登録
        drawer.ItemClicked -= Drawer_ItemClicked;

        if (e.NewValue is not null)
        {
            drawer.ItemClicked += Drawer_ItemClicked;
        }
    }

    private static void Drawer_ItemClicked(object? sender, NavigationItemClickedEventArgs e)
    {
        if (sender is not DependencyObject drawer)
            return;

        var item = e.Item;

        // NavigationItem.HasItems は XAML 宣言時点では更新されず常に false を返すため、
        // Items.Count で子項目の有無を判定する（実機検証で確認済み。
        // 詳細は docs/Assistance/Plans/20260926_SfNavigationDrawer再検証計画.md）
        if (item is null || item.Items.Count > 0)
            return;

        var header = item.Header?.ToString();
        if (string.IsNullOrEmpty(header))
            return;

        var command = GetItemClickedCommand(drawer);
        if (command is not null && command.CanExecute(header))
        {
            command.Execute(header);
        }
    }
}
