using SupportAdvance.Presentation.Shared.Icons;

namespace SupportAdvance.Tests.Presentation.Shared.Tests.Icons;

/// <summary>
/// <see cref="FluentIconCatalog"/>（アイコンの対応表）の検証
/// </summary>
/// <remarks>
/// <para>【目的】アイコンを追加するときの追加漏れ・コードポイントの重複・範囲外の値を検出する</para>
/// </remarks>
public class FluentIconCatalogTests
{
    [Fact]
    public void EveryAppIcon_HasACodePoint()
    {
        var missing = Enum.GetValues<AppIcon>().Where(icon => !FluentIconCatalog.All.ContainsKey(icon)).ToList();

        Assert.True(missing.Count == 0, $"対応表に無いアイコン: {string.Join(", ", missing)}");
    }

    [Fact]
    public void CodePoints_AreUnique()
    {
        var duplicated = FluentIconCatalog.All
            .GroupBy(pair => pair.Value)
            .Where(group => group.Count() > 1)
            .Select(group => $"0x{group.Key:X4}: {string.Join(", ", group.Select(pair => pair.Key))}")
            .ToList();

        Assert.True(duplicated.Count == 0, $"コードポイントが重複: {string.Join(" / ", duplicated)}");
    }

    [Fact]
    public void CodePoints_AreInThePrivateUseArea()
    {
        // Fluent UI System Icons のフォントは、私用領域（U+E000〜U+F8FF）に字形を割り当てている
        var outOfRange = FluentIconCatalog.All.Where(pair => pair.Value is < 0xE000 or > 0xF8FF).ToList();

        Assert.True(outOfRange.Count == 0, $"私用領域の範囲外: {string.Join(", ", outOfRange.Select(pair => pair.Key))}");
    }

    [Fact]
    public void GetGlyph_ReturnsASingleCharacterForTheCodePoint()
    {
        foreach (var (icon, codePoint) in FluentIconCatalog.All)
        {
            var glyph = FluentIconCatalog.GetGlyph(icon);

            Assert.Equal(1, glyph.Length);
            Assert.Equal(codePoint, glyph[0]);
        }
    }

    [Fact]
    public void GetGlyph_UnknownIcon_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FluentIconCatalog.GetGlyph((AppIcon)9999));
    }
}
