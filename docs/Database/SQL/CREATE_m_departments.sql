USE [SupportAdvance]
GO

/****** Object:  Table [dbo].[m_departments]    Script Date: 2026/08/09 9:12:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[m_departments](
	[row_id] [bigint] NOT NULL,
	[row_version] [timestamp] NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[created_by] [bigint] NOT NULL,
	[updated_at] [datetime2](7) NULL,
	[updated_by] [bigint] NULL,
	[deleted_at] [datetime2](7) NULL,
	[deleted_by] [bigint] NULL,
	[department_code] [char](4) NOT NULL,
	[department_name] [nvarchar](50) NOT NULL,
	[manager_employee_row_id] [bigint] NULL,
	[hierarchy_level] [int] NOT NULL,
	[parent_department_row_id] [bigint] NULL,
	[abolished_on] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[row_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[m_departments] ADD  DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence]) FOR [row_id]
GO

ALTER TABLE [dbo].[m_departments] ADD  CONSTRAINT [DF_m_department_created_at]  DEFAULT (sysdatetime()) FOR [created_at]
GO

ALTER TABLE [dbo].[m_departments] ADD  CONSTRAINT [DF_m_department_created_by]  DEFAULT ((2147483659.)) FOR [created_by]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部署コード' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'm_departments', @level2type=N'COLUMN',@level2name=N'department_code'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部署名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'm_departments', @level2type=N'COLUMN',@level2name=N'department_name'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'責任者RowId' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'm_departments', @level2type=N'COLUMN',@level2name=N'manager_employee_row_id'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'部署の階層レベル' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'm_departments', @level2type=N'COLUMN',@level2name=N'hierarchy_level'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'親部署RowId' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'm_departments', @level2type=N'COLUMN',@level2name=N'parent_department_row_id'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'廃止日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'm_departments', @level2type=N'COLUMN',@level2name=N'abolished_on'
GO

