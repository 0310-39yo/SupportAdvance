using CommunityToolkit.Mvvm.ComponentModel;

namespace SupportAdvance.Presentation.Shared.ViewModels.Tabs;

/// <summary>
/// メインウィンドウのタブ 1 枚分の ViewModel
/// </summary>
/// <remarks>
/// <para>【設計】View 型（UserControl など）の保持なし。表示する画面は <see cref="ContentViewModel"/> の型に対応する DataTemplate で決定</para>
/// </remarks>
public partial class TabItemViewModel : ObservableObject
{
    /// <summary>
    /// タブのヘッダー（表示名）
    /// </summary>
    [ObservableProperty]
    private string _header;

    /// <summary>
    /// タブの内容の ViewModel
    /// </summary>
    public object ContentViewModel { get; }

    /// <summary>
    /// <see cref="TabItemViewModel"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="header">タブのヘッダー（表示名）</param>
    /// <param name="contentViewModel">タブの内容の ViewModel</param>
    /// <exception cref="ArgumentNullException"><paramref name="header"/> または <paramref name="contentViewModel"/> が <see langword="null"/> の場合</exception>
    public TabItemViewModel(string header, object contentViewModel)
    {
        ArgumentNullException.ThrowIfNull(header);
        ArgumentNullException.ThrowIfNull(contentViewModel);

        _header = header;
        ContentViewModel = contentViewModel;
    }
}
