using SupportAdvance.Common.Clocks;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.Entities;
using SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects;
using SupportAdvance.SharedKernel.ValueObjects.Audit;
using SupportAdvance.SharedKernel.ValueObjects.Audit;

namespace SupportAdvance.Tests.Contexts.Samples.CarPreferences.Domain.Tests.Entities;

/// <summary>
/// UserPreferences Entity のテスト
/// </summary>
public sealed class UserPreferencesTests
{
    /// <summary>
    /// ENT-NEW-01: 新規作成時に CreatedAt/UpdatedAt が ValueObject で設定される
    /// </summary>
    [Fact]
    public void ENT_NEW_01_Constructor_InitializesAuditFieldsAsValueObjects()
    {
        // Arrange
        using var clock = new MockClock(new DateTime(2026, 8, 5, 12, 0, 0));
        var userId = RespondentPersonId.From(1000);
        var respondedAt = RespondentAt.From(new LocalDateTime(new DateTime(2026, 8, 5, 10, 0, 0)), clock);

        // Act
        var entity = new UserPreferences(userId, respondedAt, clock);

        // Assert
        Assert.NotNull(entity.CreatedAt);
        Assert.IsType<LocalDateTime>(entity.CreatedAt.Value);
        Assert.Equal(clock.JstNow, entity.CreatedAt.Value);

        Assert.NotNull(entity.UpdatedAt);
        Assert.True(entity.UpdatedAt.HasUpdated);
        Assert.IsType<LocalDateTime>(entity.UpdatedAt.Value);
        Assert.Equal(clock.JstNow, entity.UpdatedAt.Value!.Value);

        Assert.NotNull(entity.DeletedAt);
        Assert.False(entity.DeletedAt.IsDeleted);
    }

    /// <summary>
    /// ENT-UPD-01: 更新時に UpdatedAt が更新される
    /// </summary>
    [Fact]
    public void ENT_UPD_01_UpdatePreferredModel_UpdatesUpdatedAt()
    {
        // Arrange
        using var clock1 = new MockClock(new DateTime(2026, 8, 5, 12, 0, 0));
        var userId = RespondentPersonId.From(1000);
        var respondedAt = RespondentAt.From(new LocalDateTime(new DateTime(2026, 8, 5, 10, 0, 0)), clock1);
        var entity = new UserPreferences(userId, respondedAt, clock1);
        var initialUpdatedAt = entity.UpdatedAt.Value!.Value;

        // Act: 1時間後に更新
        using var clock2 = new MockClock(new DateTime(2026, 8, 5, 13, 0, 0));
        var newModel = CarModel.From(1);
        entity.UpdatePreferredModel(newModel, clock2);

        // Assert
        Assert.True(entity.UpdatedAt.HasUpdated);
        Assert.IsType<LocalDateTime>(entity.UpdatedAt.Value);
        Assert.Equal(clock2.JstNow, entity.UpdatedAt.Value!.Value);
        Assert.NotEqual(initialUpdatedAt, entity.UpdatedAt.Value!.Value);
    }

    /// <summary>
    /// ENT-DEL-01: SoftDelete 実行時に DeletedAt が設定される
    /// </summary>
    [Fact]
    public void ENT_DEL_01_SoftDelete_SetsDeletedAt()
    {
        // Arrange
        using var clock1 = new MockClock(new DateTime(2026, 8, 5, 12, 0, 0));
        var userId = RespondentPersonId.From(1000);
        var respondedAt = RespondentAt.From(new LocalDateTime(new DateTime(2026, 8, 5, 10, 0, 0)), clock1);
        var entity = new UserPreferences(userId, respondedAt, clock1);

        // Act: 1時間後に削除
        using var clock2 = new MockClock(new DateTime(2026, 8, 5, 13, 0, 0));
        var result = entity.SoftDelete(clock2);

        // Assert
        Assert.True(result);
        Assert.True(entity.IsDeleted);
        Assert.True(entity.DeletedAt.IsDeleted);
        Assert.IsType<LocalDateTime>(entity.DeletedAt.Value);
        Assert.Equal(clock2.JstNow, entity.DeletedAt.Value!.Value);
    }

