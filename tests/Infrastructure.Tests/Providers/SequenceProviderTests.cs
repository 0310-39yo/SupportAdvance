namespace SupportAdvance.Infrastructure.Tests.Providers;

using Xunit;
using SupportAdvance.Application.Abstractions.Identifiers;

/// <summary>
/// ISequenceProvider の Mock 実装テスト
///
/// 【テスト対象】MockSequenceProvider
/// 【テスト方針】Unit テスト（DB アクセスなし、メモリベース）
/// 【テストケース】
/// - GetNextValueAsync() 正常系
/// - GetNextValuesAsync(count) 正常系
/// - 複数呼び出しで重複なし
/// - 値が昇順であることを確認
/// </summary>
public class SequenceProviderTests
{
    /// <summary>
    /// MockSequenceProvider のテスト用実装
    ///
    /// 【責務】テスト時に連続した RowId を返す
    /// 【初期値】2147483648（テスト用開始値）
    /// </summary>
    private class MockSequenceProvider : ISequenceProvider
    {
        private long _counter = 2147483648;  // テスト用開始値

        public async Task<long> GetNextValueAsync()
        {
            return await Task.FromResult(_counter++);
        }

        public async Task<IReadOnlyList<long>> GetNextValuesAsync(int count = 1)
        {
            if (count <= 0)
                throw new ArgumentException("Count must be greater than 0.", nameof(count));

            var result = new List<long>(capacity: count);
            for (int i = 0; i < count; i++)
            {
                result.Add(_counter++);
            }

            return await Task.FromResult(result.AsReadOnly());
        }
    }

    /// <summary>
    /// Test1: GetNextValueAsync() 正常系
    ///
    /// 【期待値】
    /// - 2147483648 を返す（初期値）
    /// - 型は long
    /// </summary>
    [Fact]
    public async Task GetNextValueAsync_ReturnsValidLong_WhenCalled()
    {
        // Arrange
        var provider = new MockSequenceProvider();

        // Act
        var result = await provider.GetNextValueAsync();

        // Assert
        Assert.IsType<long>(result);
        Assert.Equal(2147483648L, result);
    }

    /// <summary>
    /// Test2: GetNextValueAsync() 複数呼び出しで重複なし
    ///
    /// 【期待値】
    /// - 3回呼び出すと 2147483648, 2147483649, 2147483650 を返す
    /// - 値が昇順
    /// </summary>
    [Fact]
    public async Task GetNextValueAsync_ReturnsIncrementingValues_OnMultipleCalls()
    {
        // Arrange
        var provider = new MockSequenceProvider();
        var firstValue = await provider.GetNextValueAsync();
        var secondValue = await provider.GetNextValueAsync();
        var thirdValue = await provider.GetNextValueAsync();

        // Assert
        Assert.Equal(2147483648L, firstValue);
        Assert.Equal(2147483649L, secondValue);
        Assert.Equal(2147483650L, thirdValue);

        // 値が昇順であることを確認
        Assert.True(firstValue < secondValue);
        Assert.True(secondValue < thirdValue);
    }

    /// <summary>
    /// Test3: GetNextValuesAsync(count) 正常系、単一値
    ///
    /// 【期待値】
    /// - count=1 の場合、長さ 1 のリストを返す
    /// </summary>
    [Fact]
    public async Task GetNextValuesAsync_ReturnsSingleValue_WhenCountIsOne()
    {
        // Arrange
        var provider = new MockSequenceProvider();

        // Act
        var result = await provider.GetNextValuesAsync(count: 1);

        // Assert
        Assert.Single(result);
        Assert.Equal(2147483648L, result[0]);
    }

    /// <summary>
    /// Test4: GetNextValuesAsync(count) 正常系、複数値
    ///
    /// 【期待値】
    /// - count=5 の場合、長さ 5 のリストを返す
    /// - 値が昇順
    /// - 重複なし
    /// </summary>
    [Fact]
    public async Task GetNextValuesAsync_ReturnsMultipleValues_WhenCountIsGreaterThanOne()
    {
        // Arrange
        var provider = new MockSequenceProvider();
        var count = 5;

        // Act
        var result = await provider.GetNextValuesAsync(count);

        // Assert
        Assert.Equal(count, result.Count);

        // 値が昇順であることを確認
        for (int i = 0; i < result.Count - 1; i++)
        {
            Assert.True(result[i] < result[i + 1],
                $"Values should be in ascending order. result[{i}]={result[i]}, result[{i+1}]={result[i+1]}");
        }

        // 重複がないことを確認
        var uniqueCount = result.Distinct().Count();
        Assert.Equal(count, uniqueCount);
    }

    /// <summary>
    /// Test5: GetNextValuesAsync() デフォルト count パラメータ
    ///
    /// 【期待値】
    /// - count パラメータなしの場合、デフォルト count=1
    /// - 長さ 1 のリストを返す
    /// </summary>
    [Fact]
    public async Task GetNextValuesAsync_ReturnsDefaultOne_WhenCountNotSpecified()
    {
        // Arrange
        var provider = new MockSequenceProvider();

        // Act
        var result = await provider.GetNextValuesAsync();

        // Assert
        Assert.Single(result);
    }

    /// <summary>
    /// Test6: GetNextValuesAsync(count) 異常系、count が 0 以下
    ///
    /// 【期待値】
    /// - ArgumentException を throw
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task GetNextValuesAsync_ThrowsArgumentException_WhenCountIsLessOrEqualZero(int invalidCount)
    {
        // Arrange
        var provider = new MockSequenceProvider();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => provider.GetNextValuesAsync(invalidCount));
    }

    /// <summary>
    /// Test7: 複数の GetNextValuesAsync() 呼び出しで重複なし
    ///
    /// 【期待値】
    /// - 最初に 3 個取得、次に 2 個取得
    /// - 合計 5 個の値すべてが重複なし
    /// </summary>
    [Fact]
    public async Task GetNextValuesAsync_NoduplicatesAcrossMultipleCalls()
    {
        // Arrange
        var provider = new MockSequenceProvider();

        // Act
        var firstBatch = await provider.GetNextValuesAsync(count: 3);
        var secondBatch = await provider.GetNextValuesAsync(count: 2);

        // Assert
        var allValues = firstBatch.Concat(secondBatch).ToList();
        var uniqueValues = allValues.Distinct().ToList();

        Assert.Equal(allValues.Count, uniqueValues.Count);
        Assert.Equal(5, uniqueValues.Count);

        // 昇順であることを確認
        for (int i = 0; i < allValues.Count - 1; i++)
        {
            Assert.True(allValues[i] < allValues[i + 1]);
        }
    }
}
