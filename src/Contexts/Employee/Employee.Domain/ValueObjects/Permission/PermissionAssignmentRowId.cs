using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Permission;

/// <summary>
/// 権限割り当ての行ID（RowId ベース）
/// </summary>
public sealed class PermissionAssignmentRowId : RowId, IEquatable<PermissionAssignmentRowId>
{
    /// <summary>
    /// 行ID として使用できる最小値
    /// </summary>
    public const long MinValue = 1L;

    private PermissionAssignmentRowId(long value) : base(value, true)
    {
    }

    /// <summary>
    /// 行ID の値からの <see cref="PermissionAssignmentRowId"/> の生成
    /// </summary>
    /// <param name="value">権限割り当ての行ID（シーケンスで採番済みの値）</param>
    /// <returns>生成したインスタンス</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> が <see cref="MinValue"/> 未満の場合</exception>
    public static PermissionAssignmentRowId From(long value) => new(value);

    /// <summary>
    /// 行ID の値の検証
    /// </summary>
    /// <param name="normalized">検証する行ID</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="normalized"/> が <see cref="MinValue"/> 未満の場合</exception>
    public override void Validate(long normalized)
    {
        if (normalized < MinValue)
        {
            throw new ArgumentOutOfRangeException(nameof(normalized),
                $"PermissionAssignmentRowId must be >= {MinValue}");
        }
    }

    /// <summary>
    /// 指定した <see cref="PermissionAssignmentRowId"/> と等しいかどうかの判定
    /// </summary>
    /// <param name="other">比較対象</param>
    /// <returns>行ID の値が等しい場合は <see langword="true"/></returns>
    public bool Equals(PermissionAssignmentRowId? other) => other != null && Value == other.Value;

    /// <summary>
    /// デバッグ用の文字列表現
    /// </summary>
    /// <returns><c>PermissionAssignmentRowId(値)</c> 形式の文字列</returns>
    public override string ToString() => $"PermissionAssignmentRowId({Value})";
}
