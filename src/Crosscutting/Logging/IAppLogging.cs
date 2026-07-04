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
    void LogInformation(string message);

    /// <summary>
    /// テンプレート + 引数で情報ログを出力
    /// </summary>
    void LogInformation(string messageTemplate, params object[] args);

    /// <summary>
    /// 警告レベルでログを出力
    /// </summary>
    void LogWarning(string message);

    /// <summary>
    /// テンプレート + 引数で警告ログを出力
    /// </summary>
    void LogWarning(string messageTemplate, params object[] args);

    /// <summary>
    /// エラーレベルでログを出力（例外情報含む）
    /// </summary>
    void LogError(string message, Exception? exception = null);

    /// <summary>
    /// テンプレート + 引数でエラーログを出力
    /// </summary>
    void LogError(string messageTemplate, params object[] args);
}
