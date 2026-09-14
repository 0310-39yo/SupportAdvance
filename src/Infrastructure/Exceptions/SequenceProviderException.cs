namespace SupportAdvance.Infrastructure.Exceptions;

/// <summary>
/// DB シーケンスから RowId を採番する際の例外
///
/// 【発生原因】
/// - SQL Server への接続失敗
/// - NEXT VALUE FOR コマンド実行エラー
/// - シーケンス値取得失敗
/// - SQL タイムアウト
///
/// 【処理方針】
/// - Application層でキャッチして、ユーザーに通知
/// - Retry Logic は実装しない（初期版）
/// </summary>
public class SequenceProviderException : InvalidOperationException
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="message">エラーメッセージ</param>
    public SequenceProviderException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// コンストラクタ（内部例外指定）
    /// </summary>
    /// <param name="message">エラーメッセージ</param>
    /// <param name="innerException">内部例外（SqlException など）</param>
    public SequenceProviderException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
