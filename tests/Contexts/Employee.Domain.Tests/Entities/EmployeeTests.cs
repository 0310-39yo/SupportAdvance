using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Person;

namespace SupportAdvance.Tests.Contexts.Employee.Domain.Entities;

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using Xunit;

/// <summary>
/// Employee Entity の単体テスト
/// </summary>
public class EmployeeTests
{
    private static LocalDateTime GetTestDate() => new(new DateTime(2026, 8, 12, 10, 0, 0));

    private readonly IClock _clock = new SystemClock();

    private Employee CreateTestEmployee(
        long rowId = 1L,
        int bizId = 1234,
        string? divisionCode = "M",
        long personRowId = 1L)
    {
        var typeDivision = divisionCode switch
        {
            "M" => BizDivision.RegularEmployee(),
            "T" => BizDivision.Dispatched(),
            "C" => BizDivision.Contractor(),
            _ => BizDivision.RegularEmployee()
        };

        var employeeBizId = BizId.From(bizId);
        var bizCode = BizCode.From(typeDivision, employeeBizId);
        var personRowIdVO = PersonRowId.From(personRowId);

        var person = Person.Create(
            personRowIdVO,
            LastName.From("山田"),
            FirstName.From("太郎"),
            LastNameKana.From("ヤマダ"),
            FirstNameKana.From("タロウ"));

        return Employee.Create(
            EmployeeRowId.From(rowId),
            typeDivision,
            employeeBizId,
            bizCode,
            null,
            person,
            new List<DepartmentMembership>()
            );
    }

    #region グループ 1: 生成メソッド（Create）

    [Fact]
    public void TestEMPCREATE01_CreateValidEmployeeReturnsValidEmployee()
    {
        // Act
        var employee = CreateTestEmployee();

        // Assert
        Assert.NotNull(employee);
        Assert.Equal(1L, employee.RowId.Value);
        Assert.Equal('M', employee.TypeDivision.Value);
        Assert.Equal("01234", employee.BizId.ToString());
        Assert.Equal("M01234", employee.BizCode.ToString());
        Assert.Equal(1L, employee.Person.RowId.Value);
    }

    [Fact]
    public void TestEMPCREATE02_CreateRegularEmployeeIsRegularEmployee()
    {
        // Act
        var employee = CreateTestEmployee(divisionCode: "M");

        // Assert
        Assert.True(employee.TypeDivision.IsRegularEmployee);
        Assert.False(employee.TypeDivision.IsDispatched);
        Assert.False(employee.TypeDivision.IsContractor);
    }

    [Fact]
    public void TestEMPCREATE03_CreateDispatchedEmployeeIsDispatched()
    {
        // Act
        var employee = CreateTestEmployee(bizId: 7500, divisionCode: "T");

        // Assert
        Assert.True(employee.TypeDivision.IsDispatched);
        Assert.False(employee.TypeDivision.IsRegularEmployee);
        Assert.False(employee.TypeDivision.IsContractor);
    }

    [Fact]
    public void TestEMPCREATE04_CreateContractorEmployeeIsContractor()
    {
        // Act
        var employee = CreateTestEmployee(bizId: 8000, divisionCode: "C");

        // Assert
        Assert.True(employee.TypeDivision.IsContractor);
        Assert.False(employee.TypeDivision.IsRegularEmployee);
        Assert.False(employee.TypeDivision.IsDispatched);
    }

    #endregion

    #region グループ 2: オプションプロパティ（RetiredOn）

    [Fact]
    public void TestEMPRETIRED01_CreateWithRetiredOnReturnsRetiredEmployee()
    {
        // Arrange
        var rowId = EmployeeRowId.From(12345L);
        var typeDivision = BizDivision.RegularEmployee();
        var bizId = BizId.From(1234);
        var bizCode = BizCode.From(typeDivision, bizId);
        var personRowId = PersonRowId.From(67890L);
        var person = Person.Create(
            personRowId,
            LastName.From("山田"),
            FirstName.From("太郎"),
            LastNameKana.From("ヤマダ"),
            FirstNameKana.From("タロウ"));
        var retiredOn = RetiredOn.From(GetTestDate());

        // Act
        var employee = Employee.Create(
            rowId,
            typeDivision,
            bizId,
            bizCode,
            retiredOn,
            person,
            new List<DepartmentMembership>()
            );

        // Assert
        Assert.NotNull(employee);
        Assert.Equal(rowId, employee.RowId);
        Assert.True(employee.RetiredOn?.HasRetired == true);
        Assert.Equal(GetTestDate(), employee.RetiredOn?.Value);
    }

