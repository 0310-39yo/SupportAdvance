USE [SupportAdvance]
GO

-- ============================================
-- 予約済みシステムアカウント seed データ
-- 【用途】created_by/updated_by/deleted_by や AuthorityRowId のフォールバック先として、
--         全BC・全テーブルから参照される予約済み Person/Employee レコードを作成する。
-- 【対象】
--   1. System User  （m_persons.row_id=2147483659, m_employees.row_id=2147483667）
--      → 自動処理・バックグラウンド処理など「人間ではない操作」の作成者/更新者/削除者として使用
--        （src/Common/WellKnownIds.cs の SystemUserPersonRowId / SystemUserEmployeeRowId）
--   2. Unknown User （m_persons.row_id=2147483648, m_employees.row_id=2147483649）
--      → employeeマスタ上で本人を特定できない人物（例: 存在しないログインIDでのログイン試行）を表す
--        （src/Common/WellKnownIds.cs の UnknownUserPersonRowId / UnknownUserEmployeeRowId。
--        AuthenticateLocalUserUseCase の失敗ログで AuthorityRowId として使用）
-- 【値の根拠】
--   s_row_id_sequence は既に int.MaxValue（2147483647）を超えて稼働中
--   （2026-09-18 確認時点で current_value=2147483851, increment=1, is_cycling=0）。
--   2147483648〜2147483658 の範囲はシーケンスが既に通過済み（is_cycling=0 のため再利用されない）
--   かつ現在未使用の空番のため、Unknown User の予約値として安全に使用できる。
-- 【biz_id】System User=1000, Unknown User=1003（ユニーク制約あり。事前に空き番であることを確認）
-- 【実行前提】m_persons, m_employees テーブルが存在。何度実行しても安全（IF NOT EXISTS）。
-- 【挿入順序】m_employees → m_persons
--   （m_persons.employee_row_id が NOT NULL で m_employees を参照するため。
--   created_by は m_employees 側も m_persons 側も NOT NULL のため、
--   System User 自身の作成時のみ自己参照（created_by = 自分の row_id）とする）
-- ============================================

DECLARE @SystemUserEmployeeRowId BIGINT = 2147483667;
DECLARE @SystemUserPersonRowId BIGINT = 2147483659;
DECLARE @UnknownUserEmployeeRowId BIGINT = 2147483649;
DECLARE @UnknownUserPersonRowId BIGINT = 2147483648;

-- ============================================
-- 1. System User
-- ============================================

IF NOT EXISTS (SELECT 1 FROM [m_employees] WHERE [row_id] = @SystemUserEmployeeRowId)
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
        @SystemUserEmployeeRowId,
        'M',
        1000,
        NULL,
        SYSDATETIME(),
        @SystemUserEmployeeRowId,  -- 自己参照（最初のレコードのため他に参照先がない）
        NULL,
        NULL,
        NULL,
        NULL
    )
END

IF NOT EXISTS (SELECT 1 FROM [m_persons] WHERE [row_id] = @SystemUserPersonRowId)
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
        @SystemUserPersonRowId,
        @SystemUserEmployeeRowId,
        N'システム',
        N'ユーザー',
        N'システム',
        N'ユーザー',
        SYSDATETIME(),
        @SystemUserEmployeeRowId,
        NULL,
        NULL,
        NULL,
        NULL
    )
END

-- ============================================
-- 2. Unknown User
-- 【前提】biz_id=1003 が空き番であること（既存データがあれば実行前に手動削除）
-- ============================================

IF NOT EXISTS (SELECT 1 FROM [m_employees] WHERE [row_id] = @UnknownUserEmployeeRowId)
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
        @UnknownUserEmployeeRowId,
        'M',
        1003,
        NULL,
        SYSDATETIME(),
        @SystemUserEmployeeRowId,  -- System User が作成した扱い
        NULL,
        NULL,
        NULL,
        NULL
    )
END

IF NOT EXISTS (SELECT 1 FROM [m_persons] WHERE [row_id] = @UnknownUserPersonRowId)
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
        @UnknownUserPersonRowId,
        @UnknownUserEmployeeRowId,
        N'不明',
        N'ユーザー',
        N'フメイ',
        N'ユーザー',
        SYSDATETIME(),
        @SystemUserEmployeeRowId,
        NULL,
        NULL,
        NULL,
        NULL
    )
END

PRINT '予約済みシステムアカウントを投入しました。';
PRINT 'System User : m_persons.row_id=2147483659, m_employees.row_id=2147483667 (biz_id=1000)';
PRINT 'Unknown User: m_persons.row_id=2147483648, m_employees.row_id=2147483649 (biz_id=1003)';
GO
