namespace SupportAdvance.Presentation.Shared.Icons;

/// <summary>
/// アプリケーションで使うアイコンの名前（WpfTrial／WinTrial 共通）
/// </summary>
/// <remarks>
/// <para>【設計】名前は見た目ではなく用途（意味）で付ける。実際に描画する字形（Fluent UI System Icons のどのアイコンか）は <see cref="FluentIconCatalog"/> で対応付けるため、
/// アイコンの差し替えは <see cref="FluentIconCatalog"/> の変更だけで済む</para>
/// <para>【追加手順】<see cref="FluentIconCatalog"/> の説明を参照</para>
/// </remarks>
public enum AppIcon
{
    /// <summary>
    /// 顧客（顧客管理のメニュー）
    /// </summary>
    Customers,

    /// <summary>
    /// 受注（受注管理のメニュー）
    /// </summary>
    Orders,

    /// <summary>
    /// 一覧画面
    /// </summary>
    List,

    /// <summary>
    /// 登録画面
    /// </summary>
    Register,

    /// <summary>
    /// 保存
    /// </summary>
    Save
}
