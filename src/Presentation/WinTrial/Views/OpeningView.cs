using System.ComponentModel;
using SupportAdvance.Presentation.Shared.ViewModels;

namespace SupportAdvance.Presentation.WinTrial.Views;

/// <summary>
/// オープニング画面（起動後、ナビゲーションが選ばれるまで表示する）
/// </summary>
/// <remarks>
/// <para>【表示内容】右下に製品名、その下にアプリケーションの名前（<see cref="OpeningViewModel"/> の内容）を描画する。将来、お知らせなどの通知もここに表示する予定</para>
/// <para>【設計】文字はコントロールの描画で表示し、<see cref="OpeningViewModel"/> の変更に合わせて再描画する。
/// MDI の領域（<see cref="MdiClient"/>）に置いて使う（タブは使わない）</para>
/// </remarks>
public sealed class OpeningView : UserControl
{
    private const int RightMargin = 32;
    private const int BottomMargin = 28;

    private static readonly Color ProductNameColor = Color.FromArgb(0x44, 0x44, 0x44);
    private static readonly Color ApplicationNameColor = Color.FromArgb(0x88, 0x88, 0x88);

    private readonly OpeningViewModel _viewModel;
    private readonly Font _productNameFont = new("Yu Gothic UI", 26F, FontStyle.Bold, GraphicsUnit.Point, 128);
    private readonly Font _applicationNameFont = new("Yu Gothic UI", 15F, FontStyle.Regular, GraphicsUnit.Point, 128);

    /// <summary>
    /// <see cref="OpeningView"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="viewModel">表示内容の ViewModel</param>
    /// <exception cref="ArgumentNullException"><paramref name="viewModel"/> が <see langword="null"/> の場合</exception>
    public OpeningView(OpeningViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;

        BackColor = Color.White;
        Dock = DockStyle.Fill;
        DoubleBuffered = true;
        ResizeRedraw = true;

        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    /// <inheritdoc/>
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        const TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.Right | TextFormatFlags.Bottom;

        // 下から順に、アプリケーションの名前、その上に製品名を、右端にそろえて描く
        var applicationNameSize = TextRenderer.MeasureText(e.Graphics, _viewModel.ApplicationName, _applicationNameFont, Size.Empty, flags);
        var productNameSize = TextRenderer.MeasureText(e.Graphics, _viewModel.ProductName, _productNameFont, Size.Empty, flags);

        var applicationNameBounds = new Rectangle(
            ClientSize.Width - RightMargin - applicationNameSize.Width,
            ClientSize.Height - BottomMargin - applicationNameSize.Height,
            applicationNameSize.Width,
            applicationNameSize.Height);
        var productNameBounds = new Rectangle(
            ClientSize.Width - RightMargin - productNameSize.Width,
            applicationNameBounds.Top - productNameSize.Height,
            productNameSize.Width,
            productNameSize.Height);

        TextRenderer.DrawText(e.Graphics, _viewModel.ProductName, _productNameFont, productNameBounds, ProductNameColor, flags);
        TextRenderer.DrawText(e.Graphics, _viewModel.ApplicationName, _applicationNameFont, applicationNameBounds, ApplicationNameColor, flags);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            _productNameFont.Dispose();
            _applicationNameFont.Dispose();
        }

        base.Dispose(disposing);
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e) => Invalidate();
}
