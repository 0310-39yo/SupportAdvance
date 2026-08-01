# インデックス戦略 - Advance データベース

**バージョン:** 1.0  
**作成日:** 2026-07-19  
**前提条件:** 数千件規模（2000件以下）、リアルタイムクエリが多い  

---

## 📋 インデックス戦略の基本方針

### 設計方針
- **数千件規模** のため、過度なインデックスは避ける
- **リアルタイムクエリ** に最適化：頻繁な検索パターンに絞る
- **INSERT/UPDATE/DELETE のオーバーヘッド** を最小化
- **複合インデックス** で複数クエリをカバー

### 実行クエリパターン（Q1-1～5）
1. ✅ 有効なレコード取得（`deleted_at IS NULL`）
2. ✅ 従業員の主所属部署検索（`employee_row_id, is_primary`）
3. ✅ 部署の従業員検索（`department_row_id, deleted_at`）
4. ✅ 期間内レコード検索（`inserted_at, updated_at`）
5. ✅ ログイン認証（`login_id`）

---

## 🔑 テーブル別インデックス設計

### **m_persons（人名マスタ）**

| インデックス名 | 種類 | カラム | 用途 | 備考 |
|---|---|---|---|---|
| **PK_m_persons_row_id** | CLUSTERED | row_id | 主キー | システム自動作成 |
| **IX_m_persons_deleted_at** | NONCLUSTERED | deleted_at | 有効レコード検索 | 論理削除フィルタ |
| **IX_m_persons_inserted_at_deleted_at** | NONCLUSTERED | inserted_at, deleted_at | 期間検索 + 有効フィルタ | 複合インデックス |
| **IX_m_persons_inserted_by** | NONCLUSTERED | inserted_by | FK参照性能 | 作成者検索 |
| **IX_m_persons_updated_by** | NONCLUSTERED | updated_by | FK参照性能 | 更新者検索（NULL許容） |

**設計理由:**
- `deleted_at` は全クエリで使用（論理削除フィルタ）
- `inserted_at` との複合インデックスで期間検索を高速化
- FK参照（*_by）は参照頻度は低いが、外部キー制約のため作成

---

### **m_employees（従業員マスタ）**

| インデックス名 | 種類 | カラム | 用途 | 備考 |
|---|---|---|---|---|
| **PK_m_employees_row_id** | CLUSTERED | row_id | 主キー | システム自動作成 |
| **UX_m_employees_employee_code** | NONCLUSTERED UNIQUE | employee_code | 従業員コード一意性 | Windows ログイン検索 |
| **IX_m_employees_person_row_id** | NONCLUSTERED | person_row_id | FK参照性能 | m_persons 結合 |
| **IX_m_employees_deleted_at** | NONCLUSTERED | deleted_at | 有効レコード検索 | 論理削除フィルタ |
| **IX_m_employees_employee_code_deleted_at** | NONCLUSTERED | employee_code, deleted_at | Windows ログイン + 有効フィルタ | 複合インデックス（推奨） |

**設計理由:**
- `employee_code` は UNIQUE（Windows ログイン M + コード検索）
- `(employee_code, deleted_at)` 複合インデックスで Windows ログイン検索を最適化
- `person_row_id` は FK として参照性能向上

---

### **m_login_credentials（ログイン認証）**

| インデックス名 | 種類 | カラム | 用途 | 備考 |
|---|---|---|---|---|
| **PK_m_login_credentials_row_id** | CLUSTERED | row_id | 主キー | システム自動作成 |
| **UX_m_login_credentials_login_id** | NONCLUSTERED UNIQUE | login_id | ログインID一意性 | ログイン認証検索 |
| **IX_m_login_credentials_employee_row_id_is_active** | NONCLUSTERED | employee_row_id, is_active | 有効なログイン情報検索 | 複合インデックス |
| **IX_m_login_credentials_deleted_at** | NONCLUSTERED | deleted_at | 有効レコード検索 | 論理削除フィルタ |

**設計理由:**
- `login_id` は UNIQUE（ログイン認証の核）
- `(employee_row_id, is_active)` で有効な認証情報を高速検索
- 認証時に `deleted_at IS NULL` フィルタも必要なため、別途インデックス

---

### **m_department（部署マスタ）**

| インデックス名 | 種類 | カラム | 用途 | 備考 |
|---|---|---|---|---|
| **PK_m_department_row_id** | CLUSTERED | row_id | 主キー | システム自動作成 |
| **UX_m_department_code** | NONCLUSTERED UNIQUE | department_code | 部署コード一意性 | 部署検索 |
| **IX_m_department_parent_department_row_id** | NONCLUSTERED | parent_department_row_id | 階層検索 | 子部門取得、self-join |
| **IX_m_department_manager_employee_row_id** | NONCLUSTERED | manager_employee_row_id | 部長検索 | 部長が参照 |
| **IX_m_department_hierarchy_level_deleted_at** | NONCLUSTERED | hierarchy_level, deleted_at | 階層別・有効検索 | 複合インデックス |
| **IX_m_department_deleted_at** | NONCLUSTERED | deleted_at | 有効レコード検索 | 論理削除フィルタ |

