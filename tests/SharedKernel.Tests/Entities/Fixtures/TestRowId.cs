using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using SupportAdvance.SharedKernel.Tests.ValueObjects.Identifiers.Fixtures;

namespace SupportAdvance.SharedKernel.Tests.Entities.Fixtures;

/// <summary>
/// テスト用RowId ファクトリ
/// テストで頻繁に使用するRowIdを簡単に生成するためのヘルパー
/// TestPersonRowId（テスト用具体的な実装）を使用
/// </summary>
public sealed class TestRowId
{
    /// <summary>
    /// 指定された値のRowIdを作成する
    /// </summary>
    /// <param name="value">主キー値（デフォルト=1）</param>
    /// <returns>TestPersonRowIdのインスタンス</returns>
    public static RowId From(long value = 1)
    {
        return TestPersonRowId.From(value);
    }
}

