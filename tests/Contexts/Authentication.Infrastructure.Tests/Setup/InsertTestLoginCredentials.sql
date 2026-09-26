-- ============================================
-- テストユーザー用 ログイン認証情報
-- 【用途】Authentication BC の Integration/E2E テスト
-- 【実行前提】
--   - m_login_credentials テーブルが存在
--   - InsertTestEmployees.sql を先に実行し、m_employees.row_id=2147483730,2147483731 が存在すること
-- 【注意】2026-09-18 修正: password_hash が旧SHA256形式（32バイト）のままで、現在の
-- PasswordHashService（PBKDF2+Salt、16バイトSalt+32バイトHash=48バイト、Base64で64文字）
-- と形式が一致せず検証が常に失敗していたため、正しい形式のハッシュに更新
-- ============================================

-- テストデータ削除（再実行時）
DELETE FROM [m_login_credentials]
WHERE [login_id] LIKE 'test_%';

-- テストユーザー1: 通常のログイン成功ケース
-- ログインID: test_user_001
-- パスワード: password123（PBKDF2+Salt ハッシュ、PasswordHashService.HashPassword() と同一形式）
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
    'AAECAwQFBgcICQoLDA0OD8WPr6JIIEVz/+J8qrhJT4YaUOny2Fkx0NvYilbGjb6n',  -- password123 の正しい PBKDF2+Salt ハッシュ
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
    'AAECAwQFBgcICQoLDA0OD8WPr6JIIEVz/+J8qrhJT4YaUOny2Fkx0NvYilbGjb6n',  -- password123 の正しい PBKDF2+Salt ハッシュ
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
