-- Advance Database - Create Tables (SQL Server 2022)
-- Version 1.0
-- Date: 2026-07-19

-- =====================================================
-- 1. Database Creation
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'Advance')
BEGIN
    CREATE DATABASE [Advance];
END
GO

USE [Advance];
GO

-- =====================================================
-- 2. Sequence Definition (for row_id)
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.sequences WHERE name = 's_row_id_sequence')
BEGIN
    CREATE SEQUENCE [dbo].[s_row_id_sequence]
        AS BIGINT
        START WITH 2147483648
        INCREMENT BY 1
        NO CYCLE;
END
GO

-- =====================================================
-- 3. テーブル定義
-- =====================================================

-- =====================================================
-- m_persons テーブル（人名マスタ）
-- =====================================================
IF OBJECT_ID('[dbo].[m_persons]', 'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[m_persons];
END
GO

CREATE TABLE [dbo].[m_persons]
(
    -- 主キー・シーケンス
    [row_id] BIGINT NOT NULL
        DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence])
        CONSTRAINT [PK_m_persons_row_id] PRIMARY KEY CLUSTERED,

    -- 楽観的ロック
    [row_version] TIMESTAMP NOT NULL,

    -- 監査情報：作成
    [inserted_at] DATETIME2(7) NOT NULL,
    [inserted_by] BIGINT NOT NULL,

    -- 監査情報：更新
    [updated_at] DATETIME2(7) NULL,
    [updated_by] BIGINT NULL,

    -- 監査情報：削除
    [deleted_at] DATETIME2(7) NULL,
    [deleted_by] BIGINT NULL,

    -- ビジネスデータ
    [last_name] NVARCHAR(50) NOT NULL,
    [first_name] NVARCHAR(50) NOT NULL,
    [last_name_kana] NVARCHAR(50) NOT NULL,
    [first_name_kana] NVARCHAR(50) NOT NULL
);

-- インデックス作成（m_persons）
CREATE NONCLUSTERED INDEX [IX_m_persons_deleted_at]
ON [dbo].[m_persons]([deleted_at]);

CREATE NONCLUSTERED INDEX [IX_m_persons_inserted_at_deleted_at]
ON [dbo].[m_persons]([inserted_at], [deleted_at])
INCLUDE ([last_name], [first_name]);

CREATE NONCLUSTERED INDEX [IX_m_persons_inserted_by]
ON [dbo].[m_persons]([inserted_by]);

CREATE NONCLUSTERED INDEX [IX_m_persons_updated_by]
ON [dbo].[m_persons]([updated_by])
WHERE [updated_by] IS NOT NULL;

GO

-- =====================================================
-- m_employees テーブル（従業員マスタ）
-- =====================================================
IF OBJECT_ID('[dbo].[m_employees]', 'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[m_employees];
END
GO

CREATE TABLE [dbo].[m_employees]
(
    -- 主キー・シーケンス
    [row_id] BIGINT NOT NULL
        DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence])
        CONSTRAINT [PK_m_employees_row_id] PRIMARY KEY CLUSTERED,

    -- 楽観的ロック
    [row_version] TIMESTAMP NOT NULL,

    -- 監査情報：作成
    [inserted_at] DATETIME2(7) NOT NULL,
    [inserted_by] BIGINT NOT NULL,

    -- 監査情報：更新
    [updated_at] DATETIME2(7) NULL,
    [updated_by] BIGINT NULL,

    -- 監査情報：削除
    [deleted_at] DATETIME2(7) NULL,
    [deleted_by] BIGINT NULL,

    -- ビジネスデータ
    [person_row_id] BIGINT NOT NULL,
    [employee_code] INT NOT NULL,
    [retire_on] DATETIME2(7) NULL,

    -- 外部キー制約
    CONSTRAINT [FK_m_employees_person_row_id]
        FOREIGN KEY ([person_row_id]) REFERENCES [dbo].[m_persons]([row_id]),

    -- 自己参照FK（作成者は従業員）
    -- 注: m_employees の自己参照。テーブル作成後に制約を追加
);

-- インデックス作成（m_employees）
CREATE UNIQUE NONCLUSTERED INDEX [UX_m_employees_employee_code]
ON [dbo].[m_employees]([employee_code])
WHERE [deleted_at] IS NULL;

CREATE NONCLUSTERED INDEX [IX_m_employees_employee_code_deleted_at]
ON [dbo].[m_employees]([employee_code], [deleted_at]);

CREATE NONCLUSTERED INDEX [IX_m_employees_deleted_at]
ON [dbo].[m_employees]([deleted_at])
INCLUDE ([employee_code], [person_row_id]);

CREATE NONCLUSTERED INDEX [IX_m_employees_person_row_id]
ON [dbo].[m_employees]([person_row_id]);

