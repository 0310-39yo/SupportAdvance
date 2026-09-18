using System.Windows;
using System.Windows.Controls;

namespace SupportAdvance.Presentation.WpfTrial.Behaviors;

/// <summary>
/// PasswordBox.Password をバインド可能にする添付ビヘイビア
///
/// 【背景】
/// PasswordBox.Password はセキュリティ上の理由から DependencyProperty ではないため、
/// 通常の {Binding} では ViewModel と同期できない。この添付プロパティが仲介する。
///
/// 【Attach が必要な理由】
/// PasswordChanged イベントの購読を BoundPassword の PropertyChangedCallback 内で行うと、
/// バインディング初期値（既定値の空文字列）と ViewModel 側の初期値（同じく空文字列）が
/// 一致する場合に WPF が「変更なし」と判断してコールバック自体が一度も呼ばれず、
/// イベント購読が永久に行われない（＝何を入力しても ViewModel に反映されない）。
/// Attach は既定値 false → true への遷移が必ず発生するため、これを使って
/// PasswordChanged の購読タイミングを BoundPassword の初期値と切り離す。
///
/// 【使い方】
/// &lt;PasswordBox behaviors:PasswordBoxAssistant.Attach="True"
///              behaviors:PasswordBoxAssistant.BoundPassword="{Binding Password, Mode=TwoWay}" /&gt;
/// </summary>
public static class PasswordBoxAssistant
{
    public static readonly DependencyProperty BoundPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BoundPassword",
            typeof(string),
            typeof(PasswordBoxAssistant),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundPasswordChanged));

    public static readonly DependencyProperty AttachProperty =
        DependencyProperty.RegisterAttached(
            "Attach",
            typeof(bool),
            typeof(PasswordBoxAssistant),
            new PropertyMetadata(false, OnAttachChanged));

    private static readonly DependencyProperty IsUpdatingProperty =
        DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(PasswordBoxAssistant));

    public static string GetBoundPassword(DependencyObject dp) => (string)dp.GetValue(BoundPasswordProperty);

    // SetValue ではなく SetCurrentValue を使う。
    // SetValue はバインディングをローカル値で上書きして切断してしまうため、
    // PasswordChanged イベントごとに呼ぶとバインディングが失われ、
    // ViewModel.Password が更新されなくなる。
    public static void SetBoundPassword(DependencyObject dp, string value) => dp.SetCurrentValue(BoundPasswordProperty, value);

    public static bool GetAttach(DependencyObject dp) => (bool)dp.GetValue(AttachProperty);

    public static void SetAttach(DependencyObject dp, bool value) => dp.SetValue(AttachProperty, value);

    private static bool GetIsUpdating(DependencyObject dp) => (bool)dp.GetValue(IsUpdatingProperty);

    private static void SetIsUpdating(DependencyObject dp, bool value) => dp.SetValue(IsUpdatingProperty, value);

    private static void OnAttachChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox passwordBox)
            return;

        if ((bool)e.OldValue)
        {
            passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
        }

        if ((bool)e.NewValue)
        {
            passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
        }
    }

    private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox passwordBox)
            return;

        if (GetIsUpdating(passwordBox))
            return;

        passwordBox.Password = e.NewValue?.ToString() ?? string.Empty;
    }

    private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        var passwordBox = (PasswordBox)sender;

        SetIsUpdating(passwordBox, true);
        SetBoundPassword(passwordBox, passwordBox.Password);
        SetIsUpdating(passwordBox, false);
    }
}
