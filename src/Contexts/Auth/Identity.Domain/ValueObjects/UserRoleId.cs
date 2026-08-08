using SupportAdvance.SharedKernel.ValueObjects.Audit;

namespace SupportAdvance.Contexts.Auth.Identity.Domain.ValueObjects;

/// <summary>
/// UserRole を識別する ID（GUID ベース）
/// 【用途】UserRole.Id として使用
/// 【生成】通常は UserRoleId.New() で自動生成
/// </summary>
public class UserRoleId : AggregateId
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    public UserRoleId(Guid value) : base(value) { }

    /// <summary>
    /// 新規 ID を生成
    /// </summary>
    public static UserRoleId New() => new(Guid.NewGuid());

    /// <summary>
    /// GUID から ID を生成
    /// </summary>
    public static UserRoleId From(Guid value) => new(value);
}
