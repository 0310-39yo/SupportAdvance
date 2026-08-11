using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.SharedKernel.Tests.Entities.Fixtures;

/// <summary>
/// テスト用 RowId（RowId ベースのテスト ID）
/// </summary>
public sealed class TestId : RowId, IEquatable<TestId>
{
    public const long MinValue = 1L;

    private TestId(long value) : base(value, true)
    {
    }

    public static TestId From(long value) => new(value);

    public override void Validate(long normalized)
    {
        if (normalized < MinValue)
            throw new ArgumentOutOfRangeException(nameof(normalized), $"TestId must be >= {MinValue}");
    }

    public bool Equals(TestId? other) => other != null && Value == other.Value;

    public override string ToString() => $"TestId({Value})";
}
