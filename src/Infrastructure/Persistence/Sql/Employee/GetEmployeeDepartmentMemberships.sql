SELECT
    dm.[row_id],
    dm.[row_version],
    dm.[employee_row_id],
    dm.[department_row_id],
    dm.[is_primary],
    dm.[end_on],
    d.[department_name]
FROM [dbo].[m_department_memberships] dm
LEFT JOIN [dbo].[m_departments] d
    ON dm.[department_row_id] = d.[row_id]
    AND d.[deleted_at] IS NULL
WHERE dm.[employee_row_id] = @EmployeeRowId
    AND dm.[deleted_at] IS NULL
ORDER BY dm.[is_primary] DESC, dm.[row_id];
