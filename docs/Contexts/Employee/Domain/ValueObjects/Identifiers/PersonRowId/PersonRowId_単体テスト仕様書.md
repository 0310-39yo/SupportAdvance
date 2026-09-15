# 単体テスト仕様書 — PersonRowId

**プロジェクト:** SupportAdvance  
**テスト対象:** PersonRowId（RowId派生型の必須Identifier）  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-09-16

---

## 0. 本書の位置づけ

本書は、Domain層の PersonRowId ValueObject が、技術仕様書および詳細設計書で定義された
**値の生成・検証・等価性比較・ハッシュ・文字列化・値の不変性** を満たすことを確認するテスト仕様書。

PersonRowId は SharedKernel の RowId クラスを継承した必須 Identifier で、オプショナル状態（IsSet）を持たない。

---

## 1. テスト目的

PersonRowId の各メンバーが、以下の仕様を満たすことを確認する：

- **値の検証**：1以上の有効な long 値のみを受け入れる
- **値の不変性**：オブジェクト生成後、内部状態が変更されない
- **等価性比較**：同じ値を持つ2つのオブジェクトは等価である
- **ハッシュ整合性**：Equals=true のオブジェクトは同一ハッシュ値
- **文字列化**：ToString が仕様通りの形式で返される
- **型安全な生成**：From/TryFrom/TryFromDbValue メソッドの動作

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | PersonRowId |
| **名前空間** | SupportAdvance.Contexts.Employee.Domain.ValueObjects.Person |
| **基底クラス** | RowId (SharedKernel) |
| **実装インターフェース** | IEquatable<PersonRowId> |
| **依拠仕様** | PersonRowId技術仕様書 v1.0 + RowId技術仕様書 |
| **前提** | RowId 基底クラスのテストが完了していること |

---

## 3. テスト観点一覧

### 観点グループ GEN：`From` メソッド（生成）

| 観点ID | 観点（説明） | 分類 | 依拠仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| VO-GEN-01 | From(1) は MinValue のインスタンスを返す | 正常系（境界値） | § 3.1 | Test Constructor |
| VO-GEN-02 | From(long.MaxValue) は最大値のインスタンスを返す | 正常系（境界値） | § 3.1 | Test Constructor |
| VO-GEN-03 | From(12345) は typical な値のインスタンスを返す | 正常系 | § 3.1 | Test Constructor |
| VO-GEN-04 | From(0) は ArgumentOutOfRangeException を発生させる | 異常系 | § 3.1 | Test Constructor |
| VO-GEN-05 | From(-1) は ArgumentOutOfRangeException を発生させる | 異常系 | § 3.1 | Test Constructor |
| VO-GEN-06 | From(long.MinValue) は ArgumentOutOfRangeException を発生させる | 異常系 | § 3.1 | Test Constructor |

### 観点グループ TRY：`TryFrom` メソッド（安全な生成）

| 観点ID | 観点（説明） | 分類 | 依拠仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| VO-TRY-01 | TryFrom(1) は true を返す | 正常系（境界値） | § 3.2 | Test Constructor |
| VO-TRY-02 | TryFrom(long.MaxValue) は true を返す | 正常系（境界値） | § 3.2 | Test Constructor |
| VO-TRY-03 | TryFrom(0) は false を返す | 異常系 | § 3.2 | Test Constructor |
| VO-TRY-04 | TryFrom(-1) は false を返す | 異常系 | § 3.2 | Test Constructor |
| VO-TRY-05 | TryFrom(valid) は result に設定済みインスタンスを返す | 正常系 | § 3.2 | Test Constructor |
| VO-TRY-06 | TryFrom(invalid) は result に Unset() のようなデフォルト値を返す | 異常系 | § 3.2 | Test Constructor |
| VO-TRY-07 | TryFrom(multiple valid values) は全て true を返す | 正常系（データドリブン） | § 3.2 | Test Constructor |

### 観点グループ DBVAL：`TryFromDbValue` メソッド（DB値変換）

