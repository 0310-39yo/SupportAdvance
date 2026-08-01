using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.SharedKernel.Tests.Entities.Fixtures;

/// <summary>
/// テスト用RowId
/// テストで頻繁に使用するRowIdを簡単に生成するためのヘルパー
/// </summary>
public sealed class TestRowId
{
    /// <summary>
    /// 指定された値のRowIdを作成する
    /// </summary>
    /// <param name="value">主キー値（デフォルト=1）</param>
    /// <returns>RowIdのインスタンス</returns>
    public static RowId From(long value = 1)
    {
        return RowId.From(value);
    }

    /// <summary>
    /// 未採番状態のRowIdを作成する
    /// </summary>
    /// <returns>value=0のRowIdのインスタンス</returns>
    public static RowId New()
    {
        return RowId.New();
    }
}