    /// <summary>
    /// ENT-DEL-02: 既に削除済みなら SoftDelete は失敗
    /// </summary>
    [Fact]
    public void ENT_DEL_02_SoftDelete_AlreadyDeleted_ReturnsFalse()
    {
        // Arrange
        using var clock1 = new MockClock(new DateTime(2026, 8, 5, 12, 0, 0));
        var userId = RespondentPersonId.From(1000);
        var respondedAt = RespondentAt.From(new LocalDateTime(new DateTime(2026, 8, 5, 10, 0, 0)), clock1);
        var entity = new UserPreferences(userId, respondedAt, clock1);
        entity.SoftDelete(clock1);

        // Act: 再度削除を試みる
        var result = entity.SoftDelete(clock1);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// ENT-REC-01: Reconstruct で DB値から Entity を復元
    /// </summary>
    [Fact]
    public void ENT_REC_01_Reconstruct_RestoresFromDbValues()
    {
        // Arrange
        var createdAtValue = new LocalDateTime(new DateTime(2026, 8, 1, 10, 0, 0));
        var updatedAtValue = new LocalDateTime(new DateTime(2026, 8, 5, 12, 0, 0));
        var deletedAtValue = new LocalDateTime(new DateTime(2026, 8, 6, 15, 0, 0));

        using var clock = new MockClock(new DateTime(2026, 8, 5, 12, 0, 0));
        var userId = RespondentPersonId.From(1000);
        var respondedAt = RespondentAt.From(new LocalDateTime(new DateTime(2026, 8, 1, 10, 0, 0)), clock);
        var createdAt = CreatedAt.From(createdAtValue);
        var updatedAt = UpdatedAt.From(updatedAtValue);
        var deletedAt = DeletedAt.From(deletedAtValue);
        var rowId = RowId.From(123);
        var userPreferencesId = UserPreferencesId.From(new Guid("550e8400-e29b-41d4-a716-446655440000"));

        // Act
        var entity = UserPreferences.Reconstruct(
            userPreferencesId, userId, respondedAt, createdAt, updatedAt, deletedAt, rowId,
            null, null, true, null, null);

        // Assert
        Assert.IsType<LocalDateTime>(entity.CreatedAt.Value);
        Assert.Equal(createdAt.Value, entity.CreatedAt.Value);
        Assert.IsType<LocalDateTime>(entity.UpdatedAt.Value);
        Assert.Equal(updatedAt.Value!.Value, entity.UpdatedAt.Value!.Value);
        Assert.IsType<LocalDateTime>(entity.DeletedAt.Value);
        Assert.Equal(deletedAt.Value!.Value, entity.DeletedAt.Value!.Value);
        Assert.True(entity.IsDeleted);
    }

    /// <summary>
    /// ENT-REC-02: Reconstruct で未更新・未削除状態を復元
    /// </summary>
    [Fact]
    public void ENT_REC_02_Reconstruct_UnsetStates()
    {
        // Arrange
        var createdAtValue = new LocalDateTime(new DateTime(2026, 8, 1, 10, 0, 0));
        using var clock = new MockClock(new DateTime(2026, 8, 5, 12, 0, 0));
        var userId = RespondentPersonId.From(1000);
        var respondedAt = RespondentAt.From(new LocalDateTime(new DateTime(2026, 8, 1, 10, 0, 0)), clock);
        var createdAt = CreatedAt.From(createdAtValue);
        var updatedAt = UpdatedAt.Unset();  // 未更新
        var deletedAt = DeletedAt.Unset();  // 未削除
        var rowId = RowId.From(123);
        var userPreferencesId = UserPreferencesId.From(new Guid("660e8400-e29b-41d4-a716-446655440001"));

        // Act
        var entity = UserPreferences.Reconstruct(
            userPreferencesId, userId, respondedAt, createdAt, updatedAt, deletedAt, rowId,
            null, null, true, null, null);

        // Assert
        Assert.False(entity.UpdatedAt.HasUpdated);
        Assert.Null(entity.UpdatedAt.Value);
        Assert.False(entity.DeletedAt.IsDeleted);
        Assert.Null(entity.DeletedAt.Value);
        Assert.False(entity.IsDeleted);
    }
}
