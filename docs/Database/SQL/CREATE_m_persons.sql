USE [SupportAdvance]
GO

/****** Object:  Table [dbo].[m_persons]    Script Date: 2026/08/09 9:04:15 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[m_persons](
	[row_id] [bigint] NOT NULL,
	[row_version] [timestamp] NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[created_by] [bigint] NOT NULL,
	[updated_at] [datetime2](7) NULL,
	[updated_by] [bigint] NULL,
	[deleted_at] [datetime2](7) NULL,
	[deleted_by] [bigint] NULL,
	[last_name] [nvarchar](50) NOT NULL,
	[first_name] [nvarchar](50) NOT NULL,
	[last_name_kana] [nvarchar](50) NOT NULL,
	[first_name_kana] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[row_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[m_persons] ADD  DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence]) FOR [row_id]
GO

ALTER TABLE [dbo].[m_persons] ADD  CONSTRAINT [DF_m_persons_created_at]  DEFAULT (sysdatetime()) FOR [created_at]
GO

ALTER TABLE [dbo].[m_persons] ADD  CONSTRAINT [DF_m_persons_created_by]  DEFAULT ((2147483659.)) FOR [created_by]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'姓' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'm_persons', @level2type=N'COLUMN',@level2name=N'last_name'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'名' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'm_persons', @level2type=N'COLUMN',@level2name=N'first_name'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'姓カタカナ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'm_persons', @level2type=N'COLUMN',@level2name=N'last_name_kana'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'名カタカナ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'm_persons', @level2type=N'COLUMN',@level2name=N'first_name_kana'
GO

