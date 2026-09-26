using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using SupportAdvance.Presentation.Shared.Icons;

namespace SupportAdvance.Presentation.WinTrial.Icons;

/// <summary>
/// Fluent UI System Icons のアイコンフォントを読み込み、アイコンの画像の作成
/// </summary>
/// <remarks>
/// <para>【設計】フォントファイルは WinTrial.csproj で埋め込みリソースとして取り込み、メモリから読み込む（アプリのフォルダーにフォントファイルを置かない）。
/// アイコンの追加は <see cref="FluentIconCatalog"/> の手順を参照</para>
/// <para>【注意】<see cref="PrivateFontCollection"/> とフォントのメモリは、アプリケーションの終了まで保持する（解放しない）</para>
/// </remarks>
internal static class FluentIconFont
{
    // FontFamily は PrivateFontCollection が生きている間だけ有効なため、コレクションも静的に保持する
    private static PrivateFontCollection? _collection;

    private static readonly Lazy<FontFamily> Family = new(LoadFontFamily);

    /// <summary>
    /// アイコンの画像の作成
    /// </summary>
    /// <param name="icon">アイコンの名前</param>
    /// <param name="pixelSize">画像の一辺の大きさ（ピクセル）</param>
    /// <param name="color">アイコンの色</param>
    /// <returns>透明な背景に、指定した色でアイコンを描いた画像。呼び出し元による破棄が必要</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelSize"/> が 1 未満の場合、または <paramref name="icon"/> が対応表に無い場合</exception>
    public static Bitmap CreateBitmap(AppIcon icon, int pixelSize, Color color)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(pixelSize, 1);

        var glyph = FluentIconCatalog.GetGlyph(icon);
        var bitmap = new Bitmap(pixelSize, pixelSize);

        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

        using var font = new Font(Family.Value, pixelSize, FontStyle.Regular, GraphicsUnit.Pixel);
        using var brush = new SolidBrush(color);

        // 【注意】GenericTypographic（余白なし）で左上に描く。矩形に収めて中央揃えにすると何も描かれず、
        // 既定の書式では余白のぶん右にずれて欠ける（このフォントは 1 文字が 1em の正方形に収まる設計）
        graphics.DrawString(glyph, font, brush, 0, 0, StringFormat.GenericTypographic);
        return bitmap;
    }

    private static FontFamily LoadFontFamily()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(FluentIconCatalog.FontFileName)
            ?? throw new InvalidOperationException($"アイコンフォントのリソースが見つかりません: {FluentIconCatalog.FontFileName}");

        var data = new byte[stream.Length];
        stream.ReadExactly(data);

        // PrivateFontCollection はメモリを解放しないため、アプリケーションの終了まで保持する
        var pointer = Marshal.AllocCoTaskMem(data.Length);
        Marshal.Copy(data, 0, pointer, data.Length);

        _collection = new PrivateFontCollection();
        _collection.AddMemoryFont(pointer, data.Length);

        return _collection.Families.Single(f => f.Name == FluentIconCatalog.FontFamilyName);
    }
}
