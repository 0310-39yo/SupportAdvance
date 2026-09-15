namespace SupportAdvance.Infrastructure.Tests.Providers;

using Xunit;
using Moq;
using SupportAdvance.Common.Configuration;
using SupportAdvance.Infrastructure.Providers;

/// <summary>
/// SupportAdvance.Infrastructure.Providers.SequenceProvider のテスト
///
/// 【テスト対象】SequenceProvider（本実装）
/// 【テスト方針】Unit テスト（接続文字列解決ロジック、パラメータ検証）
/// 【スコープ】
/// - DB接続を要する部分（GetNextValuesInternal）は結合テストで別途検証（Phase 5参照）
/// - コンストラクタの接続文字列解決ロジック、GetNextValuesAsync のパラメータ検証をテスト
/// 【テストケース】
/// - コンストラクタが appSettings=null で ArgumentNullException を投げる
/// - コンストラクタが接続文字列を解決できない場合に InvalidOperationException を投げる
/// - GetConnectionString の優先順位テスト
/// - GetNextValuesAsync(0) / GetNextValuesAsync(-1) が ArgumentException を投げる
/// </summary>
public class SequenceProviderTests
{
    /// <summary>
    /// Test1: GetConnectionString の優先順位テスト - "Default" キーが存在する場合
    ///
    /// 【期待値】
    /// - "Default" キーの値を返す
    /// </summary>
    [Fact]
    public void GetConnectionString_ResolveDefault_WhenDefaultKeyExists()
    {
        // Arrange
        var mockAppSettings = new Mock<IAppSettings>();
        mockAppSettings
            .Setup(x => x.ConnectionStrings)
            .Returns(new Dictionary<string, string>
            {
                { "Default", "DefaultConnectionString" },
                { "SupportAdvance", "SupportAdvanceConnectionString" }
            });

        // Act
        var result = SequenceProvider.GetConnectionString(mockAppSettings.Object);

        // Assert
        Assert.Equal("DefaultConnectionString", result);
    }

    /// <summary>
    /// Test2: GetConnectionString の優先順位テスト - "SupportAdvance" キーが存在する場合
    ///
    /// 【期待値】
    /// - "Default" がなく "SupportAdvance" があれば、"SupportAdvance" の値を返す
    /// </summary>
    [Fact]
    public void GetConnectionString_ResolveSupportAdvance_WhenDefaultNotExists()
    {
        // Arrange
        var mockAppSettings = new Mock<IAppSettings>();
        mockAppSettings
            .Setup(x => x.ConnectionStrings)
            .Returns(new Dictionary<string, string>
            {
                { "SupportAdvance", "SupportAdvanceConnectionString" }
            });

        // Act
        var result = SequenceProvider.GetConnectionString(mockAppSettings.Object);

        // Assert
        Assert.Equal("SupportAdvanceConnectionString", result);
    }

    /// <summary>
    /// Test3: GetConnectionString の優先順位テスト - 最初のキーが存在する場合
    ///
    /// 【期待値】
    /// - "Default" "SupportAdvance" もなければ、最初のキー（任意）の値を返す
    /// </summary>
    [Fact]
    public void GetConnectionString_ResolveFirstKey_WhenDefaultAndSupportAdvanceNotExists()
    {
        // Arrange
        var mockAppSettings = new Mock<IAppSettings>();
        mockAppSettings
            .Setup(x => x.ConnectionStrings)
            .Returns(new Dictionary<string, string>
            {
                { "CustomConnection", "CustomConnectionString" }
            });

        // Act
        var result = SequenceProvider.GetConnectionString(mockAppSettings.Object);

        // Assert
        Assert.Equal("CustomConnectionString", result);
    }

    /// <summary>
    /// Test4: GetConnectionString が null を返す場合 - ConnectionStrings が空
    ///
    /// 【期待値】
    /// - null を返す（コンストラクタで InvalidOperationException に変わる）
    /// </summary>
    [Fact]
    public void GetConnectionString_ReturnsNull_WhenConnectionStringsEmpty()
    {
        // Arrange
        var mockAppSettings = new Mock<IAppSettings>();
        mockAppSettings
            .Setup(x => x.ConnectionStrings)
            .Returns(new Dictionary<string, string>());

        // Act
        var result = SequenceProvider.GetConnectionString(mockAppSettings.Object);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Test5: コンストラクタが appSettings=null で ArgumentNullException を投げる
    ///
    /// 【期待値】
    /// - ArgumentNullException を投げる
    /// </summary>
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenAppSettingsIsNull()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new SequenceProvider(null!));
        Assert.Equal("appSettings", ex.ParamName);
    }

    /// <summary>
    /// Test6: コンストラクタが接続文字列を解決できない場合に InvalidOperationException を投げる
    ///
    /// 【期待値】
    /// - InvalidOperationException を投げる
    /// - メッセージに "No connection string is configured" を含む
    /// </summary>
    [Fact]
    public void Constructor_ThrowsInvalidOperationException_WhenNoConnectionStringResolved()
    {
        // Arrange
        var mockAppSettings = new Mock<IAppSettings>();
        mockAppSettings
            .Setup(x => x.ConnectionStrings)
            .Returns(new Dictionary<string, string>()); // 空の辞書

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(
            () => new SequenceProvider(mockAppSettings.Object));
        Assert.Contains("No connection string is configured", ex.Message);
    }

    /// <summary>
    /// Test7: GetNextValuesAsync が count=0 で ArgumentException を投げる
    ///
    /// 【期待値】
    /// - ArgumentException を投げる
    /// - パラメータ名は "count"
    /// </summary>
    [Fact]
    public async Task GetNextValuesAsync_ThrowsArgumentException_WhenCountIsZero()
    {
        // Arrange
        var mockAppSettings = new Mock<IAppSettings>();
        mockAppSettings
            .Setup(x => x.ConnectionStrings)
            .Returns(new Dictionary<string, string>
            {
                { "Default", "Data Source=.;Initial Catalog=test;" }
            });
        var provider = new SequenceProvider(mockAppSettings.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => provider.GetNextValuesAsync(0));
        Assert.Equal("count", ex.ParamName);
        Assert.Contains("Count must be greater than 0", ex.Message);
    }

    /// <summary>
    /// Test8: GetNextValuesAsync が count=-1 で ArgumentException を投げる
    ///
    /// 【期待値】
    /// - ArgumentException を投げる
    /// - パラメータ名は "count"
    /// </summary>
    [Fact]
    public async Task GetNextValuesAsync_ThrowsArgumentException_WhenCountIsNegative()
    {
        // Arrange
        var mockAppSettings = new Mock<IAppSettings>();
        mockAppSettings
            .Setup(x => x.ConnectionStrings)
            .Returns(new Dictionary<string, string>
            {
                { "Default", "Data Source=.;Initial Catalog=test;" }
            });
        var provider = new SequenceProvider(mockAppSettings.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => provider.GetNextValuesAsync(-1));
        Assert.Equal("count", ex.ParamName);
        Assert.Contains("Count must be greater than 0", ex.Message);
    }
}
