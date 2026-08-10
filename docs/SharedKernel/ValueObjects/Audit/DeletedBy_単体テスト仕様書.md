# DeletedBy 単体テスト仕様書

**バージョン:** 1.0  
**作成日:** 2026年08月10日  
**テスト対象:** DeletedBy（従業員行ID オプション値オブジェクト、論理削除用）  
**テストレベル:** 単体テスト

---

## 1. テスト目的

DeletedBy の各メンバーが、以下の仕様を満たすことを確認する：

- **値の不変性** — オブジェクト生成後、内部状態が変更されない
- **IsSet状態管理** — IsSet フラグで削除済み/未削除を表現
- **null吸収** — TryFrom(null) で null を Unset に変換して成功を返す
- **等価性比較** — Value と IsSet が同一の2つのオブジェクトは等価
- **ハッシュ整合性** — Equals=true のオブジェクトは同一ハッシュ値
- **値の検証** — 設定時は 0 以下の値を例外で拒否
- **ファクトリメソッド** — From()、Unset()、TryFrom()、TryFromDbValue() が正常に動作

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | DeletedBy |
| **名前空間** | SupportAdvance.SharedKernel.ValueObjects.Audit |
| **依拠仕様** | DeletedBy技術仕様書 v1.0 + 詳細設計書 v1.0 |

---

## 3. テスト観点一覧

### 観点グループ VO：ValueObject 基本

| 観点ID | 観点（説明） | 分類 | テストパターン |
|--------|------|------|----------------|
| VO-01 | From(正の数) は IsSet=true の DeletedBy を生成 | 正常系 | 3.1 |
| VO-02 | From(0) は ArgumentException を投げる | 異常系 | 3.2 |
| VO-03 | From(負の数) は ArgumentException を投げる | 異常系 | 3.2 |
| VO-04 | Unset() は IsSet=false のインスタンスを返す | 正常系 | 3.3 |
| VO-05 | TryFrom(null) は true と Unset を返す（null吸収） | 正常系 | 3.4 |
| VO-06 | TryFrom(正の数) は true と インスタンスを返す | 正常系 | 3.5 |
| VO-07 | TryFrom(無効値) は false と Unset を返す | 異常系 | 3.6 |
| VO-08 | TryFromDbValue(null) は true と Unset を返す | 正常系 | 3.7 |
| VO-09 | TryFromDbValue(正の数) は true と インスタンスを返す | 正常系 | 3.8 |
| VO-10 | 同じ値・同じ IsSet の2つのインスタンスは等価 | 正常系 | 3.9 |
| VO-11 | Value のみが異なる場合は非等価 | 異常系 | 3.10 |
| VO-12 | IsSet のみが異なる場合は非等価 | 異常系 | 3.11 |
| VO-13 | Unset 同士は等価 | 正常系 | 3.12 |
| VO-14 | Equals=true のペアは同一ハッシュ値 | 正常系 | 3.13 |
| VO-15 | IsDeleted で削除済み/未削除を判定 | 正常系 | 3.14 |
| VO-16 | Value は IsSet=true のときのみ値を返す | 正常系 | 3.15 |
| VO-17 | ToString は IsSet=false で "Unset" を返す | 正常系 | 3.16 |

---

## 4. テスト観点別の検証シナリオ

### 観点 VO-01：From(正の数) は IsSet=true の DeletedBy を生成

#### 4.1.1 テスト観点

DeletedBy.From() に正の従業員行IDを渡すと、IsSet=true のインスタンスが返される。

#### 4.1.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.1.2.1 | 正常系 | 通常の従業員行ID（1003） |
| 4.1.2.2 | 正常系 | 最小有効値（1） |

#### 4.1.3 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.1.2.1 | DeletedBy.From(1003L) | 通常の従業員行ID |
| 4.1.2.2 | DeletedBy.From(1L) | 最小有効値 |

#### 4.1.4 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.1.2.1 | IsSet=true, Value==1003 | From が正常に動作 |
| 4.1.2.2 | IsSet=true, Value==1 | 最小値で動作 |

#### 4.1.5 判定基準

- [ ] From(正の数) が例外を投げない
- [ ] 返却インスタンスの IsSet が true
- [ ] Value が入力値と一致

---

### 観点 VO-04：Unset() は IsSet=false のインスタンスを返す

#### 4.4.1 テスト観点

DeletedBy.Unset() は IsSet=false、Value=null のインスタンスを返す（未削除状態）。

#### 4.4.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.4.2.1 | 正常系 | Unset() 呼び出し |

#### 4.4.3 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.4.2.1 | DeletedBy.Unset() | 未削除状態 |

#### 4.4.4 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.4.2.1 | IsSet=false, Value=null | Unset 状態（未削除） |

