using System.Data;
using Dapper;

namespace SupportAdvance.Infrastructure;

/// <summary>
/// Dapper型ハンドラ登録支援ヘルパークラス
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

        SqlMapper.AddTypeMap(typeof(DateTime), DbType.DateTime2);
        SqlMapper.AddTypeMap(typeof(DateTime?), DbType.DateTime2);
    }
}
