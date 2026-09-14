-- 指定した従業員（AuthorityRowId）の最新セッションを取得
-- 【用途】UserAuthSessionRepository.GetLatestByAuthorityRowIdAsync(authorityRowId)
-- 【パラメータ】@CurrentUserRowId: 従業員 RowId
-- 【戻り値】最新の 1 セッション（見つからない場合は 0 行）

SELECT TOP 1
    [row_id],
    [current_user_row_id],
    [is_ad_authenticated],
    [login_success],
    [logged_in_at],
    [logged_out_at],
    [login_credentials_row_id],
    [row_version],
    [created_at],
    [created_by],
    [updated_at],
    [updated_by],
    [deleted_at],
    [deleted_by]
FROM
    [t_user_auth_sessions]
WHERE
    [current_user_row_id] = @CurrentUserRowId
    AND [deleted_at] IS NULL
ORDER BY
    [logged_in_at] DESC