#### 4.4.5 判定基準

- [ ] Unset() が IsSet=false のインスタンスを返す
- [ ] Value が null
- [ ] ToString() が "Unset"

---

### 観点 VO-05：TryFrom(null) は true と Unset を返す（null吸収）

#### 4.5.1 テスト観点

**重要な仕様**: DeletedBy.TryFrom(null) は true を返し、Unset インスタンスを返す。null を異常値ではなく「未削除」として扱う。

#### 4.5.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.5.2.1 | 正常系（null吸収） | null 入力で true + Unset 返却 |

#### 4.5.3 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.5.2.1 | DeletedBy.TryFrom((long?)null, out var result) | null 入力 |

#### 4.5.4 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.5.2.1 | true, result.IsSet=false | null → Unset で成功 |

#### 4.5.5 判定基準

- [ ] TryFrom(null) は true を返す（例外ではない）
- [ ] 返却インスタンスは IsSet=false（Unset）
- [ ] result.Equals(DeletedBy.Unset()) == true

---

### 観点 VO-12：IsSet のみが異なる場合は非等価

#### 4.12.1 テスト観点

DeletedBy.From(1003) と DeletedBy.Unset() は、IsSet が異なることで非等価と判定される。

#### 4.12.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.12.2.1 | 異常系 | IsSet: true vs false |
| 4.12.2.2 | 異常系 | IsSet: false vs true |

#### 4.12.3 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.12.2.1 | obj1 = DeletedBy.From(1003L)<br/>obj2 = DeletedBy.Unset() | IsSet のみ異なる |
| 4.12.2.2 | obj1 = DeletedBy.Unset()<br/>obj2 = DeletedBy.From(1003L) | IsSet のみ異なる（逆） |

#### 4.12.4 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.12.2.1 | obj1.Equals(obj2) == false | IsSet 異なると非等価 |
| 4.12.2.2 | obj1.Equals(obj2) == false | IsSet 異なると非等価 |

#### 4.12.5 判定基準

- [ ] IsSet フラグが異なると非等価と判定される
- [ ] GetHashCode も異なる値を返す

---

### 観点 VO-15：IsDeleted で削除済み/未削除を判定

#### 4.15.1 テスト観点

IsDeleted プロパティで削除済み/未削除を判定できる。

#### 4.15.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.15.2.1 | 正常系 | IsDeleted = true |
| 4.15.2.2 | 正常系 | IsDeleted = false |

#### 4.15.3 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.15.2.1 | DeletedBy.From(1003L).IsDeleted | 削除済み |
| 4.15.2.2 | DeletedBy.Unset().IsDeleted | 未削除 |

#### 4.15.4 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.15.2.1 | true | 削除済み状態 |
| 4.15.2.2 | false | 未削除状態 |

#### 4.15.5 判定基準

- [ ] IsDeleted が IsSet と同じ値を返す
- [ ] 可読性が向上（IsSet より直感的）

---

### 観点 VO-17：ToString は IsSet=false で "Unset" を返す

#### 4.17.1 テスト観点

ToString() は IsSet=false のときに "Unset"、IsSet=true のときに long 値を返す。

#### 4.17.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.17.2.1 | 正常系 | IsSet=true での ToString |
| 4.17.2.2 | 正常系 | IsSet=false での ToString |

#### 4.17.3 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.17.2.1 | DeletedBy.From(1003L).ToString() | ToString（削除済み） |
| 4.17.2.2 | DeletedBy.Unset().ToString() | ToString（未削除） |

#### 4.17.4 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.17.2.1 | "1003" | 値の文字列化 |
| 4.17.2.2 | "Unset" | "Unset" 文字列 |

#### 4.17.5 判定基準

- [ ] IsSet=false のときは "Unset" を返す
- [ ] IsSet=true のときは long.ToString() と同じ形式を返す

---

## 5. 判定基準

### 5.1 全般的な判定基準

| 項目 | 基準 |
|------|------|
| **不変性** | オブジェクト生成後、プロパティが変更されないこと |
| **IsSet状態管理** | IsSet フラグで削除済み/未削除を正確に表現 |
| **等価性** | Equals の結果が論理的に一貫していること |
| **ハッシュ整合性** | Equals=true のペアが同一ハッシュ値を返すこと |
| **null吸収** | TryFrom(null) が true + Unset を返すこと（重要） |
| **値操作安全性** | TryFrom/TryFromDbValue が例外を投げないこと |
| **文字列化** | ToString が IsSet フラグに応じた形式を返すこと |
| **論理削除対応** | 未削除状態を Unset で適切に表現すること |

---

## 6. 改版履歴

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-08-10 | 初版作成。DeletedBy の単体テスト仕様書（オプション ValueObject パターン、論理削除用） |
