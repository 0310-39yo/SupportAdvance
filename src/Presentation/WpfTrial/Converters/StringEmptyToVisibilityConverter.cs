using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SupportAdvance.Presentation.WpfTrial.Converters;

/// <summary>
/// 文字列が空でない場合に Visible、空の場合に Collapsed を返すコンバーター
/// 【用途】エラーメッセージラベルの表示/非表示切り替え
/// </summary>
public sealed class StringEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
