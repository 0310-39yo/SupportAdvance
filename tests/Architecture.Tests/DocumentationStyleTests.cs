using System.Text.RegularExpressions;
using Xunit;

namespace SupportAdvance.Tests.Architecture.Tests;

/// <summary>
/// XML ドキュメントコメントのガイドのうち、機械的に判定できる規則をソースの走査で検証
/// </summary>
/// <remarks>
/// <para>【対象】<c>src/</c> 配下の C# ソース（<c>obj</c>／<c>bin</c>／<c>.vshistory</c> と自動生成ファイルは除く）。テストコードは対象外</para>
/// <para>【規則】</para>
/// <list type="bullet">
/// <item><description>A1 <c>&lt;summary&gt;</c> は 3 行形式（1 行形式の禁止）</description></item>
/// <item><description>A2 最後の文の末尾に「。」を付けない</description></item>
/// <item><description>A4 【見出し】は <c>&lt;summary&gt;</c> ではなく <c>&lt;remarks&gt;</c> の <c>&lt;para&gt;</c> に書く</description></item>
/// <item><description>A5 プロパティには <c>&lt;returns&gt;</c> ではなく <c>&lt;value&gt;</c></description></item>
/// <item><description>A6 コンストラクターに <c>&lt;returns&gt;</c> を書かない</description></item>
/// <item><description>A7 空の <c>&lt;param&gt;</c> を残さない</description></item>
/// <item><description>A8 Markdown のコードブロック（3 連続のバッククォート）を使わない</description></item>
/// <item><description>A10 <c>Equals</c>／<c>GetHashCode</c> の override は <c>&lt;inheritdoc/&gt;</c></description></item>
/// </list>
/// <para>【対象外】体言止め（A3）は、ガイドが例外を認めていて誤検知が避けられないため、テストにせずレビューのチェックリスト（ガイド §9）で担保。コメントなし（CS1591）はビルドエラーで検出</para>
/// <para>【参照】docs/Assistance/Guides/XMLドキュメントコメント_ガイド.md、docs/Assistance/Plans/20260926_原則完全準拠_実装計画.md（フェーズ 8）</para>
/// </remarks>
public class DocumentationStyleTests
{
    private static readonly Regex DocLine = new(@"^\s*///(?!/)", RegexOptions.Compiled);

    private static readonly string TextTags = "summary|remarks|param|typeparam|returns|value|exception|para|description";

    #region ソースの走査