CREATE NONCLUSTERED INDEX [IX_m_employees_inserted_by]
ON [dbo].[m_employees]([inserted_by]);

CREATE NONCLUSTERED INDEX [IX_m_employees_updated_by]
ON [dbo].[m_employees]([updated_by])
WHERE [updated_by] IS NOT NULL;

GO

-- =====================================================
-- m_department テーブル（部署マスタ）
-- =====================================================
IF OBJECT_ID('[dbo].[m_department]', 'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[m_department];
END
GO

CREATE TABLE [dbo].[m_department]
(
    -- 主キー・シーケンス
    [row_id] BIGINT NOT NULL
        DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence])
        CONSTRAINT [PK_m_department_row_id] PRIMARY KEY CLUSTERED,

    -- 楽観的ロック
    [row_version] TIMESTAMP NOT NULL,

    -- 監査情報：作成
    [inserted_at] DATETIME2(7) NOT NULL,
    [inserted_by] BIGINT NOT NULL,

    -- 監査情報：更新
    [updated_at] DATETIME2(7) NULL,
    [updated_by] BIGINT NULL,

    -- 監査情報：削除
    [deleted_at] DATETIME2(7) NULL,
    [deleted_by] BIGINT NULL,

    -- ビジネスデータ
    [department_code] CHAR(4) NOT NULL,
    [department_name] NVARCHAR(50) NOT NULL,
    [manager_employee_row_id] BIGINT NULL,
    [hierarchy_level] INT NOT NULL,
    [parent_department_row_id] BIGINT NULL,
    [abolished_on] DATETIME2(7) NULL,

    -- 外部キー制約
    CONSTRAINT [FK_m_department_manager_employee_row_id]
        FOREIGN KEY ([manager_employee_row_id]) REFERENCES [dbo].[m_employees]([row_id]),
    CONSTRAINT [FK_m_department_parent_department_row_id]
        FOREIGN KEY ([parent_department_row_id]) REFERENCES [dbo].[m_department]([row_id])
);

-- インデックス作成（m_department）
CREATE UNIQUE NONCLUSTERED INDEX [UX_m_department_code]
ON [dbo].[m_department]([department_code])
WHERE [deleted_at] IS NULL;

CREATE NONCLUSTERED INDEX [IX_m_department_parent_department_row_id]
ON [dbo].[m_department]([parent_department_row_id])
WHERE [parent_department_row_id] IS NOT NULL;

CREATE NONCLUSTERED INDEX [IX_m_department_manager_employee_row_id]
ON [dbo].[m_department]([manager_employee_row_id])
WHERE [manager_employee_row_id] IS NOT NULL;

CREATE NONCLUSTERED INDEX [IX_m_department_hierarchy_level_deleted_at]
ON [dbo].[m_department]([hierarchy_level], [deleted_at])
INCLUDE ([department_code], [department_name]);

CREATE NONCLUSTERED INDEX [IX_m_department_deleted_at]
ON [dbo].[m_department]([deleted_at]);

CREATE NONCLUSTERED INDEX [IX_m_department_inserted_by]
ON [dbo].[m_department]([inserted_by]);

GO

-- =====================================================
-- m_employee_department テーブル（従業員所属）
-- =====================================================
IF OBJECT_ID('[dbo].[m_employee_department]', 'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[m_employee_department];
END
GO

CREATE TABLE [dbo].[m_employee_department]
(
    -- 主キー・シーケンス
    [row_id] BIGINT NOT NULL
        DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence])
        CONSTRAINT [PK_m_employee_department_row_id] PRIMARY KEY CLUSTERED,

    -- 楽観的ロック
    [row_version] TIMESTAMP NOT NULL,

    -- 監査情報：作成
    [inserted_at] DATETIME2(7) NOT NULL,
    [inserted_by] BIGINT NOT NULL,

    -- 監査情報：更新
    [updated_at] DATETIME2(7) NULL,
    [updated_by] BIGINT NULL,

    -- 監査情報：削除
    [deleted_at] DATETIME2(7) NULL,
    [deleted_by] BIGINT NULL,

    -- ビジネスデータ
    [employee_row_id] BIGINT NOT NULL,
    [department_row_id] BIGINT NOT NULL,
    [is_primary] BIT NOT NULL,
    [end_on] DATETIME2(7) NULL,

    -- 外部キー制約
    CONSTRAINT [FK_m_employee_department_employee_row_id]
        FOREIGN KEY ([employee_row_id]) REFERENCES [dbo].[m_employees]([row_id]),
    CONSTRAINT [FK_m_employee_department_department_row_id]
        FOREIGN KEY ([department_row_id]) REFERENCES [dbo].[m_department]([row_id])
);

