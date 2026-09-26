using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership;

/// <summary>
/// 主部署フラグを表す ValueObject
/// </summary>
/// <remarks>
/// <para>【型】bool のラッパー</para>
/// <para>【制約】true の場合は1つのみ、false は複数可能</para>
/// <para>【責務】従業員の複数部署所属時に主部署を識別</para>
/// </remarks>
public sealed class IsPrimary : PrimitiveValueObject<bool>
{
    /// <summary>
    /// bool 値（主部署フラグ）
    /// </summary>
    public bool Value => ValueField;

    /// <summary>
    /// 指定された値から IsPrimary を生成する（プライベートコンストラクタ）
    /// </summary>
    private IsPrimary(bool value) : base(value, true)
    {
    }

    /// <summary>
    /// 主部署を表す IsPrimary の生成
    /// </summary>
    /// <returns>主所属を表すインスタンス</returns>
    public static IsPrimary Primary() => new(true);

    /// <summary>
    /// 副部署を表す IsPrimary の生成
    /// </summary>
    /// <returns>副所属を表すインスタンス</returns>
    public static IsPrimary Secondary() => new(false);

    /// <summary>
    /// bool 値から IsPrimary の生成
    /// </summary>
    /// <param name="value">主所属の場合は <see langword="true"/></param>
    /// <returns>生成したインスタンス</returns>
    public static IsPrimary From(bool value) => new(value);

    /// <summary>
    /// 文字列表現の取得
    /// </summary>
    /// <returns>主所属の場合は <c>主部署</c>、それ以外は <c>副部署</c></returns>
    public override string ToString() => Value ? "主部署" : "副部署";
}