| 観点ID | 観点（説明） | 分類 | 依拠仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| VO-DBVAL-01 | TryFromDbValue(12345) は true を返す | 正常系 | § 3.3 | Test Constructor |
| VO-DBVAL-02 | TryFromDbValue(1) は true を返す | 正常系（境界値） | § 3.3 | Test Constructor |
| VO-DBVAL-03 | TryFromDbValue(long.MaxValue) は true を返す | 正常系（境界値） | § 3.3 | Test Constructor |
| VO-DBVAL-04 | TryFromDbValue(0) は false を返す | 異常系 | § 3.3 | Test Constructor |
| VO-DBVAL-05 | TryFromDbValue(-1) は false を返す | 異常系 | § 3.3 | Test Constructor |

### 観点グループ EQ：`Equals` — 等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | 依拠仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| VO-EQ-01 | 同じ値を持つ 2 つのオブジェクトは等価 | 正常系 | § 4.2 | Test Constructor, Equals/GetHashCode |
| VO-EQ-02 | 同一参照のオブジェクトは等価 | 正常系（自己参照） | § 4.2 | Test Constructor, Equals/GetHashCode |
| VO-EQ-03 | object 型で比較しても等価 | 正常系 | § 4.2 | Test Constructor, Equals/GetHashCode |

### 観点グループ NE：`Equals` — 非等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | 依拠仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| VO-NE-01 | 値が異なる場合は非等価 | 異常系 | § 4.2 | Test Constructor, Equals/GetHashCode |
| VO-NE-02 | null との比較は非等価 | 例外/異常系 | § 4.2 | Test Constructor, Equals/GetHashCode |
| VO-NE-03 | 型が異なる場合は非等価（値が同じでも） | 異常系 | § 4.2 | Test Constructor, Equals/GetHashCode |

### 観点グループ HC：`GetHashCode`

| 観点ID | 観点（説明） | 分類 | 依拠仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| VO-HC-01 | Equals=true の 2 つのオブジェクトは同一ハッシュ値 | 正常系 | § 4.3 | Equals/GetHashCode |
| VO-HC-02 | 値が異なるとハッシュ値が異なる | 境界値テスト | § 4.3 | Equals/GetHashCode |
| VO-HC-03 | ハッシュ値は複数呼び出しで一貫している | 正常系（副作用なし） | § 4.3 | Equals/GetHashCode |

### 観点グループ TS：`ToString`

| 観点ID | 観点（説明） | 分類 | 依拠仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| VO-TS-01 | ToString() は数値を文字列で返す（例："12345"） | 正常系 | § 4.4 | Test Constructor |
| VO-TS-02 | ToString() は MinValue のとき "1" を返す | 正常系（境界値） | § 4.4 | Test Constructor |
| VO-TS-03 | ToString() は MaxValue のとき正しい文字列を返す | 正常系（境界値） | § 4.4 | Test Constructor |

### 観点グループ IMM：不変性（Immutability）

| 観点ID | 観点（説明） | 分類 | 依拠仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| VO-IMM-01 | Value プロパティは読み取り専用である | 正常系 | § 4.5 | Test Constructor |
| VO-IMM-02 | オブジェクト生成後、Value が変更されない | 正常系 | § 4.5 | Test Constructor |

---

## 4. テスト観点別の検証シナリオ

### 観点 VO-GEN-01：From(1) は MinValue のインスタンスを返す

#### 4.1.1 テスト観点

PersonRowId.From(1) を呼び出した場合、MinValue の妥当な long 値をもつインスタンスが返される。

#### 4.1.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.1.2.1 | 正常系（境界値） | From(1) の呼び出し |

#### 4.1.3 前提条件

- PersonRowId が利用可能
- From メソッドが定義されている

#### 4.1.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.1.2.1 | 1L | PersonRowId.MinValue |

#### 4.1.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.1.2.1 | NotNull, Value == 1L | インスタンス生成 + 値が正しい |

#### 4.1.6 判定基準

- [ ] From(1) は null でないインスタンスを返す
- [ ] 返却インスタンスの Value == 1L

---

### 観点 VO-GEN-02：From(long.MaxValue) は最大値のインスタンスを返す

#### 4.2.1 テスト観点

PersonRowId.From(long.MaxValue) を呼び出した場合、最大の long 値をもつインスタンスが返される。

#### 4.2.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.2.2.1 | 正常系（境界値） | From(long.MaxValue) の呼び出し |

#### 4.2.3 前提条件

- PersonRowId が利用可能
- From メソッドが定義されている

#### 4.2.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.2.2.1 | 9223372036854775807L | long.MaxValue |

