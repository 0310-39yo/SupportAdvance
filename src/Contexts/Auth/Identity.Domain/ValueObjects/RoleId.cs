using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Auth.Identity.Domain.ValueObjects;

/// <summary>
/// Role を識別する ID（GUID ベース）
/// 【用途】Role.Id として使用
/// 【生成】通常は RoleId.New() で自動生成
/// </summary>
public class RoleId : AggregateId
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    public RoleId(Guid value) : base(value) { }

    /// <summary>
    /// 新規 ID を生成
    /// </summary>
    public static RoleId New() => new(Guid.NewGuid());

    /// <summary>
    /// GUID から ID を生成
    /// </summary>
    public static RoleId From(Guid value) => new(value);
}

