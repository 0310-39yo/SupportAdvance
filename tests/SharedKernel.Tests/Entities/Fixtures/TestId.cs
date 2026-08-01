namespace SupportAdvance.SharedKernel.Tests.Entities.Fixtures;

/// <summary>
/// テスト用 ID ValueObject
/// </summary>
public sealed class TestId : IEquatable<TestId>
{
    public string Value { get; }

    public TestId(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Value is required", nameof(value));
        Value = value;
    }

    public override bool Equals(object? obj) => Equals(obj as TestId);

    public bool Equals(TestId? other) => other != null && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;
}
