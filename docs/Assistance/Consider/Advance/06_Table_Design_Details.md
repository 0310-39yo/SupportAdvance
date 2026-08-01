# 各テーブル詳細設計書

**バージョン:** 1.0  
**作成日:** 2026-07-19  
**対象:** Advance データベース  

---

## 📋 目次

1. [m_persons（人名マスタ）](#1-m_persons人名マスタ)
2. [m_employees（従業員マスタ）](#2-m_employees従業員マスタ)
3. [m_department（部署マスタ）](#3-m_department部署マスタ)
4. [m_employee_department（従業員所属）](#4-m_employee_department従業員所属)
5. [m_login_credentials（ログイン認証）](#5-m_login_credentialsログイン認証)

---

## 1. m_persons（人名マスタ）

### 1.1 テーブル概要

**役割:** システムに登録されるすべての人物情報を保持

**対象者:**
- 従業員（社員）
- 取引先担当者
- その他システムユーザー

**特徴:**
- 最上位の人物情報マスタ
- m_employees から 1:0..1 で参照
- 非従業員も登録可能

---

### 1.2 カラム詳細設計

| カラム名 | 型 | 制約 | 説明 | 例 |
|---|---|---|---|---|
| **row_id** | BIGINT | PK | シーケンス採番 | 2147483648 |
| **row_version** | TIMESTAMP | NOT NULL | 楽観的ロック用 | 自動更新 |
| **inserted_at** | DATETIME2(7) | NOT NULL | 作成日時 | 2026-07-19 10:00:00 |
| **inserted_by** | BIGINT | NOT NULL FK | 作成者 (m_employees.row_id) | 2147483649 |
| **updated_at** | DATETIME2(7) | NULL許容 | 更新日時（新規時は NULL） | 2026-07-19 14:30:00 |
| **updated_by** | BIGINT | NULL許容 FK | 更新者 (m_employees.row_id) | 2147483649 |
| **deleted_at** | DATETIME2(7) | NULL許容 | 削除日時（NULL=有効） | NULL |
| **deleted_by** | BIGINT | NULL許容 FK | 削除者 (m_employees.row_id) | NULL |
| **last_name** | NVARCHAR(50) | NOT NULL | 苗字 | 田中 |
| **first_name** | NVARCHAR(50) | NOT NULL | 名前 | 太郎 |
| **last_name_kana** | NVARCHAR(50) | NOT NULL | 苗字カタカナ | タナカ |
| **first_name_kana** | NVARCHAR(50) | NOT NULL | 名前カタカナ | タロウ |

---

### 1.3 インデックス設計

| インデックス名 | 種類 | カラム | 用途 |
|---|---|---|---|
| **PK_m_persons_row_id** | CLUSTERED | row_id | 主キー |
| **IX_m_persons_deleted_at** | NONCLUSTERED | deleted_at | 有効レコード検索 |
| **IX_m_persons_inserted_at_deleted_at** | NONCLUSTERED | inserted_at, deleted_at | 期間検索 + 有効フィルタ |

---

### 1.4 使用例

#### 例1: 新規従業員の人名情報を登録

```sql
INSERT INTO m_persons 
  (inserted_at, inserted_by, last_name, first_name, last_name_kana, first_name_kana)
VALUES 
  (GETDATE(), @CurrentUserRowId, '新員', '太郎', 'シンイン', 'タロウ');

-- 取得した row_id を m_employees に登録する
SELECT @@IDENTITY AS PersonRowId;
```

#### 例2: 従業員の名前を更新

```sql
UPDATE m_persons
SET 
  first_name = '太郎改',
  first_name_kana = 'タロウカイ',
  updated_at = GETDATE(),
  updated_by = @CurrentUserRowId
WHERE row_id = @PersonRowId;
```

#### 例3: 有効な従業員の人数を集計

```sql
SELECT COUNT(*) AS ActivePersonCount
FROM m_persons
WHERE deleted_at IS NULL;
```

---

### 1.5 注意事項

- **deleted_at:** NULL = 有効な人物、NULL以外 = 論理削除
- **updated_at:** NULL = 新規レコード（未更新）、値 = 更新済み
- **foreign key:** inserted_by, updated_by, deleted_by は m_employees.row_id を参照（可能な限り System 管理者は必須）

---

## 2. m_employees（従業員マスタ）

### 2.1 テーブル概要

**役割:** 従業員（社員）の情報を管理

**特徴:**
- m_persons と 1:0..1 の関係（person_row_id NOT NULL）
- employee_code は UNIQUE
- 複数部門所属を m_employee_department で管理
- 退職日（retire_on）でステータス管理

---

### 2.2 カラム詳細設計

| カラム名 | 型 | 制約 | 説明 | 例 |
|---|---|---|---|---|
| **row_id** | BIGINT | PK | シーケンス採番 | 2147483649 |
| **row_version** | TIMESTAMP | NOT NULL | 楽観的ロック用 | 自動更新 |
| **inserted_at** | DATETIME2(7) | NOT NULL | 作成日時 | 2026-01-15 09:00:00 |
| **inserted_by** | BIGINT | NOT NULL FK | 作成者 (self-reference) | 2147483649 |
| **updated_at** | DATETIME2(7) | NULL許容 | 更新日時（新規時は NULL） | NULL |
| **updated_by** | BIGINT | NULL許容 FK | 更新者 (self-reference) | NULL |
| **deleted_at** | DATETIME2(7) | NULL許容 | 削除日時（NULL=有効） | NULL |
| **deleted_by** | BIGINT | NULL許容 FK | 削除者 (self-reference) | NULL |
| **person_row_id** | BIGINT | NOT NULL FK | 人名 (m_persons.row_id) | 2147483648 |
| **employee_code** | INT | NOT NULL UNIQUE | 従業員コード（1000=System） | 1001 |
| **retire_on** | DATETIME2(7) | NULL許容 | 退職日（NULL=在職中） | NULL or 2026-06-30 |

---

### 2.3 インデックス設計

| インデックス名 | 種類 | カラム | 条件 | 用途 |
|---|---|---|---|---|
| **PK_m_employees_row_id** | CLUSTERED | row_id | - | 主キー |
| **UX_m_employees_employee_code** | UNIQUE | employee_code | deleted_at IS NULL | 従業員コード一意性 |
| **IX_m_employees_employee_code_deleted_at** | NONCLUSTERED | employee_code, deleted_at | - | Windows ログイン検索 |
| **IX_m_employees_deleted_at** | NONCLUSTERED | deleted_at | - | 有効レコード検索 |

---

### 2.4 使用例

#### 例1: 従業員を登録（採用）

```sql
-- Step 1: m_persons に人名を登録（別途実施）
-- Step 2: m_employees に従業員情報を登録

INSERT INTO m_employees
  (inserted_at, inserted_by, person_row_id, employee_code, retire_on)
VALUES
  (GETDATE(), @SystemAdminRowId, @PersonRowId, 1001, NULL);
```

#### 例2: 従業員コードで検索（Windows ログイン）

```sql
-- Windows ログイン M1001 から employee_code 1001 を抽出して検索
SELECT e.*, p.last_name, p.first_name
FROM m_employees e
INNER JOIN m_persons p ON e.person_row_id = p.row_id
WHERE e.employee_code = 1001 
  AND e.deleted_at IS NULL;
```

#### 例3: 退職者を除外した従業員一覧

```sql
SELECT e.*, p.first_name, p.last_name
FROM m_employees e
INNER JOIN m_persons p ON e.person_row_id = p.row_id
WHERE e.deleted_at IS NULL
  AND e.retire_on IS NULL;  -- 在職中のみ
```

#### 例4: 従業員を退職させる

```sql
UPDATE m_employees
SET 
  retire_on = GETDATE(),
  updated_at = GETDATE(),
  updated_by = @CurrentUserRowId
WHERE row_id = @EmployeeRowId;
```

---

### 2.5 注意事項

- **employee_code:** 1000 は System 管理者用に予約
- **retire_on:** NULL = 在職中、日付 = 退職済み
- **person_row_id:** NOT NULL なので必ず m_persons を参照
- **self-reference:** inserted_by, updated_by, deleted_by は自身の m_employees.row_id を参照

---

## 3. m_department（部署マスタ）

### 3.1 テーブル概要

**役割:** 組織の部署階層を管理

**特徴:**
- **階層構造:** hierarchy_level 0（トップ）～ 4（係・チーム）
- **self-join:** parent_department_row_id で親部署参照
- **部長管理:** manager_employee_row_id で責任者を指定
- **廃止管理:** abolished_on で廃止日を記録

---

### 3.2 カラム詳細設計

| カラム名 | 型 | 制約 | 説明 | 例 |
|---|---|---|---|---|
| **row_id** | BIGINT | PK | シーケンス採番 | 2147483650 |
| **row_version** | TIMESTAMP | NOT NULL | 楽観的ロック用 | 自動更新 |
| **inserted_at** | DATETIME2(7) | NOT NULL | 作成日時 | 2025-01-01 00:00:00 |
| **inserted_by** | BIGINT | NOT NULL FK | 作成者 (m_employees.row_id) | 2147483649 |
| **updated_at** | DATETIME2(7) | NULL許容 | 更新日時（新規時は NULL） | NULL |
| **updated_by** | BIGINT | NULL許容 FK | 更新者 (m_employees.row_id) | NULL |
| **deleted_at** | DATETIME2(7) | NULL許容 | 削除日時（NULL=有効） | NULL |
| **deleted_by** | BIGINT | NULL許容 FK | 削除者 (m_employees.row_id) | NULL |
| **department_code** | CHAR(4) | NOT NULL UNIQUE | 部署コード | 0001 |
| **department_name** | NVARCHAR(50) | NOT NULL | 部署名 | 営業部 |
| **manager_employee_row_id** | BIGINT | NULL許容 FK | 部長 (m_employees.row_id) | 2147483649 |
| **hierarchy_level** | INT | NOT NULL | 階層レベル（0=トップ～4=係） | 2 |
| **parent_department_row_id** | BIGINT | NULL許容 FK | 親部署 (self-join) | 2147483650 |
| **abolished_on** | DATETIME2(7) | NULL許容 | 廃止日（NULL=有効） | NULL |

---

### 3.3 階層レベル定義

| hierarchy_level | 名称 | parent_department_row_id | 説明 |
|---|---|---|---|
| **0** | トップ | NULL | 組織最上位 |
| **1** | 本部 | (level 0) | トップの配下 |
| **2** | 部 | (level 1) | 本部の配下 |
| **3** | グループ・課 | (level 2) | 部の配下 |
| **4** | 係・チーム | (level 3) | 課の配下 |

---

### 3.4 インデックス設計

| インデックス名 | 種類 | カラム | 条件 | 用途 |
|---|---|---|---|---|
| **PK_m_department_row_id** | CLUSTERED | row_id | - | 主キー |
| **UX_m_department_code** | UNIQUE | department_code | deleted_at IS NULL | 部署コード一意性 |
| **IX_m_department_hierarchy_level_deleted_at** | NONCLUSTERED | hierarchy_level, deleted_at | - | 階層別検索 |
| **IX_m_department_parent_department_row_id** | NONCLUSTERED | parent_department_row_id | - | 子部署取得 |

---

### 3.5 使用例

#### 例1: 部署の階層構造を取得（トップから第2階層まで）

```sql
SELECT 
  d1.department_name AS TopLevel,
  d2.department_name AS SecondLevel
FROM m_department d1
LEFT JOIN m_department d2 ON d1.row_id = d2.parent_department_row_id
WHERE d1.hierarchy_level = 0
  AND d1.deleted_at IS NULL
  AND (d2.deleted_at IS NULL OR d2.row_id IS NULL);
```

#### 例2: 特定部署の直下の部署を取得

```sql
SELECT *
FROM m_department
WHERE parent_department_row_id = @DepartmentRowId
  AND hierarchy_level = 3
  AND deleted_at IS NULL;
```

#### 例3: 部長名を含む部署情報を取得

```sql
SELECT 
  d.row_id,
  d.department_code,
  d.department_name,
  p.last_name AS ManagerLastName,
  p.first_name AS ManagerFirstName
FROM m_department d
LEFT JOIN m_employees e ON d.manager_employee_row_id = e.row_id
LEFT JOIN m_persons p ON e.person_row_id = p.row_id
WHERE d.deleted_at IS NULL
  AND d.abolished_on IS NULL;  -- 廃止済みを除外
```

#### 例4: 部署を廃止する

```sql
UPDATE m_department
SET 
  abolished_on = GETDATE(),
  updated_at = GETDATE(),
  updated_by = @CurrentUserRowId
WHERE row_id = @DepartmentRowId;

-- 配属者も終了
UPDATE m_employee_department
SET end_on = GETDATE()
WHERE department_row_id = @DepartmentRowId
  AND deleted_at IS NULL;
```

---

### 3.6 注意事項

- **abolished_on vs deleted_at:**
  - abolished_on：部署の公式廃止日（ビジネス上の廃止）
  - deleted_at：レコードの論理削除日（管理上の削除）
  - 廃止済みでも履歴保持の場合は abolished_on ≠ NULL, deleted_at IS NULL
  
- **hierarchy_level:** 手動で正確に指定する必要あり（自動計算ではない）

- **parent_department_row_id:** level 0（トップ）のみ NULL

---

## 4. m_employee_department（従業員所属）

### 4.1 テーブル概要

**役割:** 従業員と部署の関連を管理（複数所属対応）

**特徴:**
- 1人が複数部門に所属可能
- 主所属（is_primary）を明確化
- 所属期間（inserted_at ～ end_on）を記録

---

### 4.2 カラム詳細設計

| カラム名 | 型 | 制約 | 説明 | 例 |
|---|---|---|---|---|
| **row_id** | BIGINT | PK | シーケンス採番 | 2147483651 |
| **row_version** | TIMESTAMP | NOT NULL | 楽観的ロック用 | 自動更新 |
| **inserted_at** | DATETIME2(7) | NOT NULL | 配属開始日時 | 2026-04-01 00:00:00 |
| **inserted_by** | BIGINT | NOT NULL FK | 配属実行者 (m_employees.row_id) | 2147483649 |
| **updated_at** | DATETIME2(7) | NULL許容 | 更新日時 | NULL |
| **updated_by** | BIGINT | NULL許容 FK | 更新者 (m_employees.row_id) | NULL |
| **deleted_at** | DATETIME2(7) | NULL許容 | 削除日時 | NULL |
| **deleted_by** | BIGINT | NULL許容 FK | 削除者 (m_employees.row_id) | NULL |
| **employee_row_id** | BIGINT | NOT NULL FK | 従業員 (m_employees.row_id) | 2147483649 |
| **department_row_id** | BIGINT | NOT NULL FK | 部署 (m_department.row_id) | 2147483650 |
| **is_primary** | BIT | NOT NULL | 主たる所属フラグ（1=主, 0=兼務） | 1 |
| **end_on** | DATETIME2(7) | NULL許容 | 配属終了日（NULL=現在） | NULL or 2026-06-30 |

---

### 4.3 インデックス設計

| インデックス名 | 種類 | カラム | 条件 | 用途 |
|---|---|---|---|---|
| **PK_m_employee_department_row_id** | CLUSTERED | row_id | - | 主キー |
| **IX_m_employee_department_employee_row_id_is_primary** | NONCLUSTERED | employee_row_id, is_primary | deleted_at IS NULL | 従業員の主所属検索 |
| **IX_m_employee_department_department_row_id_deleted_at** | NONCLUSTERED | department_row_id, deleted_at | - | 部署の従業員検索 |

---

### 4.4 使用例

#### 例1: 従業員の主所属部署を取得

```sql
SELECT d.*
FROM m_employee_department ed
INNER JOIN m_department d ON ed.department_row_id = d.row_id
WHERE ed.employee_row_id = @EmployeeRowId
  AND ed.is_primary = 1
  AND ed.deleted_at IS NULL
  AND ed.end_on IS NULL;  -- 現在の所属
```

#### 例2: 従業員のすべての所属部署を取得

```sql
SELECT 
  d.department_name,
  ed.inserted_at AS AssignmentStart,
  ed.end_on AS AssignmentEnd,
  CASE WHEN ed.is_primary = 1 THEN '主所属' ELSE '兼務' END AS Role
FROM m_employee_department ed
INNER JOIN m_department d ON ed.department_row_id = d.row_id
WHERE ed.employee_row_id = @EmployeeRowId
  AND ed.deleted_at IS NULL
ORDER BY ed.is_primary DESC, ed.inserted_at DESC;
```

#### 例3: 部署の従業員一覧を取得

```sql
SELECT e.*, p.first_name, p.last_name
FROM m_employee_department ed
INNER JOIN m_employees e ON ed.employee_row_id = e.row_id
INNER JOIN m_persons p ON e.person_row_id = p.row_id
WHERE ed.department_row_id = @DepartmentRowId
  AND ed.deleted_at IS NULL
  AND ed.end_on IS NULL  -- 現在の所属
  AND e.deleted_at IS NULL
  AND e.retire_on IS NULL;  -- 在職中
```

#### 例4: 従業員を部門異動させる

```sql
-- Step 1: 前の主所属を終了
UPDATE m_employee_department
SET end_on = GETDATE()
WHERE employee_row_id = @EmployeeRowId
  AND is_primary = 1
  AND deleted_at IS NULL;

-- Step 2: 新しい部署に配属（主所属）
INSERT INTO m_employee_department
  (inserted_at, inserted_by, employee_row_id, department_row_id, is_primary, end_on)
VALUES
  (GETDATE(), @CurrentUserRowId, @EmployeeRowId, @NewDepartmentRowId, 1, NULL);
```

#### 例5: 兼務を追加する

```sql
INSERT INTO m_employee_department
  (inserted_at, inserted_by, employee_row_id, department_row_id, is_primary, end_on)
VALUES
  (GETDATE(), @CurrentUserRowId, @EmployeeRowId, @AdditionalDepartmentRowId, 0, NULL);
```

---

### 4.6 注意事項

- **is_primary:** 同一従業員の同一時点では `is_primary = 1` は最多1件
- **end_on:** NULL = 現在配属中、日付 = 過去の配属
- **複数所属の有効性確認:**
  ```sql
  -- 従業員が複数部署に主所属でないこと
  SELECT employee_row_id, COUNT(*) AS PrimaryCount
  FROM m_employee_department
  WHERE deleted_at IS NULL AND end_on IS NULL AND is_primary = 1
  GROUP BY employee_row_id
  HAVING COUNT(*) > 1;  -- 1件以上あればエラー
  ```

---

## 5. m_login_credentials（ログイン認証）

### 5.1 テーブル概要

**役割:** ID/パスワード認証情報を管理

**特徴:**
- m_employees と 1:N（複数の認証方法を持つ可能性）
- login_id は UNIQUE
- is_active で認証の有効/無効を制御
- last_login_at でログイン履歴を記録

**認証方式:**
- Windows ログイン（別途管理、m_employees.employee_code で検索）
- ID/パスワード認証（本テーブルで管理）

---

### 5.2 カラム詳細設計

| カラム名 | 型 | 制約 | 説明 | 例 |
|---|---|---|---|---|
| **row_id** | BIGINT | PK | シーケンス採番 | 2147483652 |
| **row_version** | TIMESTAMP | NOT NULL | 楽観的ロック用 | 自動更新 |
| **inserted_at** | DATETIME2(7) | NOT NULL | 作成日時 | 2026-04-01 09:00:00 |
| **inserted_by** | BIGINT | NOT NULL FK | 作成者 (m_employees.row_id) | 2147483649 |
| **updated_at** | DATETIME2(7) | NULL許容 | 更新日時 | NULL |
| **updated_by** | BIGINT | NULL許容 FK | 更新者 (m_employees.row_id) | NULL |
| **deleted_at** | DATETIME2(7) | NULL許容 | 削除日時 | NULL |
| **deleted_by** | BIGINT | NULL許容 FK | 削除者 (m_employees.row_id) | NULL |
| **employee_row_id** | BIGINT | NOT NULL FK | 従業員 (m_employees.row_id) | 2147483649 |
| **login_id** | NVARCHAR(50) | NOT NULL UNIQUE | ログインID | tanaka_taro |
| **password_hash** | NVARCHAR(255) | NOT NULL | パスワードハッシュ（bcrypt） | $2b$12$... |
| **is_active** | BIT | NOT NULL | ログイン有効フラグ（1=有効） | 1 |
| **last_login_at** | DATETIME2(7) | NULL許容 | 最終ログイン日時 | 2026-07-19 09:30:00 |

---

### 5.3 インデックス設計

| インデックス名 | 種類 | カラム | 条件 | 用途 |
|---|---|---|---|---|
| **PK_m_login_credentials_row_id** | CLUSTERED | row_id | - | 主キー |
| **UX_m_login_credentials_login_id** | UNIQUE | login_id | deleted_at IS NULL | ログインID一意性 |
| **IX_m_login_credentials_employee_row_id_is_active** | NONCLUSTERED | employee_row_id, is_active | deleted_at IS NULL | 有効な認証検索 |

---

### 5.4 使用例

#### 例1: ID/パスワード認証

```sql
-- ログイン処理
DECLARE @LoginId NVARCHAR(50) = 'tanaka_taro';
DECLARE @InputPassword NVARCHAR(255) = @UserInputPassword;

SELECT 
  lc.row_id,
  lc.password_hash,
  lc.employee_row_id,
  e.employee_code,
  p.first_name,
  p.last_name
FROM m_login_credentials lc
INNER JOIN m_employees e ON lc.employee_row_id = e.row_id
INNER JOIN m_persons p ON e.person_row_id = p.row_id
WHERE lc.login_id = @LoginId
  AND lc.is_active = 1
  AND lc.deleted_at IS NULL
  AND e.deleted_at IS NULL
  AND e.retire_on IS NULL;

-- アプリケーション側で password_hash と @InputPassword を bcrypt で比較
-- 認証成功時は last_login_at を更新

UPDATE m_login_credentials
SET last_login_at = GETDATE()
WHERE login_id = @LoginId;
```

#### 例2: 従業員のログイン情報を作成

```sql
INSERT INTO m_login_credentials
  (inserted_at, inserted_by, employee_row_id, login_id, password_hash, is_active, last_login_at)
VALUES
  (GETDATE(), @CurrentUserRowId, @EmployeeRowId, 'new_user_id', @HashedPassword, 1, NULL);
```

#### 例3: パスワードをリセット

```sql
UPDATE m_login_credentials
SET 
  password_hash = @NewHashedPassword,
  updated_at = GETDATE(),
  updated_by = @CurrentUserRowId
WHERE employee_row_id = @EmployeeRowId
  AND deleted_at IS NULL;
```

#### 例4: ログイン認証を無効化（退職時など）

```sql
UPDATE m_login_credentials
SET is_active = 0
WHERE employee_row_id = @EmployeeRowId
  AND deleted_at IS NULL;
```

#### 例5: ログイン可能な従業員を検証

```sql
-- ログイン可能な従業員のみ取得
SELECT 
  e.row_id,
  e.employee_code,
  p.first_name,
  p.last_name,
  lc.login_id
FROM m_employees e
INNER JOIN m_persons p ON e.person_row_id = p.row_id
INNER JOIN m_login_credentials lc ON e.row_id = lc.employee_row_id
WHERE e.deleted_at IS NULL       -- 有効な従業員
  AND e.retire_on IS NULL        -- 退職していない
  AND lc.is_active = 1           -- ログイン有効
  AND lc.deleted_at IS NULL      -- 認証情報有効
ORDER BY e.employee_code;
```

---

### 5.5 注意事項

- **password_hash:** bcrypt など暗号化ハッシュを使用（平文保存厳禁）
- **login_id:** UNIQUE 制約で重複を防止
- **is_active:** ログイン禁止時は 0 に設定（論理削除ではなく）
- **last_login_at:** ログイン成功時に更新
- **セキュリティ:** パスワードリセット時は新しいハッシュを生成

---

## 📊 テーブル間の関連サマリー

```
m_persons (人名)
    │
    ├── FK: person_row_id (1:0..1)
    │
m_employees (従業員)
    │
    ├── FK: person_row_id (N:1) → m_persons
    ├── self-reference: inserted_by, updated_by, deleted_by
    │
    ├── 1:N ─┬─→ m_employee_department
    │        │
    │        └─→ N:1 → m_department (階層構造)
    │
    └── 1:N ─→ m_login_credentials (ログイン認証)
```

---

## ✅ 実装チェックリスト

- [ ] 各テーブルのカラム定義が実装されているか
- [ ] インデックスが適切に作成されているか
- [ ] 外部キー制約が設定されているか
- [ ] 監査情報（inserted_at, updated_at, deleted_at）の運用方法が明確か
- [ ] Windows ログインと ID/パスワード認証の両方に対応しているか
- [ ] 複数部署所属の管理方法が確認されているか
- [ ] 退職者や廃止部署の扱いが明確か

---

## 📚 参照資料

- [関係仕様書](05_Entity_Relationships_Specification.md) - 人・部署・ログインの関係
- [ER図](01_ER_Diagram.html) - テーブル関連図
- [CREATE TABLE](03_CREATE_TABLE_SQLServer_Fixed.sql) - 実装済みスクリプト
