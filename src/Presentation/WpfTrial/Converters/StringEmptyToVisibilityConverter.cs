using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SupportAdvance.Presentation.WpfTrial.Converters;

/// <summary>
/// 文字列が空でない場合に Visible、空の場合に Collapsed を返すコンバーター
/// </summary>
/// <remarks>
/// <para>【用途】エラーメッセージラベルの表示/非表示切り替え</para>
/// </remarks>
public sealed class StringEmptyToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// 文字列から <see cref="Visibility"/> への変換
    /// </summary>
    /// <param name="value">変換元の値（文字列を想定）</param>
    /// <param name="targetType">変換先の型（未使用）</param>
    /// <param name="parameter">コンバーターのパラメーター（未使用）</param>
    /// <param name="culture">カルチャ（未使用）</param>
    /// <returns>
    /// <paramref name="value"/> が <see langword="null"/>・空文字・文字列以外の場合は <see cref="Visibility.Collapsed"/>、それ以外は <see cref="Visibility.Visible"/>
    /// </returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>
    /// 逆変換（未対応）
    /// </summary>
    /// <param name="value">変換元の値</param>
    /// <param name="targetType">変換先の型</param>
    /// <param name="parameter">コンバーターのパラメーター</param>
    /// <param name="culture">カルチャ</param>
    /// <returns>なし（常に例外）</returns>
    /// <exception cref="NotSupportedException">常に送出（一方向バインド専用）</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