#### 4.2.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.2.2.1 | NotNull, Value == long.MaxValue | インスタンス生成 + 値が正しい |

#### 4.2.6 判定基準

- [ ] From(long.MaxValue) は null でないインスタンスを返す
- [ ] 返却インスタンスの Value == long.MaxValue

---

### 観点 VO-GEN-03：From(12345) は typical な値のインスタンスを返す

#### 4.3.1 テスト観点

PersonRowId.From(12345) を呼び出した場合、典型的な long 値をもつインスタンスが返される。

#### 4.3.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.3.2.1 | 正常系 | From(12345) の呼び出し |

#### 4.3.3 前提条件

- PersonRowId が利用可能
- From メソッドが定義されている

#### 4.3.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.3.2.1 | 12345L | 典型的な値 |

#### 4.3.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.3.2.1 | NotNull, Value == 12345L | インスタンス生成 + 値が正しい |

#### 4.3.6 判定基準

- [ ] From(12345) は null でないインスタンスを返す
- [ ] 返却インスタンスの Value == 12345L

---

### 観点 VO-GEN-04：From(0) は ArgumentOutOfRangeException を発生させる

#### 4.4.1 テスト観点

PersonRowId.From(0) を呼び出した場合、ArgumentOutOfRangeException が発生する（0 は無効）。

#### 4.4.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.4.2.1 | 異常系 | From(0) で例外発生 |

#### 4.4.3 前提条件

- PersonRowId が利用可能
- From メソッドが定義されている
- 検証ロジックが実装されている

#### 4.4.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.4.2.1 | 0L | 無効な値（MinValue 未満） |

#### 4.4.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.4.2.1 | ArgumentOutOfRangeException | 例外が発生 |

#### 4.4.6 判定基準

- [ ] From(0) は ArgumentOutOfRangeException を発生させる
- [ ] 例外は捕捉可能である

---

### 観点 VO-GEN-05：From(-1) は ArgumentOutOfRangeException を発生させる

#### 4.5.1 テスト観点

PersonRowId.From(-1) を呼び出した場合、ArgumentOutOfRangeException が発生する（負数は無効）。

#### 4.5.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.5.2.1 | 異常系 | From(-1) で例外発生 |

#### 4.5.3 前提条件

- PersonRowId が利用可能
- From メソッドが定義されている
- 検証ロジックが実装されている

#### 4.5.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.5.2.1 | -1L | 無効な値（負数） |

#### 4.5.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.5.2.1 | ArgumentOutOfRangeException | 例外が発生 |

#### 4.5.6 判定基準

- [ ] From(-1) は ArgumentOutOfRangeException を発生させる
- [ ] 例外は捕捉可能である

---

### 観点 VO-GEN-06：From(long.MinValue) は ArgumentOutOfRangeException を発生させる

#### 4.6.1 テスト観点

PersonRowId.From(long.MinValue) を呼び出した場合、ArgumentOutOfRangeException が発生する。

#### 4.6.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.6.2.1 | 異常系 | From(long.MinValue) で例外発生 |

#### 4.6.3 前提条件

- PersonRowId が利用可能
- From メソッドが定義されている
- 検証ロジックが実装されている

#### 4.6.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.6.2.1 | -9223372036854775808L | long.MinValue |

#### 4.6.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.6.2.1 | ArgumentOutOfRangeException | 例外が発生 |

#### 4.6.6 判定基準

- [ ] From(long.MinValue) は ArgumentOutOfRangeException を発生させる
- [ ] 例外は捕捉可能である

---

### 観点 VO-TRY-01：TryFrom(1) は true を返す

#### 4.7.1 テスト観点

PersonRowId.TryFrom(1, out var result) を呼び出した場合、true が返される。

#### 4.7.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.7.2.1 | 正常系（境界値） | TryFrom(1) で true 返却 |

#### 4.7.3 前提条件

- PersonRowId が利用可能
- TryFrom メソッドが定義されている

#### 4.7.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.7.2.1 | 1L | PersonRowId.MinValue |

#### 4.7.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.7.2.1 | true | 戻り値が true + result に設定済みインスタンス |

#### 4.7.6 判定基準

- [ ] TryFrom(1) は true を返す
- [ ] out パラメータに有効なインスタンスが設定される
- [ ] result.Value == 1L

---

### 観点 VO-TRY-02：TryFrom(long.MaxValue) は true を返す

