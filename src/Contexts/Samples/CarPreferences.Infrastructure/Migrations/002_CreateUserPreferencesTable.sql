-- Migration: 002 - Create t_UserPreferences table
-- Purpose: User car preferences transaction table
-- Date: 2026-08-01
-- Naming: t_* = Transaction table (業務テーブル)

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_UserPreferences')
BEGIN
    CREATE TABLE [dbo].[t_UserPreferences]
    (
        -- 監査カラム（全テーブル必須）
        [row_id] [bigint] NOT NULL PRIMARY KEY DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence]),
        [row_version] [timestamp] NOT NULL,
        [created_at] [datetime2](7) NOT NULL,
        [created_by] [bigint] NOT NULL,
        [updated_at] [datetime2](7) NULL,
        [updated_by] [bigint] NULL,
        [deleted_at] [datetime2](7) NULL,
        [deleted_by] [bigint] NULL,

        -- ビジネスカラム
        [user_id] [int] NOT NULL UNIQUE,
        [preferred_model] [int] NULL,
        [preferred_body_type] [nvarchar](50) NULL,
        [prefers_automatic] [bit] NOT NULL DEFAULT 1,
        [budget_from] [decimal](10, 2) NULL,
        [budget_to] [decimal](10, 2) NULL,

        -- 制約
        CONSTRAINT [CK_tUserPreferences_Budget] CHECK ([budget_from] IS NULL OR [budget_to] IS NULL OR [budget_from] <= [budget_to]),
        CONSTRAINT [CK_tUserPreferences_PreferredModel] CHECK ([preferred_model] IS NULL OR ([preferred_model] >= 0 AND [preferred_model] < 1000))
    )
    ON [PRIMARY]
END
GO
