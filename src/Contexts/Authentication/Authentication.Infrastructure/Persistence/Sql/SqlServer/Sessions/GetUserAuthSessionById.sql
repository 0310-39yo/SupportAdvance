-- UserAuthSession を RowId で取得
-- 【用途】UserAuthSessionRepository.GetByIdAsync(id)
-- 【パラメータ】@RowId: UserAuthSession RowId
-- 【戻り値】該当セッション（見つからない場合は 0 行）

SELECT
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
    [row_id] = @RowId
    AND [deleted_at] IS NULL
