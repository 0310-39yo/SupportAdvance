-- ============================================
-- テスト用従業員データ（Identity BC テスト用）
-- 【用途】m_employees テーブルにテスト従業員を挿入
-- 【実行前提】m_employees と m_persons テーブルが存在
-- ============================================

-- テスト従業員 1（ログイン成功ケース用）
-- 従業員ID: test_employee_001
-- row_id: 1（InsertTestLoginCredentials.sql で参照）
DECLARE @CurrentDateTime DATETIME2 = CONVERT(DATETIME2, '2026-09-14 00:00:00', 121);
DECLARE @SystemUserId BIGINT = 2147483667;

-- m_persons（個人情報）に挿入
IF NOT EXISTS (SELECT 1 FROM [m_persons] WHERE [row_id] = 1)
BEGIN
    INSERT INTO [m_persons]
    (
        [row_id],
        [employee_number],
        [name_family_name],
        [name_given_name],
        [phone_number],
        [mobile_number],
        [created_at],
        [created_by],
        [updated_at],
        [updated_by],
        [deleted_at],
        [deleted_by]
    )
    VALUES
    (
        1,  -- row_id
        'TEST001',  -- employee_number
        'テスト',  -- family_name
        '太郎',  -- given_name
        '01-0000-0001',  -- phone_number
        '080-1111-1111',  -- mobile_number
        @CurrentDateTime,
        @SystemUserId,
        NULL,
        NULL,
        NULL,
        NULL
    )
END

-- m_employees（従業員）に挿入
IF NOT EXISTS (SELECT 1 FROM [m_employees] WHERE [row_id] = 1)
BEGIN
    INSERT INTO [m_employees]
    (
        [row_id],
        [biz_division],
        [hire_date],
        [retired_date],
        [mapping_person_row_id],
        [department_row_id],
        [created_at],
        [created_by],
        [updated_at],
        [updated_by],
        [deleted_at],
        [deleted_by]
    )
    VALUES
    (
        1,  -- row_id
        1,  -- biz_division（一般社員）
        CONVERT(DATETIME2, '2020-01-01 00:00:00', 121),  -- hire_date
        NULL,  -- retired_date（未退職）
        1,  -- mapping_person_row_id（m_persons.row_id = 1）
        1,  -- department_row_id（部署ID = 1）
        @CurrentDateTime,
        @SystemUserId,
        NULL,
        NULL,
        NULL,
        NULL
    )
END

-- テスト従業員 2（非アクティブ状態用）
-- row_id: 2（InsertTestLoginCredentials.sql で参照）
IF NOT EXISTS (SELECT 1 FROM [m_persons] WHERE [row_id] = 2)
BEGIN
    INSERT INTO [m_persons]
    (
        [row_id],
        [employee_number],
        [name_family_name],
        [name_given_name],
        [phone_number],
        [mobile_number],
        [created_at],
        [created_by],
        [updated_at],
        [updated_by],
        [deleted_at],
        [deleted_by]
    )
    VALUES
    (
        2,  -- row_id
        'TEST002',  -- employee_number
        'テスト',  -- family_name
        '花子',  -- given_name
        '01-0000-0002',  -- phone_number
        '080-2222-2222',  -- mobile_number
        @CurrentDateTime,
        @SystemUserId,
        NULL,
        NULL,
        NULL,
        NULL
    )
END

IF NOT EXISTS (SELECT 1 FROM [m_employees] WHERE [row_id] = 2)
BEGIN
    INSERT INTO [m_employees]
    (
        [row_id],
        [biz_division],
        [hire_date],
        [retired_date],
        [mapping_person_row_id],
        [department_row_id],
        [created_at],
        [created_by],
        [updated_at],
        [updated_by],
        [deleted_at],
        [deleted_by]
    )
    VALUES
    (
        2,  -- row_id
        1,  -- biz_division
        CONVERT(DATETIME2, '2020-06-01 00:00:00', 121),  -- hire_date
        NULL,  -- retired_date
        2,  -- mapping_person_row_id
        1,  -- department_row_id
        @CurrentDateTime,
        @SystemUserId,
        NULL,
        NULL,
        NULL,
        NULL
    )
END

PRINT 'テスト用従業員を挿入しました。';
PRINT 'test_employee_001: row_id = 1（ログイン成功ケース用）';
PRINT 'test_employee_002: row_id = 2（非アクティブ状態用）';