#### 4.8.1 テスト観点

PersonRowId.TryFrom(long.MaxValue, out var result) を呼び出した場合、true が返される。

#### 4.8.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.8.2.1 | 正常系（境界値） | TryFrom(long.MaxValue) で true 返却 |

#### 4.8.3 前提条件

- PersonRowId が利用可能
- TryFrom メソッドが定義されている

#### 4.8.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.8.2.1 | 9223372036854775807L | long.MaxValue |

#### 4.8.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.8.2.1 | true | 戻り値が true + result に設定済みインスタンス |

#### 4.8.6 判定基準

- [ ] TryFrom(long.MaxValue) は true を返す
- [ ] out パラメータに有効なインスタンスが設定される
- [ ] result.Value == long.MaxValue

---

### 観点 VO-TRY-03：TryFrom(0) は false を返す

#### 4.9.1 テスト観点

PersonRowId.TryFrom(0, out var result) を呼び出した場合、false が返される。

#### 4.9.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.9.2.1 | 異常系 | TryFrom(0) で false 返却 |

#### 4.9.3 前提条件

- PersonRowId が利用可能
- TryFrom メソッドが定義されている

#### 4.9.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.9.2.1 | 0L | 無効な値 |

#### 4.9.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.9.2.1 | false | 戻り値が false（例外なし） |

#### 4.9.6 判定基準

- [ ] TryFrom(0) は false を返す
- [ ] 例外は発生しない
- [ ] 処理は正常に完了する

---

### 観点 VO-TRY-04：TryFrom(-1) は false を返す

#### 4.10.1 テスト観点

PersonRowId.TryFrom(-1, out var result) を呼び出した場合、false が返される。

#### 4.10.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.10.2.1 | 異常系 | TryFrom(-1) で false 返却 |

#### 4.10.3 前提条件

- PersonRowId が利用可能
- TryFrom メソッドが定義されている

#### 4.10.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.10.2.1 | -1L | 無効な値（負数） |

#### 4.10.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.10.2.1 | false | 戻り値が false（例外なし） |

#### 4.10.6 判定基準

- [ ] TryFrom(-1) は false を返す
- [ ] 例外は発生しない
- [ ] 処理は正常に完了する

---

### 観点 VO-DBVAL-01：TryFromDbValue(12345) は true を返す

#### 4.11.1 テスト観点

PersonRowId.TryFromDbValue(12345, out var result) を呼び出した場合、true が返される。

#### 4.11.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.11.2.1 | 正常系 | TryFromDbValue(12345) で true 返却 |

#### 4.11.3 前提条件

- PersonRowId が利用可能
- TryFromDbValue メソッドが定義されている

#### 4.11.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.11.2.1 | 12345L | DB から取得した典型的な値 |

#### 4.11.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.11.2.1 | true | 戻り値が true + result に設定済みインスタンス |

#### 4.11.6 判定基準

- [ ] TryFromDbValue(12345) は true を返す
- [ ] out パラメータに有効なインスタンスが設定される
- [ ] result.Value == 12345L

---

### 観点 VO-DBVAL-02：TryFromDbValue(1) は true を返す

#### 4.12.1 テスト観点

PersonRowId.TryFromDbValue(1, out var result) を呼び出した場合、true が返される。

#### 4.12.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.12.2.1 | 正常系（境界値） | TryFromDbValue(1) で true 返却 |

#### 4.12.3 前提条件

- PersonRowId が利用可能
- TryFromDbValue メソッドが定義されている

#### 4.12.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.12.2.1 | 1L | MinValue |

#### 4.12.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.12.2.1 | true | 戻り値が true + result に設定済みインスタンス |

#### 4.12.6 判定基準

- [ ] TryFromDbValue(1) は true を返す
- [ ] out パラメータに有効なインスタンスが設定される
- [ ] result.Value == 1L

---

### 観点 VO-DBVAL-03：TryFromDbValue(long.MaxValue) は true を返す

#### 4.13.1 テスト観点

PersonRowId.TryFromDbValue(long.MaxValue, out var result) を呼び出した場合、true が返される。

#### 4.13.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.13.2.1 | 正常系（境界値） | TryFromDbValue(long.MaxValue) で true 返却 |

#### 4.13.3 前提条件

- PersonRowId が利用可能
- TryFromDbValue メソッドが定義されている