-- インデックス作成（m_employee_department）
CREATE NONCLUSTERED INDEX [IX_m_employee_department_employee_row_id_is_primary]
ON [dbo].[m_employee_department]([employee_row_id], [is_primary])
WHERE [deleted_at] IS NULL;

CREATE NONCLUSTERED INDEX [IX_m_employee_department_department_row_id_deleted_at]
ON [dbo].[m_employee_department]([department_row_id], [deleted_at])
INCLUDE ([employee_row_id], [is_primary]);

CREATE NONCLUSTERED INDEX [IX_m_employee_department_deleted_at]
ON [dbo].[m_employee_department]([deleted_at]);

CREATE NONCLUSTERED INDEX [IX_m_employee_department_end_on]
ON [dbo].[m_employee_department]([end_on])
WHERE [end_on] IS NULL;

CREATE NONCLUSTERED INDEX [IX_m_employee_department_inserted_by]
ON [dbo].[m_employee_department]([inserted_by]);

GO

-- =====================================================
-- m_login_credentials テーブル（ログイン認証情報）
-- =====================================================
IF OBJECT_ID('[dbo].[m_login_credentials]', 'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[m_login_credentials];
END
GO

CREATE TABLE [dbo].[m_login_credentials]
(
    -- 主キー・シーケンス
    [row_id] BIGINT NOT NULL
        DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence])
        CONSTRAINT [PK_m_login_credentials_row_id] PRIMARY KEY CLUSTERED,

    -- 楽観的ロック
    [row_version] TIMESTAMP NOT NULL,

    -- 監査情報：作成
    [inserted_at] DATETIME2(7) NOT NULL,
    [inserted_by] BIGINT NOT NULL,

    -- 監査情報：更新
    [updated_at] DATETIME2(7) NULL,
    [updated_by] BIGINT NULL,

    -- 監査情報：削除
    [deleted_at] DATETIME2(7) NULL,
    [deleted_by] BIGINT NULL,

    -- ビジネスデータ
    [employee_row_id] BIGINT NOT NULL,
    [login_id] NVARCHAR(50) NOT NULL,
    [password_hash] NVARCHAR(255) NOT NULL,
    [is_active] BIT NOT NULL,
    [last_login_at] DATETIME2(7) NULL,

    -- 外部キー制約
    CONSTRAINT [FK_m_login_credentials_employee_row_id]
        FOREIGN KEY ([employee_row_id]) REFERENCES [dbo].[m_employees]([row_id])
);

-- インデックス作成（m_login_credentials）
CREATE UNIQUE NONCLUSTERED INDEX [UX_m_login_credentials_login_id]
ON [dbo].[m_login_credentials]([login_id])
WHERE [deleted_at] IS NULL;

CREATE NONCLUSTERED INDEX [IX_m_login_credentials_employee_row_id_is_active]
ON [dbo].[m_login_credentials]([employee_row_id], [is_active])
WHERE [deleted_at] IS NULL;

CREATE NONCLUSTERED INDEX [IX_m_login_credentials_deleted_at]
ON [dbo].[m_login_credentials]([deleted_at]);

CREATE NONCLUSTERED INDEX [IX_m_login_credentials_inserted_by]
ON [dbo].[m_login_credentials]([inserted_by]);

GO

-- =====================================================
-- 4. 外部キー制約（自己参照）追加
-- =====================================================

-- m_persons の監査情報 FK
ALTER TABLE [dbo].[m_persons]
ADD CONSTRAINT [FK_m_persons_inserted_by]
    FOREIGN KEY ([inserted_by]) REFERENCES [dbo].[m_employees]([row_id]);

ALTER TABLE [dbo].[m_persons]
ADD CONSTRAINT [FK_m_persons_updated_by]
    FOREIGN KEY ([updated_by]) REFERENCES [dbo].[m_employees]([row_id]);

ALTER TABLE [dbo].[m_persons]
ADD CONSTRAINT [FK_m_persons_deleted_by]
    FOREIGN KEY ([deleted_by]) REFERENCES [dbo].[m_employees]([row_id]);

-- m_employees の監査情報 FK（自己参照）
ALTER TABLE [dbo].[m_employees]
ADD CONSTRAINT [FK_m_employees_inserted_by]
    FOREIGN KEY ([inserted_by]) REFERENCES [dbo].[m_employees]([row_id]);

ALTER TABLE [dbo].[m_employees]
ADD CONSTRAINT [FK_m_employees_updated_by]
    FOREIGN KEY ([updated_by]) REFERENCES [dbo].[m_employees]([row_id]);

ALTER TABLE [dbo].[m_employees]
ADD CONSTRAINT [FK_m_employees_deleted_by]
    FOREIGN KEY ([deleted_by]) REFERENCES [dbo].[m_employees]([row_id]);

