namespace SupportAdvance.Presentation.WinTrial.Views;

/// <summary>
/// 未実装の画面を表す MDI 子フォーム（メインウィンドウのタブとして表示）
/// </summary>
/// <remarks>
/// <para>【設計】画面ごとの ViewModel が用意できるまでのプレースホルダー。対応する ViewModel は <c>EmptyTabContentViewModel</c></para>
/// </remarks>
public sealed class EmptyTabForm : Form
{
    /// <summary>
    /// <see cref="EmptyTabForm"/> クラスの新しいインスタンスの初期化
    /// </summary>
    public EmptyTabForm()
    {
        Controls.Add(new Label
        {
            Text = "（未実装）",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        });
    }
}