#### 4.13.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.13.2.1 | 9223372036854775807L | long.MaxValue |

#### 4.13.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.13.2.1 | true | 戻り値が true + result に設定済みインスタンス |

#### 4.13.6 判定基準

- [ ] TryFromDbValue(long.MaxValue) は true を返す
- [ ] out パラメータに有効なインスタンスが設定される
- [ ] result.Value == long.MaxValue

---

### 観点 VO-DBVAL-04：TryFromDbValue(0) は false を返す

#### 4.14.1 テスト観点

PersonRowId.TryFromDbValue(0, out var result) を呼び出した場合、false が返される。

#### 4.14.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.14.2.1 | 異常系 | TryFromDbValue(0) で false 返却 |

#### 4.14.3 前提条件

- PersonRowId が利用可能
- TryFromDbValue メソッドが定義されている

#### 4.14.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.14.2.1 | 0L | 無効な値 |

#### 4.14.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.14.2.1 | false | 戻り値が false（例外なし） |

#### 4.14.6 判定基準

- [ ] TryFromDbValue(0) は false を返す
- [ ] 例外は発生しない
- [ ] 処理は正常に完了する

---

### 観点 VO-DBVAL-05：TryFromDbValue(-1) は false を返す

#### 4.15.1 テスト観点

PersonRowId.TryFromDbValue(-1, out var result) を呼び出した場合、false が返される。

#### 4.15.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.15.2.1 | 異常系 | TryFromDbValue(-1) で false 返却 |

#### 4.15.3 前提条件

- PersonRowId が利用可能
- TryFromDbValue メソッドが定義されている

#### 4.15.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.15.2.1 | -1L | 無効な値（負数） |

#### 4.15.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.15.2.1 | false | 戻り値が false（例外なし） |

#### 4.15.6 判定基準

- [ ] TryFromDbValue(-1) は false を返す
- [ ] 例外は発生しない
- [ ] 処理は正常に完了する

---

### 観点 VO-EQ-01：同じ値を持つ 2 つのオブジェクトは等価

#### 4.16.1 テスト観点

同一の値を持つ2つのPersonRowIdインスタンスは、Equals メソッドで等価と判定される。

#### 4.16.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.16.2.1 | 正常系 | 同じ値（12345）の2つインスタンス |

#### 4.16.3 前提条件

- 2つの異なるインスタンスを生成
- 値が完全に一致

#### 4.16.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.16.2.1 | obj1 = PersonRowId.From(12345L)<br/>obj2 = PersonRowId.From(12345L) | 同値のインスタンス |

#### 4.16.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.16.2.1 | obj1.Equals(obj2) == true<br/>obj1 == obj2 == true | Equals が true、== も true |

#### 4.16.6 判定基準

- [ ] 同じ値のPersonRowIdは Equals で true を返す
- [ ] == 演算子でも true を返す
- [ ] 複数回の Equals 呼び出しで一貫性がある

---

### 観点 VO-EQ-02：同一参照のオブジェクトは等価

#### 4.17.1 テスト観点

同一参照のPersonRowIdオブジェクトは、Equals メソッドで等価と判定される。

#### 4.17.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.17.2.1 | 正常系（自己参照） | 同一参照の比較 |

#### 4.17.3 前提条件

- インスタンスを1つ生成

#### 4.17.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.17.2.1 | var rowId1 = PersonRowId.From(12345L);<br/>ReferenceEquals(rowId1, rowId1) == true | 自己参照 |

#### 4.17.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.17.2.1 | rowId1.Equals(rowId1) == true | Equals が true |

#### 4.17.6 判定基準

- [ ] 同一参照は Equals で true を返す

---

### 観点 VO-EQ-03：object 型で比較しても等価

#### 4.18.1 テスト観点

PersonRowId を object 型にキャストして比較しても、等価と判定される。

#### 4.18.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.18.2.1 | 正常系 | object 型での比較 |

#### 4.18.3 前提条件

- 2つのインスタンスを生成
- 値が同一

#### 4.18.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.18.2.1 | var rowId1 = PersonRowId.From(12345L);<br/>object obj = PersonRowId.From(12345L);<br/>rowId1.Equals(obj) | object 型での比較 |

#### 4.18.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.18.2.1 | true | Equals が true を返す |

#### 4.18.6 判定基準

