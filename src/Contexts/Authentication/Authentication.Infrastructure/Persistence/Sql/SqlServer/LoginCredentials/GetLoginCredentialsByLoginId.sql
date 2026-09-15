-- ログインID でローカル認証情報マスターを取得
-- 【用途】LoginCredentialsQueryService.GetByLoginIdAsync(loginId)
-- 【パラメータ】@LoginId: ログインID（従業員番号など）
-- 【戻り値】該当する認証情報マスター（見つからない場合は 0 行）

SELECT
    [row_id],
    [mapping_employee_row_id],
    [login_id],
    [password_hash],
    [is_active]
FROM
    [m_login_credentials]
WHERE
    [login_id] = @LoginId
    AND [deleted_at] IS NULL