    [Fact]
    public void SourceComments_ShouldFollowMachineCheckableGuideRules()
    {
        var files = SourceFiles();
        Assert.True(files.Count > 150, $"走査対象のファイルが少なすぎる（空振りの疑い）: {files.Count}");

        var violations = new List<string>();
        var blockCount = 0;
        foreach (var file in files)
        {
            var relative = Path.GetRelativePath(RepositoryRoot(), file);
            foreach (var block in ReadBlocks(File.ReadAllLines(file)))
            {
                blockCount++;
                violations.AddRange(CheckBlock(block.Text, block.Declaration).Select(v => $"{relative}:{block.StartLine}: {v}"));
            }
        }

        Assert.True(blockCount > 1000, $"走査したコメントブロックが少なすぎる（空振りの疑い）: {blockCount}");
        Assert.True(
            violations.Count == 0,
            $"XML ドキュメントコメントのガイド違反（{violations.Count} 件）:{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }

    #endregion

    #region 検出処理の自己検証

    [Fact]
    public void Detector_FindsOneLineSummary()
    {
        var text = new[] { "<summary>1 行形式</summary>" };

        Assert.Contains(CheckBlock(text, "public int X { get; }"), v => v.StartsWith("A1"));
    }

    [Fact]
    public void Detector_FindsTrailingPeriod_InSingleAndMultiLineText()
    {
        var single = new[] { "<summary>", "本文", "</summary>", "<param name=\"x\">説明。</param>" };
        var multi = new[] { "<summary>", "1 文目。", "2 文目。", "</summary>" };

        Assert.Contains(CheckBlock(single, "void M(int x)"), v => v.StartsWith("A2"));
        Assert.Contains(CheckBlock(multi, "void M()"), v => v.StartsWith("A2"));
    }

    [Fact]
    public void Detector_AllowsPeriodBetweenSentences()
    {
        var text = new[] { "<summary>", "1 文目。2 文目", "</summary>", "<param name=\"x\">説明。補足</param>" };

        Assert.Empty(CheckBlock(text, "void M(int x)"));
    }

    [Fact]
    public void Detector_FindsHeadingInSummary()
    {
        var text = new[] { "<summary>", "要約", "【責務】詳細", "</summary>" };

        Assert.Contains(CheckBlock(text, "public class C"), v => v.StartsWith("A4"));
    }

    [Fact]
    public void Detector_FindsReturnsOnPropertyAndConstructor()
    {
        var text = new[] { "<summary>", "本文", "</summary>", "<returns>値</returns>" };

        Assert.Contains(CheckBlock(text, "public int X { get; }"), v => v.StartsWith("A5"));
        Assert.Contains(CheckBlock(text, "public Foo(int x)"), v => v.StartsWith("A6"));
        Assert.DoesNotContain(CheckBlock(text, "public int Compute(int x)"), v => v.StartsWith("A5") || v.StartsWith("A6"));
    }

    [Fact]
    public void Detector_FindsEmptyParamAndMarkdownFence()
    {
        var empty = new[] { "<summary>", "本文", "</summary>", "<param name=\"x\"></param>" };
        var fence = new[] { "<summary>", "本文", "</summary>", "<remarks>", "```", "</remarks>" };

        Assert.Contains(CheckBlock(empty, "void M(int x)"), v => v.StartsWith("A7"));
        Assert.Contains(CheckBlock(fence, "void M()"), v => v.StartsWith("A8"));
    }

    [Fact]
    public void Detector_FindsEqualsOverrideWithoutInheritdoc()
    {
        var text = new[] { "<summary>", "等価性の判定", "</summary>" };
        var inherit = new[] { "<inheritdoc/>" };

        Assert.Contains(CheckBlock(text, "public override bool Equals(object? obj) => false;"), v => v.StartsWith("A10"));
        Assert.Contains(CheckBlock(text, "public override int GetHashCode() => 0;"), v => v.StartsWith("A10"));
        Assert.Empty(CheckBlock(inherit, "public override bool Equals(object? obj) => false;"));
    }

    [Fact]
    public void Detector_DoesNotFlagCompliantBlock()
    {
        var text = new[]
        {
            "<summary>",
            "従業員の退職処理",
            "</summary>",
            "<param name=\"retiredOn\">退職日（JST）</param>",
            "<remarks>",
            "<para>【副作用】<c>RetiredOn</c> の更新</para>",
            "</remarks>"
        };

        Assert.Empty(CheckBlock(text, "public void Retire(LocalDateTime retiredOn)"));
    }

    #endregion

    #region 検出処理

    private sealed record Block(int StartLine, string[] Text, string Declaration);

    private static IEnumerable<Block> ReadBlocks(string[] lines)
    {
        var i = 0;
        while (i < lines.Length)
        {
            if (!DocLine.IsMatch(lines[i]))
            {
                i++;
                continue;
            }

            var start = i;
            while (i < lines.Length && DocLine.IsMatch(lines[i]))
            {
                i++;
            }

            var text = lines[start..i].Select(l => Regex.Replace(l, @"^\s*///\s?", string.Empty)).ToArray();

            var j = i;
            while (j < lines.Length && (Regex.IsMatch(lines[j], @"^\s*(\[.*\]\s*)+$") || lines[j].Trim().Length == 0 || lines[j].TrimStart().StartsWith('#')))
            {
                j++;
            }

            yield return new Block(start + 1, text, j < lines.Length ? lines[j] : string.Empty);
        }
    }

    private static IEnumerable<string> CheckBlock(string[] text, string declaration)
    {
        var joined = string.Join("\n", text);

        if (text.Any(l => Regex.IsMatch(l, @"^\s*<summary>.+</summary>\s*$")))
        {
            yield return "A1 <summary> が 1 行形式";
        }

        if (Regex.IsMatch(joined, $@"。\s*</({TextTags})>"))
        {
            yield return "A2 最後の文の末尾に「。」がある";
        }

        var summary = Regex.Match(joined, @"(?s)<summary>(.*?)</summary>");
        if (summary.Success && summary.Groups[1].Value.Contains('【'))
        {
            yield return "A4 <summary> に【見出し】がある（<remarks> の <para> に移す）";
        }

        if (joined.Contains("<returns>"))
        {
            var isProperty = !declaration.Contains('(') && (declaration.Contains('{') || declaration.Contains("=>"));
            if (isProperty)
            {
                yield return "A5 プロパティに <returns> がある（<value> を使う）";
            }

            if (Regex.IsMatch(declaration, @"^\s*(public|private|protected|internal)\s+(static\s+)?[A-Z]\w*\s*\("))
            {
                yield return "A6 コンストラクターに <returns> がある";
            }
        }

        if (Regex.IsMatch(joined, @"<param name=""[^""]+""\s*(/>|>\s*</param>)"))
        {
            yield return "A7 空の <param> がある";
        }

        if (joined.Contains("```"))
        {
            yield return "A8 Markdown のコードブロックがある";
        }

        if (Regex.IsMatch(declaration, @"override\s+(bool\s+Equals\(object|int\s+GetHashCode\()") && !joined.Contains("<inheritdoc"))
        {
            yield return "A10 Equals／GetHashCode の override に <inheritdoc/> がない";
        }
    }

    private static List<string> SourceFiles()
    {
        var src = Path.Combine(RepositoryRoot(), "src");
        return Directory.EnumerateFiles(src, "*.cs", SearchOption.AllDirectories)
            .Where(f => !Regex.IsMatch(f, @"[\\/](obj|bin|\.vshistory)[\\/]"))
            .Where(f => !Regex.IsMatch(Path.GetFileName(f), @"\.(g|designer)\.cs$"))
            .ToList();
    }

    private static string RepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "src")) && Directory.Exists(Path.Combine(dir.FullName, "tests")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("リポジトリのルート（src と tests を持つフォルダ）が見つからない");
    }

    #endregion
}