- [ ] object 型でキャストして比較しても等価

---

### 観点 VO-NE-01：値が異なる場合は非等価

#### 4.19.1 テスト観点

値が異なるPersonRowIdオブジェクトは、Equals メソッドで非等価と判定される。

#### 4.19.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.19.2.1 | 異常系 | 値が異なる（12345 vs 67890） |

#### 4.19.3 前提条件

- 2つのインスタンスを生成
- 値が異なる

#### 4.19.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.19.2.1 | obj1 = PersonRowId.From(12345L)<br/>obj2 = PersonRowId.From(67890L) | 異なる値 |

#### 4.19.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.19.2.1 | obj1.Equals(obj2) == false<br/>obj1 != obj2 == true | Equals が false、!= が true |

#### 4.19.6 判定基準

- [ ] 値が異なるPersonRowIdは Equals で false を返す
- [ ] != 演算子で true を返す

---

### 観点 VO-NE-02：null との比較は非等価

#### 4.20.1 テスト観点

PersonRowId が null と比較される場合、Equals メソッドで false を返す。

#### 4.20.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.20.2.1 | 例外/異常系 | null との比較 |

#### 4.20.3 前提条件

- PersonRowId インスタンスを生成

#### 4.20.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.20.2.1 | var rowId = PersonRowId.From(12345L);<br/>rowId.Equals(null) | null との比較 |

#### 4.20.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.20.2.1 | false | Equals が false を返す |

#### 4.20.6 判定基準

- [ ] null との比較は false を返す
- [ ] 例外は発生しない

---

### 観点 VO-NE-03：型が異なる場合は非等価（値が同じでも）

#### 4.21.1 テスト観点

PersonRowId と異なる型（例：long）が比較される場合、Equals メソッドで false を返す。

#### 4.21.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.21.2.1 | 異常系 | 型が異なる（PersonRowId vs long） |

#### 4.21.3 前提条件

- PersonRowId インスタンスを生成

#### 4.21.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.21.2.1 | var rowId = PersonRowId.From(12345L);<br/>rowId.Equals(12345L) | long 型との比較 |

#### 4.21.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.21.2.1 | false | Equals が false を返す |

#### 4.21.6 判定基準

- [ ] 型が異なれば、値が同じでも false を返す
- [ ] 例外は発生しない

---

### 観点 VO-HC-01：Equals=true の 2 つのオブジェクトは同一ハッシュ値

#### 4.22.1 テスト観点

Equals メソッドで true を返す2つのPersonRowIdは、GetHashCode で同一のハッシュ値を返す。

#### 4.22.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.22.2.1 | 正常系 | 同値オブジェクトのハッシュ比較 |

#### 4.22.3 前提条件

- 複数の同値PersonRowIdインスタンスを生成

#### 4.22.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.22.2.1 | obj1 = PersonRowId.From(12345L)<br/>obj2 = PersonRowId.From(12345L) | 同値ペア |

#### 4.22.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.22.2.1 | obj1.GetHashCode() == obj2.GetHashCode() | ハッシュ値が同一 |

#### 4.22.6 判定基準

- [ ] Equals=true のオブジェクトペアは同一ハッシュ値を返す
- [ ] Dictionary / HashSet で正しく機能する（同一キーとして認識される）

---

### 観点 VO-HC-02：値が異なるとハッシュ値が異なる

#### 4.23.1 テスト観点

値が異なるPersonRowIdは、通常異なるハッシュ値を返す（ハッシュ衝突は許容）。

#### 4.23.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.23.2.1 | 境界値テスト | 異なる値のハッシュ比較 |

#### 4.23.3 前提条件

- 異なる値のPersonRowIdインスタンスを生成

#### 4.23.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.23.2.1 | obj1 = PersonRowId.From(12345L)<br/>obj2 = PersonRowId.From(67890L) | 異なる値 |

#### 4.23.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.23.2.1 | obj1.GetHashCode() != obj2.GetHashCode() | ハッシュ値が異なる（通常） |

#### 4.23.6 判定基準

- [ ] 異なる値は異なるハッシュ値を返す（ハッシュ衝突は許容）

---

### 観点 VO-HC-03：ハッシュ値は複数呼び出しで一貫している

#### 4.24.1 テスト観点

GetHashCode の複数呼び出しが、同一のハッシュ値を返す（不変性）。

