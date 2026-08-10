---
name: AggregateId設計変更_実装計画
description: 集約ID を GUID ベースの固有ValueObject に統一。複数テーブル集約対応、型安全性確保
metadata:
  type: 実装計画
---

# AggregateId 設計変更 - 実装計画

**目的**: 集約を一意識別する ID を `AggregateRoot<RowId>` から `AggregateRoot<XXXId>` パターンに全面移行

**背景**: 
- 複数テーブル集約の場合、「どのテーブルのRowId」が曖昧
- 型安全性がない（異なる集約のIDが混在可能）
- DUID ベースの EventId 導入に伴い、AggregateRootId を明確化する必要

**方針**:
- 各集約の ID は **GUID ベースの ValueObject** を別途定義
- 共通基底クラス `AggregateId` を作成して実装を共有
- RowId はテーブルの物理キーのみに限定

---

## 📋 実装フェーズ

### Phase 1: 基盤整備（ドキュメント・基底クラス実装）

**作業内容**:
1. `AggregateId_設計ガイド.md` を作成
2. SharedKernel に `AggregateId` 基底クラスを実装
3. 既存ドキュメント 6 ファイルを修正

**依存**: なし  
**期間**: 2-3 日（ドキュメント作成 1 日 + コード実装 1 日 + ドキュメント修正 1 日）

---

### Phase 2: Identity コンテキスト（小規模・単一テーブル）

**対象集約**:
- User（ユーザー）
- Role（ロール）
- UserRole（ユーザーロール関連）

**作業内容**:
1. `UserId` ValueObject を実装（基底 `AggregateId` を継承）
2. `RoleId` ValueObject を実装
3. `User : AggregateRoot<UserId>` に修正
4. `Role : AggregateRoot<RoleId>` に修正
5. Mapper で `UserId ↔ RowId` マッピング実装
6. Repository で `GetByIdAsync(UserId userId)` を追加
7. イベント定義を更新（AggregateRootId = UserId）

**修正ファイル数**: ~10-15 ファイル  
**優先度**: ⭐⭐⭐ 高（最初に実装すべき）  
**理由**: 
- 規模が小さい
- 単一テーブル集約のみ
- 実装パターンの確立に最適

---

### Phase 3: Employee コンテキスト（中規模・マスターデータ）

**対象集約**:
- Employee（従業員）
- Department（部門）

**作業内容**:
1. `EmployeeId` ValueObject を実装
2. `DepartmentId` ValueObject を実装
3. `Employee : AggregateRoot<EmployeeId>` に修正
4. `Department : AggregateRoot<DepartmentId>` に修正
5. 場合によっては `EmployeeNumber` を ビジネスID として追加
6. Mapper、Repository を修正

**修正ファイル数**: ~15-20 ファイル  
**優先度**: ⭐⭐⭐ 高（Phase 2 の後）  
**理由**:
- 規模は中程度
- Domain イベント実装の確認が必要
- Phase 2 で確立したパターンの適用練習

---

### Phase 4: CarPreferences コンテキスト（複数テーブル集約）

**対象集約**:
- UserPreferences（ユーザー選好）← **複数テーブル**

**作業内容**:
1. `UserPreferencesId` ValueObject を実装
2. `UserPreferences : AggregateRoot<UserPreferencesId>` に修正
3. **複数テーブル対応**：
   - `t_user_preferences` : RowId（物理キー）
   - `t_user_preference_details` : RowId（物理キー）
   - 集約ID : UserPreferencesId（GUID）
4. Mapper で複数テーブル ↔ UserPreferencesId マッピング
5. Repository で `GetByIdAsync(UserPreferencesId id)` を複数テーブルJOIN で実装

**修正ファイル数**: ~10-15 ファイル  
**優先度**: ⭐⭐ 中（Phase 2, 3 の後）  
**理由**:
- 複数テーブル集約は複雑
- Phase 2, 3 で基本パターンを確立後に
- 複数テーブル対応の「ベストプラクティス」になる

---

## 🔄 実装順序（推奨）

```
Phase 1: 基盤整備
    ↓
Phase 2: Identity コンテキスト（最初）
    ↓
Phase 3: Employee コンテキスト
    ↓
Phase 4: CarPreferences コンテキスト（複数テーブル対応確認）
```

**理由**:
1. 小規模 → 中規模 → 複雑 の段階的実装
2. Phase 2 でパターン確立 → Phase 3, 4 で応用
3. 最後の Phase 4 で複数テーブル集約の処理パターンを確立

---

## 📊 影響範囲サマリー

| コンテキスト | 集約数 | テーブル数 | 修正難度 | 期間 |
|------------|--------|-----------|--------|------|
| Identity | 3 | 3 | 低 | 1-2日 |
| Employee | 2 | 2 | 低 | 1-2日 |
| CarPreferences | 1 | 2 | 中 | 1-2日 |
| **合計** | **6** | **7** | — | **3-6日** |

---

## ⚠️ リスク・注意点

### 1. イベント定義の整合性
- 既存イベントの `AggregateRootId` 型を修正（long → XXXId ValueObject）
- 影響: すべてのイベント定義・ハンドラーの修正が必要

**対応**: Phase 1 のドキュメント作成時に、イベント仕様を明確化

### 2. Repository クエリの変更
- 現在: `GetByRowIdAsync(long rowId)`
- 変更後: `GetByIdAsync(XXXId id)`

**対応**: 段階的に古いメソッドを削除

### 3. Mapper の複雑性
- ビジネスID（GUID）とテーブルRowId（long）の対応を明示

**対応**: Mapper_パターンガイド.md で詳細例を示す

### 4. テストの修正
- Entity テスト、Repository テスト、Mapper テストの修正

**対応**: テスト用ファクトリを更新

---

## 📝 各 Phase のチェックリスト

### Phase 1

- [ ] `AggregateId_設計ガイド.md` 作成
- [ ] SharedKernel に `AggregateId` 基底クラス実装
- [ ] 既存ドキュメント 6 ファイル修正
- [ ] 修正内容を CLAUDE.md に反映
- [ ] ビルド確認（エラーなし）

### Phase 2 (Identity)

- [ ] `UserId` ValueObject 実装
- [ ] `RoleId` ValueObject 実装
- [ ] Entity 修正（User, Role）
- [ ] Mapper 修正
- [ ] Repository 修正
- [ ] イベント定義 修正
- [ ] ビルド確認
- [ ] テスト実行
- [ ] コード変更承認取得

### Phase 3 (Employee)

- [ ] Phase 2 と同様の作業

### Phase 4 (CarPreferences)

- [ ] Phase 2, 3 と同様 + 複数テーブルJOIN対応確認

---

## 🔗 関連ドキュメント

作成予定:
- `AggregateId_設計ガイド.md` （Phase 1）

修正予定:
- `Entity_設計ガイドライン.md`
- `ドメインイベント_設計ガイド.md`
- `Entity_And_DomainEvents_技術仕様.md`
- `Entity_And_DomainEvents_詳細設計.md`
- `Mapper_パターンガイド.md`
- `Repository_パターンガイド.md`
- `CLAUDE.md`

---

## 📅 スケジュール（概算）

- **Day 1**: Phase 1 実施（ドキュメント + 基底クラス実装）
- **Day 2**: Phase 2 実施（Identity）
- **Day 3**: Phase 3 実施（Employee）
- **Day 4-5**: Phase 4 実施（CarPreferences）+ テスト・確認

**合計**: 4-5 営業日