**設計理由:**
- `parent_department_row_id` は階層構造の核（子部門検索に必須）
- `(hierarchy_level, deleted_at)` で階層別・有効部署を検索
- `manager_employee_row_id` は FK 参照性能向上

---

### **m_employee_department（従業員所属）**

| インデックス名 | 種類 | カラム | 用途 | 備考 |
|---|---|---|---|---|
| **PK_m_employee_department_row_id** | CLUSTERED | row_id | 主キー | システム自動作成 |
| **IX_m_employee_department_employee_row_id_is_primary** | NONCLUSTERED | employee_row_id, is_primary | 従業員の主所属検索 | Q1-2 クエリ最適化 |
| **IX_m_employee_department_department_row_id_deleted_at** | NONCLUSTERED | department_row_id, deleted_at | 部署の従業員検索 | Q1-3 クエリ最適化 |
| **IX_m_employee_department_deleted_at** | NONCLUSTERED | deleted_at | 有効レコード検索 | 論理削除フィルタ |
| **IX_m_employee_department_end_on** | NONCLUSTERED | end_on | 離脱日による検索 | 現在の所属検索 |

**設計理由:**
- `(employee_row_id, is_primary)` で従業員の主所属部署を高速検索（Q1-2）
- `(department_row_id, deleted_at)` で部署の従業員を有効フィルタ付きで検索（Q1-3）
- `end_on IS NULL` で現在の所属を検索（兼務管理）

---

## 📊 クエリパターン別インデックス対応

### **Q1-1: 有効なレコードを取得**
```sql
SELECT * FROM m_employees 
WHERE deleted_at IS NULL;
```
**対応インデックス:** `IX_m_employees_deleted_at`

---

### **Q1-2: 従業員の主所属部署を検索**
```sql
SELECT * FROM m_employee_department 
WHERE employee_row_id = @empId 
  AND is_primary = 1
  AND deleted_at IS NULL;
```
**対応インデックス:** `IX_m_employee_department_employee_row_id_is_primary`  
**補助インデックス:** `IX_m_employee_department_deleted_at`

---

### **Q1-3: 部署の従業員を検索**
```sql
SELECT e.* FROM m_employee_department ed
INNER JOIN m_employees e ON ed.employee_row_id = e.row_id
WHERE ed.department_row_id = @deptId 
  AND ed.deleted_at IS NULL
  AND e.deleted_at IS NULL;
```
**対応インデックス:** `IX_m_employee_department_department_row_id_deleted_at`

---

### **Q1-4: 期間内に更新されたレコード**
```sql
SELECT * FROM m_persons 
WHERE deleted_at IS NULL
  AND inserted_at BETWEEN @startDate AND @endDate;
```
**対応インデックス:** `IX_m_persons_inserted_at_deleted_at`

---

### **Q1-5: ログイン認証（ID/パスワード）**
```sql
SELECT e.* FROM m_login_credentials lc
INNER JOIN m_employees e ON lc.employee_row_id = e.row_id
WHERE lc.login_id = @loginId 
  AND lc.is_active = 1
  AND lc.deleted_at IS NULL;
```
**対応インデックス:** `UX_m_login_credentials_login_id`  
**補助インデックス:** `IX_m_login_credentials_employee_row_id_is_active`

---

### **Windows ログイン（M + 従業員コード）**
```sql
SELECT * FROM m_employees 
WHERE employee_code = @code 
  AND deleted_at IS NULL;
```
**対応インデックス:** `IX_m_employees_employee_code_deleted_at`

---

## 📈 インデックスサマリー

| テーブル | UNIQUE | NONCLUSTERED | 複合 | 合計 |
|---|---|---|---|---|
| **m_persons** | 0 | 2 | 1 | 3 |
| **m_employees** | 1 | 2 | 1 | 4 |
| **m_login_credentials** | 1 | 2 | 1 | 4 |
| **m_department** | 1 | 4 | 1 | 6 |
| **m_employee_department** | 0 | 4 | 2 | 6 |
| **合計** | 3 | 14 | 6 | 23 |

**設計評価:**
- ✅ 23個のインデックス（PK含む）：数千件規模に適切
- ✅ 複合インデックスで複数クエリをカバー
- ✅ 外部キー参照性能向上
- ✅ 論理削除フィルタの最適化

---

