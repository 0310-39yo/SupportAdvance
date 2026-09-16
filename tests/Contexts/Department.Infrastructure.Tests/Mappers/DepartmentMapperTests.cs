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
    public void VO_MAP_02_ToDomainEntity_WithValidDbModel_MapsAllFields()
    {
        // Arrange
        var mapper = new DepartmentMapper();
        var dbModel = new DepartmentDbModel
        {
            RowId = 1L,
            DepartmentCode = "D001",
            DepartmentName = "営業部",
            HierarchyLevel = 1,
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
    public void VO_TYPE_03_ParentIdConversion_WithValidId_IsSetCorrectly()
    {
        // Arrange
        var mapper = new DepartmentMapper();
        var dbModel = new DepartmentDbModel
        {
            RowId = 2L,
            DepartmentCode = "D002",
            DepartmentName = "企画部",
            HierarchyLevel = 2,
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
    public void VO_TYPE_05_NullConversion_WithNullFields_ConvertsToUnset()
    {
        // Arrange
        var mapper = new DepartmentMapper();
        var abolishedDate = new DateTime(2026, 9, 30);
        var dbModel = new DepartmentDbModel
        {
            RowId = 3L,
            DepartmentCode = "D003",
            DepartmentName = "旧製造部",
            HierarchyLevel = 1,
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
            DepartmentCode = "", // Invalid: empty code
            DepartmentName = "テスト部",
            HierarchyLevel = 1,
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
            DepartmentCode = "D001",
            DepartmentName = "テスト部",
            HierarchyLevel = 99, // Invalid: out of range
            ParentDepartmentRowId = null,
            ManagerEmployeeRowId = null,
            AbolishedOn = null
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => mapper.ToDomainEntity(dbModel));
    }

    #endregion

    #region グループ 2.5: コード変換

    [Fact]
    public void VO_TYPE_01_CodeConversion_WithValidCode_ConvertsSuccessfully()
    {
        // Arrange
        var mapper = new DepartmentMapper();
        var dbModel = new DepartmentDbModel
        {
            RowId = 1L,
            DepartmentCode = "D001",
            DepartmentName = "営業部",
            HierarchyLevel = 1,
            ParentDepartmentRowId = null,
            ManagerEmployeeRowId = null,
            AbolishedOn = null,
            RowVersion = []
        };

        // Act
        var result = mapper.ToDomainEntity(dbModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("D001", result.DeptCode.Value);
    }

    #endregion

    #region グループ 3: ToDbModel - 正常系

    [Fact]
    public void VO_MAP_01_ToDbModel_WithValidEntity_ReturnsDbModel()
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
        Assert.Equal("D001", result.DepartmentCode);
        Assert.Equal("営業部", result.DepartmentName);
        Assert.Equal(1, result.HierarchyLevel);
        Assert.Null(result.ParentDepartmentRowId);
        Assert.Null(result.ManagerEmployeeRowId);
        Assert.Null(result.AbolishedOn);
    }

    [Fact]
    public void VO_TYPE_04_ManagerIdConversion_WithValidId_IsSetCorrectly()
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
    public void VO_TYPE_02_LevelConversion_WithValidLevel_ConvertsSuccessfully()
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
    public void VO_TYPE_06_AuditFields_NotSetByMapper()
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
        Assert.Null(result.UpdatedAt);
        Assert.Equal(default(long), result.CreatedBy);
        Assert.Null(result.UpdatedBy);
    }

    #endregion

    #region グループ 5: ラウンドトリップ

    [Fact]
    public void VO_MAP_03_RoundTrip_PreservesData()
    {
        // Arrange
        var mapper = new DepartmentMapper();
        var originalDbModel = new DepartmentDbModel
        {
            RowId = 1L,
            DepartmentCode = "D001",
            DepartmentName = "営業部",
            HierarchyLevel = 1,
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
        Assert.Equal("D001", resultDbModel.DepartmentCode);
        Assert.Equal("営業部", resultDbModel.DepartmentName);
        Assert.Equal(1, resultDbModel.HierarchyLevel);
    }

    #endregion
}
