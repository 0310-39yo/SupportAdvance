-- Advance Database - Create Tables (SQL Server 2022)
-- Version 1.0, Fixed
-- Date: 2026-07-19

-- =====================================================
-- Database Creation
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'Advance')
BEGIN
    CREATE DATABASE [Advance];
END
GO

USE [Advance];
GO

-- =====================================================
-- Sequence Definition (for row_id)
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
-- m_persons Table
-- =====================================================
IF OBJECT_ID('[dbo].[m_persons]', 'U') IS NOT NULL
    DROP TABLE [dbo].[m_persons];
GO

CREATE TABLE [dbo].[m_persons]
(
    [row_id] BIGINT NOT NULL
        DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence])
        CONSTRAINT [PK_m_persons_row_id] PRIMARY KEY CLUSTERED,
    [row_version] TIMESTAMP NOT NULL,
    [inserted_at] DATETIME2(7) NOT NULL,
    [inserted_by] BIGINT NOT NULL,
    [updated_at] DATETIME2(7) NULL,
    [updated_by] BIGINT NULL,
    [deleted_at] DATETIME2(7) NULL,
    [deleted_by] BIGINT NULL,
    [last_name] NVARCHAR(50) NOT NULL,
    [first_name] NVARCHAR(50) NOT NULL,
    [last_name_kana] NVARCHAR(50) NOT NULL,
    [first_name_kana] NVARCHAR(50) NOT NULL
);

CREATE NONCLUSTERED INDEX [IX_m_persons_deleted_at]
ON [dbo].[m_persons]([deleted_at]);

CREATE NONCLUSTERED INDEX [IX_m_persons_inserted_at_deleted_at]
ON [dbo].[m_persons]([inserted_at], [deleted_at]);

GO

-- =====================================================
-- m_employees Table
-- =====================================================
IF OBJECT_ID('[dbo].[m_employees]', 'U') IS NOT NULL
    DROP TABLE [dbo].[m_employees];
GO

CREATE TABLE [dbo].[m_employees]
(
    [row_id] BIGINT NOT NULL
        DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence])
        CONSTRAINT [PK_m_employees_row_id] PRIMARY KEY CLUSTERED,
    [row_version] TIMESTAMP NOT NULL,
    [inserted_at] DATETIME2(7) NOT NULL,
    [inserted_by] BIGINT NOT NULL,
    [updated_at] DATETIME2(7) NULL,
    [updated_by] BIGINT NULL,
    [deleted_at] DATETIME2(7) NULL,
    [deleted_by] BIGINT NULL,
    [person_row_id] BIGINT NOT NULL,
    [employee_code] INT NOT NULL,
    [retire_on] DATETIME2(7) NULL,
    CONSTRAINT [FK_m_employees_person_row_id]
        FOREIGN KEY ([person_row_id]) REFERENCES [dbo].[m_persons]([row_id])
);

CREATE UNIQUE NONCLUSTERED INDEX [UX_m_employees_employee_code]
ON [dbo].[m_employees]([employee_code])
WHERE [deleted_at] IS NULL;

CREATE NONCLUSTERED INDEX [IX_m_employees_employee_code_deleted_at]
ON [dbo].[m_employees]([employee_code], [deleted_at]);

CREATE NONCLUSTERED INDEX [IX_m_employees_deleted_at]
ON [dbo].[m_employees]([deleted_at]);

GO

-- =====================================================
-- m_department Table
-- =====================================================
IF OBJECT_ID('[dbo].[m_department]', 'U') IS NOT NULL
    DROP TABLE [dbo].[m_department];
GO

CREATE TABLE [dbo].[m_department]
(
    [row_id] BIGINT NOT NULL
        DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence])
        CONSTRAINT [PK_m_department_row_id] PRIMARY KEY CLUSTERED,
    [row_version] TIMESTAMP NOT NULL,
    [inserted_at] DATETIME2(7) NOT NULL,
    [inserted_by] BIGINT NOT NULL,
    [updated_at] DATETIME2(7) NULL,
    [updated_by] BIGINT NULL,
    [deleted_at] DATETIME2(7) NULL,
    [deleted_by] BIGINT NULL,
    [department_code] CHAR(4) NOT NULL,
    [department_name] NVARCHAR(50) NOT NULL,
    [manager_employee_row_id] BIGINT NULL,
    [hierarchy_level] INT NOT NULL,
    [parent_department_row_id] BIGINT NULL,
    [abolished_on] DATETIME2(7) NULL
);

