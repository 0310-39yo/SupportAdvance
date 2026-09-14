# ValueObject_設計ガイド 齟齬修正計画

**版:** 1.0  
**作成日:** 2026-09-12  
**対象:** ValueObject_設計ガイド.md（Assistance/Guides および SharedKernel/ValueObjects）  
**ステータス:** 計画段階

---

## 1. 概要

### 1.1 問題の背景

ValueObject の設計ガイドと実装コードの間に以下の齟齬が存在：
- **実装があるが文書化されていない** パターン（3件、HIGH優先度）
- **文書化があるが実装がない** パターン（1件、HIGH優先度）
- **実装がドキュメントと異なる** パターン（3件、MEDIUM優先度）

### 1.2 修正の目的

ガイドと実装を完全に一致させることで：
- 開発者が正確な実装パターンをガイドから学べる
- 新規 ValueObject 実装時の参考になる
- ドキュメント信頼性の向上

---

## 2. 修正対象一覧

### HIGH 優先度（必須）

#### 2.1 FromDbValue / ToDbValue パターン の文書化

**現状：**
- ✅ 実装完了（CreatedAt, UpdatedAt, DeletedAt, EffectiveAt, EndOn, RetiredOn）
- ❌ ガイドに記載なし

**修正内容：**
- `docs/Assistance/Guides/ValueObject_設計ガイド.md` に新セクション追加
  - セクション名：「9.X DateTime ↔ LocalDateTime マッピング（FromDbValue/ToDbValue）」
  - 内容：
    - FromDbValue(DateTime) の用途と実装
    - ToDbValue() の用途と実装
    - TryFromDbValue(DateTime?) パターン（DB null → Unset()）
    - 実装例：CreatedAt, UpdatedAt, DeletedAt
    - Repository での使用例

**対象ドキュメント：**
- `docs/Assistance/Guides/ValueObject_設計ガイド.md`
- `docs/SharedKernel/ValueObjects/ValueObject_設計ガイド.md`（同期）

**見積：** 1-2時間

---

#### 2.2 IValidateWithClock パターン の文書化

**現状：**
- ✅ 実装完了（IValidateWithClock<TValue>, IOptionalValidateWithClock<TSelf,TValue>）
- ❌ ガイドに記載なし（インターフェースは完全設計済み）

**修正内容：**
- `docs/Assistance/Guides/ValueObject_設計ガイド.md` に新セクション追加
  - セクション名：「9.Y Clock ベースの検証パターン（IValidateWithClock）」
  - 内容：
    - IValidateWithClock<TValue> インターフェース説明
    - IOptionalValidateWithClock<TSelf, TValue> インターフェース説明
    - 用途：現在日時に基づく検証（例：有効期限チェック）
    - From(TValue, IClock) / TryFrom(TValue?, IClock, out TSelf) シグネチャ
    - 実装例（EffectiveAt, EndOn など）
    - Repository での IClock 注入パターン

**対象ドキュメント：**
- `docs/Assistance/Guides/ValueObject_設計ガイド.md`
- `docs/SharedKernel/ValueObjects/ValueObject_設計ガイド.md`（同期）

**見積：** 1-2時間

---

#### 2.3 AggregateId の削除 OR 実装 判定

**現状：**
- ✅ ガイドに詳細設計（Section 2.3, 11.2）
- ❌ コードベースに実装なし
- ❌ 使用している ValueObject なし

**選択肢：**

**A. ドキュメント削除（推奨）**
- 理由：3年以上実装されず、使用例もない
- 処理：ガイドから Section 2.3, 11.2 の AggregateId を廃版フォルダに移動
- 見積：30分
- 結果：ガイドが簡潔に

**B. コード実装**
- 理由：設計は完全で、今後のビジネス ID ValueObject に使える
- 処理：
  1. AggregateId 抽象クラスを作成
  2. EmployeeRowId, PersonRowId など既存クラスで実装確認
- 見積：3-4時間
- 結果：次の Context で再利用可能

**推奨：A（削除）** ← ユーザー判断が必要

---

### MEDIUM 優先度（推奨）

#### 2.4 RowId ドキュメント修正

**現状：**
- ドキュメント：直接 ValueObject 継承、private _value フィール
- 実装：PrimitiveValueObject<long> 継承、protected コンストラクタで IsSet 対応

**修正内容：**
- `docs/Assistance/Guides/ValueObject_設計ガイド.md` Section 5.1b 更新
  - 実装パターンを PrimitiveValueObject<long> に修正
  - IsSet フラグが optional variant で使える説明追加
  - 実装例：PersonRowId, EmployeeRowId

**対象ドキュメント：**
- `docs/Assistance/Guides/ValueObject_設計ガイド.md`
- `docs/SharedKernel/ValueObjects/ValueObject_設計ガイド.md`（同期）

