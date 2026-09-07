USE [SupportAdvance]
GO

/****** Object:  Table [dbo].[m_employees]    Script Date: 2026/09/07 5:36:47 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[m_employees](
	[row_id] [bigint] NOT NULL,
	[row_version] [timestamp] NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[created_by] [bigint] NOT NULL,
	[updated_at] [datetime2](7) NULL,
	[updated_by] [bigint] NULL,
	[deleted_at] [datetime2](7) NULL,
	[deleted_by] [bigint] NULL,
	[biz_division] [char](1) NOT NULL,
	[biz_id] [int] NOT NULL,
	[retired_on] [datetime2](7) NULL,
 CONSTRAINT [PK__m_employ__6965AB57F60720BD] PRIMARY KEY CLUSTERED 
(
	[row_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[m_employees] ADD  CONSTRAINT [DF_m_employees_row_id]  DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence]) FOR [row_id]
GO

ALTER TABLE [dbo].[m_employees] ADD  CONSTRAINT [DF_m_employees_created_at]  DEFAULT (sysdatetime()) FOR [created_at]
GO

ALTER TABLE [dbo].[m_employees] ADD  CONSTRAINT [DF_m_employees_created_by]  DEFAULT ((2147483659.)) FOR [created_by]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'従業員区分' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'm_employees', @level2type=N'COLUMN',@level2name=N'biz_division'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'従業員No' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'm_employees', @level2type=N'COLUMN',@level2name=N'biz_id'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'離職日' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'm_employees', @level2type=N'COLUMN',@level2name=N'retired_on'
GO

