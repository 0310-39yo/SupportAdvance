using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using SupportAdvance.Presentation.Shared.Icons;
using SupportAdvance.Presentation.WpfTrial.Controls;

namespace SupportAdvance.Presentation.WpfTrial.Markup;

/// <summary>
/// Fluent UI System Icons のアイコンを、<see cref="ImageSource"/> として提供するマークアップ拡張
/// </summary>
/// <remarks>
/// <para>【用途】アイコンを <see cref="System.Windows.Media.ImageSource"/> で受け取るコントロール（Ribbon の <c>SmallIcon</c>／<c>LargeIcon</c> など）に指定する。
/// 文字として表示できる場所には <see cref="FluentIcon"/> を使う</para>
/// <para>【使い方】&lt;syncfusion:RibbonButton SmallIcon="{markup:FluentIconImage Icon=Save, Size=16}" /&gt;</para>
/// <para>【設計】アイコンフォントの字形をベクターの図形に変換して描画するため、拡大しても粗くならない。
/// 画像の大きさは一辺 <see cref="Size"/> の正方形（字形の外側の余白も含めるため、アイコンごとに大きさが変わらない）</para>
/// </remarks>
[MarkupExtensionReturnType(typeof(ImageSource))]
public sealed class FluentIconImageExtension : MarkupExtension
{
    // Office2019White の文字色に近い濃い灰色
    private static readonly Color DefaultColor = Color.FromRgb(0x44, 0x44, 0x44);

    /// <summary>
    /// 表示するアイコン
    /// </summary>
    public AppIcon Icon { get; set; }

    /// <summary>
    /// 画像の一辺の大きさ（デバイス非依存ピクセル）
    /// </summary>
    /// <value>既定値は 16</value>
    public double Size { get; set; } = 16;

    /// <summary>
    /// アイコンの色
    /// </summary>
    /// <value>既定値は濃い灰色（<c>#444444</c>）</value>
    public Color Color { get; set; } = DefaultColor;

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException"><see cref="Size"/> が 0 以下の場合</exception>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (Size <= 0)
        {
            throw new InvalidOperationException("Size には 0 より大きい値を指定してください");
        }

        var typeface = new Typeface(FluentIcon.IconFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        var brush = new SolidColorBrush(Color);
        brush.Freeze();

        var text = new FormattedText(
            FluentIconCatalog.GetGlyph(Icon),
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            typeface,
            Size,
            brush,
            1.0);

        // 字形の図形に加えて、一辺 Size の正方形の透明な領域を含めて、画像の大きさを一定にする
        var group = new DrawingGroup();
        group.Children.Add(new GeometryDrawing(Brushes.Transparent, null, new RectangleGeometry(new Rect(0, 0, Size, Size))));
        group.Children.Add(new GeometryDrawing(brush, null, text.BuildGeometry(new Point(0, 0))));
        group.Freeze();

        var image = new DrawingImage(group);
        image.Freeze();
        return image;
    }
}