    #endregion

    #region グループ 3: プロパティアクセス

    [Fact]
    public void TestEMPPROP01_RowIdPropertyReturnsEmployeeRowId()
    {
        // Arrange
        var employee = CreateTestEmployee(rowId: 12345L);

        // Act
        var resultRowId = employee.RowId;

        // Assert
        Assert.Equal(12345L, resultRowId.Value);
    }

    [Fact]
    public void TestEMPPROP02_TypeDivisionPropertyReturnsTypeDivision()
    {
        // Arrange
        var employee = CreateTestEmployee(divisionCode: "M");

        // Act
        var resultTypeDivision = employee.TypeDivision;

        // Assert
        Assert.Equal('M', resultTypeDivision.Value);
        Assert.True(resultTypeDivision.IsRegularEmployee);
    }

    [Fact]
    public void TestEMPPROP03_BizCodePropertyReturnsBizCode()
    {
        // Arrange
        var employee = CreateTestEmployee();

        // Act
        var resultBizCode = employee.BizCode;

        // Assert
        Assert.Equal("M01234", resultBizCode.ToString());
    }

    [Fact]
    public void TestEMPPROP04_PersonInfoPropertyReturnsPerson()
    {
        // Arrange
        var employee = CreateTestEmployee(personRowId: 67890L);

        // Act
        var resultPersonInfo = employee.Person;

        // Assert
        Assert.NotNull(resultPersonInfo);
        Assert.Equal(67890L, resultPersonInfo.RowId.Value);
        Assert.Equal("山田", resultPersonInfo.LastName.Value);
    }

    #endregion

    #region グループ 4: 等価性（Equality）

    [Fact]
    public void TestEMPEQ01_SameRowIdAreEqual()
    {
        // Arrange
        var employee1 = CreateTestEmployee(rowId: 1L);
        var employee2 = CreateTestEmployee(rowId: 1L, bizId: 5678);

        // Assert
        Assert.Equal(employee1, employee2);  // Entity<TId> は RowId で比較
    }

    [Fact]
    public void TestEMPEQ02_DifferentRowIdAreNotEqual()
    {
        // Arrange
        var employee1 = CreateTestEmployee(rowId: 1L);
        var employee2 = CreateTestEmployee(rowId: 2L);

        // Assert
        Assert.NotEqual(employee1, employee2);
    }

    [Fact]
    public void TestEMPEQ03_HashCodesAreEqual()
    {
        // Arrange
        var employee1 = CreateTestEmployee(rowId: 1L);
        var employee2 = CreateTestEmployee(rowId: 1L, bizId: 5678);

        // Assert
        Assert.Equal(employee1.GetHashCode(), employee2.GetHashCode());
    }

    #endregion

    #region グループ 5: 勤務状態判定

    [Fact]
    public void TestEMPSTATE01_IsActiveReturnsTrueForCurrentEmployee()
    {
        // Arrange
        var employee = CreateTestEmployee();
        var checkDate = GetTestDate();

        // Act
        var isActive = employee.IsActive(checkDate);

        // Assert
        Assert.True(isActive);
    }

    [Fact]
    public void TestEMPSTATE02_IsActiveReturnsFalseForRetiredEmployee()
    {
        // Arrange
        var now = GetTestDate();
        var retiredOn = RetiredOn.From(now);
        var personRowId = PersonRowId.From(1L);
        var person = Person.Create(
            personRowId,
            LastName.From("山田"),
            FirstName.From("太郎"),
            LastNameKana.From("ヤマダ"),
            FirstNameKana.From("タロウ"));

        var retiredEmployee = Employee.Create(
            EmployeeRowId.From(1L),
            BizDivision.RegularEmployee(),
            BizId.From(1234),
            BizCode.From(BizDivision.RegularEmployee(), BizId.From(1234)),
            retiredOn,
            person,
            new List<DepartmentMembership>()
            );

        // Act
        var isActive = retiredEmployee.IsActive(now);

        // Assert
        Assert.False(isActive);
    }

    #endregion

    #region グループ 6: ValueObject 検証統合

    [Fact]
    public void TestEMPVO01_InvalidBizIdThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateTestEmployee(bizId: 1000));  // 1000は予約済み
    }

    [Fact]
    public void TestEMPVO02_InvalidRowIdThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateTestEmployee(rowId: 0L));  // 無効
    }

    [Fact]
    public void TestEMPVO03_InvalidPersonRowIdThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateTestEmployee(personRowId: 0L));  // 無効
    }

    #endregion
}
