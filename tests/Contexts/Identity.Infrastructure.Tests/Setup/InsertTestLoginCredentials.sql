-- ============================================
-- テストユーザー用 ログイン認証情報
-- 【用途】Identity BC の Integration/E2E テスト
-- 【実行前提】
--   - m_login_credentials テーブルが存在
--   - m_employees テーブルにテストユーザーが存在
-- ============================================

-- テストデータ削除（再実行時）
DELETE FROM [m_login_credentials]
WHERE [login_id] LIKE 'test_%';

-- テストユーザー1: 通常のログイン成功ケース
-- ログインID: test_user_001
-- パスワード: password123（SHA256 ハッシュ）
INSERT INTO [m_login_credentials]
(
    [mapping_employee_row_id],
    [login_id],
    [password_hash],
    [is_active],
    [created_at],
    [created_by],
    [updated_at],
    [updated_by],
    [deleted_at],
    [deleted_by]
)
VALUES
(
    1,  -- テスト従業員rowId（m_employees.row_id=1を前提）
    'test_user_001',
    'YJfN1x5e8gZ2Hs3Kq9Lm1Oa5Bc7Df9Gj3Np2Rx4Sv6Ty8Uz0Wd5Kp+7Lm=',  -- password123 の SHA256 ハッシュ値（プレースホルダー）
    1,  -- is_active = true
    CONVERT(DATETIME2, '2026-09-14 00:00:00', 121),
    2147483667,  -- System User
    NULL,
    NULL,
    NULL,
    NULL
);

-- テストユーザー2: 非アクティブ状態（ログイン失敗ケース）
INSERT INTO [m_login_credentials]
(
    [mapping_employee_row_id],
    [login_id],
    [password_hash],
    [is_active],
    [created_at],
    [created_by],
    [updated_at],
    [updated_by],
    [deleted_at],
    [deleted_by]
)
VALUES
(
    2,  -- テスト従業員rowId（m_employees.row_id=2を前提）
    'test_user_inactive',
    'YJfN1x5e8gZ2Hs3Kq9Lm1Oa5Bc7Df9Gj3Np2Rx4Sv6Ty8Uz0Wd5Kp+7Lm=',
    0,  -- is_active = false
    CONVERT(DATETIME2, '2026-09-14 00:00:00', 121),
    2147483667,  -- System User
    NULL,
    NULL,
    NULL,
    NULL
);

PRINT 'テストユーザーを挿入しました。';
PRINT 'test_user_001: アクティブ状態（ログイン成功ケース）';
PRINT 'test_user_inactive: 非アクティブ状態（ログイン失敗ケース）';
