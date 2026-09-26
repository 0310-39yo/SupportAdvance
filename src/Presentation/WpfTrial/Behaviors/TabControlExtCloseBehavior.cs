using System.Windows;
using System.Windows.Input;
using Syncfusion.Windows.Tools.Controls;

namespace SupportAdvance.Presentation.WpfTrial.Behaviors;

/// <summary>
/// <see cref="TabControlExt"/> でタブが閉じられたことを、ViewModel のコマンドに橋渡しする添付ビヘイビア
/// </summary>
/// <remarks>
/// <para>【背景】<c>ItemsSource</c> にバインドした <see cref="TabControlExt"/> のタブを閉じても、元のコレクション（ViewModel のタブの一覧）から項目が取り除かれるとは限らない。取り除かれないと、ViewModel は、すべてのタブが閉じたこと（オープニング画面に戻す条件）を知ることができない。そのため、タブが閉じられたときに、閉じられたタブの項目をコマンドに渡して通知</para>
/// <para>【動作】</para>
/// <list type="bullet">
/// <item><description>個別のタブの閉じるボタン、「他のタブを閉じる」「すべてのタブを閉じる」のいずれの場合も、閉じられた各タブの項目（<c>DataContext</c>）を、コマンドのパラメーターとして 1 つずつ実行</description></item>
/// <item><description>すでに元のコレクションから取り除かれている項目に対しては、コマンド側で処理なし（<c>CloseTabCommand</c> は、開いていないタブを無視）</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// &lt;syncfusion:TabControlExt behaviors:TabControlExtCloseBehavior.TabClosedCommand="{Binding CloseTabCommand}" /&gt;
/// </code>
/// </example>
public static class TabControlExtCloseBehavior
{
    /// <summary>
    /// <c>TabClosedCommand</c> 添付プロパティの識別子（タブが閉じられたときに、閉じられた項目ごとに実行するコマンド）
    /// </summary>
    public static readonly DependencyProperty TabClosedCommandProperty =
        DependencyProperty.RegisterAttached(
            "TabClosedCommand",
            typeof(ICommand),
            typeof(TabControlExtCloseBehavior),
            new PropertyMetadata(null, OnTabClosedCommandChanged));

    /// <summary>
    /// <c>TabClosedCommand</c> 添付プロパティの値の取得
    /// </summary>
    /// <param name="d">対象の要素（<see cref="TabControlExt"/>）</param>
    /// <returns>設定されているコマンド。未設定の場合は <see langword="null"/></returns>
    public static ICommand? GetTabClosedCommand(DependencyObject d) => (ICommand?)d.GetValue(TabClosedCommandProperty);

    /// <summary>
    /// <c>TabClosedCommand</c> 添付プロパティの値の設定
    /// </summary>
    /// <param name="d">対象の要素（<see cref="TabControlExt"/>）</param>
    /// <param name="value">タブが閉じられたときに実行するコマンド</param>
    public static void SetTabClosedCommand(DependencyObject d, ICommand? value) => d.SetValue(TabClosedCommandProperty, value);

    private static void OnTabClosedCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TabControlExt tabs)
            return;

        // 購読の二重登録を避けるため、いったん解除してから、コマンドがある場合のみ登録
        tabs.TabClosed -= Tabs_TabClosed;

        if (e.NewValue is not null)
        {
            tabs.TabClosed += Tabs_TabClosed;
        }
    }

    private static void Tabs_TabClosed(object sender, CloseTabEventArgs e)
    {
        if (sender is not DependencyObject tabs)
            return;

        var command = GetTabClosedCommand(tabs);
        if (command is null)
            return;

        // 個別に閉じた場合は TargetTabItem、まとめて閉じた場合は ClosingTabItems に、閉じられたタブが入る
        var closed = new List<object?> { e.TargetTabItem };
        if (e.ClosingTabItems is not null)
        {
            closed.AddRange(e.ClosingTabItems);
        }

        var dataItems = closed
            .Select(tab => tab is FrameworkElement element ? element.DataContext : tab)
            .Where(item => item is not null)

            // タブの項目に DataContext が設定されていない場合は、親から引き継いだ画面全体の DataContext になるため、除く
            .Where(item => !ReferenceEquals(item, (tabs as FrameworkElement)?.DataContext))
            .Distinct()
            .ToList();

        foreach (var item in dataItems)
        {
            if (command.CanExecute(item))
            {
                command.Execute(item);
            }
        }
    }
}
