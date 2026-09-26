using System.Globalization;
using System.Windows.Data;

namespace SupportAdvance.Presentation.WpfTrial.Converters;

/// <summary>
/// 真偽値を反転するコンバーター
/// 【用途】処理中フラグ（IsLoading）から、入力欄・ボタンの有効／無効（IsEnabled）への変換
/// </summary>
public sealed class InverseBooleanConverter : IValueConverter
{
    /// <summary>
    /// 真偽値の反転
    /// </summary>
    /// <param name="value">変換元の値（<see cref="bool"/> を想定）</param>
    /// <param name="targetType">変換先の型（未使用）</param>
    /// <param name="parameter">コンバーターのパラメーター（未使用）</param>
    /// <param name="culture">カルチャ（未使用）</param>
    /// <returns><paramref name="value"/> が <see langword="false"/> の場合は <see langword="true"/>、<see langword="true"/> の場合は <see langword="false"/>。<see cref="bool"/> 以外の場合は <see langword="true"/></returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not bool flag || !flag;

    /// <summary>
    /// 逆変換（<see cref="Convert"/> と同じ反転）
    /// </summary>
    /// <param name="value">変換元の値（<see cref="bool"/> を想定）</param>
    /// <param name="targetType">変換先の型（未使用）</param>
    /// <param name="parameter">コンバーターのパラメーター（未使用）</param>
    /// <param name="culture">カルチャ（未使用）</param>
    /// <returns><paramref name="value"/> の反転</returns>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Convert(value, targetType, parameter, culture);
}
