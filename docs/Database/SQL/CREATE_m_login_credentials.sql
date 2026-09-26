USE [SupportAdvance]
GO

/****** Object:  Table [dbo].[m_login_credentials]    Script Date: 2026/09/26 22:44:56 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[m_login_credentials](
	[row_id] [bigint] NOT NULL,
	[row_version] [timestamp] NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[created_by] [bigint] NOT NULL,
	[updated_at] [datetime2](7) NULL,
	[updated_by] [bigint] NULL,
	[deleted_at] [datetime2](7) NULL,
	[deleted_by] [bigint] NULL,
	[mapping_employee_row_id] [bigint] NOT NULL,
	[login_id] [nvarchar](50) NOT NULL,
	[password_hash] [nvarchar](255) NOT NULL,
	[is_active] [bit] NOT NULL,
	[last_login_at] [datetime2](7) NULL,
 CONSTRAINT [PK__m_login___6965AB575369672C] PRIMARY KEY CLUSTERED
(
	[row_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[m_login_credentials] ADD  CONSTRAINT [DF_m_login_credentials_row_id]  DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence]) FOR [row_id]
GO

ALTER TABLE [dbo].[m_login_credentials] ADD  CONSTRAINT [DF_m_login_credentials_created_at]  DEFAULT (sysdatetime()) FOR [created_at]
GO

ALTER TABLE [dbo].[m_login_credentials] ADD  CONSTRAINT [DF_m_login_credentials_created_by]  DEFAULT ((2147483667.)) FOR [created_by]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'マッピング従業員RowId' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'm_login_credentials', @level2type=N'COLUMN',@level2name=N'mapping_employee_row_id'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ログインId' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'm_login_credentials', @level2type=N'COLUMN',@level2name=N'login_id'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ログインパスワードハッシュ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'm_login_credentials', @level2type=N'COLUMN',@level2name=N'password_hash'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'使用可否1の時使用可' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'm_login_credentials', @level2type=N'COLUMN',@level2name=N'is_active'
GO
