namespace SupportAdvance.Contexts.Employee.Infrastructure.Tests.Mappers;

using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Employee.Domain.Entities;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.DepartmentMembership;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Employee;
using SupportAdvance.Contexts.Employee.Domain.ValueObjects.Person;
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
    private readonly IClock _clock = new SystemClock();

    #region グループ 1: Entity → DbModel 変換

    [Fact]
    public void TestToDbModel01_WithValidEmployeeConvertsCorrectly()
    {
        // Arrange
        var rowId = EmployeeRowId.From(100L);
        var typeDivision = BizDivision.RegularEmployee();
        var bizId = BizId.From(1234);
        var bizCode = BizCode.From(typeDivision, bizId);
        var personRowId = PersonRowId.From(50L);
        var person = Person.Create(
            personRowId,
            LastName.From("山田"),
            FirstName.From("太郎"),
            LastNameKana.From("ヤマダ"),
            FirstNameKana.From("タロウ"));
        var employee = Employee.Create(rowId, typeDivision, bizId, bizCode, null, person, new List<DepartmentMembership>(), _clock);

        var mapper = CreateMapper();

        // Act
        var dbModel = mapper.ToDbModel(employee);

        // Assert
        Assert.NotNull(dbModel);
        Assert.Equal(rowId.Value, dbModel.RowId);
        Assert.Equal("M", dbModel.BizDivision);  // RegularEmployee = M
        Assert.Equal(1234, dbModel.BizId);
    }

    [Fact]
    public void TestToDbModel02_WithDispatchedEmployeeConvertsCorrectly()
    {
        // Arrange
        var typeDivision = BizDivision.Dispatched();
        var bizId = BizId.From(7500);
        var bizCode = BizCode.From(typeDivision, bizId);
        var personRowId = PersonRowId.From(1L);
        var person = Person.Create(
            personRowId,
            LastName.From("鈴木"),
            FirstName.From("花子"),
            LastNameKana.From("スズキ"),
            FirstNameKana.From("ハナコ"));
        var employee = Employee.Create(
            EmployeeRowId.From(1L),
            typeDivision,
            bizId,
            bizCode,
            null,
            person,
            new List<DepartmentMembership>(),
            _clock);

        var mapper = CreateMapper();

        // Act
        var dbModel = mapper.ToDbModel(employee);

        // Assert
        Assert.Equal("T", dbModel.BizDivision);  // Dispatched = T
    }

    [Fact]
    public void TestToDbModel03_WithContractorEmployeeConvertsCorrectly()
    {
        // Arrange
        var typeDivision = BizDivision.Contractor();
        var bizId = BizId.From(8000);
        var bizCode = BizCode.From(typeDivision, bizId);
        var personRowId = PersonRowId.From(1L);
        var person = Person.Create(
            personRowId,
            LastName.From("佐藤"),
            FirstName.From("次郎"),
            LastNameKana.From("サトウ"),
            FirstNameKana.From("ジロウ"));
        var employee = Employee.Create(
            EmployeeRowId.From(1L),
            typeDivision,
            bizId,
            bizCode,
            null,
            person,
            new List<DepartmentMembership>(),
            _clock);

        var mapper = CreateMapper();

        // Act
        var dbModel = mapper.ToDbModel(employee);

        // Assert
        Assert.Equal("C", dbModel.BizDivision);  // Contractor = C
    }

    #endregion

    #region グループ 2: DbModel → Entity 変換

    [Fact]
    public void TestToDomainEntity01_WithValidDbModelConvertsCorrectly()
    {
        // Arrange
        var rowId = 100L;
        var personDbModel = new PersonDbModel
        {
            RowId = 50L,
            EmployeeRowId = rowId,
            LastName = "山田",
            FirstName = "太郎",
            LastNameKana = "ヤマダ",
            FirstNameKana = "タロウ",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = 1L
        };
        var dbModel = new EmployeeDbModel
        {
            RowId = rowId,
            BizDivision = "M",
            BizId = 1234,
            RetiredOn = null,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = 1L
        };

        var mapper = CreateMapper();

        // Act
        var employee = mapper.ToDomainEntity(dbModel, personDbModel, _clock, null);

        // Assert
        Assert.NotNull(employee);
        Assert.Equal(rowId, employee.RowId.Value);
        Assert.True(employee.TypeDivision.IsRegularEmployee);
        Assert.Equal(1234, employee.BizId.Value);
        Assert.Equal(50L, employee.Person.RowId.Value);
    }

    [Fact]
    public void TestToDomainEntity02_WithDispatchedDivisionConvertsCorrectly()
    {
        // Arrange
        var personDbModel = new PersonDbModel
        {
            RowId = 1L,
            EmployeeRowId = 1L,
            LastName = "鈴木",
            FirstName = "花子",
            LastNameKana = "スズキ",
            FirstNameKana = "ハナコ",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = 1L
        };
        var dbModel = new EmployeeDbModel
        {
            RowId = 1L,
            BizDivision = "T",
            BizId = 7500,
            RetiredOn = null,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = 1L
        };

        var mapper = CreateMapper();

        // Act
        var employee = mapper.ToDomainEntity(dbModel, personDbModel, _clock, null);

        // Assert
        Assert.True(employee.TypeDivision.IsDispatched);
    }

    [Fact]
    public void TestToDomainEntity03_WithContractorDivisionConvertsCorrectly()
    {
        // Arrange
        var personDbModel = new PersonDbModel
        {
            RowId = 1L,
            EmployeeRowId = 1L,
            LastName = "佐藤",
            FirstName = "次郎",
            LastNameKana = "サトウ",
            FirstNameKana = "ジロウ",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = 1L
        };
        var dbModel = new EmployeeDbModel
        {
            RowId = 1L,
            BizDivision = "C",
            BizId = 8000,
            RetiredOn = null,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = 1L
        };

        var mapper = CreateMapper();

        // Act
        var employee = mapper.ToDomainEntity(dbModel, personDbModel, _clock, null);

        // Assert
        Assert.True(employee.TypeDivision.IsContractor);
    }

    #endregion

    #region グループ 3: ラウンドトリップ変換

    [Fact]
    public void TestRoundTrip01_EntityToDbModelToEntityIsConsistent()
    {
        // Arrange
        var originalRowId = EmployeeRowId.From(100L);
        var originalTypeDivision = BizDivision.RegularEmployee();
        var originalBizId = BizId.From(1234);
        var originalBizCode = BizCode.From(originalTypeDivision, originalBizId);
        var originalPersonRowId = PersonRowId.From(50L);
        var originalPerson = Person.Create(
            originalPersonRowId,
            LastName.From("山田"),
            FirstName.From("太郎"),
            LastNameKana.From("ヤマダ"),
            FirstNameKana.From("タロウ"));
        var originalEmployee = Employee.Create(
            originalRowId,
            originalTypeDivision,
            originalBizId,
            originalBizCode,
            null,
            originalPerson,
            new List<DepartmentMembership>(),
            _clock);

        var mapper = CreateMapper();

        // Act
        var dbModel = mapper.ToDbModel(originalEmployee);
        var personDbModel = mapper.ToPersonDbModel(originalPerson, originalRowId.Value);
        var reconstructedEmployee = mapper.ToDomainEntity(dbModel, personDbModel, _clock, null);

        // Assert
        Assert.Equal(originalRowId.Value, reconstructedEmployee.RowId.Value);
        Assert.Equal(originalTypeDivision.Value, reconstructedEmployee.TypeDivision.Value);
        Assert.Equal(originalBizId.Value, reconstructedEmployee.BizId.Value);
        Assert.Equal(originalPersonRowId.Value, reconstructedEmployee.Person.RowId.Value);
    }

    #endregion

    #region グループ 4: エラーハンドリング

    [Fact]
    public void TestToDomainEntity04_WithInvalidDivisionThrowsException()
    {
        // Arrange
        var personDbModel = new PersonDbModel
        {
            RowId = 1L,
            EmployeeRowId = 1L,
            LastName = "太郎",
            FirstName = "山田",
            LastNameKana = "タロウ",
            FirstNameKana = "ヤマダ",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = 1L
        };
        var dbModel = new EmployeeDbModel
        {
            RowId = 1L,
            BizDivision = "X",  // 無効な値
            BizId = 1234,
            RetiredOn = null,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = 1L
        };

        var mapper = CreateMapper();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => mapper.ToDomainEntity(dbModel, personDbModel, _clock, null));
    }

    #endregion

    #region グループ 5: 所属の部署名（表示用）

    [Fact]
    public void TestToDomainEntity05_WithMembershipDepartmentName_ConvertsToSetName()
    {
        var employee = MapWithMembershipName("営業部");

        var membership = Assert.Single(employee.DepartmentMemberships);
        Assert.True(membership.DepartmentDisplayName.HasName);
        Assert.Equal("営業部", membership.DepartmentDisplayName.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void TestToDomainEntity06_WithNullOrEmptyDepartmentName_ConvertsToUnset(string? departmentName)
    {
        // LEFT JOIN で部署が見つからない場合（NULL）や空文字は「名前なし」として扱う
        var employee = MapWithMembershipName(departmentName);

        var membership = Assert.Single(employee.DepartmentMemberships);
        Assert.NotNull(membership.DepartmentDisplayName);
        Assert.False(membership.DepartmentDisplayName.HasName);
    }

    [Fact]
    public void TestToDepartmentMembershipDbModel01_WithUnsetName_ConvertsToNull()
    {
        var mapper = CreateMapper();
        var membership = DepartmentMembership.Create(
            DepartmentMembershipRowId.From(1L),
            EmployeeRowId.From(100L),
            DepartmentRowId.From(10L),
            IsPrimary.Primary());

        var result = mapper.ToDepartmentMembershipDbModel(membership);

        Assert.Null(result.DepartmentName);
    }

    [Fact]
    public void TestToDepartmentMembershipDbModel02_WithName_ConvertsToValue()
    {
        var mapper = CreateMapper();
        var membership = DepartmentMembership.Create(
            DepartmentMembershipRowId.From(1L),
            EmployeeRowId.From(100L),
            DepartmentRowId.From(10L),
            IsPrimary.Primary(),
            departmentDisplayName: DepartmentDisplayName.From("企画部"));

        var result = mapper.ToDepartmentMembershipDbModel(membership);

        Assert.Equal("企画部", result.DepartmentName);
    }

    #endregion

    #region ヘルパーメソッド

    private Employee MapWithMembershipName(string? departmentName)
    {
        var personDbModel = new PersonDbModel
        {
            RowId = 50L,
            EmployeeRowId = 100L,
            LastName = "山田",
            FirstName = "太郎",
            LastNameKana = "ヤマダ",
            FirstNameKana = "タロウ",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = 1L
        };
        var dbModel = new EmployeeDbModel
        {
            RowId = 100L,
            BizDivision = "M",
            BizId = 1234,
            RetiredOn = null,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = 1L
        };
        var memberships = new List<DepartmentMembershipDbModel>
        {
            new()
            {
                RowId = 1L,
                EmployeeRowId = 100L,
                DepartmentRowId = 10L,
                IsPrimary = true,
                EndOn = null,
                DepartmentName = departmentName
            }
        };

        return CreateMapper().ToDomainEntity(dbModel, personDbModel, _clock, memberships);
    }

    private EmployeeMapper CreateMapper()
    {
        return new EmployeeMapper();
    }

    #endregion
}
