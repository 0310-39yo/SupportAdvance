USE [SupportAdvance]
GO

/****** Object:  Table [dbo].[m_employee_department]    Script Date: 2026/08/16 2:19:27 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[m_employee_department](
	[row_id] [bigint] NOT NULL,
	[row_version] [timestamp] NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[created_by] [bigint] NOT NULL,
	[updated_at] [datetime2](7) NULL,
	[updated_by] [bigint] NULL,
	[deleted_at] [datetime2](7) NULL,
	[deleted_by] [bigint] NULL,
	[employee_row_id] [bigint] NOT NULL,
	[department_row_id] [bigint] NOT NULL,
	[is_primary] [bit] NOT NULL,
	[end_on] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[row_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[m_employee_department] ADD  CONSTRAINT [DF_m_employee_department_row_id]  DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence]) FOR [row_id]
GO

ALTER TABLE [dbo].[m_employee_department] ADD  CONSTRAINT [DF_m_employee_department_created_at]  DEFAULT (sysdatetime()) FOR [created_at]
GO

ALTER TABLE [dbo].[m_employee_department] ADD  CONSTRAINT [DF_m_employee_department_created_by]  DEFAULT ((2147483659.)) FOR [created_by]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'従業員RowId' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'm_employee_department', @level2type=N'COLUMN',@level2name=N'employee_row_id'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部署RowId' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'm_employee_department', @level2type=N'COLUMN',@level2name=N'department_row_id'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'主たる所属の時1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'm_employee_department', @level2type=N'COLUMN',@level2name=N'is_primary'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'終了日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'm_employee_department', @level2type=N'COLUMN',@level2name=N'end_on'
GO


