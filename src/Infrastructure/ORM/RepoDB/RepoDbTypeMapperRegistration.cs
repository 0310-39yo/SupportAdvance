using System.Data;
using RepoDb;
using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Infrastructure.ORM.RepoDB;

/// <summary>
/// RepoDb型マッパー登録支援ヘルパークラス
/// </summary>
/// <remarks>
/// <para>【責務】</para>
/// <list type="bullet">
/// <item><description>RepoDb のグローバルな型マッピング設定</description></item>
/// <item><description>LocalDateTime（JST）を SqlServer の datetime2 にマッピング</description></item>
/// <item><description>RowId を long にマッピング</description></item>
/// </list>
/// <para>【原則】Dapper と同じ方針で LocalDateTime と RowId を統一使用</para>
/// </remarks>
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

        // 【原則】RowId は DbModel では long のまま保持し、Mapper で RowId に変換
        // type mapping のみ登録（値の自動変換は Mapper が責務）
        TypeMapper.Add<RowId>(DbType.Int64, true);
    }
}

