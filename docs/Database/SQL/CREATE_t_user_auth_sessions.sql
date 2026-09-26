USE [SupportAdvance]
GO

/****** Object:  Table [dbo].[t_user_auth_sessions]    Script Date: 2026/09/26 22:45:25 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t_user_auth_sessions](
	[row_id] [bigint] NOT NULL,
	[row_version] [timestamp] NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[created_by] [bigint] NOT NULL,
	[updated_at] [datetime2](7) NULL,
	[updated_by] [bigint] NULL,
	[deleted_at] [datetime2](7) NULL,
	[deleted_by] [bigint] NULL,
	[current_user_row_id] [bigint] NOT NULL,
	[is_ad_authenticated] [bit] NOT NULL,
	[login_success] [bit] NOT NULL,
	[logged_in_at] [datetime2](7) NOT NULL,
	[logged_out_at] [datetime2](7) NULL,
	[login_credentials_row_id] [bigint] NULL,
 CONSTRAINT [PK_t_user_auth_sessions] PRIMARY KEY CLUSTERED
(
	[row_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[t_user_auth_sessions] ADD  CONSTRAINT [DF_t_user_auth_sessions_row_id]  DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence]) FOR [row_id]
GO

ALTER TABLE [dbo].[t_user_auth_sessions] ADD  CONSTRAINT [DF_t_user_auth_sessions_created_at]  DEFAULT (sysdatetime()) FOR [created_at]
GO

ALTER TABLE [dbo].[t_user_auth_sessions] ADD  CONSTRAINT [DF_t_user_auth_sessions_created_by]  DEFAULT ((2147483667.)) FOR [created_by]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'作業者employee_row_id' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N't_user_auth_sessions', @level2type=N'COLUMN',@level2name=N'current_user_row_id'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'認証方式（0=ローカル認証、1=AD認証）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N't_user_auth_sessions', @level2type=N'COLUMN',@level2name=N'is_ad_authenticated'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'認証成功/失敗（1=成功、0=失敗）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N't_user_auth_sessions', @level2type=N'COLUMN',@level2name=N'login_success'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ログイン操作日時（失敗時も記録）' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N't_user_auth_sessions', @level2type=N'COLUMN',@level2name=N'logged_in_at'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ログアウト日時（アプリ終了時に記録）。NULL = 非正常終了' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N't_user_auth_sessions', @level2type=N'COLUMN',@level2name=N'logged_out_at'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ローカル認証時のみ値あり。m_login_credentials.row_id' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N't_user_auth_sessions', @level2type=N'COLUMN',@level2name=N'login_credentials_row_id'
GO
