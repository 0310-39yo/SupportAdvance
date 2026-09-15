namespace SupportAdvance.Contexts.Department.Infrastructure.Tests.Mappers;

using SupportAdvance.Contexts.Department.Domain.Entities;
using SupportAdvance.Contexts.Department.Domain.ValueObjects;
using SupportAdvance.Contexts.Department.Infrastructure.DbModels;
using SupportAdvance.Contexts.Department.Infrastructure.Mappers;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;
using Xunit;

/// <summary>
/// DepartmentMapper の単体テスト（DB非依存の純粋ロジック）
/// </summary>
public class DepartmentMapperTests
{
    #region グループ 1: ToDomainEntity - 正常系

    [Fact]
    public void Test1_1_ToDomainEntity_WithValidDbModel_ReturnsDepartmentEntity()
    {
        // Arrange
        var mapper = new DepartmentMapper();
        var dbModel = new DepartmentDbModel
        {
            RowId = 1L,
            Code = "D001",
            Name = "営業部",
            Level = 1,
            ParentDepartmentRowId = null,
            ManagerEmployeeRowId = null,
            AbolishedOn = null,
            RowVersion = []
        };

        // Act
        var result = mapper.ToDomainEntity(dbModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1L, result.RowId.Value);
        Assert.Equal("D001", result.DeptCode.Value);
        Assert.Equal("営業部", result.Name);
        Assert.Equal(1, result.Level.Value);
    }

    [Fact]
    public void Test1_2_ToDomainEntity_WithAllValueObjects_ConvertsSuccessfully()
    {
        // Arrange
        var mapper = new DepartmentMapper();
        var dbModel = new DepartmentDbModel
        {
            RowId = 2L,
            Code = "D002",
            Name = "企画部",
            Level = 2,
            ParentDepartmentRowId = 1L,
            ManagerEmployeeRowId = 100L,
            AbolishedOn = null,
            RowVersion = [0x01, 0x02, 0x03]
        };

        // Act
        var result = mapper.ToDomainEntity(dbModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2L, result.RowId.Value);
        Assert.Equal("D002", result.DeptCode.Value);
        Assert.Equal("企画部", result.Name);
        Assert.Equal(2, result.Level.Value);
        Assert.True(result.ParentId.IsSet);
        Assert.Equal(1L, result.ParentId.Value);
        Assert.True(result.ManagerId.IsSet);
        Assert.Equal(100L, result.ManagerId.Value);
    }