CREATE UNIQUE NONCLUSTERED INDEX [UX_m_department_code]
ON [dbo].[m_department]([department_code])
WHERE [deleted_at] IS NULL;

CREATE NONCLUSTERED INDEX [IX_m_department_hierarchy_level_deleted_at]
ON [dbo].[m_department]([hierarchy_level], [deleted_at]);

CREATE NONCLUSTERED INDEX [IX_m_department_deleted_at]
ON [dbo].[m_department]([deleted_at]);

GO

-- =====================================================
-- m_employee_department Table
-- =====================================================
IF OBJECT_ID('[dbo].[m_employee_department]', 'U') IS NOT NULL
    DROP TABLE [dbo].[m_employee_department];
GO

CREATE TABLE [dbo].[m_employee_department]
(
    [row_id] BIGINT NOT NULL
        DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence])
        CONSTRAINT [PK_m_employee_department_row_id] PRIMARY KEY CLUSTERED,
    [row_version] TIMESTAMP NOT NULL,
    [inserted_at] DATETIME2(7) NOT NULL,
    [inserted_by] BIGINT NOT NULL,
    [updated_at] DATETIME2(7) NULL,
    [updated_by] BIGINT NULL,
    [deleted_at] DATETIME2(7) NULL,
    [deleted_by] BIGINT NULL,
    [employee_row_id] BIGINT NOT NULL,
    [department_row_id] BIGINT NOT NULL,
    [is_primary] BIT NOT NULL,
    [end_on] DATETIME2(7) NULL,
    CONSTRAINT [FK_m_employee_department_employee_row_id]
        FOREIGN KEY ([employee_row_id]) REFERENCES [dbo].[m_employees]([row_id]),
    CONSTRAINT [FK_m_employee_department_department_row_id]
        FOREIGN KEY ([department_row_id]) REFERENCES [dbo].[m_department]([row_id])
);

CREATE NONCLUSTERED INDEX [IX_m_employee_department_employee_row_id_is_primary]
ON [dbo].[m_employee_department]([employee_row_id], [is_primary])
WHERE [deleted_at] IS NULL;

CREATE NONCLUSTERED INDEX [IX_m_employee_department_department_row_id_deleted_at]
ON [dbo].[m_employee_department]([department_row_id], [deleted_at]);

CREATE NONCLUSTERED INDEX [IX_m_employee_department_deleted_at]
ON [dbo].[m_employee_department]([deleted_at]);

GO

-- =====================================================
-- m_login_credentials Table
-- =====================================================
IF OBJECT_ID('[dbo].[m_login_credentials]', 'U') IS NOT NULL
    DROP TABLE [dbo].[m_login_credentials];
GO

CREATE TABLE [dbo].[m_login_credentials]
(
    [row_id] BIGINT NOT NULL
        DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence])
        CONSTRAINT [PK_m_login_credentials_row_id] PRIMARY KEY CLUSTERED,
    [row_version] TIMESTAMP NOT NULL,
    [inserted_at] DATETIME2(7) NOT NULL,
    [inserted_by] BIGINT NOT NULL,
    [updated_at] DATETIME2(7) NULL,
    [updated_by] BIGINT NULL,
    [deleted_at] DATETIME2(7) NULL,
    [deleted_by] BIGINT NULL,
    [employee_row_id] BIGINT NOT NULL,
    [login_id] NVARCHAR(50) NOT NULL,
    [password_hash] NVARCHAR(255) NOT NULL,
    [is_active] BIT NOT NULL,
    [last_login_at] DATETIME2(7) NULL,
    CONSTRAINT [FK_m_login_credentials_employee_row_id]
        FOREIGN KEY ([employee_row_id]) REFERENCES [dbo].[m_employees]([row_id])
);

