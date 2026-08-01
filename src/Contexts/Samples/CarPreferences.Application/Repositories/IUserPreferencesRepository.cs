using SupportAdvance.Contexts.Samples.CarPreferences.Domain.Entities;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Application.Repositories;

/// <summary>
/// ユーザープリファレンス Repository インターフェース
///
/// 【責務】UserPreferences の永続化・取得
/// 【実装】Infrastructure層で実装（EF Core等）
/// </summary>
public interface IUserPreferencesRepository
{
    /// <summary>
    /// ユーザーID で好み情報を取得
    ///
    /// 【パラメータ】userId - 取得対象のユーザーID
    /// 【戻り値】見つかった場合は UserPreferences、見つからない場合は null
    /// </summary>
    Task<UserPreferences?> GetAsync(RespondentPersonId userId);

    /// <summary>
    /// 好み情報を新規追加
    ///
    /// 【パラメータ】preferences - 追加する UserPreferences
    /// 【例外】既に同じ ID が存在する場合は InvalidOperationException
    /// </summary>
    Task AddAsync(UserPreferences preferences);

    /// <summary>
    /// 好み情報を更新
    ///
    /// 【パラメータ】preferences - 更新する UserPreferences
    /// 【例外】見つからない場合は InvalidOperationException
    /// </summary>
    Task UpdateAsync(UserPreferences preferences);

    /// <summary>
    /// 好み情報を削除
    ///
    /// 【パラメータ】userId - 削除対象のユーザーID
    /// 【戻り値】削除された場合は true、見つからない場合は false
    /// </summary>
    Task<bool> DeleteAsync(RespondentPersonId userId);

    /// <summary>
    /// すべての好み情報を取得
    ///
    /// 【用途】統計・分析用
    /// 【戻り値】すべての UserPreferences のリスト
    /// </summary>
    Task<IReadOnlyList<UserPreferences>> GetAllAsync();
}