#### 4.24.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.24.2.1 | 正常系（副作用なし） | 複数回の GetHashCode 呼び出し |

#### 4.24.3 前提条件

- PersonRowId インスタンスを生成

#### 4.24.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.24.2.1 | var rowId = PersonRowId.From(12345L) | インスタンス |

#### 4.24.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.24.2.1 | rowId.GetHashCode() == rowId.GetHashCode() | 複数呼び出しで同一値 |

#### 4.24.6 判定基準

- [ ] ハッシュ値は複数呼び出しで一貫している
- [ ] GetHashCode は副作用を持たない

---

### 観点 VO-TS-01：ToString() は数値を文字列で返す

#### 4.25.1 テスト観点

PersonRowId.ToString() は、long 値を文字列に変換して返す。

#### 4.25.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.25.2.1 | 正常系 | ToString() で文字列化 |

#### 4.25.3 前提条件

- PersonRowId インスタンスを生成

#### 4.25.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.25.2.1 | PersonRowId.From(12345L).ToString() | 数値の文字列化 |

#### 4.25.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.25.2.1 | "12345" | 数値文字列が返される |

#### 4.25.6 判定基準

- [ ] ToString() が数値を正確に文字列化して返す

---

### 観点 VO-TS-02：ToString() は MinValue のとき "1" を返す

#### 4.26.1 テスト観点

PersonRowId.MinValue の場合、ToString() は "1" を返す。

#### 4.26.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.26.2.1 | 正常系（境界値） | ToString() で MinValue を文字列化 |

#### 4.26.3 前提条件

- PersonRowId インスタンスを生成

#### 4.26.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.26.2.1 | PersonRowId.From(1L).ToString() | MinValue の文字列化 |

#### 4.26.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.26.2.1 | "1" | 文字列が "1" |

#### 4.26.6 判定基準

- [ ] ToString() が "1" を返す

---

### 観点 VO-TS-03：ToString() は MaxValue のとき正しい文字列を返す

#### 4.27.1 テスト観点

PersonRowId.MaxValue の場合、ToString() は long.MaxValue の文字列表現を返す。

#### 4.27.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.27.2.1 | 正常系（境界値） | ToString() で MaxValue を文字列化 |

#### 4.27.3 前提条件

- PersonRowId インスタンスを生成

#### 4.27.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.27.2.1 | PersonRowId.From(long.MaxValue).ToString() | MaxValue の文字列化 |

#### 4.27.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.27.2.1 | "9223372036854775807" | 文字列が long.MaxValue と一致 |

#### 4.27.6 判定基準

- [ ] ToString() が long.MaxValue の正確な文字列表現を返す

---

### 観点 VO-IMM-01：Value プロパティは読み取り専用である

#### 4.28.1 テスト観点

PersonRowId.Value プロパティは読み取り専用で、外部から変更不可。

#### 4.28.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.28.2.1 | 正常系 | Value の読み取り |

#### 4.28.3 前提条件

- PersonRowId インスタンスを生成

#### 4.28.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.28.2.1 | var rowId = PersonRowId.From(12345L);<br/>rowId.Value | Value プロパティアクセス |

#### 4.28.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.28.2.1 | 12345L | Value が読み取り可能 |

#### 4.28.6 判定基準

- [ ] Value プロパティから値を読み取れる
- [ ] setter が存在しない（読み取り専用）

---

### 観点 VO-IMM-02：オブジェクト生成後、Value が変更されない

#### 4.29.1 テスト観点

PersonRowId オブジェクト生成後、Value は複数回読み取っても同じ値を返す。

#### 4.29.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.29.2.1 | 正常系 | 複数回の読み取り |

#### 4.29.3 前提条件

- PersonRowId インスタンスを生成

#### 4.29.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.29.2.1 | var rowId = PersonRowId.From(12345L);<br/>firstRead = rowId.Value;<br/>secondRead = rowId.Value | 複数回読み取り |

#### 4.29.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.29.2.1 | firstRead == secondRead == 12345L | 複数回の読み取りで一貫 |

#### 4.29.6 判定基準

- [ ] 複数回読み取っても同一値
- [ ] オブジェクトは不変である

---

## 5. 判定基準

### 5.1 全般的な判定基準