## 🔧 SQL Server インデックス定義例

### m_employees テーブル

```sql
-- UNIQUE インデックス
CREATE UNIQUE NONCLUSTERED INDEX UX_m_employees_employee_code
ON m_employees(employee_code)
WHERE deleted_at IS NULL;  -- フィルタインデックス（削除済みを除外）

-- 複合インデックス（Windows ログイン検索最適化）
CREATE NONCLUSTERED INDEX IX_m_employees_employee_code_deleted_at
ON m_employees(employee_code, deleted_at);

-- 単一インデックス（有効レコード検索）
CREATE NONCLUSTERED INDEX IX_m_employees_deleted_at
ON m_employees(deleted_at)
INCLUDE (employee_code, person_row_id);  -- INCLUDE句で追加カラムをカバー

-- FK参照
CREATE NONCLUSTERED INDEX IX_m_employees_person_row_id
ON m_employees(person_row_id);
```

### m_employee_department テーブル

```sql
-- Q1-2 最適化：従業員の主所属検索
CREATE NONCLUSTERED INDEX IX_m_employee_department_employee_row_id_is_primary
ON m_employee_department(employee_row_id, is_primary)
WHERE deleted_at IS NULL;  -- フィルタインデックス

-- Q1-3 最適化：部署の従業員検索
CREATE NONCLUSTERED INDEX IX_m_employee_department_department_row_id_deleted_at
ON m_employee_department(department_row_id, deleted_at)
INCLUDE (employee_row_id, is_primary);  -- 結合時のカバー

-- 有効レコード検索
CREATE NONCLUSTERED INDEX IX_m_employee_department_deleted_at
ON m_employee_department(deleted_at);
```

---

## ⚠️ 注意事項

### **フィルタインデックス（WHERE句）の活用**
SQL Server では、`WHERE deleted_at IS NULL` 条件でフィルタインデックスを作成できます。

```sql
-- 削除済みレコードは含めない UNIQUE インデックス
CREATE UNIQUE NONCLUSTERED INDEX UX_m_employees_employee_code
ON m_employees(employee_code)
WHERE deleted_at IS NULL;
```

**利点:**
- インデックスサイズが小さい
- INSERT/UPDATE/DELETE のオーバーヘッド最小化
- 有効レコードのみが対象

---

### **INCLUDE句の活用**
複합インデックスにカバー列（INCLUDE）を追加すると、インデックスのみで クエリ結果が得られる（インデックスシーク + キーレックアップなし）。

```sql
CREATE NONCLUSTERED INDEX IX_m_employees_deleted_at
ON m_employees(deleted_at)
INCLUDE (employee_code, person_row_id, retire_on);  -- カバー列
```

---

## 📝 マイグレーション手順

1. **インデックス作成順序** （外部キー参照の順番に注意）
   ```sql
   -- 1. m_persons のインデックス作成
   -- 2. m_employees のインデックス作成（person_row_id FK があるため）
   -- 3. m_department のインデックス作成（self-join）
   -- 4. m_employee_department のインデックス作成
   -- 5. m_login_credentials のインデックス作成
   ```

2. **段階的なデプロイ**
   - 開発環境で動作確認
   - テスト環境で性能測定
   - 本番環境へのデプロイ時は予定メンテナンスウィンドウで実施

3. **インデックスの監視**
   - 定期的な断片化率チェック：`ALTER INDEX ... REBUILD`
   - クエリプランの確認（実行計画でインデックス利用状況確認）
   - DMV（sys.dm_db_index_usage_stats）でインデックス使用状況を監視

---

## 📊 PostgreSQL への対応

PostgreSQL では SQL Server と異なるインデックス構文を使用します：

```sql
-- PostgreSQL のインデックス作成例
CREATE UNIQUE INDEX uix_m_employees_employee_code 
ON m_employees(employee_code) 
WHERE deleted_at IS NULL;

-- 複合インデックス（INCLUDE 相当は INCLUDE 句がないため、複合インデックスで対応）
CREATE INDEX ix_m_employee_department_employee_row_id_is_primary 
ON m_employee_department(employee_row_id, is_primary) 
WHERE deleted_at IS NULL;

-- パーティアルインデックス（WHERE 句）
CREATE INDEX ix_m_employees_deleted_at 
ON m_employees(deleted_at) 
WHERE deleted_at IS NULL;
```

---

## ✅ 次のステップ

1. ✅ インデックス戦略確定（本ドキュメント）
2. ⏳ CREATE TABLE 文の詳細作成（SQL Server 版）
3. ⏳ マイグレーションスクリプト作成
4. ⏳ PostgreSQL 移行ガイド

---

**このインデックス戦略でよろしいでしょうか？** 

修正・追加要望があればお知らせください。
