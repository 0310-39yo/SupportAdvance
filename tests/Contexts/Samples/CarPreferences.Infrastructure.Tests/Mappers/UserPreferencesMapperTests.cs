using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.Entities;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;
using SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure.DataAccess.Models;
using SupportAdvance.Contexts.Samples.CarPreferences.Infrastructure.Mappers;
using SupportAdvance.SharedKernel.ValueObjects.Audit;
using SupportAdvance.SharedKernel.ValueObjects.Audit;

namespace SupportAdvance.Tests.Contexts.Samples.CarPreferences.Infrastructure.Tests.Mappers;

/// <summary>
/// UserPreferencesMapper のテスト
/// </summary>
public sealed class UserPreferencesMapperTests
{
    /// <summary>
    /// MAP-TO-DB-01: Entity → DbModel で LocalDateTime → DateTime 変換、型チェック
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
        // 【型チェック】DbModel フィールドが DateTime であることを確認
        Assert.IsType<DateTime>(dbModel.CreatedAt);

        // 【値チェック】Entity の LocalDateTime が DbModel の DateTime に正しく変換されたことを確認
        Assert.Equal(entity.CreatedAt.Value.Value, dbModel.CreatedAt);

        // 【NULL チェック】UpdatedAt は DateTime? でサポート
        Assert.NotNull(dbModel.UpdatedAt);
        Assert.IsType<DateTime>(dbModel.UpdatedAt.Value);
        Assert.Equal(entity.UpdatedAt.Value!.Value.Value, dbModel.UpdatedAt.Value);

