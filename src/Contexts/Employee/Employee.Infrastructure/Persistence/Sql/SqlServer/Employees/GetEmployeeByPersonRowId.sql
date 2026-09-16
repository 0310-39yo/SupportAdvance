SELECT
    e.[row_id],
    e.[row_version],
    e.[biz_division],
    e.[biz_id],
    e.[retired_on],
    e.[created_at],
    e.[created_by],
    e.[updated_at],
    e.[updated_by],
    e.[deleted_at],
    e.[deleted_by]
FROM [dbo].[m_employees] e
INNER JOIN [dbo].[m_persons] p ON p.[employee_row_id] = e.[row_id]
WHERE p.[row_id] = @PersonRowId
    AND e.[deleted_at] IS NULL;
