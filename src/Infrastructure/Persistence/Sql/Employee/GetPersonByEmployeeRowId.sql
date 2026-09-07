SELECT
    p.[row_id],
    p.[row_version],
    p.[employee_row_id],
    p.[last_name],
    p.[first_name],
    p.[last_name_kana],
    p.[first_name_kana],
    p.[created_at],
    p.[created_by],
    p.[updated_at],
    p.[updated_by],
    p.[deleted_at],
    p.[deleted_by]
FROM [dbo].[m_persons] p
WHERE p.[employee_row_id] = @EmployeeRowId
    AND p.[deleted_at] IS NULL;