**見積：** 30分

---

#### 2.5 状態別名（HasRetired, HasEnded）の文書化

**現状：**
- ✅ 実装完了（Employee Domain ValueObjects）
- ✅ HasUpdated, IsDeleted は文書化
- ❌ HasRetired, HasEnded は文書化なし

**修正内容：**
- `docs/Assistance/Guides/ValueObject_設計ガイド.md` の IsSet 別名セクション拡張
  - 既存：HasUpdated (UpdatedAt), IsDeleted (DeletedAt)
  - 追加：HasRetired (RetiredOn), HasEnded (EndOn)
  - 命名規則：Has{動詞過去分詞}, Is{形容詞}

**対象ドキュメント：**
- `docs/Assistance/Guides/ValueObject_設計ガイド.md`

**見積：** 15分

---

### LOW 優先度（オプション）

#### 2.6 RetiredOn GetValueComponents 修正

**現状：**
- ドキュメント：「IsSet を GetValueComponents に含めない」
- 実装：RetiredOn は IsSet を yield している

**修正内容（2択）：**

**A. コード修正（簡潔）**
- RetiredOn.GetValueComponents から IsSet yield を削除
- 見積：15分
- 理由：base class が IsSet を handle する

**B. ドキュメント修正**
- GetValueComponents パターンが複数あることを説明
- 見積：30分
- 理由：実装の柔軟性を反映

**推奨：A（コード修正）**

---

#### 2.7 RetiredOn / EndOn / EffectiveAt パターン明確化

**現状：**
- ドキュメント：すべて PrimitiveValueObject<LocalDateTime?> パターン推奨
- 実装：直接 ValueObject 継承（Normalize/Validate 不要）

**修正内容：**
- ガイドに「パターン選択基準」セクション追加
  - PrimitiveValueObject を使う：Normalize/Validate が必要な場合
  - 直接 ValueObject を使う：Normalize/Validate が不要な場合
  - Employee Domain の例：RetiredOn, EndOn, EffectiveAt

**対象ドキュメント：**
- `docs/Assistance/Guides/ValueObject_設計ガイド.md`

**見積：** 30分

---

## 3. 実施スケジュール

### フェーズ 1：HIGH 優先度（必須）

| 項番 | タスク | 担当 | 見積 | 期限 |
|-----|--------|-----|------|------|
| 2.1 | FromDbValue/ToDbValue 文書化 | Claude | 1-2h | 本セッション |
| 2.2 | IValidateWithClock 文書化 | Claude | 1-2h | 本セッション |
| 2.3 | AggregateId 判定 | ユーザー | 判断 | 本セッション |

### フェーズ 2：MEDIUM 優先度（推奨）

| 項番 | タスク | 担当 | 見積 | 期限 |
|-----|--------|-----|------|------|
| 2.4 | RowId ドキュメント修正 | Claude | 30min | 本セッション |
| 2.5 | 別名（HasRetired等）文書化 | Claude | 15min | 本セッション |

### フェーズ 3：LOW 優先度（オプション）

| 項番 | タスク | 担当 | 見積 | 期限 |
|-----|--------|-----|------|------|
| 2.6 | RetiredOn 修正 | Claude | 15min | 次セッション |
| 2.7 | パターン選択基準 説明 | Claude | 30min | 次セッション |

---

## 4. 実装方法

### 4.1 ドキュメント修正ツール

- ファイル編集：Edit tool
- ファイル作成：Write tool
- 廃版処理：廃版処理手順書に従う

### 4.2 同期ルール

修正箇所：
- `docs/Assistance/Guides/ValueObject_設計ガイド.md` （メイン）
- `docs/SharedKernel/ValueObjects/ValueObject_設計ガイド.md` （同期）

両ファイルを常に同期（内容の重複は許容）

### 4.3 コード修正確認

修正後：
- `dotnet build` でコンパイル確認
- 実装例のコード片が正確か確認

---

## 5. 決定待ち項目

### 5.1 AggregateId の扱い【ユーザー判断】

選択肢：
- **A：ドキュメント削除** （推奨）
- **B：コード実装**

**推奨理由：** 
- 3年以上実装されず、現在のビジネス要件にない
- 今後必要になれば、その時に設計し直す方が良い

---

## 6. 成功基準

修正完了時：

- ✅ 実装パターンが ガイドに100%記載
- ✅ ガイドの推奨例がコードに存在
- ✅ ドキュメント修正時に git commit で記録
- ✅ 修正前後でビルドエラーなし

---

## 7. 改版履歴

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-09-12 | 初版作成。ValueObject 齟齬調査に基づいて修正計画を策定 |

