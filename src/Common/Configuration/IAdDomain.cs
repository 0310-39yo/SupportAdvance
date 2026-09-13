namespace SupportAdvance.Common.Configuration;

/// <summary>
/// ADドメイン関連設定インターフェース
/// 【責務】
/// アプリケーションが使用するADドメイン名の管理
/// 【変更理由】
/// プロジェクト構成の変更、ADドメイン名の再構成が必要な場合   
/// 【関連ファイル】
/// </summary>
public interface IAdDomain
{
    /// <summary>
    /// ADドメイン名
    /// </summary>
    string ActiveDirectoryDomain { get; init; }
}
