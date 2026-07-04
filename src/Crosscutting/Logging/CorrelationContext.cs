namespace SupportAdvance.Crosscutting.Logging;

/// <summary>
/// コリレーションID管理コンテキストクラス
/// </summary>
public sealed class CorrelationContext : ICorrelationContext
{
    private static readonly AsyncLocal<string?> _correlationId = new();

    /// <summary>
    /// コリレーションID取得または生成処理
    /// </summary>
    /// <returns>コリレーションID</returns>
    public string GetOrCreate()
    {
        if (string.IsNullOrEmpty(_correlationId.Value))
        {
            _correlationId.Value = Guid.NewGuid().ToString("N");
        }

        return _correlationId.Value;
    }

    /// <summary>
    /// コリレーションID設定処理
    /// </summary>
    /// <param name="id">コリレーションID</param>
    public void Set(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        _correlationId.Value = id;
    }
}