-- m_department の監査情報 FK
ALTER TABLE [dbo].[m_department]
ADD CONSTRAINT [FK_m_department_inserted_by]
    FOREIGN KEY ([inserted_by]) REFERENCES [dbo].[m_employees]([row_id]);

ALTER TABLE [dbo].[m_department]
ADD CONSTRAINT [FK_m_department_updated_by]
    FOREIGN KEY ([updated_by]) REFERENCES [dbo].[m_employees]([row_id]);

ALTER TABLE [dbo].[m_department]
ADD CONSTRAINT [FK_m_department_deleted_by]
    FOREIGN KEY ([deleted_by]) REFERENCES [dbo].[m_employees]([row_id]);

-- m_employee_department の監査情報 FK
ALTER TABLE [dbo].[m_employee_department]
ADD CONSTRAINT [FK_m_employee_department_inserted_by]
    FOREIGN KEY ([inserted_by]) REFERENCES [dbo].[m_employees]([row_id]);

ALTER TABLE [dbo].[m_employee_department]
ADD CONSTRAINT [FK_m_employee_department_updated_by]
    FOREIGN KEY ([updated_by]) REFERENCES [dbo].[m_employees]([row_id]);

ALTER TABLE [dbo].[m_employee_department]
ADD CONSTRAINT [FK_m_employee_department_deleted_by]
    FOREIGN KEY ([deleted_by]) REFERENCES [dbo].[m_employees]([row_id]);

-- m_login_credentials の監査情報 FK
ALTER TABLE [dbo].[m_login_credentials]
ADD CONSTRAINT [FK_m_login_credentials_inserted_by]
    FOREIGN KEY ([inserted_by]) REFERENCES [dbo].[m_employees]([row_id]);

ALTER TABLE [dbo].[m_login_credentials]
ADD CONSTRAINT [FK_m_login_credentials_updated_by]
    FOREIGN KEY ([updated_by]) REFERENCES [dbo].[m_employees]([row_id]);

ALTER TABLE [dbo].[m_login_credentials]
ADD CONSTRAINT [FK_m_login_credentials_deleted_by]
    FOREIGN KEY ([deleted_by]) REFERENCES [dbo].[m_employees]([row_id]);

GO

-- =====================================================
-- 5. CDC (Change Data Capture) 有効化
-- =====================================================

-- データベースレベルで CDC を有効化
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'Advance' AND is_cdc_enabled = 1)
BEGIN
    EXEC sys.sp_cdc_enable_db;
END
GO

-- 各テーブルで CDC を有効化
EXEC sys.sp_cdc_enable_table
    @source_schema = N'dbo',
    @source_name = N'm_persons',
    @role_name = NULL,
    @supports_net_changes = 1;

EXEC sys.sp_cdc_enable_table
    @source_schema = N'dbo',
    @source_name = N'm_employees',
    @role_name = NULL,
    @supports_net_changes = 1;

EXEC sys.sp_cdc_enable_table
    @source_schema = N'dbo',
    @source_name = N'm_department',
    @role_name = NULL,
    @supports_net_changes = 1;

EXEC sys.sp_cdc_enable_table
    @source_schema = N'dbo',
    @source_name = N'm_employee_department',
    @role_name = NULL,
    @supports_net_changes = 1;

EXEC sys.sp_cdc_enable_table
    @source_schema = N'dbo',
    @source_name = N'm_login_credentials',
    @role_name = NULL,
    @supports_net_changes = 1;

GO

-- =====================================================
-- 6. 検証スクリプト
-- =====================================================

-- シーケンス確認
SELECT * FROM sys.sequences WHERE name = 's_row_id_sequence';

-- テーブル確認
SELECT
    TABLE_NAME,
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME) AS ColumnCount
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_SCHEMA = 'dbo'
  AND TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- インデックス確認
SELECT
    OBJECT_NAME(i.object_id) AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType
FROM sys.indexes i
WHERE OBJECT_SCHEMA_NAME(i.object_id) = 'dbo'
  AND i.name IS NOT NULL
  AND i.type > 0
ORDER BY OBJECT_NAME(i.object_id), i.name;

-- 外部キー確認
SELECT
    OBJECT_NAME(constraint_object_id) AS ConstraintName,
    OBJECT_NAME(parent_object_id) AS TableName,
    COL_NAME(parent_object_id, parent_column_id) AS ColumnName
FROM sys.foreign_key_columns
WHERE OBJECT_SCHEMA_NAME(parent_object_id) = 'dbo'
ORDER BY OBJECT_NAME(parent_object_id), COL_NAME(parent_object_id, parent_column_id);

GO

PRINT 'データベース Advance のテーブル作成が完了しました。';
