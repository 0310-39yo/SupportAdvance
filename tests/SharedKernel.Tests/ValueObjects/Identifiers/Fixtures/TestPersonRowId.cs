namespace SupportAdvance.SharedKernel.Tests.ValueObjects.Identifiers.Fixtures;

/// <summary>
/// テスト用 PersonRowId（RowId abstract class のテスト実装）
/// </summary>
public sealed class TestPersonRowId : SupportAdvance.SharedKernel.ValueObjects.Identifiers.RowId
{
    public const long MinValue = 1L;

    private TestPersonRowId(long value) : base(value, true) { }

    public static TestPersonRowId From(long value) => new(value);

    public override void Validate(long normalized)
    {
        if (normalized < MinValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(normalized),
                normalized,
                $"TestPersonRowId must be {MinValue} or higher.");
        }
    }
}
