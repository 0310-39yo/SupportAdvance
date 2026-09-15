-- ============================================
-- テストユーザー用 ログイン認証情報
-- 【用途】Authentication BC の Integration/E2E テスト
-- 【実行前提】
--   - m_login_credentials テーブルが存在
--   - m_employees テーブルにテストユーザーが存在
-- ============================================

-- テストデータ削除（再実行時）
DELETE FROM [m_login_credentials]
WHERE [login_id] LIKE 'test_%';

-- テストユーザー1: 通常のログイン成功ケース
-- ログインID: test_user_001
-- パスワード: password123（SHA256 ハッシュ: 75K3eLr+dx6JJFuJ7LwIpEpOFmwGZZkRiB84PURz6U8=）
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
    2147483730,  -- テスト従業員rowId（m_employees.row_id=2147483730、biz_id=1001）
    'test_user_001',
    '75K3eLr+dx6JJFuJ7LwIpEpOFmwGZZkRiB84PURz6U8=',  -- password123 の正しい SHA256 ハッシュ
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
    2147483731,  -- テスト従業員rowId（m_employees.row_id=2147483731、biz_id=1002）
    'test_user_inactive',
    '75K3eLr+dx6JJFuJ7LwIpEpOFmwGZZkRiB84PURz6U8=',  -- password123 の正しい SHA256 ハッシュ
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
