namespace SupportAdvance.Tests.Contexts.Employee.Domain.Entities;

using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Identifiers;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using Xunit;

/// <summary>
/// Employee Entity の単体テスト
/// </summary>
public class EmployeeTests
{
    #region グループ 1: 生成メソッド（Create）

    [Fact]
    public void TestEMPCREATE01_CreateValidEmployeeReturnsValidEmployee()
    {
        // Arrange
        var id = EmployeeId.NewId();
        var rowId = EmployeeRowId.From(1L);
        var code = EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1234));
        var personRowId = PersonRowId.From(1L);

        // Act
        var employee = Employee.Create(id, rowId, code, personRowId);

        // Assert
        Assert.NotNull(employee);
        Assert.Equal(id, employee.Id);
        Assert.Equal(rowId, employee.RowId);
        Assert.Equal(code, employee.Code);
        Assert.Equal(personRowId, employee.PersonRowId);
    }

    [Fact]
    public void TestEMPCREATE02_CreateMultipleEmployeesWithDifferentCodes()
    {
        // Arrange & Act
        var employee1 = Employee.Create(
            EmployeeId.NewId(),
            EmployeeRowId.From(1L),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1234)),
            PersonRowId.From(1L));

        var employee2 = Employee.Create(
            EmployeeId.NewId(),
            EmployeeRowId.From(2L),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(5678)),
            PersonRowId.From(2L));

        // Assert
        Assert.NotEqual(employee1.Id, employee2.Id);
        Assert.NotEqual(employee1.Code, employee2.Code);
    }

    [Fact]
    public void TestEMPCREATE03_CreateRegularEmployeeIsMRegularEmployee()
    {
        // Act
        var employee = Employee.Create(
            EmployeeId.NewId(),
            EmployeeRowId.From(1L),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1234)),
            PersonRowId.From(1L));

        // Assert
        Assert.True(employee.Code.Division.IsRegularEmployee);
        Assert.False(employee.Code.Division.IsDispatched);
        Assert.False(employee.Code.Division.IsContractor);
    }

    [Fact]
    public void TestEMPCREATE04_CreateDispatchedEmployeeIsTDispatched()
    {
        // Act
        var employee = Employee.Create(
            EmployeeId.NewId(),
            EmployeeRowId.From(1L),
            EmployeeCode.From(EmployeeDivision.Dispatched(), EmployeeNumber.From(7500)),
            PersonRowId.From(1L));

        // Assert
        Assert.True(employee.Code.Division.IsDispatched);
        Assert.False(employee.Code.Division.IsRegularEmployee);
        Assert.False(employee.Code.Division.IsContractor);
    }

    [Fact]
    public void TestEMPCREATE05_CreateContractorEmployeeIsCContractor()
    {
        // Act
        var employee = Employee.Create(
            EmployeeId.NewId(),
            EmployeeRowId.From(1L),
            EmployeeCode.From(EmployeeDivision.Contractor(), EmployeeNumber.From(8000)),
            PersonRowId.From(1L));

        // Assert
        Assert.True(employee.Code.Division.IsContractor);
        Assert.False(employee.Code.Division.IsRegularEmployee);
        Assert.False(employee.Code.Division.IsDispatched);
    }

    #endregion

    #region グループ 2: 復元メソッド（Reconstruct）

    [Fact]
    public void TestEMPRECONSTRUCT01_ReconstructFromDbValuesReturnsValidEmployee()
    {
        // Arrange
        var id = EmployeeId.From(new Guid("12345678-1234-1234-1234-123456789012"));
        var rowId = EmployeeRowId.From(12345L);
        var code = EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1234));
        var personRowId = PersonRowId.From(67890L);

        // Act
        var employee = Employee.Reconstruct(id, rowId, code, personRowId);

        // Assert
        Assert.NotNull(employee);
        Assert.Equal(id, employee.Id);
        Assert.Equal(rowId, employee.RowId);
        Assert.Equal(code, employee.Code);
        Assert.Equal(personRowId, employee.PersonRowId);
    }

    [Fact]
    public void TestEMPRECONSTRUCT02_ReconstructMultipleEmployeesWithDifferentIds()
    {
        // Arrange & Act
        var employee1 = Employee.Reconstruct(
            EmployeeId.From(new Guid("12345678-1234-1234-1234-123456789012")),
            EmployeeRowId.From(1L),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1234)),
            PersonRowId.From(1L));

        var employee2 = Employee.Reconstruct(
            EmployeeId.From(new Guid("87654321-4321-4321-4321-210987654321")),
            EmployeeRowId.From(2L),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(5678)),
            PersonRowId.From(2L));

        // Assert
        Assert.NotEqual(employee1.Id, employee2.Id);
        Assert.NotEqual(employee1.RowId, employee2.RowId);
    }

    #endregion

    #region グループ 3: プロパティアクセス

    [Fact]
    public void TestEMPPROP01_IdPropertyReturnsEmployeeId()
    {
        // Arrange
        var id = EmployeeId.NewId();
        var employee = Employee.Create(
            id,
            EmployeeRowId.From(1L),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1234)),
            PersonRowId.From(1L));

        // Act
        var resultId = employee.Id;

        // Assert
        Assert.Equal(id, resultId);
    }

    [Fact]
    public void TestEMPPROP02_RowIdPropertyReturnsEmployeeRowId()
    {
        // Arrange
        var rowId = EmployeeRowId.From(12345L);
        var employee = Employee.Create(
            EmployeeId.NewId(),
            rowId,
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1234)),
            PersonRowId.From(1L));

        // Act
        var resultRowId = employee.RowId;

        // Assert
        Assert.Equal(rowId, resultRowId);
    }

    [Fact]
    public void TestEMPPROP03_CodePropertyReturnsEmployeeCode()
    {
        // Arrange
        var code = EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1234));
        var employee = Employee.Create(
            EmployeeId.NewId(),
            EmployeeRowId.From(1L),
            code,
            PersonRowId.From(1L));

        // Act
        var resultCode = employee.Code;

        // Assert
        Assert.Equal(code, resultCode);
        Assert.NotNull(resultCode.Division);
        Assert.NotNull(resultCode.Number);
    }

    [Fact]
    public void TestEMPPROP04_PersonRowIdPropertyReturnsPersonRowId()
    {
        // Arrange
        var personRowId = PersonRowId.From(67890L);
        var employee = Employee.Create(
            EmployeeId.NewId(),
            EmployeeRowId.From(1L),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1234)),
            personRowId);

        // Act
        var resultPersonRowId = employee.PersonRowId;

        // Assert
        Assert.Equal(personRowId, resultPersonRowId);
    }

    [Fact]
    public void TestEMPPROP05_PropertiesAreReadOnly()
    {
        // Arrange
        var employee = Employee.Create(
            EmployeeId.NewId(),
            EmployeeRowId.From(1L),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1234)),
            PersonRowId.From(1L));

        // Act & Assert
        // 以下はコンパイルエラーになる（CS0200: Property cannot be assigned to）
        // employee.Code = newCode;
        // employee.RowId = newRowId;
        // employee.PersonRowId = newPersonRowId;

        // 読み取りのみ可能
        Assert.NotNull(employee.Code);
    }

    #endregion

    #region グループ 4: 等価性（Equality）

    [Fact]
    public void TestEMPEQ01_SameEmployeeIdAreEqual()
    {
        // Arrange
        var id = EmployeeId.NewId();
        var employee1 = Employee.Create(
            id,
            EmployeeRowId.From(1L),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1234)),
            PersonRowId.From(1L));

        var employee2 = Employee.Create(
            id,  // 同じ ID
            EmployeeRowId.From(2L),  // 異なる RowId
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(5678)),  // 異なる Code
            PersonRowId.From(2L));  // 異なる PersonRowId

        // Assert
        Assert.Equal(employee1, employee2);  // Entity<TId> は Id で比較
    }

    [Fact]
    public void TestEMPEQ02_DifferentEmployeeIdAreNotEqual()
    {
        // Arrange
        var employee1 = Employee.Create(
            EmployeeId.NewId(),
            EmployeeRowId.From(1L),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1234)),
            PersonRowId.From(1L));

        var employee2 = Employee.Create(
            EmployeeId.NewId(),  // 異なる ID
            EmployeeRowId.From(1L),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1234)),
            PersonRowId.From(1L));

        // Assert
        Assert.NotEqual(employee1, employee2);
    }

    [Fact]
    public void TestEMPEQ03_HashCodesAreEqual()
    {
        // Arrange
        var id = EmployeeId.NewId();
        var employee1 = Employee.Create(
            id,
            EmployeeRowId.From(1L),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1234)),
            PersonRowId.From(1L));

        var employee2 = Employee.Create(
            id,
            EmployeeRowId.From(2L),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(5678)),
            PersonRowId.From(2L));

        // Assert
        Assert.Equal(employee1.GetHashCode(), employee2.GetHashCode());
    }

    [Fact]
    public void TestEMPEQ04_CanBeUsedAsDictionaryKey()
    {
        // Arrange
        var id = EmployeeId.NewId();
        var employee1 = Employee.Create(
            id,
            EmployeeRowId.From(1L),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1234)),
            PersonRowId.From(1L));

        var employee2 = Employee.Create(
            id,
            EmployeeRowId.From(2L),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(5678)),
            PersonRowId.From(2L));

        var dict = new Dictionary<Employee, string>();

        // Act
        dict.Add(employee1, "Employee1");
        dict[employee2] = "Employee2";  // 同じ ID なので上書き

        // Assert
        Assert.Single(dict);
        Assert.Equal("Employee2", dict[employee1]);
    }

    [Fact]
    public void TestEMPEQ05_EqualsNullReturnsFalse()
    {
        // Arrange
        var employee = Employee.Create(
            EmployeeId.NewId(),
            EmployeeRowId.From(1L),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1234)),
            PersonRowId.From(1L));

        // Act & Assert
        Assert.False(employee.Equals(null));
    }

    #endregion

    #region グループ 5: 統合テスト

    [Fact]
    public void TestEMPINTEG01_AllPropertiesAreCoherent()
    {
        // Arrange
        var id = EmployeeId.NewId();
        var rowId = EmployeeRowId.From(12345L);
        var code = EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1234));
        var personRowId = PersonRowId.From(67890L);

        // Act
        var employee = Employee.Create(id, rowId, code, personRowId);

        // Assert - すべてのプロパティが有効
        Assert.NotEqual(Guid.Empty, employee.Id.Value);
        Assert.Equal(12345L, employee.RowId.Value);
        Assert.Equal("M1234", employee.Code.ToString());
        Assert.Equal(67890L, employee.PersonRowId.Value);
    }

    [Fact]
    public void TestEMPINTEG02_PropertyImmutability()
    {
        // Arrange
        var employee = Employee.Create(
            EmployeeId.NewId(),
            EmployeeRowId.From(1L),
            EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1234)),
            PersonRowId.From(1L));

        // Act
        var code1 = employee.Code;
        var code2 = employee.Code;

        // Assert
        Assert.Same(code1, code2);  // 同じインスタンス（値が変わらない）
    }

    #endregion

    #region グループ 6: ValueObject 検証統合

    [Fact]
    public void TestEMPVO01_InvalidEmployeeCodeThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            Employee.Create(
                EmployeeId.NewId(),
                EmployeeRowId.From(1L),
                EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(7500)),  // 無効
                PersonRowId.From(1L)));
    }

    [Fact]
    public void TestEMPVO02_InvalidRowIdThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Employee.Create(
                EmployeeId.NewId(),
                EmployeeRowId.From(0L),  // 無効
                EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1234)),
                PersonRowId.From(1L)));
    }

    [Fact]
    public void TestEMPVO03_InvalidPersonRowIdThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            Employee.Create(
                EmployeeId.NewId(),
                EmployeeRowId.From(1L),
                EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1234)),
                PersonRowId.From(0L)));  // 無効
    }

    #endregion
}
