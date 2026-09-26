namespace SupportAdvance.Presentation.Shared;

/// <summary>
/// 起動しているアプリケーションの識別情報（WpfTrial／WinTrial などの、アプリごとの名前）
/// </summary>
/// <remarks>
/// <para>【用途】オープニング画面などの、アプリの名前を表示する画面で使う。各アプリの DI 登録（Composition Root）で、アプリごとに 1 つ登録する</para>
/// </remarks>
/// <param name="Name">アプリケーションの名前（例: <c>WpfTrial</c>）</param>
public sealed record AppIdentity(string Name);
