-- Migration: 003 - Add indexes to t_UserPreferences
-- Purpose: Performance optimization
-- Date: 2026-08-01
-- Note: Indexes can be dropped/recreated without affecting data

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tUserPreferences_UserId' AND object_id = OBJECT_ID('dbo.t_UserPreferences'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_tUserPreferences_UserId]
        ON [dbo].[t_UserPreferences]([user_id])
        WHERE [deleted_at] IS NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tUserPreferences_UpdatedAt' AND object_id = OBJECT_ID('dbo.t_UserPreferences'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tUserPreferences_UpdatedAt]
        ON [dbo].[t_UserPreferences]([updated_at])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tUserPreferences_DeletedAt' AND object_id = OBJECT_ID('dbo.t_UserPreferences'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tUserPreferences_DeletedAt]
        ON [dbo].[t_UserPreferences]([deleted_at])
        WHERE [deleted_at] IS NOT NULL
END
GO
