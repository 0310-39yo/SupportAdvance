# テーブル設計標準ガイド

SupportAdvance プロジェクトのすべてのテーブルは、以下の設計標準に従う必要があります。

---

## 監査用カラムの8つの要件

### 概要

監査追跡（Audit Trail）の完全性を保証するため、**すべてのテーブルは以下の8つのカラムを必須とします。**

```
┌─────────────────────────────────────────────────────────┐
│ テーブル（t_YourTable）                                   │
├─────────────────────────────────────────────────────────┤
│ ✓ row_id        │ システム基本ID（主キー）               │
│ ✓ row_version   │ 楽観ロック用タイムスタンプ             │
│ ✓ created_at    │ 作成日時（LocalDateTime/JST）         │
│ ✓ created_by    │ 作成者（従業員rowId）                 │
│ ✓ updated_at    │ 更新日時（LocalDateTime/JST）         │
│ ✓ updated_by    │ 更新者（従業員rowId）                 │
│ ✓ deleted_at    │ 削除日時（論理削除用）                │
│ ✓ deleted_by    │ 削除者（従業員rowId）                 │
├─────────────────────────────────────────────────────────┤
│ ビジネスカラム（テーブル固有）                           │
└─────────────────────────────────────────────────────────┘
```

### 各カラムの詳細仕様

#### 1. row_id（主キー）

```sql
[row_id] [bigint] NOT NULL PRIMARY KEY 
  DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence])
```

- **型**: bigint
- **制約**: PK, NOT NULL
- **採番方法**: プログラム側で ISequenceProvider により採番（ApplicationService で INSERT 前に確定）
  - **注記**: テーブルの DEFAULT で Sequence 設定があるが、実装ではプログラムが先に採番するため、実際には DEFAULT は使用されない
- **用途**: システム全体で一意に行を識別するID（マイナンバーのような基本ID）
- **範囲**: 1～9,223,372,036,854,775,807
- **注記**: DB技術的な識別子、ビジネスロジックでは使用しない

#### 2. row_version（楽観ロック）

```sql
[row_version] [timestamp] NOT NULL
```

- **型**: timestamp（自動更新）
- **制約**: NOT NULL
- **用途**: SQL Serverが自動管理するバージョンタイムスタンプ（楽観ロック用）
- **注記**: UPDATE時にSQL Serverが自動更新、手動入力不可

#### 3. created_at（作成日時）

```sql
[created_at] [datetime2](7) NOT NULL
```

- **型**: datetime2(7)（ミリ秒精度）
- **制約**: NOT NULL
- **用途**: レコード作成日時
- **タイムゾーン**: LocalDateTime（JST）
- **デフォルト**: 行を挿入するアプリケーションが設定
- **注記**: 作成後は変更不可（Updateで更新しない）

#### 4. created_by（作成者）

```sql
[created_by] [bigint] NOT NULL 
  CONSTRAINT [FK_tYourTable_CreatedBy] 
  FOREIGN KEY REFERENCES [m_persons]([row_id])
```

- **型**: bigint
- **制約**: NOT NULL, FK → m_persons(row_id)
- **用途**: レコードを作成した従業員のrowId
- **値範囲**: 1～9,223,372,036,854,775,807
- **デフォルト**: 認証コンテキストの ICurrentUserService から取得
- **注記**: 作成後は変更不可

#### 5. updated_at（更新日時）

```sql
[updated_at] [datetime2](7) NULL
```

- **型**: datetime2(7)
- **制約**: NULL許可（初期値はNULL、初回作成時は未設定）
- **用途**: レコード最終更新日時
- **タイムゾーン**: LocalDateTime（JST）
- **デフォルト**: NULL（作成時）
- **更新ルール**: UPDATE時にアプリケーションが設定

#### 6. updated_by（更新者）

```sql
[updated_by] [bigint] NULL 
  CONSTRAINT [FK_tYourTable_UpdatedBy] 
  FOREIGN KEY REFERENCES [m_persons]([row_id])
```

- **型**: bigint
- **制約**: NULL許可, FK → m_persons(row_id)
- **用途**: レコードを最後に更新した従業員のrowId
- **デフォルト**: NULL（作成時）
- **更新ルール**: UPDATE時に ICurrentUserService から取得

#### 7. deleted_at（削除日時 - 論理削除用）

```sql
[deleted_at] [datetime2](7) NULL
```

- **型**: datetime2(7)
- **制約**: NULL許可
- **用途**: 論理削除の日時（物理削除は行わない）
- **タイムゾーン**: LocalDateTime（JST）
- **デフォルト**: NULL（有効なレコード）
- **削除ルール**: DELETE時にNULLを設定、UPDATE時に削除日時を設定

#### 8. deleted_by（削除者）

```sql
[deleted_by] [bigint] NULL 
  CONSTRAINT [FK_tYourTable_DeletedBy] 
  FOREIGN KEY REFERENCES [m_persons]([row_id])
```

- **型**: bigint
- **制約**: NULL許可, FK → m_persons(row_id)
- **用途**: レコードを削除した従業員のrowId
- **デフォルト**: NULL（有効なレコード）
- **削除ルール**: DELETE時に ICurrentUserService から取得

---

## テーブル設計テンプレート

### 完全な CREATE TABLE テンプレート

