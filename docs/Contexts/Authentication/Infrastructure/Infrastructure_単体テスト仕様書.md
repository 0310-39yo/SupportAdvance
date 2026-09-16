# Authentication Context Infrastructure層 - 単体テスト仕様書

**作成日:** 2026-09-16  
**対象読者:** テスト実装者  
**内容:** Mapper・Repository・Service のテストケース一覧

---

## 📋 概要

本書は、Authentication Context Infrastructure層の **単体テスト仕様** です。

各 Mapper・Repository・Service のテストケースを定義し、認証情報の永続化・暗号化処理を検証します。

---

## 🧪 テスト構成

### テストプロジェクト構造

```
tests/Contexts/Authentication.Infrastructure.Tests/
├── Authentication.Infrastructure.Tests.csproj
├── Mappers/
│   └── UserAuthSessionMapperTests.cs
├── Repositories/
│   └── UserAuthSessionRepositoryTests.cs
├── Services/
│   ├── PasswordHashServiceTests.cs
│   └── LoginCredentialsRepositoryTests.cs
└── Fixtures/
    └── InfrastructureFixture.cs
```

---

## 1. テスト観点一覧

### UserAuthSessionMapper

#### VO-MAP: マッピング（Entity ↔ DbModel）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-MAP-01 | Entity → DbModel で全フィールドが正しくマップされる | 正常系 | ✅ UserAuthSessionMapperTests.cs::VO_MAP_01_ToDbModel_WithValidEntity_MapsAllFields |
| VO-MAP-02 | ログアウト済み Entity で logout_at がマップされる | 正常系 | ✅ UserAuthSessionMapperTests.cs::VO_MAP_02_ToDbModel_WithLoggedOutSession_SetsLogoutAt |
| VO-MAP-03 | DbModel → Entity で全フィールドが正しくマップされる | 正常系 | ✅ UserAuthSessionMapperTests.cs::VO_MAP_03_ToDomainEntity_WithValidDbModel_MapsAllFields |

#### VO-TYPE: 型変換（DateTime ↔ LocalDateTime）

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-TYPE-01 | 日時フィールドの往復変換が一貫性を保つ | 正常系 | ✅ UserAuthSessionMapperTests.cs::VO_TYPE_01_DateTimeConversion_LocalDateTimeRoundTrip |

---

### UserAuthSessionRepository

#### VO-CRUD: CRUD 操作

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-CRUD-01 | SaveAsync で Entity が DB に保存される | 正常系 | ⏸️ Skip: 結合テスト化予定 |
| VO-CRUD-02 | GetByIdAsync で存在する ID から Entity が返される | 正常系 | ⏸️ Skip: 結合テスト化予定 |
| VO-CRUD-03 | GetLatestByLoginIdAsync で最新セッションが返される | 正常系 | ⏸️ Skip: 結合テスト化予定 |
| VO-CRUD-04 | UpdateAsync で Entity が DB で更新される | 正常系 | ⏸️ Skip: 結合テスト化予定 |
| VO-CRUD-05 | DeleteAsync で Entity が論理削除される | 正常系 | ⏸️ Skip: 結合テスト化予定 |

#### VO-AUDIT: 監査フィールド

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-AUDIT-01 | SaveAsync で created_at が自動設定される | 正常系 | ⏸️ Skip: 結合テスト化予定 |
| VO-AUDIT-02 | UpdateAsync で updated_at が更新される | 正常系 | ⏸️ Skip: 結合テスト化予定 |
| VO-AUDIT-03 | DeleteAsync で deleted_by が設定される | 正常系 | ⏸️ Skip: 結合テスト化予定 |

---

### PasswordHashService

