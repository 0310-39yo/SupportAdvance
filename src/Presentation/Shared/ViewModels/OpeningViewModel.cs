using CommunityToolkit.Mvvm.ComponentModel;

namespace SupportAdvance.Presentation.Shared.ViewModels;

/// <summary>
/// オープニング画面（起動後、ナビゲーションが選ばれるまで表示する画面）の ViewModel
/// </summary>
/// <remarks>
/// <para>【表示内容】右下に、製品名（<see cref="ProductName"/>）と、その下にアプリケーションの名前（<see cref="ApplicationName"/>）</para>
/// <para>【将来】お知らせなどの通知も、この ViewModel に追加して表示する予定（今回は未実装）</para>
/// </remarks>
public partial class OpeningViewModel : ObservableObject
{
    /// <summary>
    /// 製品名
    /// </summary>
    public const string DefaultProductName = "Support Advance";

    /// <summary>
    /// 製品名の表示
    /// </summary>
    [ObservableProperty]
    private string _productName = DefaultProductName;

    /// <summary>
    /// アプリケーションの名前の表示（製品名の下に表示。例: <c>WpfTrial</c>）
    /// </summary>
    [ObservableProperty]
    private string _applicationName;

    /// <summary>
    /// <see cref="OpeningViewModel"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="appIdentity">起動しているアプリケーションの識別情報</param>
    /// <exception cref="ArgumentNullException"><paramref name="appIdentity"/> が <see langword="null"/> の場合</exception>
    public OpeningViewModel(AppIdentity appIdentity)
    {
        ArgumentNullException.ThrowIfNull(appIdentity);

        _applicationName = appIdentity.Name;
    }
}