```sql
-- Migration: XXX - Create t_YourTable
-- Purpose: Business purpose description
-- Date: YYYY-MM-DD

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 't_YourTable')
BEGIN
    CREATE TABLE [dbo].[t_YourTable]
    (
        -- 監査カラム（すべてのテーブルで必須）
        [row_id] [bigint] NOT NULL PRIMARY KEY 
          DEFAULT (NEXT VALUE FOR [dbo].[s_row_id_sequence]),
        [row_version] [timestamp] NOT NULL,
        [created_at] [datetime2](7) NOT NULL,
        [created_by] [bigint] NOT NULL,
        [updated_at] [datetime2](7) NULL,
        [updated_by] [bigint] NULL,
        [deleted_at] [datetime2](7) NULL,
        [deleted_by] [bigint] NULL,

        -- ビジネスカラム（テーブル固有）
        [your_business_id] [int] NOT NULL UNIQUE,
        [your_column] [nvarchar](100) NOT NULL,
        [your_amount] [decimal](10, 2) NULL,

        -- 制約
        CONSTRAINT [CK_tYourTable_YourColumn] 
          CHECK ([your_column] IS NOT NULL),
        CONSTRAINT [CK_tYourTable_YourAmount] 
          CHECK ([your_amount] IS NULL OR [your_amount] > 0)
    )
    ON [PRIMARY]
END
GO

-- 外部キー制約を別スクリプトで追加（後続の migration で実施）
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name = 'FK_tYourTable_CreatedBy'
)
BEGIN
    ALTER TABLE [dbo].[t_YourTable]
    ADD CONSTRAINT [FK_tYourTable_CreatedBy]
    FOREIGN KEY ([created_by]) REFERENCES [m_persons]([row_id])
END
GO
```

### インデックス戦略

```sql
-- 監査カラムのインデックス
CREATE INDEX [IX_tYourTable_DeletedAt] 
ON [dbo].[t_YourTable]([deleted_at])
WHERE [deleted_at] IS NULL;

-- ビジネスカラムのインデックス
CREATE INDEX [IX_tYourTable_YourBusinessId] 
ON [dbo].[t_YourTable]([your_business_id])
WHERE [deleted_at] IS NULL;
```

---

## チェックリスト

新規テーブル設計時に使用してください。

- [ ] **row_id**: プログラム側（ISequenceProvider）で採番されるPK（ApplicationService で INSERT 前に確定）
- [ ] **row_version**: timestamp で楽観ロック対応
- [ ] **created_at**: datetime2(7), NOT NULL, 作成時に設定
- [ ] **created_by**: bigint, NOT NULL, FK → m_persons
- [ ] **updated_at**: datetime2(7), NULL, 更新時に設定
- [ ] **updated_by**: bigint, NULL, FK → m_persons
- [ ] **deleted_at**: datetime2(7), NULL, 論理削除用
- [ ] **deleted_by**: bigint, NULL, FK → m_persons
- [ ] **タイムゾーン**: すべての datetime2 カラムが LocalDateTime（JST）
- [ ] **論理削除**: WHERE deleted_at IS NULL で有効行をフィルタ
- [ ] **マイグレーション**: 8つの監査カラムをすべて含める
- [ ] **Entity設計**: DbModel に LocalDateTime を使用
- [ ] **Mapper実装**: LocalDateTime を直接マッピング
- [ ] **Repository実装**: RepositoryBase を継承して createdBy を自動設定

---

## 実装例

- **t_UserPreferences** テーブル
  - ファイル: `src/Contexts/Samples/CarPreferences.Infrastructure/Migrations/002_CreateUserPreferencesTable.sql`
  - Entity: `src/Contexts/Samples/CarPreferences.Domain/Entities/UserPreferences.cs`
  - DbModel: `src/Contexts/Samples/CarPreferences.Infrastructure/DataAccess/Models/UserPreferencesDbModel.cs`
  - Repository: `src/Contexts/Samples/CarPreferences.Infrastructure/Repositories/UserPreferencesRepository.cs`

---

## FAQ

### Q: row_id と ビジネス識別子の使い分けは？

**A**: 
- **row_id**: システム内部の主キー（プログラム側で ISequenceProvider により採番、技術的識別子）
  - ApplicationService で INSERT 前に ISequenceProvider で採番値を取得して確定させる
- **ビジネス識別子**: ドメイン層で使用するID（GUID ベース集約ID）

Entity.Id は ビジネス識別子を使用し、DbModel の row_id はシステム技術的です。

### Q: 削除されたレコードをクエリするには？

**A**: 
```sql
-- 有効なレコード（削除されていない）
SELECT * FROM t_YourTable WHERE deleted_at IS NULL;

-- 削除されたレコード
SELECT * FROM t_YourTable WHERE deleted_at IS NOT NULL;
```

### Q: updated_at が NULL の場合の意味は？

**A**: 作成後に一度も更新されていないレコードです。created_at == 最終変更日時。

### Q: LocalDateTime と datetime2 の関係は？

**A**: 
- **LocalDateTime**: アプリケーション層の型（JST/タイムゾーン意識）
- **datetime2**: SQL Server の列型（タイムゾーン情報なし、JST として解釈）

Dapper と RepoDb のグローバルマッピング設定で LocalDateTime ↔ datetime2 の変換を自動化。

---

## 関連ドキュメント

- **CLAUDE.md**: プロジェクト全体のガイドライン
- **CLEAN_ARCHITECTURE_GUIDELINES.md**: アーキテクチャ原則
- **Database Design**: SQL Server DDL テンプレート