| 項目 | 基準 |
|------|------|
| **値の検証** | 1以上の値のみが受け入れられること |
| **不変性** | オブジェクト生成後、プロパティが変更されないこと |
| **等価性** | Equals の結果が論理的に一貫していること |
| **ハッシュ整合性** | Equals=true のペアが同一ハッシュ値を返すこと |
| **演算子整合性** | == と Equals の結果が矛盾しないこと |
| **文字列化** | ToString が数値の文字列表現を返すこと |
| **型安全な生成** | From は例外、TryFrom/TryFromDbValue は false を返すこと |

### 5.2 検証方法

```
【VO-GEN-01 観点の検証】
  var rowId = PersonRowId.From(1L);
  Assert.NotNull(rowId);
  Assert.Equal(1L, rowId.Value);

【VO-TRY-01 観点の検証】
  bool success = PersonRowId.TryFrom(1L, out var rowId);
  Assert.True(success);
  Assert.Equal(1L, rowId.Value);

【VO-DBVAL-01 観点の検証】
  bool success = PersonRowId.TryFromDbValue(12345L, out var rowId);
  Assert.True(success);
  Assert.Equal(12345L, rowId.Value);

【VO-EQ-01 観点の検証】
  var rowId1 = PersonRowId.From(12345L);
  var rowId2 = PersonRowId.From(12345L);
  Assert.True(rowId1.Equals(rowId2));
  Assert.Equal(rowId1, rowId2);

【VO-HC-01 観点の検証】
  var rowId1 = PersonRowId.From(12345L);
  var rowId2 = PersonRowId.From(12345L);
  Assert.Equal(rowId1.GetHashCode(), rowId2.GetHashCode());

【VO-TS-01 観点の検証】
  var rowId = PersonRowId.From(12345L);
  Assert.Equal("12345", rowId.ToString());

【VO-IMM-01 観点の検証】
  var rowId = PersonRowId.From(12345L);
  Assert.Equal(12345L, rowId.Value);
  // setter なし（読み取り専用確認）
```

---

## 6. テスト用実装の設定

### 6.1 前提環境

| 項目 | 内容 |
|------|------|
| **テスティングフレームワーク** | xUnit |
| **Assertion** | Assert.True / Assert.Equal / Assert.Throws / Assert.NotNull |
| **Mock/Stub** | 不要（ValueObject は依存性なし） |

### 6.2 テストクラス構成

```
tests/Contexts/Employee.Domain.Tests/
└── ValueObjects/
    └── Employee/
        └── PersonRowIdTests.cs
```

### 6.3 テストコード参照

実装済みのテストコードは以下の場所に存在し、本仕様書に基づいて全テストが合格している：

```
tests/Contexts/Employee.Domain.Tests/ValueObjects/Employee/PersonRowIdTests.cs
```

テストは以下の6グループに分類されている：
- グループ 1: 生成メソッド（From）- 6個テスト
- グループ 2: 安全な生成（TryFrom）- 7個テスト
- グループ 3: DB値変換（TryFromDbValue）- 5個テスト
- グループ 4: 等価性（Equality）- 7個テスト
- グループ 5: 表示形式（Display）- 6個テスト
- グループ 6: 不変性（Immutability）- 2個テスト

**合計 33 個のテストケース**

---

## 7. 改版履歴

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-09-16 | 初版作成。PersonRowId（RowId派生型の必須Identifier）の単体テスト仕様書。テスト観点 21個（VO-GEN, VO-TRY, VO-DBVAL, VO-EQ, VO-NE, VO-HC, VO-TS, VO-IMM）。テストコード全 33ケース対応 |

---

## 補足：PersonRowId と SharedKernel RowId の関係

PersonRowId は SharedKernel の RowId クラスを継承した Domain 層固有の Identifier です。

### 特徴

- **基底クラス**: RowId（SharedKernel）
- **型**: 必須の long ベース Identifier
- **検証**: MinValue = 1L（0以下は無効）
- **用途**: m_persons テーブルの行ID参照
- **状態管理**: IsSet フラグなし（常に設定済み状態）

### SharedKernel RowId との関係

RowId が提供する基本機能：
- Validate メソッド（オーバーライド）
- Value プロパティ
- Equal、GetHashCode の実装

PersonRowId が追加する機能：
- Domain 層固有の型安全性
- IEquatable<PersonRowId> の実装
- ToString の再実装

---
