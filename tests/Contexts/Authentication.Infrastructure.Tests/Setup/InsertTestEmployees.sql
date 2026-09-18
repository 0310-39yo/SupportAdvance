-- ============================================
-- テスト用従業員データ（Authentication BC テスト用）
-- 【用途】m_employees / m_persons テーブルにテスト従業員を挿入
-- 【実行前提】m_employees と m_persons テーブルが存在
-- 【注意】2026-09-18 時点の実スキーマに合わせて改訂:
--   m_persons: last_name/first_name/last_name_kana/first_name_kana + employee_row_id（NOT NULL）
--   m_employees: biz_division/biz_id/retired_on のみ（mapping_person_row_id・department_row_id は廃止）
--   挿入順序は m_employees → m_persons（m_persons.employee_row_id が m_employees.row_id を参照するため）
-- 【row_id / biz_id】既存の予約範囲との重複を避けるため 2147483730 / 2147483731（biz_id=1001/1002）を使用
-- ============================================

DECLARE @CurrentDateTime DATETIME2 = CONVERT(DATETIME2, '2026-09-14 00:00:00', 121);
DECLARE @SystemUserId BIGINT = 2147483667;

-- テスト従業員 1（ログイン成功ケース用）
-- row_id: 2147483730（InsertTestLoginCredentials.sql で参照）
IF NOT EXISTS (SELECT 1 FROM [m_employees] WHERE [row_id] = 2147483730)
BEGIN
    INSERT INTO [m_employees]
    (
        [row_id],
        [biz_division],
        [biz_id],
        [retired_on],
        [created_at],
        [created_by],
        [updated_at],
        [updated_by],
        [deleted_at],
        [deleted_by]
    )
    VALUES
    (
        2147483730,  -- row_id
        'M',  -- biz_division（従業員）
        1001,  -- biz_id
        NULL,  -- retired_on（未退職）
        @CurrentDateTime,
        @SystemUserId,
        NULL,
        NULL,
        NULL,
        NULL
    )
END

IF NOT EXISTS (SELECT 1 FROM [m_persons] WHERE [row_id] = 2147483730)
BEGIN
    INSERT INTO [m_persons]
    (
        [row_id],
        [employee_row_id],
        [last_name],
        [first_name],
        [last_name_kana],
        [first_name_kana],
        [created_at],
        [created_by],
        [updated_at],
        [updated_by],
        [deleted_at],
        [deleted_by]
    )
    VALUES
    (
        2147483730,  -- row_id
        2147483730,  -- employee_row_id（m_employees.row_id = 2147483730）
        N'テスト',
        N'太郎',
        N'テスト',
        N'タロウ',
        @CurrentDateTime,
        @SystemUserId,
        NULL,
        NULL,
        NULL,
        NULL
    )
END

-- テスト従業員 2（非アクティブ状態用）
-- row_id: 2147483731（InsertTestLoginCredentials.sql で参照）
IF NOT EXISTS (SELECT 1 FROM [m_employees] WHERE [row_id] = 2147483731)
BEGIN
    INSERT INTO [m_employees]
    (
        [row_id],
        [biz_division],
        [biz_id],
        [retired_on],
        [created_at],
        [created_by],
        [updated_at],
        [updated_by],
        [deleted_at],
        [deleted_by]
    )
    VALUES
    (
        2147483731,  -- row_id
        'M',  -- biz_division
        1002,  -- biz_id
        NULL,  -- retired_on
        @CurrentDateTime,
        @SystemUserId,
        NULL,
        NULL,
        NULL,
        NULL
    )
END

IF NOT EXISTS (SELECT 1 FROM [m_persons] WHERE [row_id] = 2147483731)
BEGIN
    INSERT INTO [m_persons]
    (
        [row_id],
        [employee_row_id],
        [last_name],
        [first_name],
        [last_name_kana],
        [first_name_kana],
        [created_at],
        [created_by],
        [updated_at],
        [updated_by],
        [deleted_at],
        [deleted_by]
    )
    VALUES
    (
        2147483731,  -- row_id
        2147483731,  -- employee_row_id（m_employees.row_id = 2147483731）
        N'テスト',
        N'花子',
        N'テスト',
        N'ハナコ',
        @CurrentDateTime,
        @SystemUserId,
        NULL,
        NULL,
        NULL,
        NULL
    )
END

PRINT 'テスト用従業員を挿入しました。';
PRINT 'test_employee_001: row_id = 2147483730（ログイン成功ケース用、biz_id=1001）';
PRINT 'test_employee_002: row_id = 2147483731（非アクティブ状態用、biz_id=1002）';
