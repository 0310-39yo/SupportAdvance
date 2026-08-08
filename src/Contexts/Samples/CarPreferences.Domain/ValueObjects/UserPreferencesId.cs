using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;

/// <summary>
/// ユーザー好み設定を識別する ID（GUID ベース）
/// 【用途】UserPreferences.Id として使用
/// 【生成】通常は UserPreferencesId.New() で自動生成
/// 【型安全性】他の Aggregate ID と混同されない
/// </summary>
public class UserPreferencesId : AggregateId
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    public UserPreferencesId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// 新規 ID を生成
    /// </summary>
    public static UserPreferencesId New() => new(Guid.NewGuid());

    /// <summary>
    /// GUID から ID を生成
    /// </summary>
    public static UserPreferencesId From(Guid value) => new(value);
}