CREATE UNIQUE NONCLUSTERED INDEX [UX_m_login_credentials_login_id]
ON [dbo].[m_login_credentials]([login_id])
WHERE [deleted_at] IS NULL;

CREATE NONCLUSTERED INDEX [IX_m_login_credentials_employee_row_id_is_active]
ON [dbo].[m_login_credentials]([employee_row_id], [is_active])
WHERE [deleted_at] IS NULL;

CREATE NONCLUSTERED INDEX [IX_m_login_credentials_deleted_at]
ON [dbo].[m_login_credentials]([deleted_at]);

GO

-- =====================================================
-- Foreign Keys (Audit Info & Self-References)
-- =====================================================
ALTER TABLE [dbo].[m_persons]
ADD CONSTRAINT [FK_m_persons_inserted_by]
    FOREIGN KEY ([inserted_by]) REFERENCES [dbo].[m_employees]([row_id]);

ALTER TABLE [dbo].[m_persons]
ADD CONSTRAINT [FK_m_persons_updated_by]
    FOREIGN KEY ([updated_by]) REFERENCES [dbo].[m_employees]([row_id]);

ALTER TABLE [dbo].[m_persons]
ADD CONSTRAINT [FK_m_persons_deleted_by]
    FOREIGN KEY ([deleted_by]) REFERENCES [dbo].[m_employees]([row_id]);

ALTER TABLE [dbo].[m_employees]
ADD CONSTRAINT [FK_m_employees_inserted_by]
    FOREIGN KEY ([inserted_by]) REFERENCES [dbo].[m_employees]([row_id]);

ALTER TABLE [dbo].[m_employees]
ADD CONSTRAINT [FK_m_employees_updated_by]
    FOREIGN KEY ([updated_by]) REFERENCES [dbo].[m_employees]([row_id]);

ALTER TABLE [dbo].[m_employees]
ADD CONSTRAINT [FK_m_employees_deleted_by]
    FOREIGN KEY ([deleted_by]) REFERENCES [dbo].[m_employees]([row_id]);

ALTER TABLE [dbo].[m_department]
ADD CONSTRAINT [FK_m_department_manager_employee_row_id]
    FOREIGN KEY ([manager_employee_row_id]) REFERENCES [dbo].[m_employees]([row_id]);

ALTER TABLE [dbo].[m_department]
ADD CONSTRAINT [FK_m_department_parent_department_row_id]
    FOREIGN KEY ([parent_department_row_id]) REFERENCES [dbo].[m_department]([row_id]);

ALTER TABLE [dbo].[m_department]
ADD CONSTRAINT [FK_m_department_inserted_by]
    FOREIGN KEY ([inserted_by]) REFERENCES [dbo].[m_employees]([row_id]);

ALTER TABLE [dbo].[m_department]
ADD CONSTRAINT [FK_m_department_updated_by]
    FOREIGN KEY ([updated_by]) REFERENCES [dbo].[m_employees]([row_id]);

ALTER TABLE [dbo].[m_department]
ADD CONSTRAINT [FK_m_department_deleted_by]
    FOREIGN KEY ([deleted_by]) REFERENCES [dbo].[m_employees]([row_id]);

ALTER TABLE [dbo].[m_employee_department]
ADD CONSTRAINT [FK_m_employee_department_inserted_by]
    FOREIGN KEY ([inserted_by]) REFERENCES [dbo].[m_employees]([row_id]);

ALTER TABLE [dbo].[m_employee_department]
ADD CONSTRAINT [FK_m_employee_department_updated_by]
    FOREIGN KEY ([updated_by]) REFERENCES [dbo].[m_employees]([row_id]);

ALTER TABLE [dbo].[m_employee_department]
ADD CONSTRAINT [FK_m_employee_department_deleted_by]
    FOREIGN KEY ([deleted_by]) REFERENCES [dbo].[m_employees]([row_id]);

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
-- Enable CDC (Change Data Capture)
-- =====================================================
EXEC sys.sp_cdc_enable_db;

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
-- Verification Queries
-- =====================================================
SELECT 'Tables created successfully' AS Status;

SELECT
    TABLE_NAME,
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME) AS ColumnCount
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_SCHEMA = 'dbo'
  AND TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
