using System.Data;
using Dapper;
using SupportAdvance.Common.Clocks;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Infrastructure.ORM.Dapper;

/// <summary>
/// Dapper型ハンドラ登録支援ヘルパークラス
///
/// 【責務】
/// - Dapper のグローバルな型マッピング設定
/// - LocalDateTime（JST）を SqlServer の datetime2 にマッピング
/// - RowId を long にマッピング
/// - アンダースコア命名規則（snake_case）を対応
/// </summary>
public static class DapperTypeHandlerRegistration
{
    /// <summary>
    /// Dapper型ハンドラ登録処理
    /// </summary>
    public static void Register()
    {
        if (!DefaultTypeMap.MatchNamesWithUnderscores)
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
        }

        // 【原則】全層で LocalDateTime（JST）を使用してタイムゾーン一貫性を保証
        SqlMapper.AddTypeMap(typeof(LocalDateTime), DbType.DateTime2);
        SqlMapper.AddTypeMap(typeof(LocalDateTime?), DbType.DateTime2);

        // 【原則】RowId は DbModel では long のまま保持し、Mapper で RowId に変換
        // type mapping のみ登録（値の自動変換は Mapper が責務）
        SqlMapper.AddTypeMap(typeof(RowId), DbType.Int64);
    }
}
