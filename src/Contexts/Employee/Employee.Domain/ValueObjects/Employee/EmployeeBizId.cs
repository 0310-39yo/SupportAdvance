using SupportAdvance.SharedKernel.ValueObjects.Abstractions;

namespace SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;

/// <summary>
/// 従業員の通し番号を表すValueObject（ビジネスID）
/// 【範囲】1001以上（1000は管理者予約。区分ごとの有効範囲が検証される）
/// 【表示】左0埋めで5桁（例："01234"）
/// 【責務】ビジネスIDとしての従業員番号の管理と検証
/// </summary>
public sealed class EmployeeBizId : PrimitiveValueObject<int>, IEquatable<EmployeeBizId>
{
    public const int MinValue = 1001;
    public const int ReservedValue = 1000;

    private EmployeeBizId(int value) : base(value, true) { }

    public static EmployeeBizId From(int value) => new(value);

    public static bool TryFrom(int? input, out EmployeeBizId result)
    {
        result = null!;
        if (!input.HasValue)
            return false;
        try
        {
            result = From(input.Value);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    public static EmployeeBizId FromDbValue(int value) => new(value);

    public static bool TryFromDbValue(int? input, out EmployeeBizId result)
    {
        result = null!;
        if (!input.HasValue)
            return false;
        try
        {
            result = FromDbValue(input.Value);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    public int Value => ValueField;

    public override string ToString() => ValueField.ToString("D5");

    public override bool Equals(object? obj) => Equals(obj as EmployeeBizId);

    public bool Equals(EmployeeBizId? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ValueField == other.ValueField;
    }

    public override int GetHashCode() => ValueField.GetHashCode();

    protected override IEnumerable<object?> GetValueComponents()
    {
        yield return ValueField;
    }

    public override void Validate(int normalized)
    {
        base.Validate(normalized);
        if (normalized == ReservedValue)
            throw new ArgumentOutOfRangeException(nameof(normalized), normalized,
                $"EmployeeBizId {ReservedValue} is reserved for system administrator.");
        if (normalized < MinValue)
            throw new ArgumentOutOfRangeException(nameof(normalized), normalized,
                $"EmployeeBizId must be {MinValue} or higher ({ReservedValue} is reserved).");
    }
}
