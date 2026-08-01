-- Migration: 000 - Create MigrationHistory table (Must run first)
-- Purpose: Track executed migrations to prevent re-execution
-- Date: 2026-08-01

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '__MigrationHistory')
BEGIN
    CREATE TABLE __MigrationHistory
    (
        MigrationName NVARCHAR(255) NOT NULL PRIMARY KEY,
        ExecutedAt NVARCHAR(30) NOT NULL,
        Success BIT NOT NULL DEFAULT 1
    )

    CREATE INDEX IX_MigrationHistory_ExecutedAt ON __MigrationHistory(ExecutedAt)
END
GO
