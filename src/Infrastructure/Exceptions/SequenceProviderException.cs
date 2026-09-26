namespace SupportAdvance.Infrastructure.Exceptions;

/// <summary>
/// DB シーケンスから RowId を採番する際の例外
/// </summary>
/// <remarks>
/// <para>【発生原因】</para>
/// <list type="bullet">
/// <item><description>SQL Server への接続失敗</description></item>
/// <item><description>NEXT VALUE FOR コマンド実行エラー</description></item>
/// <item><description>シーケンス値取得失敗</description></item>
/// <item><description>SQL タイムアウト</description></item>
/// </list>
/// <para>【処理方針】</para>
/// <list type="bullet">
/// <item><description>Application層でキャッチして、ユーザーに通知</description></item>
/// <item><description>Retry Logic は実装しない（初期版）</description></item>
/// </list>
/// </remarks>
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
