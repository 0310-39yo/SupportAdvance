using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

/// <summary>
/// 従業員コードを表すValueObject（ビジネスコード）
/// 【値】BizDivision + BizId
/// 【表示】"M01234" 形式（区分+5桁0埋め番号、スペースなし）
/// 【責務】ビジネスコードの管理、範囲検証
/// </summary>
/// <remarks>
/// <para>【不変条件】<see cref="BizId"/> は <see cref="Division"/> ごとの有効範囲内</para>
/// <list type="bullet">
/// <item><description>従業員（M）: 1001〜6999、または 10000 以上</description></item>
/// <item><description>派遣社員（T）: 7500〜7999、または 70000 以上</description></item>
/// <item><description>請負者（C）: 8000〜8499、または 80000 以上</description></item>
/// </list>
/// </remarks>
public sealed class BizCode : ValueObject, IEquatable<BizCode>
{
    /// <summary>
    /// 従業員種別区分（M／T／C）
    /// </summary>
    public BizDivision Division { get; }

    /// <summary>
    /// 従業員の通し番号
    /// </summary>
    /// <value><see cref="Division"/> ごとの有効範囲内の値</value>
    public BizId BizId { get; }

    private BizCode(BizDivision division, BizId bizId)
    {
        Division = division;
        BizId = bizId;
    }

    /// <summary>
    /// 区分と通し番号からの <see cref="BizCode"/> の生成
    /// </summary>
    /// <param name="division">従業員種別区分</param>
    /// <param name="bizId">従業員の通し番号</param>
    /// <returns>生成したインスタンス</returns>
    /// <exception cref="ArgumentException"><paramref name="bizId"/> が <paramref name="division"/> の有効範囲外の場合</exception>
    public static BizCode From(BizDivision division, BizId bizId)
    {
        ValidateDivisionAndBizId(division, bizId);
        return new BizCode(division, bizId);
    }

    /// <summary>
    /// 区分と通し番号からの <see cref="BizCode"/> 生成の試行。例外の送出なし
    /// </summary>
    /// <param name="division">従業員種別区分</param>
    /// <param name="bizId">従業員の通し番号</param>
    /// <param name="result">成功した場合は生成したインスタンス。失敗した場合は <see langword="null"/>（使用禁止）</param>
    /// <returns>
    /// 成功した場合は <see langword="true"/>。いずれかの引数が <see langword="null"/> の場合、
    /// または <paramref name="bizId"/> が <paramref name="division"/> の有効範囲外の場合は <see langword="false"/>
    /// </returns>
    public static bool TryFrom(BizDivision division, BizId bizId, out BizCode result)
    {
        result = null!;
        if (division == null || bizId == null)
        {
            return false;
        }

        try
        {
            result = From(division, bizId);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    /// <summary>
    /// 表示形式の文字列（例: <c>M1234</c>）からの <see cref="BizCode"/> 解析の試行。例外の送出なし
    /// </summary>
    /// <param name="input">先頭 1 文字が区分、残りが通し番号の文字列</param>
    /// <param name="result">成功した場合は解析したインスタンス。失敗した場合は <see langword="null"/>（使用禁止）</param>
    /// <returns>
    /// 成功した場合は <see langword="true"/>。<see langword="null"/>・2 文字未満・区分や番号が不正・範囲外の場合は <see langword="false"/>
    /// </returns>
    public static bool TryParse(string? input, out BizCode result)
    {
        result = null!;
        if (string.IsNullOrEmpty(input) || input.Length < 2)
        {
            return false;
        }

        try
        {
            var divisionChar = input[0];
            var bizIdStr = input[1..];

            if (!BizDivision.TryFromDbValue(divisionChar.ToString(), out var division))
            {
                return false;
            }

            if (!int.TryParse(bizIdStr, out var bizIdInt))
            {
                return false;
            }

            if (!BizId.TryFromDbValue(bizIdInt, out var bizId))
            {
                return false;
            }

            return TryFrom(division, bizId, out result);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// DB の区分列と番号列からの <see cref="BizCode"/> 生成の試行（Infrastructure 層での型変換用）。例外の送出なし
    /// </summary>
    /// <param name="divisionChar">DB の区分値（<c>M</c>／<c>T</c>／<c>C</c>）</param>
    /// <param name="bizIdInt">DB の通し番号</param>
    /// <param name="result">成功した場合は生成したインスタンス。失敗した場合は <see langword="null"/>（使用禁止）</param>
    /// <returns>成功した場合は <see langword="true"/>。区分や番号が不正、または範囲外の場合は <see langword="false"/></returns>
    public static bool TryFromDbValues(char divisionChar, int bizIdInt, out BizCode result)
    {
        result = null!;
        try
        {
            if (!BizDivision.TryFromDbValue(divisionChar.ToString(), out var division))
            {
                return false;
            }

            if (!BizId.TryFromDbValue(bizIdInt, out var bizId))
            {
                return false;
            }

            return TryFrom(division, bizId, out result);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 表示用の文字列表現
    /// </summary>
    /// <returns>区分と 5 桁の通し番号をつなげた文字列（例: <c>M01234</c>）。<see cref="TryParse"/> で解析可能な形式</returns>
    public override string ToString() => $"{Division.Value}{BizId.ToString()}";

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as BizCode);

    /// <summary>
    /// 指定した <see cref="BizCode"/> と等しいかどうかの判定
    /// </summary>
    /// <param name="other">比較対象</param>
    /// <returns><see cref="Division"/> と <see cref="BizId"/> がどちらも等しい場合は <see langword="true"/></returns>
    public bool Equals(BizCode? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Division.Equals(other.Division) && BizId.Equals(other.BizId);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Division, BizId);

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return Division;
        yield return BizId;
    }

    private static void ValidateDivisionAndBizId(BizDivision division, BizId bizId)
    {
        var num = bizId.Value;

        if (division.IsRegularEmployee)
        {
            if (!((1001 <= num && num <= 6999) || num >= 10000))
            {
                throw new ArgumentException($"Regular employee number {num} must be in range 1001-6999 or 10000+.");
            }
        }
        else if (division.IsDispatched)
        {
            if (!((7500 <= num && num <= 7999) || num >= 70000))
            {
                throw new ArgumentException($"Dispatched employee number {num} must be in range 7500-7999 or 70000+.");
            }
        }
        else if (division.IsContractor)
        {
            if (!((8000 <= num && num <= 8499) || num >= 80000))
            {
                throw new ArgumentException($"Contractor number {num} must be in range 8000-8499 or 80000+.");
            }
        }
    }
}
