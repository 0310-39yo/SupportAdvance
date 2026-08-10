-- Advance Database - Initial Data Insert (System Administrator)
-- Version 1.0
-- Date: 2026-07-19
-- Purpose: Register System Administrator (employee_code=1000) for bootstrapping

USE [Advance];
GO

-- =====================================================
-- Disable Foreign Key Constraints (for bootstrapping)
-- =====================================================
ALTER TABLE [dbo].[m_persons] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[m_employees] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[m_department] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[m_employee_department] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[m_login_credentials] NOCHECK CONSTRAINT ALL;
GO

-- =====================================================
-- 1. Insert System Administrator into m_persons
-- =====================================================
DECLARE @SystemAdminPersonRowId BIGINT;

INSERT INTO [dbo].[m_persons]
(
    [inserted_at],
    [inserted_by],
    [last_name],
    [first_name],
    [last_name_kana],
    [first_name_kana]
)
VALUES
(
    GETDATE(),
    0,  -- Placeholder for System Admin (will be updated after employee creation)
    'System',
    'Administrator',
    'システム',
    'アドミニストレータ'
);

-- Get the inserted person_row_id
SET @SystemAdminPersonRowId = IDENT_CURRENT('[dbo].[m_persons]');

PRINT 'System Administrator person_row_id: ' + CAST(@SystemAdminPersonRowId AS NVARCHAR(20));

GO

-- =====================================================
-- 2. Insert System Administrator into m_employees
-- =====================================================
DECLARE @SystemAdminPersonRowId BIGINT;
DECLARE @SystemAdminEmployeeRowId BIGINT;

-- Get person_row_id of System Administrator
SELECT TOP 1 @SystemAdminPersonRowId = [row_id]
FROM [dbo].[m_persons]
WHERE [last_name] = 'System' AND [first_name] = 'Administrator'
ORDER BY [row_id] DESC;

-- Insert into m_employees
INSERT INTO [dbo].[m_employees]
(
    [inserted_at],
    [inserted_by],
    [person_row_id],
    [employee_code],
    [retire_on]
)
VALUES
(
    GETDATE(),
    0,  -- Placeholder (self-reference will be set after this insert)
    @SystemAdminPersonRowId,
    1000,  -- System Administrator code
    NULL
);

-- Get the inserted employee_row_id
SET @SystemAdminEmployeeRowId = IDENT_CURRENT('[dbo].[m_employees]');

PRINT 'System Administrator employee_row_id: ' + CAST(@SystemAdminEmployeeRowId AS NVARCHAR(20));

-- Store for next step
DECLARE @AdminRowId BIGINT;
SET @AdminRowId = @SystemAdminEmployeeRowId;

GO

-- =====================================================
-- 3. Update inserted_by to self-reference (System Admin)
-- =====================================================
DECLARE @SystemAdminEmployeeRowId BIGINT;

-- Get System Administrator employee_row_id
SELECT TOP 1 @SystemAdminEmployeeRowId = [row_id]
FROM [dbo].[m_employees]
WHERE [employee_code] = 1000
ORDER BY [row_id] DESC;

-- Update m_persons
UPDATE [dbo].[m_persons]
SET [inserted_by] = @SystemAdminEmployeeRowId
WHERE [last_name] = 'System' AND [first_name] = 'Administrator';

-- Update m_employees (self-reference)
UPDATE [dbo].[m_employees]
SET [inserted_by] = @SystemAdminEmployeeRowId
WHERE [employee_code] = 1000;

PRINT 'Updated inserted_by to System Administrator row_id.';

GO

-- =====================================================
-- 4. Insert Login Credentials for System Administrator
-- =====================================================
DECLARE @SystemAdminEmployeeRowId BIGINT;

-- Get System Administrator employee_row_id
SELECT TOP 1 @SystemAdminEmployeeRowId = [row_id]
FROM [dbo].[m_employees]
WHERE [employee_code] = 1000
ORDER BY [row_id] DESC;

-- Insert login credentials
-- Note: password_hash should be a real bcrypt/PBKDF2 hash in production
-- For initial setup, using a simple hash. Update with secure password in production.
INSERT INTO [dbo].[m_login_credentials]
(
    [inserted_at],
    [inserted_by],
    [employee_row_id],
    [login_id],
    [password_hash],
    [is_active],
    [last_login_at]
)
VALUES
(
    GETDATE(),
    @SystemAdminEmployeeRowId,  -- System Admin creates its own credential
    @SystemAdminEmployeeRowId,
    'system',  -- Login ID
    '$2b$12$R9h7cIPz0gi.URNNX3kh2OPST9/PgBkqquzi.Ee5A8L5bzBJfLSXu',  -- Placeholder hash (must be updated in production)
    1,  -- is_active = true
    NULL
);

PRINT 'System Administrator login credentials created.';

GO

-- =====================================================
-- Re-enable Foreign Key Constraints
-- =====================================================
ALTER TABLE [dbo].[m_persons] CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[m_employees] CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[m_department] CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[m_employee_department] CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[m_login_credentials] CHECK CONSTRAINT ALL;
GO

-- =====================================================
-- Verification Queries
-- =====================================================
PRINT '';
PRINT '=== System Administrator Registration Verification ===';
PRINT '';

PRINT 'Person Information';
SELECT [row_id], [last_name], [first_name], [inserted_at], [inserted_by]
FROM [dbo].[m_persons]
WHERE [last_name] = 'System' AND [first_name] = 'Administrator';

PRINT '';
PRINT 'Employee Information';
SELECT [row_id], [employee_code], [person_row_id], [inserted_at], [inserted_by]
FROM [dbo].[m_employees]
WHERE [employee_code] = 1000;

PRINT '';
PRINT 'Login Credentials';
SELECT [row_id], [login_id], [is_active], [employee_row_id], [inserted_at]
FROM [dbo].[m_login_credentials]
WHERE [login_id] = 'system';

PRINT '';
PRINT 'System Administrator registration complete.';
PRINT '';
PRINT 'Important Notes:';
PRINT '1. Default login: login_id="system"';
PRINT '2. Password hash is a placeholder - update with production password';
PRINT '3. Windows login: M1000 (M + employee_code 1000)';
PRINT '4. For password reset, update password_hash in m_login_credentials';
