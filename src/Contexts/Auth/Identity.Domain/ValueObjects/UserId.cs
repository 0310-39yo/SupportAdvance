using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Auth.Identity.Domain.ValueObjects;

/// <summary>
/// User を識別する ID（GUID ベース）
/// 【用途】User.Id として使用
/// 【生成】通常は UserId.New() で自動生成
/// </summary>
public class UserId : AggregateId
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    public UserId(Guid value) : base(value) { }

    /// <summary>
    /// 新規 ID を生成
    /// </summary>
    public static UserId New() => new(Guid.NewGuid());

    /// <summary>
    /// GUID から ID を生成
    /// </summary>
    public static UserId From(Guid value) => new(value);
}

