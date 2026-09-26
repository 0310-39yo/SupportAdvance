using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SupportAdvance.Presentation.Shared.Icons;

namespace SupportAdvance.Presentation.WpfTrial.Controls;

/// <summary>
/// Fluent UI System Icons のアイコンを表示するコントロール
/// </summary>
/// <remarks>
/// <para>【使い方】&lt;controls:FluentIcon Icon="Save" FontSize="18" Foreground="Gray" /&gt;</para>
/// <para>【設計】アイコンフォントの 1 文字を表示する <see cref="TextBlock"/>。大きさは <see cref="TextBlock.FontSize"/>、色は <see cref="TextBlock.Foreground"/> で指定する。
/// アイコンの追加は <see cref="FluentIconCatalog"/> の手順を参照</para>
/// <para>【前提】フォントファイルは WpfTrial.csproj で <c>Assets/Fonts</c> にリソースとして取り込む</para>
/// </remarks>
public sealed class FluentIcon : TextBlock
{
    private static readonly FontFamily IconFontFamily =
        new($"pack://application:,,,/Assets/Fonts/#{FluentIconCatalog.FontFamilyName}");

    /// <summary>
    /// <c>Icon</c> 依存関係プロパティの識別子（表示するアイコン）
    /// </summary>
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(AppIcon?),
            typeof(FluentIcon),
            new PropertyMetadata(null, OnIconChanged));

    /// <summary>
    /// <see cref="FluentIcon"/> クラスの新しいインスタンスの初期化
    /// </summary>
    public FluentIcon()
    {
        FontFamily = IconFontFamily;
        TextAlignment = TextAlignment.Center;
        VerticalAlignment = VerticalAlignment.Center;
        IsHitTestVisible = false;
    }

    /// <summary>
    /// 表示するアイコン
    /// </summary>
    /// <value>未設定（<see langword="null"/>）の場合は何も表示しない</value>
    public AppIcon? Icon
    {
        get => (AppIcon?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (FluentIcon)d;
        control.Text = e.NewValue is AppIcon icon ? FluentIconCatalog.GetGlyph(icon) : string.Empty;
    }
}
