using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure.DataAccess.Models;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure.DataAccess;

/// <summary>
/// UserPreferences データアクセスインターフェース
///
/// 【責務】低レベルのDB操作抽象化
/// 【用途】RepoDb/Dapper での直接SQL実行
/// 【実装】Repository層から呼び出される
///
/// 【設計原則】
/// - Domain層への依存なし（DbModel のみ操作）
/// - SQL実行のみ（トランザクション管理は呼び出し側）
/// - ドメインイベント非考慮（Application層で処理）
/// </summary>
public interface IUserPreferencesDataAccess
{
    /// <summary>
    /// ユーザーIDでプリファレンス検索
    ///
    /// 【SQL】SELECT * FROM UserPreferences WHERE UserId = @UserId AND DeletedAt IS NULL
    /// 【戻り値】存在しない場合は null
    /// </summary>
    Task<UserPreferencesDbModel?> GetByUserIdAsync(int userId);

    /// <summary>
    /// row_idでプリファレンス検索
    ///
    /// 【SQL】SELECT * FROM UserPreferences WHERE RowId = @RowId AND DeletedAt IS NULL
    /// 【戻り値】存在しない場合は null
    /// </summary>
    Task<UserPreferencesDbModel?> GetByRowIdAsync(long rowId);

    /// <summary>
    /// 新規プリファレンス�挿入
    ///
    /// 【SQL】INSERT INTO UserPreferences VALUES (...)
    /// 【例外】制約違反時は ArgumentException をスロー
    /// </summary>
    Task InsertAsync(UserPreferencesDbModel model);

    /// <summary>
    /// プリファレンス更新
    ///
    /// 【SQL】UPDATE UserPreferences SET ... WHERE UserId = @UserId AND DeletedAt IS NULL
    /// 【例外】対象行なしの場合は InvalidOperationException をスロー
    /// </summary>
    Task UpdateAsync(UserPreferencesDbModel model);

    /// <summary>
    /// プリファレンス削除（論理削除）
    ///
    /// 【SQL】UPDATE UserPreferences SET DeletedAt = @DeletedAt WHERE UserId = @UserId
    /// 【例外】対象行なしの場合は InvalidOperationException をスロー
    /// </summary>
    Task DeleteAsync(int userId, LocalDateTime deletedAt);

    /// <summary>
    /// すべてのプリファレンス取得（アクティブのみ）
    ///
    /// 【SQL】SELECT * FROM UserPreferences WHERE DeletedAt IS NULL
    /// 【戻り値】空リスト（マッチなし時）
    /// </summary>
    Task<IEnumerable<UserPreferencesDbModel>> GetAllAsync();
}