        // 【論理削除】未削除なので null
        Assert.Null(dbModel.DeletedAt);
    }

    /// <summary>
    /// MAP-TO-DB-02: 削除状態を DbModel に反映、LocalDateTime → DateTime 変換
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
        // 【型チェック】DeletedAt は DateTime? で存在
        Assert.NotNull(dbModel.DeletedAt);
        Assert.IsType<DateTime>(dbModel.DeletedAt.Value);

        // 【値チェック】削除時刻が正しく変換されたことを確認
        var expectedDeletedAt = clock.JstNow.Value;  // LocalDateTime → DateTime
        Assert.Equal(expectedDeletedAt, dbModel.DeletedAt.Value);
    }

    /// <summary>
    /// MAP-FROM-DB-01: DbModel → Entity で TryFrom を使用した型安全な変換、DateTime → LocalDateTime
    /// </summary>
    [Fact]
    public void MAP_FROM_DB_01_ToDomainEntity_UsesTryFromForTypeConversion()
    {
        // Arrange
        var mapper = new UserPreferencesMapper();
        using var clock = new MockClock(new DateTime(2026, 8, 5, 12, 0, 0));
        var createdAtDateTime = new DateTime(2026, 8, 1, 10, 0, 0);
        var updatedAtDateTime = new DateTime(2026, 8, 5, 12, 0, 0);

        var dbModel = new UserPreferencesDbModel
        {
            UserPreferencesId = new Guid("550e8400-e29b-41d4-a716-446655440000"),
            RowId = 123,
            UserId = 1000,
            CreatedAt = createdAtDateTime,  // ← DbModel は DateTime
            CreatedBy = 1,
            UpdatedAt = updatedAtDateTime,  // ← DbModel は DateTime?
            UpdatedBy = 1,
            DeletedAt = null,
            DeletedBy = null,
            PrefersAutomatic = true
        };

        // Act
        var entity = mapper.ToDomainEntity(dbModel, clock);

        // Assert
        Assert.NotNull(entity);

        // 【型チェック】Entity の CreatedAt.Value は LocalDateTime
        Assert.IsType<LocalDateTime>(entity.CreatedAt.Value);

        // 【値チェック】DbModel の DateTime が Entity の LocalDateTime に正しく変換されたことを確認
        Assert.Equal(createdAtDateTime, entity.CreatedAt.Value.Value);

        // 【NULL チェック】UpdatedAt も LocalDateTime? でサポート
        Assert.NotNull(entity.UpdatedAt.Value);
        Assert.IsType<LocalDateTime>(entity.UpdatedAt.Value.Value);
        Assert.Equal(updatedAtDateTime, entity.UpdatedAt.Value.Value.Value);

        // 【論理削除】未削除なので IsDeleted = false
        Assert.False(entity.DeletedAt.IsDeleted);
    }

    /// <summary>
    /// MAP-FROM-DB-02: UpdatedAt が null の場合は Unset() で変換（NULL 安全性）
    /// </summary>
    [Fact]
    public void MAP_FROM_DB_02_ToDomainEntity_UpdatedAtNullBecomesUnset()
    {
        // Arrange
        var mapper = new UserPreferencesMapper();
        using var clock = new MockClock(new DateTime(2026, 8, 5, 12, 0, 0));
        var createdAtDateTime = new DateTime(2026, 8, 1, 10, 0, 0);

        var dbModel = new UserPreferencesDbModel
        {
            RowId = 123,
            UserPreferencesId = new Guid("550e8400-e29b-41d4-a716-446655440001"),
            UserId = 1000,
            CreatedAt = createdAtDateTime,  // ← DbModel は DateTime
            CreatedBy = 1,
            UpdatedAt = null,  // ← null（未更新）を Unset に変換
            UpdatedBy = null,
            DeletedAt = null,
            DeletedBy = null,
            PrefersAutomatic = true
        };

        // Act
        var entity = mapper.ToDomainEntity(dbModel, clock);

        // Assert
        // 【NULL 安全性】UpdatedAt が null の場合、Entity の UpdatedAt.HasUpdated は false（Unset状態）
        Assert.False(entity.UpdatedAt.HasUpdated);
        Assert.Null(entity.UpdatedAt.Value);  // 値も null（Unset状態）
    }

    /// <summary>
    /// MAP-FROM-DB-03: DeletedAt が null の場合は Unset() で変換（論理削除 NULL 安全性）
    /// </summary>
    [Fact]
    public void MAP_FROM_DB_03_ToDomainEntity_DeletedAtNullBecomesUnset()
    {
        // Arrange
        var mapper = new UserPreferencesMapper();
        using var clock = new MockClock(new DateTime(2026, 8, 5, 12, 0, 0));
        var createdAtDateTime = new DateTime(2026, 8, 1, 10, 0, 0);
        var updatedAtDateTime = new DateTime(2026, 8, 2, 15, 30, 0);

        var dbModel = new UserPreferencesDbModel
        {
            RowId = 123,
            UserPreferencesId = new Guid("550e8400-e29b-41d4-a716-446655440002"),
            UserId = 1000,
            CreatedAt = createdAtDateTime,  // ← DbModel は DateTime
            CreatedBy = 1,
            UpdatedAt = updatedAtDateTime,  // ← DbModel は DateTime?
            UpdatedBy = null,
            DeletedAt = null,  // ← null（未削除）を Unset に変換
            DeletedBy = null,
            PrefersAutomatic = true
        };

        // Act
        var entity = mapper.ToDomainEntity(dbModel, clock);

        // Assert
        // 【論理削除 NULL 安全性】DeletedAt が null の場合、Entity の DeletedAt.IsDeleted は false（Unset状態）
        Assert.False(entity.DeletedAt.IsDeleted);
        Assert.Null(entity.DeletedAt.Value);  // 値も null（Unset状態）
    }

    /// <summary>
    /// MAP-FROM-DB-04: ビジネス ValueObject も復元、DateTime → LocalDateTime の並行変換確認
    /// </summary>
    [Fact]
    public void MAP_FROM_DB_04_ToDomainEntity_RestoresBusinessValueObjects()
    {
        // Arrange
        var mapper = new UserPreferencesMapper();
        using var clock = new MockClock(new DateTime(2026, 8, 5, 12, 0, 0));
        var createdAtDateTime = new DateTime(2026, 8, 1, 10, 0, 0);
        var updatedAtDateTime = new DateTime(2026, 8, 3, 14, 20, 0);

        var dbModel = new UserPreferencesDbModel
        {
            RowId = 123,
            UserPreferencesId = new Guid("550e8400-e29b-41d4-a716-446655440003"),
            UserId = 1000,
            CreatedAt = createdAtDateTime,  // ← DbModel は DateTime
            CreatedBy = 1,
            UpdatedAt = updatedAtDateTime,  // ← DbModel は DateTime?
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
        // 【審査値】ビジネス ValueObject が正しく復元されたことを確認
        Assert.False(entity.PrefersAutomatic);
        Assert.NotNull(entity.PreferredModel);
        Assert.Equal(5, entity.PreferredModel!.Value);
        Assert.NotNull(entity.PreferredBodyType);
        Assert.Equal("SUV", entity.PreferredBodyType!.Value);
        Assert.NotNull(entity.BudgetFrom);
        Assert.Equal(200m, entity.BudgetFrom!.Amount);
        Assert.NotNull(entity.BudgetTo);
        Assert.Equal(400m, entity.BudgetTo!.Amount);

        // 【並行変換】同時に DateTime → LocalDateTime 変換も正しく行われたことを確認
        Assert.IsType<LocalDateTime>(entity.CreatedAt.Value);
        Assert.Equal(createdAtDateTime, entity.CreatedAt.Value.Value);
    }

    /// <summary>
    /// MAP-ROUNDTRIP-01: Entity → DbModel → Entity のラウンドトリップ、DateTime 往復変換検証
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
        // 【ラウンドトリップ】Entity → DbModel → Entity で型と値が保存されることを確認

        // 【監査フィールド往復変換】LocalDateTime → DateTime → LocalDateTime が正しく往復変換
        Assert.Equal(originalEntity.CreatedAt.Value.Value, restoredEntity.CreatedAt.Value.Value);
        Assert.Equal(
            originalEntity.UpdatedAt.Value!.Value.Value,
            restoredEntity.UpdatedAt.Value!.Value.Value);

        // 【ビジネスフィールド】往復変換後も値が保存される
        Assert.Equal(originalEntity.PreferredModel?.Value, restoredEntity.PreferredModel?.Value);
        Assert.Equal(originalEntity.BudgetFrom?.Amount, restoredEntity.BudgetFrom?.Amount);
        Assert.Equal(originalEntity.BudgetTo?.Amount, restoredEntity.BudgetTo?.Amount);

        // 【型チェック】最終的な型が LocalDateTime であることを確認（往復変換で型が正しく復元）
        Assert.IsType<LocalDateTime>(restoredEntity.CreatedAt.Value);
        Assert.IsType<LocalDateTime>(restoredEntity.UpdatedAt.Value!.Value);
    }

    /// <summary>
    /// MAP-ROUNDTRIP-02: 削除状態のラウンドトリップ、DateTime 往復変換検証
    /// </summary>
    [Fact]
    public void MAP_ROUNDTRIP_02_ToDbModelAndBack_PreservesDeletedState()
    {
        // Arrange
        var mapper = new UserPreferencesMapper();
        using var clock = new MockClock(new DateTime(2026, 8, 5, 12, 0, 0));
        var userId = RespondentPersonId.From(1000);
        var respondedAt = RespondentAt.From(new LocalDateTime(new DateTime(2026, 8, 5, 10, 0, 0)), clock);
        var originalEntity = new UserPreferences(userId, respondedAt, clock);
        originalEntity.SoftDelete(clock);  // 削除状態に変更

        // Act
        var dbModel = mapper.ToDbModel(originalEntity);
        var restoredEntity = mapper.ToDomainEntity(dbModel, clock);

        // Assert
        // 【ラウンドトリップ】削除状態も往復変換で保存される
        Assert.True(originalEntity.DeletedAt.IsDeleted);
        Assert.True(restoredEntity.DeletedAt.IsDeleted);

        // 【DateTime 往復変換】DeletedAt も LocalDateTime → DateTime → LocalDateTime で往復変換
        if (originalEntity.DeletedAt.Value.HasValue)
        {
            Assert.NotNull(restoredEntity.DeletedAt.Value);
            Assert.Equal(
                originalEntity.DeletedAt.Value.Value.Value,
                restoredEntity.DeletedAt.Value.Value.Value);
            Assert.IsType<LocalDateTime>(restoredEntity.DeletedAt.Value.Value);
        }
    }
}
