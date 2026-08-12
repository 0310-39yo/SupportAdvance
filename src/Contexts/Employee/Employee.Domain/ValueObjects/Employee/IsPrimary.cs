using SupportAdvance.SharedKernel.ValueObjects;
using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

/// <summary>
/// 主部署フラグを表す ValueObject
/// 【型】bool のラッパー
/// 【制約】true の場合は1つのみ、false は複数可能
/// 【責務】従業員の複数部署所属時に主部署を識別
/// </summary>
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
    /// 主部署を表す IsPrimary を生成する
    /// </summary>
    public static IsPrimary Primary() => new(true);

    /// <summary>
    /// 副部署を表す IsPrimary を生成する
    /// </summary>
    public static IsPrimary Secondary() => new(false);

    /// <summary>
    /// bool 値から IsPrimary を生成する
    /// </summary>
    public static IsPrimary From(bool value) => new(value);

    /// <summary>
    /// 文字列表現を取得する
    /// </summary>
    public override string ToString() => Value ? "主部署" : "副部署";
}
