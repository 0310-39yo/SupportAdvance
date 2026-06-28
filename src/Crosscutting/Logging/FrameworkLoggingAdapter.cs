using Microsoft.Extensions.Logging;
using NLog;
using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Crosscutting.Logging;

/// <summary>
/// アプリケーションログアダプタ
/// ILogger&lt;T&gt; をラップし、CorrelationId と時刻管理を提供
/// </summary>
public class FrameworkLoggingAdapter<T>(
    ILogger<T> logger,
    ICorrelationContext correlationContext,
    IClock clock) : IAppLogging<T>
{
    public void LogInformation(string message)
    {
        SetContextToGdc();
        logger.LogInformation(message);
    }

    public void LogInformation(string messageTemplate, params object[] args)
    {
        SetContextToGdc();
        logger.LogInformation(messageTemplate, args);
    }

    public void LogWarning(string message)
    {
        SetContextToGdc();
        logger.LogWarning(message);
    }

    public void LogWarning(string messageTemplate, params object[] args)
    {
        SetContextToGdc();
        logger.LogWarning(messageTemplate, args);
    }

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

    public void LogError(string messageTemplate, params object[] args)
    {
        SetContextToGdc();
        logger.LogError(messageTemplate, args);
    }

    /// <summary>
    /// CorrelationId と時刻を GlobalDiagnosticsContext に設定
    /// 毎回のログ出力時に呼び出される
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
