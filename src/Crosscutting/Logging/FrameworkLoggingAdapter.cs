using Microsoft.Extensions.Logging;
using NLog;
using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Crosscutting.Logging;

/// <summary>
/// アプリケーションログアダプタ
/// ILogger&lt;T&gt; をラップし、CorrelationId と時刻管理を提供
/// </summary>
/// <typeparam name="T">ログを出力するクラスの型（ロギングコンテキスト）</typeparam>
/// <param name="logger">実際の出力先となる Microsoft.Extensions.Logging のロガー</param>
/// <param name="correlationContext">ログに付与する CorrelationId の取得元</param>
/// <param name="clock">ログに付与する時刻（JST）の取得元</param>
/// <remarks>
/// <para>【副作用】出力のたびに NLog の <c>GlobalDiagnosticsContext</c> の <c>ClockDateTime</c>／<c>CorrelationId</c>（先頭 8 文字）を更新</para>
/// <para>【時刻】ログの時刻はシステム時刻ではなく <see cref="IClock"/> の値（テスト用クロックの時刻がそのまま出力）</para>
/// </remarks>
public class FrameworkLoggingAdapter<T>(
    ILogger<T> logger,
    ICorrelationContext correlationContext,
    IClock clock) : IAppLogging<T>
{
    /// <inheritdoc/>
    public void LogInformation(string? message)
    {
        SetContextToGdc();
        logger.LogInformation(message);
    }

    /// <inheritdoc/>
    public void LogInformation(string messageTemplate, params object[] args)
    {
        SetContextToGdc();
        logger.LogInformation(messageTemplate, args);
    }

    /// <inheritdoc/>
    public void LogWarning(string message)
    {
        SetContextToGdc();
        logger.LogWarning(message);
    }

    /// <inheritdoc/>
    public void LogWarning(string messageTemplate, params object[] args)
    {
        SetContextToGdc();
        logger.LogWarning(messageTemplate, args);
    }

    /// <inheritdoc/>
    public void LogError(string message, Exception? exception = null)
    {
        SetContextToGdc();
        if (exception != null)
        {
            logger.LogError(exception, message);
        }
        else
        {
            logger.LogError(message);
        }
    }

    /// <inheritdoc/>
    public void LogError(string messageTemplate, params object[] args)
    {
        SetContextToGdc();
        logger.LogError(messageTemplate, args);
    }

    /// <summary>
    /// CorrelationId と時刻を GlobalDiagnosticsContext に設定
    /// 毎回のログ出力時の呼び出し
    /// </summary>
    private void SetContextToGdc()
    {
        // [1] IClock から現在時刻を取得（毎回呼び出し）
        var clockDateTime = clock.JstNow.Value.ToString("yyyy-MM-dd HH:mm:ss.fff");
        GlobalDiagnosticsContext.Set("ClockDateTime", clockDateTime);

        // [2] CorrelationId を取得（内部32文字）
        var correlationIdFull = correlationContext.GetOrCreate();

        // [3] 最初の8文字に短縮してから設定（ログ出力用）
        var correlationIdShort = correlationIdFull.Length >= 8
            ? correlationIdFull.Substring(0, 8)
            : correlationIdFull;
        GlobalDiagnosticsContext.Set("CorrelationId", correlationIdShort);
    }
}