#### VO-HASH: ハッシュ生成と検証

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-HASH-01 | 有効なパスワードでハッシュが生成される | 正常系 | ✅ PasswordHashServiceTests.cs::VO_HASH_01_Hash_WithValidPassword_GeneratesHash |
| VO-HASH-02 | 同じパスワードでも異なるハッシュが生成される（Salt差） | 正常系 | ✅ PasswordHashServiceTests.cs::VO_HASH_02_Hash_GeneratesDifferentHashesForSamePw |
| VO-HASH-03 | 同じ Salt で同じハッシュが生成される | 正常系 | ✅ PasswordHashServiceTests.cs::VO_HASH_03_Hash_GeneratesDeterministicHash_WithSameSalt |
| VO-HASH-04 | パスワード一致で Verify が true を返す | 正常系 | ✅ PasswordHashServiceTests.cs::VO_HASH_04_Verify_WithCorrectPassword_ReturnsTrue |
| VO-HASH-05 | パスワード不一致で Verify が false を返す | 異常系 | ✅ PasswordHashServiceTests.cs::VO_HASH_05_Verify_WithIncorrectPassword_ReturnsFalse |

#### VO-SECURITY: セキュリティ検証

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-SECURITY-01 | 空パスワードで ArgumentException が発生する | 異常系 | ✅ PasswordHashServiceTests.cs::VO_SECURITY_01_Hash_WithEmptyPassword_ThrowsException |
| VO-SECURITY-02 | null ハッシュで Verify が false を返す | 異常系 | ✅ PasswordHashServiceTests.cs::VO_SECURITY_02_Verify_WithNullHash_ReturnsFalse |
| VO-SECURITY-03 | 破損ハッシュで Verify が false を返す | 異常系 | ✅ PasswordHashServiceTests.cs::VO_SECURITY_03_Verify_WithCorruptedHash_ReturnsFalse |

---

### LoginCredentialsRepository

#### VO-CRUD: CRUD 操作

| 観点ID | 観点（説明） | 分類 | テスト実装 |
|--------|------|------|-----------|
| VO-CRUD-01 | SaveAsync で LoginCredentials が DB に保存される | 正常系 | ⏸️ Skip: 結合テスト化予定 |
| VO-CRUD-02 | GetByLoginIdAsync で LoginId から Credentials が返される | 正常系 | ⏸️ Skip: 結合テスト化予定 |
| VO-CRUD-03 | UpdateAsync で Credentials が DB で更新される | 正常系 | ⏸️ Skip: 結合テスト化予定 |
| VO-CRUD-04 | DeleteAsync で Credentials が論理削除される | 正常系 | ⏸️ Skip: 結合テスト化予定 |
| VO-CRUD-05 | 削除済み Credentials で null が返される | 正常系 | ⏸️ Skip: 結合テスト化予定 |

---

## 🧪 Mapper テストケース詳細

### 1. UserAuthSessionMapperTests

**ファイル:** `Mappers/UserAuthSessionMapperTests.cs`

**責務:** DbModel（DB保存型） ↔ UserAuthSession Entity（Domain型）の双方向変換

| # | テスト | 条件 | 検証 |
|---|--------|------|------|
| 1.1 | ToDbModel_WithValidEntity_MapsAllFields | 有効な Entity | 全フィールド マップ |
| 1.2 | ToDbModel_WithLoggedOutSession_SetsLogoutAt | ログアウト済み | logout_at フィールド |
| 1.3 | ToDomainEntity_WithValidDbModel_MapsAllFields | 有効な DbModel | 全フィールド マップ |
| 1.4 | DateTimeConversion_LocalDateTimeRoundTrip | 日時フィールド | 往復変換の一貫性 |

**テストケース数:** 4

---

## 🧪 Repository テストケース

### 2. UserAuthSessionRepositoryTests

**ファイル:** `Repositories/UserAuthSessionRepositoryTests.cs`

**責務:** UserAuthSession Entity の永続化（Save・Get・Update・Delete）

