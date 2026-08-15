USE [SupportAdvance]
GO

/****** Object:  Table [dbo].[m_employee_attributes]    Script Date: 2026/08/16 3:23:25 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[m_employee_attributes](
	[row_id] [bigint] NOT NULL,
	[row_version] [timestamp] NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[created_by] [bigint] NOT NULL,
	[updated_at] [datetime2](7) NULL,
	[updated_by] [bigint] NULL,
	[deleted_at] [datetime2](7) NULL,
	[deleted_by] [bigint] NULL,
	[employee__row_id] [bigint] NOT NULL,
	[attribute_type] [nvarchar](50) NOT NULL,
	[attribute_row_id] [bigint] NOT NULL,
 CONSTRAINT [PK_m_employee_attributes] PRIMARY KEY CLUSTERED 
(
	[row_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[m_employee_attributes] ADD  CONSTRAINT [DF_m_employee_attributes_row_id]  DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence]) FOR [row_id]
GO

ALTER TABLE [dbo].[m_employee_attributes] ADD  CONSTRAINT [DF_m_employee_attributes_created_at]  DEFAULT (sysdatetime()) FOR [created_at]
GO

ALTER TABLE [dbo].[m_employee_attributes] ADD  CONSTRAINT [DF_m_employee_attributes_created_by]  DEFAULT ((2147483659.)) FOR [created_by]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'従業員RowId' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'm_employee_attributes', @level2type=N'COLUMN',@level2name=N'employee__row_id'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'属性タイプ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'm_employee_attributes', @level2type=N'COLUMN',@level2name=N'attribute_type'
GO

