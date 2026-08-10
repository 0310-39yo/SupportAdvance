namespace SupportAdvance.Contexts.Employee.Infrastructure.Tests.Mappers;

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.Contexts.Employee.Infrastructure.Mappers;
using SupportAdvance.Contexts.Employee.Infrastructure.Models;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using Xunit;

/// <summary>
/// EmployeeMapper の単体テスト
///
/// 責務：Entity ↔ DbModel の双方向マッピングの正確性を検証
/// </summary>
public class EmployeeMapperTests
{
    #region グループ 1: Entity → DbModel 変換

    [Fact]
    public void TestToDbModel01_WithValidEmployeeConvertsCorrectly()
    {
        // Arrange
        var id = EmployeeId.NewId();
        var rowId = EmployeeRowId.From(100L);
        var division = EmployeeDivision.RegularEmployee();
        var number = EmployeeNumber.From(1234);
        var code = EmployeeCode.From(division, number);
        var personRowId = PersonRowId.From(50L);
        var employee = Employee.Create(id, rowId, code, personRowId);

        var mapper = CreateMapper();

        // Act
        var dbModel = mapper.ToDbModel(employee);

        // Assert
        Assert.NotNull(dbModel);
        Assert.Equal(id.Value, dbModel.EmployeeId);
        Assert.Equal(rowId.Value, dbModel.RowId);
        Assert.Equal("M", dbModel.EmployeeCodeDivision);  // RegularEmployee = M
        Assert.Equal(1234, dbModel.EmployeeCodeNumber);
        Assert.Equal(personRowId.Value, dbModel.PersonRowId);
    }

    [Fact]
    public void TestToDbModel02_WithDispatchedEmployeeConvertsCorrectly()
    {
        // Arrange
        var division = EmployeeDivision.Dispatched();
        var code = EmployeeCode.From(division, EmployeeNumber.From(7500));
        var employee = Employee.Create(
            EmployeeId.NewId(),
            EmployeeRowId.From(1L),
            code,
            PersonRowId.From(1L));

        var mapper = CreateMapper();

        // Act
        var dbModel = mapper.ToDbModel(employee);

        // Assert
        Assert.Equal("T", dbModel.EmployeeCodeDivision);  // Dispatched = T
    }

    [Fact]
    public void TestToDbModel03_WithContractorEmployeeConvertsCorrectly()
    {
        // Arrange
        var division = EmployeeDivision.Contractor();
        var code = EmployeeCode.From(division, EmployeeNumber.From(8000));
        var employee = Employee.Create(
            EmployeeId.NewId(),
            EmployeeRowId.From(1L),
            code,
            PersonRowId.From(1L));

        var mapper = CreateMapper();

        // Act
        var dbModel = mapper.ToDbModel(employee);

        // Assert
        Assert.Equal("C", dbModel.EmployeeCodeDivision);  // Contractor = C
    }

    #endregion

    #region グループ 2: DbModel → Entity 変換

    [Fact]
    public void TestToDomainEntity01_WithValidDbModelConvertsCorrectly()
    {
        // Arrange
        var id = EmployeeId.NewId();
        var rowId = 100L;
        var dbModel = new EmployeeDbModel
        {
            EmployeeId = id.Value,
            RowId = rowId,
            EmployeeCodeDivision = "M",
            EmployeeCodeNumber = 1234,
            PersonRowId = 50L,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = 1L
        };

        var mapper = CreateMapper();

        // Act
        var employee = mapper.ToDomainEntity(dbModel);

        // Assert
        Assert.NotNull(employee);
        Assert.Equal(id, employee.Id);
        Assert.Equal(rowId, employee.RowId.Value);
        Assert.True(employee.Code.Division.IsRegularEmployee);
        Assert.Equal(1234, employee.Code.Number.Value);
        Assert.Equal(50L, employee.PersonRowId.Value);
    }

    [Fact]
    public void TestToDomainEntity02_WithDispatchedDivisionConvertsCorrectly()
    {
        // Arrange
        var dbModel = new EmployeeDbModel
        {
            EmployeeId = EmployeeId.NewId().Value,
            RowId = 1L,
            EmployeeCodeDivision = "T",
            EmployeeCodeNumber = 7500,
            PersonRowId = 1L,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = 1L
        };

        var mapper = CreateMapper();

        // Act
        var employee = mapper.ToDomainEntity(dbModel);

        // Assert
        Assert.True(employee.Code.Division.IsDispatched);
    }

    [Fact]
    public void TestToDomainEntity03_WithContractorDivisionConvertsCorrectly()
    {
        // Arrange
        var dbModel = new EmployeeDbModel
        {
            EmployeeId = EmployeeId.NewId().Value,
            RowId = 1L,
            EmployeeCodeDivision = "C",
            EmployeeCodeNumber = 8000,
            PersonRowId = 1L,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = 1L
        };

        var mapper = CreateMapper();

        // Act
        var employee = mapper.ToDomainEntity(dbModel);

        // Assert
        Assert.True(employee.Code.Division.IsContractor);
    }

    #endregion

    #region グループ 3: ラウンドトリップ変換

    [Fact]
    public void TestRoundTrip01_EntityToDbModelToEntityIsConsistent()
    {
        // Arrange
        var originalId = EmployeeId.NewId();
        var originalRowId = EmployeeRowId.From(100L);
        var originalCode = EmployeeCode.From(EmployeeDivision.RegularEmployee(), EmployeeNumber.From(1234));
        var originalPersonRowId = PersonRowId.From(50L);
        var originalEmployee = Employee.Create(originalId, originalRowId, originalCode, originalPersonRowId);

        var mapper = CreateMapper();

        // Act
        var dbModel = mapper.ToDbModel(originalEmployee);
        var reconstructedEmployee = mapper.ToDomainEntity(dbModel);

        // Assert
        Assert.Equal(originalId, reconstructedEmployee.Id);
        Assert.Equal(originalRowId, reconstructedEmployee.RowId);
        Assert.Equal(originalCode, reconstructedEmployee.Code);
        Assert.Equal(originalPersonRowId, reconstructedEmployee.PersonRowId);
    }

    #endregion

    #region グループ 4: エラーハンドリング

    [Fact]
    public void TestToDomainEntity04_WithInvalidDivisionThrowsException()
    {
        // Arrange
        var dbModel = new EmployeeDbModel
        {
            EmployeeId = EmployeeId.NewId().Value,
            RowId = 1L,
            EmployeeCodeDivision = "X",  // 無効な値
            EmployeeCodeNumber = 1234,
            PersonRowId = 1L,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = 1L
        };

        var mapper = CreateMapper();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => mapper.ToDomainEntity(dbModel));
    }

    #endregion

    #region ヘルパーメソッド

    private EmployeeMapper CreateMapper()
    {
        return new EmployeeMapper();
    }

    #endregion
}
