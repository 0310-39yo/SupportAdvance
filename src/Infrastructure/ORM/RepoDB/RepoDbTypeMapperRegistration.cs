using System.Data;
using RepoDb;
using SupportAdvance.Common.Clocks;

namespace SupportAdvance.Infrastructure.ORM.RepoDB;

/// <summary>
/// RepoDb型マッパー登録支援ヘルパークラス
///
/// 【責務】
/// - RepoDb のグローバルな型マッピング設定
/// - LocalDateTime（JST）を SqlServer の datetime2 にマッピング
/// 【原則】Dapper と同じ方針で LocalDateTime を統一使用
/// </summary>
public static class RepoDbTypeMapperRegistration
{
    /// <summary>
    /// RepoDb型マッパー登録処理
    /// </summary>
    public static void Register()
    {
        // 【原則】全層で LocalDateTime（JST）を使用してタイムゾーン一貫性を保証
        TypeMapper.Add<LocalDateTime>(DbType.DateTime2, true);
        TypeMapper.Add<LocalDateTime?>(DbType.DateTime2, true);
    }
}
