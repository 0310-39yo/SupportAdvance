using System.Windows.Controls;

namespace SupportAdvance.Presentation.WpfTrial.ViewModels.Models;

/// <summary>
/// 開いているタブのデータを保持するモデルクラス
/// </summary>
public class TabItemData
{
    /// <summary>
    /// タブのヘッダー（表示名）
    /// </summary>
    public string Header { get; set; } = string.Empty;

    /// <summary>
    /// タブのコンテンツ（UserControl）
    /// </summary>
    public UserControl Content { get; set; } = null!;
}
