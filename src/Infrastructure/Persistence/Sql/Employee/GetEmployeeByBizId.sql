SELECT
    e.[row_id],
    e.[biz_division],
    e.[biz_id],
    e.[retired_on],
    p.[row_id] AS PersonRowId,
    p.[last_name],
    p.[first_name],
    p.[last_name_kana],
    p.[first_name_kana]
FROM [dbo].[m_employees] e
INNER JOIN [dbo].[m_employee_attributes] ea
    ON e.[row_id] = ea.[employee__row_id]
    AND ea.[attribute_type] = 'person'
INNER JOIN [dbo].[m_persons] p
    ON ea.[attribute_row_id] = p.[row_id]
WHERE e.[biz_id] = @BizId
    AND e.[deleted_at] IS NULL
    AND ea.[deleted_at] IS NULL
    AND p.[deleted_at] IS NULL;
