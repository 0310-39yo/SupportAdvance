using SupportAdvance.SharedKernel.ValueObjects;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

/// <summary>
/// 従業員コードを表すValueObject（ビジネスコード）
/// 【値】EmployeeTypeDivision + EmployeeBizId
/// 【表示】"M1234" 形式（区分+番号、スペースなし）
/// 【責務】ビジネスコードの管理、範囲検証
/// </summary>
public sealed class EmployeeBizCode : ValueObject, IEquatable<EmployeeBizCode>
{
    public EmployeeTypeDivision Division { get; }
    public EmployeeBizId BizId { get; }

    private EmployeeBizCode(EmployeeTypeDivision division, EmployeeBizId bizId)
    {
        Division = division;
        BizId = bizId;
    }

    public static EmployeeBizCode From(EmployeeTypeDivision division, EmployeeBizId bizId)
    {
        ValidateDivisionAndBizId(division, bizId);
        return new EmployeeBizCode(division, bizId);
    }

    public static bool TryFrom(EmployeeTypeDivision division, EmployeeBizId bizId, out EmployeeBizCode result)
    {
        result = null!;
        if (division == null || bizId == null)
            return false;
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

    public static bool TryParse(string? input, out EmployeeBizCode result)
    {
        result = null!;
        if (string.IsNullOrEmpty(input) || input.Length < 2)
            return false;

        try
        {
            var divisionChar = input[0];
            var bizIdStr = input[1..];

            if (!EmployeeTypeDivision.TryFromDbValue(divisionChar.ToString(), out var division))
                return false;
            if (!int.TryParse(bizIdStr, out var bizIdInt))
                return false;
            if (!EmployeeBizId.TryFromDbValue(bizIdInt, out var bizId))
                return false;

            return TryFrom(division, bizId, out result);
        }
        catch
        {
            return false;
        }
    }

    public static bool TryFromDbValues(char divisionChar, int bizIdInt, out EmployeeBizCode result)
    {
        result = null!;
        try
        {
            if (!EmployeeTypeDivision.TryFromDbValue(divisionChar.ToString(), out var division))
                return false;
            if (!EmployeeBizId.TryFromDbValue(bizIdInt, out var bizId))
                return false;
            return TryFrom(division, bizId, out result);
        }
        catch
        {
            return false;
        }
    }

    public override string ToString() => $"{Division.Value}{BizId.ToString()}";

    public override bool Equals(object? obj) => Equals(obj as EmployeeBizCode);

    public bool Equals(EmployeeBizCode? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Division.Equals(other.Division) && BizId.Equals(other.BizId);
    }

    public override int GetHashCode() => HashCode.Combine(Division, BizId);

    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return Division;
        yield return BizId;
    }

    private static void ValidateDivisionAndBizId(EmployeeTypeDivision division, EmployeeBizId bizId)
    {
        var num = bizId.Value;

        if (division.IsRegularEmployee)
        {
            if (!((1001 <= num && num <= 6999) || num >= 10000))
                throw new ArgumentException($"Regular employee number {num} must be in range 1001-6999 or 10000+.");
        }
        else if (division.IsDispatched)
        {
            if (!((7500 <= num && num <= 7999) || num >= 70000))
                throw new ArgumentException($"Dispatched employee number {num} must be in range 7500-7999 or 70000+.");
        }
        else if (division.IsContractor)
        {
            if (!((8000 <= num && num <= 8499) || num >= 80000))
                throw new ArgumentException($"Contractor number {num} must be in range 8000-8499 or 80000+.");
        }
    }
}