| # | テスト | 条件 | 検証 |
|---|--------|------|------|
| 2.1 | SaveAsync_WithValidEntity_PersistsToDb | 有効な Entity | DB に保存 |
| 2.2 | SaveAsync_SetsAuditFields_CreatedAt_CreatedBy | 監査フィールド | created_at 設定 |
| 2.3 | GetByIdAsync_WithValidId_ReturnsSession | 存在する ID | UserAuthSession返却 |
| 2.4 | GetByIdAsync_WithNonExistentId_ReturnsNull | 存在しない ID | null返却 |
| 2.5 | GetLatestByLoginIdAsync_WithValidLoginId_ReturnsSession | 有効なLoginId | 最新セッション返却 |
| 2.6 | GetLatestByLoginIdAsync_WithNonExistentLoginId_ReturnsNull | 存在しないLoginId | null返却 |
| 2.7 | UpdateAsync_WithValidEntity_UpdatesDb | 有効な Entity | DB 更新 |
| 2.8 | UpdateAsync_SetsAuditFields_UpdatedAt | 監査フィールド | updated_at 設定 |
| 2.9 | DeleteAsync_WithValidId_LogicallyDeletes | 存在する ID | deleted_at 設定 |
| 2.10 | DeleteAsync_SetsDeletedBy_FromClock | 監査フィールド | deleted_by 設定 |

**テストケース数:** 10

---

## 🧪 Service テストケース

### 3. PasswordHashServiceTests

**ファイル:** `Services/PasswordHashServiceTests.cs`

**責務:** パスワード暗号化（PBKDF2 + Salt）と検証

| # | テスト | 条件 | 検証 |
|---|--------|------|------|
| 3.1 | Hash_WithValidPassword_GeneratesHash | 有効なパスワード | ハッシュ値生成 |
| 3.2 | Hash_WithEmptyPassword_ThrowsException | 空パスワード | ArgumentException |
| 3.3 | Hash_GeneratesDifferentHashesForSamePw | 同じパスワード × 2回 | 異なるハッシュ（Salt差） |
| 3.4 | Hash_GeneratesDeterministicHash_WithSameSalt | 同じSalt | 同じハッシュ |
| 3.5 | Verify_WithCorrectPassword_ReturnsTrue | パスワード一致 | true返却 |
| 3.6 | Verify_WithIncorrectPassword_ReturnsFalse | パスワード不一致 | false返却 |
| 3.7 | Verify_WithNullHash_ReturnsFalse | nullハッシュ | false返却 |
| 3.8 | Verify_WithCorruptedHash_ReturnsFalse | 破損ハッシュ | false返却 |

**テストケース数:** 8

---

### 4. LoginCredentialsRepositoryTests

**ファイル:** `Repositories/LoginCredentialsRepositoryTests.cs`

**責務:** LoginCredentials（ログイン認証情報）の永続化・検索

| # | テスト | 条件 | 検証 |
|---|--------|------|------|
| 4.1 | SaveAsync_WithValidCredentials_PersistsToDb | 有効な認証情報 | DB保存 |
| 4.2 | GetByLoginIdAsync_WithValidId_ReturnsCredentials | 存在するLoginId | LoginCredentials返却 |
| 4.3 | GetByLoginIdAsync_WithNonExistentId_ReturnsNull | 存在しないLoginId | null返却 |
| 4.4 | GetByLoginIdAsync_WithDeletedCredentials_ReturnsNull | 削除済み認証情報 | null返却 |
| 4.5 | UpdateAsync_WithValidCredentials_UpdatesDb | 有効な認証情報 | DB更新 |

**テストケース数:** 5

---

## 📊 テストケース集計

| コンポーネント | テスト数 | 合計 |
|----------|---------|------|
| UserAuthSessionMapper | 4 | |
| UserAuthSessionRepository | 10 | |
| PasswordHashService | 8 | |
| LoginCredentialsRepository | 5 | |
| **合計** | | **27** |

---

## 📋 テスト実装チェックリスト

### Mapper テストファイル
- [ ] UserAuthSessionMapperTests (4テスト)

### Repository テストファイル
- [ ] UserAuthSessionRepositoryTests (10テスト)
- [ ] LoginCredentialsRepositoryTests (5テスト)

### Service テストファイル
- [ ] PasswordHashServiceTests (8テスト、セキュリティ重要）

### Fixture/Helper
- [ ] InfrastructureFixture.cs
- [ ] In-memory SQLite DB初期化

### テスト検証
- [ ] Red 状態確認（27 失敗）
- [ ] 実装実施
- [ ] Green 状態確認（27 成功）
- [ ] セキュリティテスト（パスワード処理）の確認

---

**ドキュメント作成完了** ✅
