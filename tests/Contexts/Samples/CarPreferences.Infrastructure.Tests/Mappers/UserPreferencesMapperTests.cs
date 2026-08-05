using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.Entities;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;
using SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure.DataAccess.Models;
using SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure.Mappers;
using SupportAdvance.SharedKernel.ValueObjects.Audit;
using SupportAdvance.SharedKernel.ValueObjects.Identifiers;

namespace SupportAdvance.Tests.Contexts.Samples.CarPreferences.Infrastructure.Tests.Mappers;

/// <summary>
/// UserPreferencesMapper のテスト
/// </summary>
public sealed class UserPreferencesMapperTests
{
    /// <summary>
    /// MAP-TO-DB-01: Entity → DbModel で ValueObject から LocalDateTime に変換
    /// </summary>
    [Fact]
    public void MAP_TO_DB_01_ToDbModel_ExtracsValuesFromValueObjects()
    {
        // Arrange
        var mapper = new UserPreferencesMapper();
        using var clock = new MockClock(new DateTime(2026, 8, 5, 12, 0, 0));
        var userId = RespondentPersonId.From(1000);
        var respondedAt = RespondentAt.From(new LocalDateTime(new DateTime(2026, 8, 5, 10, 0, 0)), clock);
        var entity = new UserPreferences(userId, respondedAt, clock);

        // Act
        var dbModel = mapper.ToDbModel(entity);

        // Assert
        Assert.Equal(entity.CreatedAt.Value, dbModel.CreatedAt.Value);
        Assert.Equal(entity.UpdatedAt.Value!.Value, dbModel.UpdatedAt!.Value.Value);
        Assert.Null(dbModel.DeletedAt);  // 未削除なので null
    }

    /// <summary>
    /// MAP-TO-DB-02: 削除状態を DbModel に反映
    /// </summary>
    [Fact]
    public void MAP_TO_DB_02_ToDbModel_ReflectsDeletedState()
    {
        // Arrange
        var mapper = new UserPreferencesMapper();
        using var clock = new MockClock(new DateTime(2026, 8, 5, 12, 0, 0));
        var userId = RespondentPersonId.From(1000);
        var respondedAt = RespondentAt.From(new LocalDateTime(new DateTime(2026, 8, 5, 10, 0, 0)), clock);
        var entity = new UserPreferences(userId, respondedAt, clock);
        entity.SoftDelete(clock);

        // Act
        var dbModel = mapper.ToDbModel(entity);

        // Assert
        Assert.NotNull(dbModel.DeletedAt);
        Assert.Equal(clock.JstNow, dbModel.DeletedAt!);
    }

    /// <summary>
    /// MAP-FROM-DB-01: DbModel → Entity で TryFrom を使用した型安全な変換
    /// </summary>
    [Fact]
    public void MAP_FROM_DB_01_ToDomainEntity_UsesTryFromForTypeConversion()
    {
        // Arrange
        var mapper = new UserPreferencesMapper();
        using var clock = new MockClock(new DateTime(2026, 8, 5, 12, 0, 0));
        var createdAtValue = new LocalDateTime(new DateTime(2026, 8, 1, 10, 0, 0));
        var updatedAtValue = new LocalDateTime(new DateTime(2026, 8, 5, 12, 0, 0));

        var dbModel = new UserPreferencesDbModel
        {
            RowId = 123,
            UserId = 1000,
            CreatedAt = createdAtValue,
            CreatedBy = 1,
            UpdatedAt = updatedAtValue,
            UpdatedBy = 1,
            DeletedAt = null,
            DeletedBy = null,
            PrefersAutomatic = true
        };

        // Act
        var entity = mapper.ToDomainEntity(dbModel, clock);

        // Assert
        Assert.NotNull(entity);
        Assert.Equal(createdAtValue.Value, entity.CreatedAt.Value);
        Assert.Equal(updatedAtValue.Value, entity.UpdatedAt.Value!.Value);
        Assert.False(entity.DeletedAt.IsDeleted);
    }

    /// <summary>
    /// MAP-FROM-DB-02: UpdatedAt が null の場合は Unset() で変換
    /// </summary>
    [Fact]
    public void MAP_FROM_DB_02_ToDomainEntity_UpdatedAtNullBecomesUnset()
    {
        // Arrange
        var mapper = new UserPreferencesMapper();
        using var clock = new MockClock(new DateTime(2026, 8, 5, 12, 0, 0));
        var createdAtValue = new LocalDateTime(new DateTime(2026, 8, 1, 10, 0, 0));

        var dbModel = new UserPreferencesDbModel
        {
            RowId = 123,
            UserId = 1000,
            CreatedAt = createdAtValue,
            CreatedBy = 1,
            UpdatedAt = null,  // ← null（未更新）
            UpdatedBy = null,
            DeletedAt = null,
            DeletedBy = null,
            PrefersAutomatic = true
        };

        // Act
        var entity = mapper.ToDomainEntity(dbModel, clock);

        // Assert
        Assert.False(entity.UpdatedAt.HasUpdated);  // Unset 状態
    }

