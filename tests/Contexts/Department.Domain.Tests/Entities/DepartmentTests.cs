namespace SupportAdvance.Contexts.Department.Domain.Tests.Entities;

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Department.Domain.Entities;
using SupportAdvance.Contexts.Department.Domain.ValueObjects;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

public class DepartmentTests
{
    [Fact]
    public void Create_ValidParameters_ReturnsInstance()
    {
        var rowId = DepartmentRowId.From(1);
        var code = DepartmentCode.From("DEPT");
        var level = HierarchyLevel.Department();

        var dept = Department.Create(rowId, code, "Sales", level);

        Assert.Equal(rowId, dept.RowId);
        Assert.Equal(code, dept.DeptCode);
        Assert.Equal("Sales", dept.Name);
        Assert.Equal(level, dept.Level);
        Assert.False(dept.ParentId.HasParent);
        Assert.False(dept.ManagerId.HasManager);
        Assert.False(dept.AbolishedOn.IsAbolished);
    }

    [Fact]
    public void Create_WithParentAndManager_ReturnsInstance()
    {
        var rowId = DepartmentRowId.From(2);
        var code = DepartmentCode.From("TEAM");
        var parentId = ParentDepartmentRowId.From(1);
        var managerId = ManagerEmployeeRowId.From(100);

        var dept = Department.Create(
            rowId, code, "Team A", HierarchyLevel.Team(),
            parentId, managerId);

        Assert.True(dept.ParentId.HasParent);
        Assert.True(dept.ManagerId.HasManager);
    }

    [Fact]
    public void IsActive_NotAbolished_ReturnsTrue()
    {
        var dept = Department.Create(
            DepartmentRowId.From(1),
            DepartmentCode.From("ACTV"),
            "Active Department",
            HierarchyLevel.Department());

        var result = dept.IsActive(new LocalDateTime(new DateTime(2026, 9, 15, 10, 0, 0)));
        Assert.True(result);
    }

    [Fact]
    public void IsActive_AbolishedBeforeCheckDate_ReturnsFalse()
    {
        var checkDate = new LocalDateTime(new DateTime(2026, 9, 15, 10, 0, 0));
        var abolishDate = new LocalDateTime(new DateTime(2026, 9, 14, 10, 0, 0));
        var dept = Department.Create(
            DepartmentRowId.From(1),
            DepartmentCode.From("OLDR"),
            "Old Department",
            HierarchyLevel.Department(),
            abolishedOn: AbolishedOn.From(abolishDate));

        var result = dept.IsActive(checkDate);
        Assert.False(result);
    }

    [Fact]
    public void IsActive_AbolishedAfterCheckDate_ReturnsTrue()
    {
        var checkDate = new LocalDateTime(new DateTime(2026, 9, 15, 10, 0, 0));
        var abolishDate = new LocalDateTime(new DateTime(2026, 9, 16, 10, 0, 0));
        var dept = Department.Create(
            DepartmentRowId.From(1),
            DepartmentCode.From("FUTU"),
            "Future Abolish",
            HierarchyLevel.Department(),
            abolishedOn: AbolishedOn.From(abolishDate));

        var result = dept.IsActive(checkDate);
        Assert.True(result);
    }

    [Fact]
    public void UpdateParent_SelfReference_ThrowsException()
    {
        var rowId = DepartmentRowId.From(1);
        var dept = Department.Create(rowId, DepartmentCode.From("SELF"), "Self", HierarchyLevel.Department());

        Assert.Throws<InvalidOperationException>(() => dept.UpdateParent(ParentDepartmentRowId.From(1)));
    }

    [Fact]
    public void UpdateParent_DifferentParent_Succeeds()
    {
        var dept = Department.Create(
            DepartmentRowId.From(2),
            DepartmentCode.From("CHLD"),
            "Child",
            HierarchyLevel.Team());

        var newParent = ParentDepartmentRowId.From(1);
        dept.UpdateParent(newParent);

        Assert.Equal(newParent, dept.ParentId);
    }

    [Fact]
    public void Abolish_SetsAbolishedOn()
    {
        var dept = Department.Create(
            DepartmentRowId.From(1),
            DepartmentCode.From("ABOL"),
            "To Abolish",
            HierarchyLevel.Department());

        var abolishDate = new LocalDateTime(new DateTime(2026, 9, 15, 10, 0, 0));
        dept.Abolish(AbolishedOn.From(abolishDate));

        Assert.True(dept.AbolishedOn.IsAbolished);
        Assert.Equal(abolishDate, dept.AbolishedOn.Value);
    }

    [Fact]
    public void RestoreFromAbolithing_ClearsAbolishedOn()
    {
        var dept = Department.Create(
            DepartmentRowId.From(1),
            DepartmentCode.From("REST"),
            "Restore",
            HierarchyLevel.Department(),
            abolishedOn: AbolishedOn.From(new LocalDateTime(new DateTime(2026, 9, 15, 10, 0, 0))));

        dept.RestoreFromAbolithing();

        Assert.False(dept.AbolishedOn.IsAbolished);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var dept = Department.Create(
            DepartmentRowId.From(1),
            DepartmentCode.From("TEST"),
            "Test Department",
            HierarchyLevel.Department());

        var str = dept.ToString();
        Assert.Contains("RowId=1", str);
        Assert.Contains("Code=TEST", str);
        Assert.Contains("Name=Test Department", str);
    }

    [Fact]
    public void Reconstruct_WithAllParameters_ReturnsInstance()
    {
        var dept = Department.Reconstruct(
            DepartmentRowId.From(1),
            DepartmentCode.From("RCON"),
            "Reconstructed",
            HierarchyLevel.Department(),
            ParentDepartmentRowId.Unset(),
            ManagerEmployeeRowId.Unset(),
            AbolishedOn.Unset(),
            new byte[] { 1, 2, 3 });

        Assert.Equal("Reconstructed", dept.Name);
        Assert.NotNull(dept.RowVersion);
    }
}
