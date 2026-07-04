namespace SupportAdvance.Crosscutting.Logging;

/// <summary>
/// コリレーションID管理インターフェース
/// </summary>
public interface ICorrelationContext
{
    /// <summary>
    /// コリレーションID取得または生成処理
    /// </summary>
    /// <returns>コリレーションID</returns>
    string GetOrCreate();

    /// <summary>
    /// コリレーションID設定処理
    /// </summary>
    /// <param name="id">コリレーションID</param>
    void Set(string id);
}