    /// <summary>
    /// MAP-FROM-DB-03: DeletedAt が null の場合は Unset() で変換
    /// </summary>
    [Fact]
    public void MAP_FROM_DB_03_ToDomainEntity_DeletedAtNullBecomesUnset()
    {
        // Arrange
        var mapper = new UserPreferencesMapper();
        using var clock = new MockClock(new DateTime(2026, 8, 5, 12, 0, 0));
        var createdAtValue = new LocalDateTime(new DateTime(2026, 8, 1, 10, 0, 0));

        var dbModel = new UserPreferencesDbModel
        {
            RowId = 123,
            UserId = 1000,
            CreatedAt = createdAtValue,
            CreatedBy = 1,
            UpdatedAt = createdAtValue,
            UpdatedBy = null,
            DeletedAt = null,  // ← null（未削除）
            DeletedBy = null,
            PrefersAutomatic = true
        };

        // Act
        var entity = mapper.ToDomainEntity(dbModel, clock);

        // Assert
        Assert.False(entity.DeletedAt.IsDeleted);  // Unset 状態
    }

    /// <summary>
    /// MAP-FROM-DB-04: ビジネス ValueObject も復元
    /// </summary>
    [Fact]
    public void MAP_FROM_DB_04_ToDomainEntity_RestoresBusinessValueObjects()
    {
        // Arrange
        var mapper = new UserPreferencesMapper();
        using var clock = new MockClock(new DateTime(2026, 8, 5, 12, 0, 0));
        var createdAtValue = new LocalDateTime(new DateTime(2026, 8, 1, 10, 0, 0));

        var dbModel = new UserPreferencesDbModel
        {
            RowId = 123,
            UserId = 1000,
            CreatedAt = createdAtValue,
            CreatedBy = 1,
            UpdatedAt = createdAtValue,
            UpdatedBy = null,
            DeletedAt = null,
            DeletedBy = null,
            PrefersAutomatic = false,
            PreferredModel = 5,
            PreferredBodyType = "SUV",
            BudgetFrom = 200m,
            BudgetTo = 400m
        };

        // Act
        var entity = mapper.ToDomainEntity(dbModel, clock);

        // Assert
        Assert.False(entity.PrefersAutomatic);
        Assert.NotNull(entity.PreferredModel);
        Assert.Equal(5, entity.PreferredModel!.Value);
        Assert.NotNull(entity.PreferredBodyType);
        Assert.Equal("SUV", entity.PreferredBodyType!.Value);
        Assert.NotNull(entity.BudgetFrom);
        Assert.Equal(200m, entity.BudgetFrom!.Amount);
        Assert.NotNull(entity.BudgetTo);
        Assert.Equal(400m, entity.BudgetTo!.Amount);
    }

    /// <summary>
    /// MAP-ROUNDTRIP-01: Entity → DbModel → Entity のラウンドトリップ
    /// </summary>
    [Fact]
    public void MAP_ROUNDTRIP_01_ToDbModelAndBack_PreservesData()
    {
        // Arrange
        var mapper = new UserPreferencesMapper();
        using var clock = new MockClock(new DateTime(2026, 8, 5, 12, 0, 0));
        var userId = RespondentPersonId.From(1000);
        var respondedAt = RespondentAt.From(new LocalDateTime(new DateTime(2026, 8, 5, 10, 0, 0)), clock);
        var originalEntity = new UserPreferences(userId, respondedAt, clock);
        originalEntity.UpdatePreferredModel(CarModel.From(5), clock);
        originalEntity.UpdateBudget(Money.From(200m), Money.From(400m), clock);

        // Act
        var dbModel = mapper.ToDbModel(originalEntity);
        var restoredEntity = mapper.ToDomainEntity(dbModel, clock);

        // Assert
        Assert.Equal(originalEntity.CreatedAt.Value, restoredEntity.CreatedAt.Value);
        Assert.Equal(originalEntity.UpdatedAt.Value!.Value, restoredEntity.UpdatedAt.Value!.Value);
        Assert.Equal(originalEntity.PreferredModel?.Value, restoredEntity.PreferredModel?.Value);
        Assert.Equal(originalEntity.BudgetFrom?.Amount, restoredEntity.BudgetFrom?.Amount);
        Assert.Equal(originalEntity.BudgetTo?.Amount, restoredEntity.BudgetTo?.Amount);
    }
}
