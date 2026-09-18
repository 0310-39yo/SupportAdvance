using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Role;

/// <summary>
/// ロール割り当ての行ID（RowId ベース）
/// </summary>
public sealed class RoleAssignmentRowId : RowId, IEquatable<RoleAssignmentRowId>
{
    /// <summary>
    /// 行ID として使用できる最小値
    /// </summary>
    public const long MinValue = 1L;

    private RoleAssignmentRowId(long value) : base(value, true)
    {
    }

    /// <summary>
    /// 行ID の値からの <see cref="RoleAssignmentRowId"/> の生成
    /// </summary>
    /// <param name="value">ロール割り当ての行ID（シーケンスで採番済みの値）</param>
    /// <returns>生成したインスタンス</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> が <see cref="MinValue"/> 未満の場合</exception>
    public static RoleAssignmentRowId From(long value) => new(value);

    /// <summary>
    /// 行ID の値の検証
    /// </summary>
    /// <param name="normalized">検証する行ID</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="normalized"/> が <see cref="MinValue"/> 未満の場合</exception>
    public override void Validate(long normalized)
    {
        if (normalized < MinValue)
        {
            throw new ArgumentOutOfRangeException(nameof(normalized), $"RoleAssignmentRowId must be >= {MinValue}");
        }
    }

    /// <summary>
    /// 指定した <see cref="RoleAssignmentRowId"/> と等しいかどうかの判定
    /// </summary>
    /// <param name="other">比較対象</param>
    /// <returns>行ID の値が等しい場合は <see langword="true"/></returns>
    public bool Equals(RoleAssignmentRowId? other) => other != null && Value == other.Value;

    /// <summary>
    /// デバッグ用の文字列表現
    /// </summary>
    /// <returns><c>RoleAssignmentRowId(値)</c> 形式の文字列</returns>
    public override string ToString() => $"RoleAssignmentRowId({Value})";
}
