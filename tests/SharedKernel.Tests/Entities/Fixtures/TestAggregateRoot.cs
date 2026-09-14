using SupportAdvance.SharedKernel.Entities;

namespace SupportAdvance.SharedKernel.Tests.Entities.Fixtures;

/// <summary>
/// テスト用 AggregateRoot
/// </summary>
public sealed class TestAggregateRoot : AggregateRoot<TestId>
{
    public string Status { get; private set; }

    public TestAggregateRoot(TestId id)
    {
        RowId = id;
        Status = "Initial";
    }

    public void ChangeStatus(string newStatus)
    {
        Status = newStatus;
    }
}
