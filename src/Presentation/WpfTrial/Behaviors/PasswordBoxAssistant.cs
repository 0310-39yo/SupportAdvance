using System.Windows;
using System.Windows.Controls;

namespace SupportAdvance.Presentation.WpfTrial.Behaviors;

/// <summary>
/// <see cref="PasswordBox.Password"/> をバインド可能にする添付ビヘイビア
/// </summary>
/// <remarks>
/// <para>【背景】<c>PasswordBox.Password</c> はセキュリティ上の理由から <c>DependencyProperty</c> ではないため、通常の <c>{Binding}</c> では ViewModel と同期できない。この添付プロパティが仲介</para>
/// <para>【<c>Attach</c> が必要な理由】<c>PasswordChanged</c> イベントの購読を <c>BoundPassword</c> の <c>PropertyChangedCallback</c> 内で行うと、バインディング初期値（既定値の空文字列）と ViewModel 側の初期値（同じく空文字列）が一致する場合に、WPF が「変更なし」と判断してコールバック自体が一度も呼ばれず、イベント購読が永久に行われない（何を入力しても ViewModel に反映されない）。<c>Attach</c> は既定値 <see langword="false"/> → <see langword="true"/> への遷移が必ず発生するため、これを使って <c>PasswordChanged</c> の購読タイミングを <c>BoundPassword</c> の初期値と切り離す</para>
/// </remarks>
/// <example>
/// <code>
/// &lt;PasswordBox behaviors:PasswordBoxAssistant.Attach="True"
///              behaviors:PasswordBoxAssistant.BoundPassword="{Binding Password, Mode=TwoWay}" /&gt;
/// </code>
/// </example>
public static class PasswordBoxAssistant
{
    /// <summary>
    /// <c>BoundPassword</c> 添付プロパティの識別子（<see cref="PasswordBox.Password"/> と双方向に同期する文字列）
    /// </summary>
    /// <remarks>
    /// <para>【事前条件】同期には <c>Attach="True"</c> の指定が必要</para>
    /// <para>【注意】既定で双方向バインド（<see cref="FrameworkPropertyMetadataOptions.BindsTwoWayByDefault"/>）</para>
    /// </remarks>
    public static readonly DependencyProperty BoundPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BoundPassword",
            typeof(string),
            typeof(PasswordBoxAssistant),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundPasswordChanged));

    /// <summary>
    /// <c>Attach</c> 添付プロパティの識別子（<see langword="true"/> で <see cref="PasswordBox.PasswordChanged"/> の購読を開始）
    /// </summary>
    /// <remarks>
    /// <para>【設計】購読の開始を <c>BoundPassword</c> の初期値と切り離すための専用プロパティ（詳細は型の説明）</para>
    /// </remarks>
    public static readonly DependencyProperty AttachProperty =
        DependencyProperty.RegisterAttached(
            "Attach",
            typeof(bool),
            typeof(PasswordBoxAssistant),
            new PropertyMetadata(false, OnAttachChanged));

    private static readonly DependencyProperty IsUpdatingProperty =
        DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(PasswordBoxAssistant));

    /// <summary>
    /// <c>BoundPassword</c> 添付プロパティの値の取得
    /// </summary>
    /// <param name="dp">対象の要素（通常は <see cref="PasswordBox"/>）</param>
    /// <returns>現在のパスワード文字列</returns>
    public static string GetBoundPassword(DependencyObject dp) => (string)dp.GetValue(BoundPasswordProperty);

    /// <summary>
    /// <c>BoundPassword</c> 添付プロパティの値の設定
    /// </summary>
    /// <param name="dp">対象の要素（通常は <see cref="PasswordBox"/>）</param>
    /// <param name="value">設定するパスワード文字列</param>
    /// <remarks>
    /// <para>【重要】バインディングを切断しないよう <see cref="DependencyObject.SetCurrentValue"/> で設定</para>
    /// </remarks>
    // SetValue ではなく SetCurrentValue を使う。
    // SetValue はバインディングをローカル値で上書きして切断してしまうため、
    // PasswordChanged イベントごとに呼ぶとバインディングが失われ、
    // ViewModel.Password が更新されなくなる。
    public static void SetBoundPassword(DependencyObject dp, string value) => dp.SetCurrentValue(BoundPasswordProperty, value);

    /// <summary>
    /// <c>Attach</c> 添付プロパティの値の取得
    /// </summary>
    /// <param name="dp">対象の要素</param>
    /// <returns>同期が有効な場合は <see langword="true"/></returns>
    public static bool GetAttach(DependencyObject dp) => (bool)dp.GetValue(AttachProperty);

    /// <summary>
    /// <c>Attach</c> 添付プロパティの値の設定
    /// </summary>
    /// <param name="dp">対象の要素（<see cref="PasswordBox"/> 以外の場合、値は設定されるが同期なし）</param>
    /// <param name="value"><see langword="true"/> で同期の開始、<see langword="false"/> で同期の停止</param>
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
