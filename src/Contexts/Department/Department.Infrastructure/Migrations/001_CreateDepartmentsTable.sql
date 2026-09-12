-- =====================================================
-- 001_CreateDepartmentsTable.sql
-- 部署マスターテーブル作成
-- =====================================================

-- テーブル作成
CREATE TABLE [dbo].[m_departments] (
    -- 主キー・採番
    [row_id]                          [bigint]          NOT NULL PRIMARY KEY DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence]),

    -- 楽観ロック
    [row_version]                     [timestamp]       NOT NULL,

    -- ビジネスカラム
    [code]                            [nvarchar](4)     NOT NULL UNIQUE,
    [name]                            [nvarchar](100)   NOT NULL,
    [level]                           [int]             NOT NULL,
    [parent_department_row_id]        [bigint]          NULL,
    [manager_employee_row_id]         [bigint]          NULL,
    [abolished_on]                    [datetime2](7)    NULL,

    -- 監査カラム（作成）
    [created_at]                      [datetime2](7)    NOT NULL,
    [created_by]                      [bigint]          NOT NULL,

    -- 監査カラム（更新）
    [updated_at]                      [datetime2](7)    NULL,
    [updated_by]                      [bigint]          NULL,

    -- 監査カラム（論理削除）
    [deleted_at]                      [datetime2](7)    NULL,
    [deleted_by]                      [bigint]          NULL
);

-- インデックス
CREATE INDEX [IX_m_departments_code] ON [dbo].[m_departments]([code])
    WHERE [deleted_at] IS NULL;
CREATE INDEX [IX_m_departments_parent_department_row_id] ON [dbo].[m_departments]([parent_department_row_id])
    WHERE [deleted_at] IS NULL;
CREATE INDEX [IX_m_departments_created_at] ON [dbo].[m_departments]([created_at]);
CREATE INDEX [IX_m_departments_deleted_at] ON [dbo].[m_departments]([deleted_at]);

-- 制約
ALTER TABLE [dbo].[m_departments]
    ADD CONSTRAINT [FK_m_departments_parent_department]
        FOREIGN KEY ([parent_department_row_id])
        REFERENCES [dbo].[m_departments]([row_id]);

ALTER TABLE [dbo].[m_departments]
    ADD CONSTRAINT [FK_m_departments_manager_employee]
        FOREIGN KEY ([manager_employee_row_id])
        REFERENCES [dbo].[m_persons]([row_id]);

ALTER TABLE [dbo].[m_departments]
    ADD CONSTRAINT [FK_m_departments_created_by]
        FOREIGN KEY ([created_by])
        REFERENCES [dbo].[m_persons]([row_id]);

ALTER TABLE [dbo].[m_departments]
    ADD CONSTRAINT [FK_m_departments_updated_by]
        FOREIGN KEY ([updated_by])
        REFERENCES [dbo].[m_persons]([row_id]);

ALTER TABLE [dbo].[m_departments]
    ADD CONSTRAINT [FK_m_departments_deleted_by]
        FOREIGN KEY ([deleted_by])
        REFERENCES [dbo].[m_persons]([row_id]);

ALTER TABLE [dbo].[m_departments]
    ADD CONSTRAINT [CK_m_departments_level]
        CHECK ([level] >= 0 AND [level] <= 4);

-- テスト用サンプルデータ（オプション）
-- INSERT INTO [dbo].[m_departments] ([code], [name], [level], [created_at], [created_by])
-- VALUES (N'COMP', N'Company', 0, GETUTCDATE(), 1);
