namespace SupportAdvance.Crosscutting.Logging;

/// <summary>
/// アプリケーション共通ログインターフェース
/// Information / Warning / Error の3レベルのみを公開
/// </summary>
/// <typeparam name="T">ログを出力するクラスの型（ロギングコンテキスト）</typeparam>
public interface IAppLogging<T>
{
    /// <summary>
    /// 情報レベルでログを出力
    /// </summary>
    /// <param name="message">出力するメッセージ</param>
    void LogInformation(string? message);

    /// <summary>
    /// テンプレート + 引数で情報ログを出力
    /// </summary>
    /// <param name="messageTemplate">構造化ログのメッセージテンプレート（例: <c>"User {LoginId} logged in"</c>）</param>
    /// <param name="args">テンプレートのプレースホルダーに順に埋め込む値</param>
    void LogInformation(string messageTemplate, params object[] args);

    /// <summary>
    /// 警告レベルでログを出力
    /// </summary>
    /// <param name="message">出力するメッセージ</param>
    void LogWarning(string message);

    /// <summary>
    /// テンプレート + 引数で警告ログを出力
    /// </summary>
    /// <param name="messageTemplate">構造化ログのメッセージテンプレート</param>
    /// <param name="args">テンプレートのプレースホルダーに順に埋め込む値</param>
    void LogWarning(string messageTemplate, params object[] args);

    /// <summary>
    /// エラーレベルでログを出力（例外情報含む）
    /// </summary>
    /// <param name="message">出力するメッセージ</param>
    /// <param name="exception">原因の例外。<see langword="null"/> の場合はメッセージのみ出力</param>
    void LogError(string message, Exception? exception = null);

    /// <summary>
    /// テンプレート + 引数でエラーログを出力
    /// </summary>
    /// <param name="messageTemplate">構造化ログのメッセージテンプレート</param>
    /// <param name="args">テンプレートのプレースホルダーに順に埋め込む値</param>
    void LogError(string messageTemplate, params object[] args);
}