    [Fact]
    public void Test1_3_ToDomainEntity_WithAbolishedOn_ConvertsSuccessfully()
    {
        // Arrange
        var mapper = new DepartmentMapper();
        var abolishedDate = new DateTime(2026, 9, 30);
        var dbModel = new DepartmentDbModel
        {
            RowId = 3L,
            Code = "D003",
            Name = "旧製造部",
            Level = 1,
            ParentDepartmentRowId = null,
            ManagerEmployeeRowId = null,
            AbolishedOn = abolishedDate,
            RowVersion = []
        };

        // Act
        var result = mapper.ToDomainEntity(dbModel);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.AbolishedOn.IsAbolished);
    }

    #endregion

    #region グループ 2: ToDomainEntity - 異常系

    [Fact]
    public void Test2_1_ToDomainEntity_WithNullDbModel_ThrowsArgumentNullException()
    {
        // Arrange
        var mapper = new DepartmentMapper();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => mapper.ToDomainEntity(null!));
    }

    [Fact]
    public void Test2_2_ToDomainEntity_WithInvalidCode_ThrowsInvalidOperationException()
    {
        // Arrange
        var mapper = new DepartmentMapper();
        var dbModel = new DepartmentDbModel
        {
            RowId = 1L,
            Code = "", // Invalid: empty code
            Name = "テスト部",
            Level = 1,
            ParentDepartmentRowId = null,
            ManagerEmployeeRowId = null,
            AbolishedOn = null
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => mapper.ToDomainEntity(dbModel));
    }

    [Fact]
    public void Test2_3_ToDomainEntity_WithInvalidLevel_ThrowsInvalidOperationException()
    {
        // Arrange
        var mapper = new DepartmentMapper();
        var dbModel = new DepartmentDbModel
        {
            RowId = 1L,
            Code = "D001",
            Name = "テスト部",
            Level = 99, // Invalid: out of range
            ParentDepartmentRowId = null,
            ManagerEmployeeRowId = null,
            AbolishedOn = null
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => mapper.ToDomainEntity(dbModel));
    }

    #endregion

    #region グループ 3: ToDbModel - 正常系

    [Fact]
    public void Test3_1_ToDbModel_WithValidEntity_ReturnsDbModel()
    {
        // Arrange
        var mapper = new DepartmentMapper();
        var entity = Department.Create(
            DepartmentRowId.From(1L),
            DepartmentCode.From("D001"),
            "営業部",
            HierarchyLevel.From(1)
        );

        // Act
        var result = mapper.ToDbModel(entity);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1L, result.RowId);
        Assert.Equal("D001", result.Code);
        Assert.Equal("営業部", result.Name);
        Assert.Equal(1, result.Level);
        Assert.Null(result.ParentDepartmentRowId);
        Assert.Null(result.ManagerEmployeeRowId);
        Assert.Null(result.AbolishedOn);
    }

    [Fact]
    public void Test3_2_ToDbModel_WithParentAndManager_ConvertsSuccessfully()
    {
        // Arrange
        var mapper = new DepartmentMapper();
        var entity = Department.Create(
            DepartmentRowId.From(2L),
            DepartmentCode.From("D002"),
            "企画部",
            HierarchyLevel.From(2),
            ParentDepartmentRowId.From(1L),
            ManagerEmployeeRowId.From(100L)
        );

        // Act
        var result = mapper.ToDbModel(entity);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2L, result.RowId);
        Assert.Equal(1L, result.ParentDepartmentRowId);
        Assert.Equal(100L, result.ManagerEmployeeRowId);
        Assert.Null(result.AbolishedOn);
    }

    [Fact]
    public void Test3_3_ToDbModel_WithUnsetParentAndManager_SetsNullValues()
    {
        // Arrange
        var mapper = new DepartmentMapper();
        var entity = Department.Create(
            DepartmentRowId.From(3L),
            DepartmentCode.From("D003"),
            "トップ部門",
            HierarchyLevel.From(0)
        );

        // Act
        var result = mapper.ToDbModel(entity);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.ParentDepartmentRowId);
        Assert.Null(result.ManagerEmployeeRowId);
    }

    [Fact]
    public void Test3_4_ToDbModel_WithAbolishedEntity_ConvertsAbolishedOn()
    {
        // Arrange
        var mapper = new DepartmentMapper();
        var abolishedDate = new DateTime(2026, 9, 30, 0, 0, 0);
        var localDateTime = new SupportAdvance.Common.Clocks.LocalDateTime(abolishedDate);
        var entity = Department.Create(
            DepartmentRowId.From(4L),
            DepartmentCode.From("D004"),
            "旧製造部",
            HierarchyLevel.From(1),
            abolishedOn: AbolishedOn.From(localDateTime)
        );

        // Act
        var result = mapper.ToDbModel(entity);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.AbolishedOn);
        Assert.Equal(abolishedDate, result.AbolishedOn.Value);
    }

    #endregion

    #region グループ 4: ToDbModel - 異常系

    [Fact]
    public void Test4_1_ToDbModel_WithNullEntity_ThrowsArgumentNullException()
    {
        // Arrange
        var mapper = new DepartmentMapper();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => mapper.ToDbModel(null!));
    }

    [Fact]
    public void Test4_2_ToDbModel_ShouldNotSetAuditFields()
    {
        // Arrange
        var mapper = new DepartmentMapper();
        var entity = Department.Create(
            DepartmentRowId.From(1L),
            DepartmentCode.From("D001"),
            "テスト部",
            HierarchyLevel.From(1)
        );

        // Act
        var result = mapper.ToDbModel(entity);

        // Assert
        // Mapper は監査フィールドを設定しない（Repository の責務）
        Assert.Equal(default, result.CreatedAt);
        Assert.Equal(default, result.UpdatedAt);
        Assert.Equal(default(long), result.CreatedBy);
        Assert.Equal(default(long), result.UpdatedBy);
    }

    #endregion

    #region グループ 5: ラウンドトリップ

    [Fact]
    public void Test5_1_RoundTrip_DbModelToDomainAndBack_PreservesData()
    {
        // Arrange
        var mapper = new DepartmentMapper();
        var originalDbModel = new DepartmentDbModel
        {
            RowId = 1L,
            Code = "D001",
            Name = "営業部",
            Level = 1,
            ParentDepartmentRowId = null,
            ManagerEmployeeRowId = null,
            AbolishedOn = null,
            RowVersion = [0x01]
        };

        // Act
        var domainEntity = mapper.ToDomainEntity(originalDbModel);
        var resultDbModel = mapper.ToDbModel(domainEntity);

        // Assert
        Assert.Equal(1L, resultDbModel.RowId);
        Assert.Equal("D001", resultDbModel.Code);
        Assert.Equal("営業部", resultDbModel.Name);
        Assert.Equal(1, resultDbModel.Level);
    }

    #endregion
}
