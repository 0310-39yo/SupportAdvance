# 単体テスト仕様書テンプレート — RespondentName

**プロジェクト:** SupportAdvance  
**テスト対象:** ValueObject（値オブジェクト）  
**テストレベル:** 単体テスト  
**版:** 2.0 / 2026-07-08（v2.0 IOptionalValueObject対応版）  
**対応観点ID:** VO-IS-01/02 | VO-OPT-01～07 | VO-EQ-01～04 | VO-NE-01～05 | VO-HC-01～04 | VO-OP-01～05 | VO-TS-01～03 | VO-VLR-01～03

---

## 0. 本書の位置づけ

本書は、Domain層の RespondentName クラスが、技術仕様書および詳細設計書で定義された
**等価性比較・ハッシュ・文字列化・値の不変性・IsSet状態管理・オプショナル値操作** を満たすことを確認する単体テスト仕様書。

---

## 1. テスト目的

RespondentName の各メンバーが、以下の仕様を満たすことを確認する：

- **値の不変性**：オブジェクト生成後、内部状態が変更されない
- **等価性比較**：同じ値を持つ2つのオブジェクトは等価である
- **ハッシュ整合性**：Equals=true のオブジェクトは同一ハッシュ値
- **文字列化**：ToString が仕様通りの形式で返される
- **IsSet状態管理**：IsSet フラグが正しく機能する
- **オプショナル値操作**（IOptionalValueObject実装時）：Unset・From・TryFrom・TryGetValue の動作

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | RespondentName |
| **名前空間** | SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects |
| **依存仕様** | ValueObject技術仕様書 v1.1 ＋ IOptionalValueObject技術仕様書 v1.3 |
| **前提** | ValueObjectComponentNormalizer のテストが完了していることとし、PrimitiveValueObject<string> 基盤が動作すること |

---

## 2.X テスト用実装と DI 設定

この仕様書では、RespondentName のテストに使用するテスト用実装を明記します。

| 実装名 | 役割 | 対応する観点ID | 説明 |
|--------|------|----------------|------|
| DIFixture | DI コンテナの構成 | 全観点 | 依存性注入による検証（通常は不要） |
| Test Constructor | テスト用コンストラクタ | VO-IS-01～02, VO-OPT-01～07 | 値オブジェクトの初期化検証 |
| Equals/GetHashCode | 標準メソッド | VO-EQ, VO-HC など | .NET の Equals・GetHashCode メソッド検証 |
| TryFrom / TryGetValue | IOptionalValueObject メソッド | VO-OPT-03～07 | TryFrom と TryGetValue 中心の検証 |

---

## 3. テスト観点一覧

### 観点グループ IS：`IsSet` プロパティ

| 観点ID | 観点（説明） | 分類 | 依存仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| VO-IS-01 | IsSet=true で構築したオブジェクトは IsSet が true を返す | 正常系 | § 2.1 | DIFixture, Test Constructor |
| VO-IS-02 | IsSet=false で構築したオブジェクトは IsSet が false を返す | 正常系 | § 2.1 | DIFixture, Test Constructor |

### 観点グループ OPT：`IOptionalValueObject` メソッド（null吸収・値操作）

| 観点ID | 観点（説明） | 分類 | 依存仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| VO-OPT-01 | Unset() は IsSet=false のインスタンスを返す | 正常系 | IOptionalValueObject v1.3 § 3.1 | Test Constructor |
| VO-OPT-02 | From(value) は IsSet=true のインスタンスを返す | 正常系 | IOptionalValueObject v1.3 § 3.2 | Test Constructor |
| VO-OPT-03 | TryFrom(null) は true を返し、Unset インスタンスを返す | 正常系（null吸収） | IOptionalValueObject v1.3 § 3.3 | Test Constructor |
| VO-OPT-04 | TryFrom(valid) は true を返し、設定済みインスタンスを返す | 正常系 | IOptionalValueObject v1.3 § 3.3 | Test Constructor |
| VO-OPT-05 | TryFrom(invalid) は false を返し、Unset インスタンスを返す | 異常系（検証失敗） | IOptionalValueObject v1.3 § 3.3 | Test Constructor |
| VO-OPT-06 | TryGetValue(out value) は IsSet=true で true を返す | 正常系 | IOptionalValueObject v1.3 § 3.4 | Test Constructor |
| VO-OPT-07 | TryGetValue(out value) は IsSet=false で false を返す | 正常系 | IOptionalValueObject v1.3 § 3.4 | Test Constructor |

### 観点グループ EQ：`Equals` — 等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | 依存仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| VO-EQ-01 | 同じ型・同じ値・同じ IsSet を持つ 2 つのオブジェクトは等価 | 正常系 | § 2.3 | Test Constructor, Equals/GetHashCode |
| VO-EQ-02 | 同一参照のオブジェクトは等価 | 正常系（自己参照） | § 2.3 | Test Constructor, Equals/GetHashCode |
| VO-EQ-03 | 複数コンポーネントがすべて一致する場合は等価 | 正常系 | § 2.3 | Test Constructor, Equals/GetHashCode |
| VO-EQ-04 | 両方が IsSet=false かつ値が同じ場合は等価 | 正常系 | § 2.3 | Test Constructor, Equals/GetHashCode |

### 観点グループ NE：`Equals` — 非等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | 依存仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| VO-NE-01 | コンポーネント値が異なる場合は非等価 | 異常系 | § 2.3 | Test Constructor, Equals/GetHashCode |
| VO-NE-02 | IsSet が異なる場合は非等価 | 境界値テスト | § 2.3 | Test Constructor, Equals/GetHashCode |
| VO-NE-03 | 型が異なる場合は非等価（値が同じでも） | 異常系 | § 2.3 | Test Constructor, Equals/GetHashCode |
| VO-NE-04 | null との比較は非等価 | 例外/異常系 | § 2.3 | Test Constructor, Equals/GetHashCode |
| VO-NE-05 | 複数コンポーネントの一部が異なる場合は非等価 | 境界値テスト | § 2.3 | Test Constructor, Equals/GetHashCode |

### 観点グループ HC：`GetHashCode`

| 観点ID | 観点（説明） | 分類 | 依存仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| VO-HC-01 | Equals=true の 2 つのオブジェクトは同一ハッシュ値 | 正常系 | § 2.4 | Equals/GetHashCode |
| VO-HC-02 | IsSet が異なるとハッシュ値が異なる | 境界値テスト | § 2.4 | Equals/GetHashCode |
| VO-HC-03 | コンポーネント値が異なるとハッシュ値が異なる | 境界値テスト | § 2.4 | Equals/GetHashCode |
| VO-HC-04 | ハッシュ値は複数呼び出しで一貫している | 正常系（副作用なし） | § 2.4 | Equals/GetHashCode |

### 観点グループ OP：`==` / `!=` 演算子

| 観点ID | 観点（説明） | 分類 | 依存仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| VO-OP-01 | 等価なオブジェクトに == を適用すると true | 正常系 | § 2.5 | Equals/GetHashCode |
| VO-OP-02 | 非等価なオブジェクトに == を適用すると false | 異常系 | § 2.5 | Equals/GetHashCode |
| VO-OP-03 | 両辺が null のとき == は true | 準正常系（null チェック） | § 2.5 | Equals/GetHashCode |
| VO-OP-04 | 片方のみ null のとき == は false | 異常系 | § 2.5 | Equals/GetHashCode |
| VO-OP-05 | != は == の否定と一致 | 正常系 | § 2.5 | Equals/GetHashCode |

### 観点グループ TS：`ToString`

| 観点ID | 観点（説明） | 分類 | 依存仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| VO-TS-01 | IsSet=false のとき "Unset" を返す | 正常系 | § 2.6 | Test Constructor |
| VO-TS-02 | IsSet=true のとき、コンポーネント値をそのまま返す | 正常系 | § 2.6 | Test Constructor |
| VO-TS-03 | ToString 出力に IsSet の値そのものが含まれない | 正常系 | § 2.6 | Test Constructor |

### 観点グループ VLR：`Validate` — 値の長さ/形式チェック（RespondentName 固有）

| 観点ID | 観点（説明） | 分類 | 依存仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| VO-VLR-01 | 1～50文字の名前は From で受け入れられる | 正常系 | RespondentName.Validate | Test Constructor |
| VO-VLR-02 | 空文字列は ArgumentException をスロー | 異常系（エッジケース） | RespondentName.Validate | Test Constructor |
| VO-VLR-03 | 51文字以上の名前は ArgumentException をスロー | 異常系 | RespondentName.Validate | Test Constructor |

---

## 4. テスト観点別の検証シナリオ

### 観点 VO-IS-01：IsSet=true で構築したオブジェクトは IsSet が true を返す

#### 4.1.1 テスト観点

RespondentName をコンストラクタで IsSet=true で初期化した場合、IsSet プロパティが true を返す。

#### 4.1.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.1.2.1 | 正常系 | 名前文字列入力、IsSet=true |

#### 4.1.3 前提条件

- RespondentName コンストラクタが利用可能
- IsSet プロパティが読み取り可能

#### 4.1.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.1.2.1 | new RespondentName("太郎", IsSet: true) | 正常な名前入力 |

#### 4.1.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.1.2.1 | IsSet == true | プロパティが true |

#### 4.1.6 判定基準

- [ ] IsSet=true を指定した場合、IsSet プロパティが true を返す
- [ ] IsSet プロパティは読み取り専用で、外部から変更不可

---

### 観点 VO-OPT-01：Unset() は IsSet=false のインスタンスを返す

#### 4.2.1 テスト観点

IOptionalValueObject を実装した RespondentName に対して Unset() 静的メソッドを呼び出した場合、IsSet=false のインスタンスが返される。

#### 4.2.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.2.2.1 | 正常系 | Unset() の呼び出し |
| 4.2.2.2 | 正常系 | Unset().TryGetValue() の連鎖呼び出し |

#### 4.2.3 前提条件

- RespondentName が IOptionalValueObject を実装している

#### 4.2.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.2.2.1 | RespondentName.Unset() | 未設定のRespondentName |
| 4.2.2.2 | RespondentName.Unset().TryGetValue(out _) | チェーン呼び出し |

#### 4.2.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.2.2.1 | IsSet == false | 未設定状態 |
| 4.2.2.2 | bool result == false | TryGetValue でfalse返却 |

#### 4.2.6 判定基準

- [ ] Unset() は IsSet=false のインスタンスを返す
- [ ] Unset インスタンスから値取得を試みると false を返す
- [ ] Unset インスタンスの ToString() は "Unset" を返す

---

### 観点 VO-OPT-03：TryFrom(null) は true を返し、Unset インスタンスを返す

#### 4.3.1 テスト観点

IOptionalValueObject を実装した RespondentName に対して TryFrom(null) を呼び出した場合、true を返し、Unset インスタンスが out パラメータに設定される。

**これは、「null を異常値ではなく未設定状態として扱う」という IOptionalValueObject の核となる仕様である。**

#### 4.3.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.3.2.1 | 正常系（null吸収） | null 入力で true ＋ Unset 返却 |
| 4.3.2.2 | 正常系 | null → Unset の IsSet 確認 |

#### 4.3.3 前提条件

- RespondentName が IOptionalValueObject を実装している
- TryFrom(string?) メソッドが定義されている

#### 4.3.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.3.2.1 | RespondentName.TryFrom((string?)null, out var result) | null 入力 |
| 4.3.2.2 | result.IsSet | 返却インスタンス確認 |

#### 4.3.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.3.2.1 | true | 返り値が true（正常処理） |
| 4.3.2.2 | false | Unset 状態（IsSet=false） |

#### 4.3.6 判定基準

- [ ] TryFrom(null) は true を返す（例外ではなく正常系）
- [ ] 返却インスタンスは IsSet=false である
- [ ] result.Equals(RespondentName.Unset()) == true

---

### 観点 VO-OPT-04：TryFrom(valid) は true を返し、設定済みインスタンスを返す

#### 4.4.1 テスト観点

IOptionalValueObject を実装した RespondentName に対して TryFrom(有効値) を呼び出した場合、true を返し、設定済みインスタンスが out パラメータに設定される。

#### 4.4.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.4.2.1 | 正常系 | 有効値入力で true ＋ 設定済み 返却 |
| 4.4.2.2 | 正常系 | 返却インスタンスから値取得 |

#### 4.4.3 前提条件

- RespondentName が IOptionalValueObject を実装している
- TryFrom(string?) メソッドが定義されている

#### 4.4.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.4.2.1 | RespondentName.TryFrom("太郎", out var result) | 有効な名前 |
| 4.4.2.2 | result.TryGetValue(out var value) | 値取得 |

#### 4.4.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.4.2.1 | true | 返り値が true |
| 4.4.2.2 | ("太郎", true) | 値が "太郎" で取得成功 |

#### 4.4.6 判定基準

- [ ] TryFrom(有効値) は true を返す
- [ ] 返却インスタンスは IsSet=true である
- [ ] result.TryGetValue(out value) で value == "太郎"

---

### 観点 VO-OPT-05：TryFrom(invalid) は false を返し、Unset インスタンスを返す

#### 4.5.1 テスト観点

IOptionalValueObject を実装した RespondentName に対して TryFrom(無効値) を呼び出した場合、false を返し、Unset インスタンスが out パラメータに設定される。

検証失敗時は例外ではなく、false ＋ Unset という安全な結果を返す（TryPattern）。

#### 4.5.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.5.2.1 | 異常系（検証失敗） | 無効値入力で false ＋ Unset 返却 |
| 4.5.2.2 | 異常系 | 返却インスタンスの IsSet 確認 |

#### 4.5.3 前提条件

- RespondentName が IOptionalValueObject を実装している
- Validate() メソッドで無効値を検出できる

#### 4.5.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.5.2.1 | RespondentName.TryFrom("", out var result) | 空文字列（無効） |
| 4.5.2.2 | RespondentName.TryFrom("x" * 51, out var result) | 51文字以上（無効） |

#### 4.5.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.5.2.1 | false | 返り値が false（検証失敗） |
| 4.5.2.2 | false | 返却インスタンスは Unset |

#### 4.5.6 判定基準

- [ ] TryFrom(無効値) は false を返す（例外ではない）
- [ ] 返却インスタンスは IsSet=false である
- [ ] 例外は発生しない（TryPattern）

---

### 観点 VO-OPT-06：TryGetValue(out value) は IsSet=true で true を返す

#### 4.6.1 テスト観点

IOptionalValueObject を実装した RespondentName が IsSet=true の場合、TryGetValue() は true を返し、値が out パラメータに設定される。

#### 4.6.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.6.2.1 | 正常系 | IsSet=true で TryGetValue() 呼び出し |
| 4.6.2.2 | 正常系 | out パラメータに値が設定される |

#### 4.6.3 前提条件

- RespondentName が IOptionalValueObject を実装している
- インスタンスが IsSet=true 状態である

#### 4.6.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.6.2.1 | RespondentName.From("太郎") | 設定済みインスタンス |
| 4.6.2.2 | obj.TryGetValue(out var value) | 値取得試行 |

#### 4.6.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.6.2.1 | true | 返り値が true |
| 4.6.2.2 | "太郎" | 値が取得できる |

#### 4.6.6 判定基準

- [ ] TryGetValue で true が返される
- [ ] out 値が正しく設定される

---

### 観点 VO-OPT-07：TryGetValue(out value) は IsSet=false で false を返す

#### 4.7.1 テスト観点

IOptionalValueObject を実装した RespondentName が IsSet=false の場合、TryGetValue() は false を返す。

#### 4.7.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.7.2.1 | 正常系 | IsSet=false で TryGetValue() 呼び出し |

#### 4.7.3 前提条件

- RespondentName が IOptionalValueObject を実装している
- インスタンスが IsSet=false 状態である

#### 4.7.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.7.2.1 | RespondentName.Unset() | 未設定インスタンス |

#### 4.7.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.7.2.1 | false | 返り値が false |

#### 4.7.6 判定基準

- [ ] Unset インスタンスの TryGetValue で false が返される

---

### 観点 VO-EQ-01：同じ型・同じ値・同じ IsSet を持つ 2 つのオブジェクトは等価

#### 4.8.1 テスト観点

RespondentName における等価性検証。

#### 4.8.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.8.2.1 | 正常系 | 同じ値で From() 生成した 2 つのインスタンス |

#### 4.8.3 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.8.2.1 | obj1 = From("太郎"), obj2 = From("太郎") | 同一値 |

#### 4.8.4 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.8.2.1 | true | Equals == true |

#### 4.8.5 判定基準

- [ ] obj1.Equals(obj2) == true
- [ ] obj1 == obj2 == true
- [ ] obj1.GetHashCode() == obj2.GetHashCode()

---

### 観点 VO-TS-01：IsSet=false のとき "Unset" を返す

#### 4.9.1 テスト観点

RespondentName の ToString() が IsSet の状態に応じた適切な文字列を返す。

#### 4.9.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.9.2.1 | 正常系 | Unset().ToString() |

#### 4.9.3 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.9.2.1 | RespondentName.Unset() | 未設定インスタンス |

#### 4.9.4 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.9.2.1 | "Unset" | 文字列が "Unset" |

#### 4.9.5 判定基準

- [ ] Unset インスタンスの ToString() は "Unset" を返す

---

### 観点 VO-TS-02：IsSet=true のとき、値をそのまま返す

#### 4.10.1 テスト観点

设定済みインスタンスの ToString() は値をそのまま返す。

#### 4.10.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.10.2.1 | 正常系 | From("太郎").ToString() |

#### 4.10.3 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.10.2.1 | RespondentName.From("太郎") | 設定済みインスタンス |

#### 4.10.4 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.10.2.1 | "太郎" | 値が返される |

#### 4.10.5 判定基準

- [ ] 設定済みインスタンスの ToString() は値をそのまま返す

---

### 観点 VO-VLR-01：1～50文字の名前は From で受け入れられる

#### 4.11.1 テスト観点

RespondentName.Validate() が 1～50文字の有効範囲をチェック。

#### 4.11.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.11.2.1 | 正常系 | 1文字 |
| 4.11.2.2 | 正常系 | 25文字（中間値） |
| 4.11.2.3 | 正常系 | 50文字（上限） |

#### 4.11.3 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.11.2.1 | "A" | 1文字 |
| 4.11.2.2 | "AbCdEfGhIjKlMnOpQrStUvWxYz12345" | 25文字 |
| 4.11.2.3 | "A" * 50 | 50文字 |

#### 4.11.4 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.11.2.1 | IsSet=true | インスタンス生成成功 |
| 4.11.2.2 | IsSet=true | インスタンス生成成功 |
| 4.11.2.3 | IsSet=true | インスタンス生成成功 |

#### 4.11.5 判定基準

- [ ] 1～50文字の名前は From() で受け入れられる
- [ ] 返却インスタンスは IsSet=true

---

### 観点 VO-VLR-02：空文字列は ArgumentException をスロー

#### 4.12.1 テスト観点

RespondentName.Validate() が空文字列を拒否。

#### 4.12.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.12.2.1 | 異常系 | 空文字列入力 |

#### 4.12.3 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.12.2.1 | RespondentName.From("") | 空文字列 |

#### 4.12.4 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.12.2.1 | ArgumentException | 例外がスロー |

#### 4.12.5 判定基準

- [ ] 空文字列の From() は ArgumentException をスロー

---

### 観点 VO-VLR-03：51文字以上の名前は ArgumentException をスロー

#### 4.13.1 テスト観点

RespondentName.Validate() が 51文字以上を拒否。

#### 4.13.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.13.2.1 | 異常系 | 51文字 |
| 4.13.2.2 | 異常系 | 100文字 |

#### 4.13.3 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.13.2.1 | RespondentName.From("A" * 51) | 51文字 |
| 4.13.2.2 | RespondentName.From("A" * 100) | 100文字 |

#### 4.13.4 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.13.2.1 | ArgumentException | 例外がスロー |
| 4.13.2.2 | ArgumentException | 例外がスロー |

#### 4.13.5 判定基準

- [ ] 51文字以上の From() は ArgumentException をスロー

---

## 5. 判定基準

### 5.1 全般的な判定基準

| 項目 | 基準 |
|------|------|
| **不変性** | オブジェクト生成後、プロパティが変更されないこと |
| **等価性** | Equals の結果が論理的に一貫していること |
| **ハッシュ整合性** | Equals=true のペアが同一ハッシュ値を返すこと |
| **演算子整合性** | == と Equals の結果が矛盾しないこと |
| **文字列化** | ToString が仕様通りの形式を返すこと |
| **null吸収** | TryFrom(null) が true ＋ Unset を返すこと |
| **値操作安全性** | TryGetValue が例外ではなく bool で結果を示すこと |

### 5.2 検証方法

```
【VO-IS 観点の検証】
  var vo = new RespondentName("太郎", IsSet: true);
  Assert.True(vo.IsSet);

【VO-OPT-03 観点の検証 - TryFrom(null) テスト】
  var result = RespondentName.TryFrom((string?)null, out var unset);
  Assert.True(result);              // 返り値が true
  Assert.False(unset.IsSet);        // Unset 状態
  Assert.Equal(RespondentName.Unset(), unset);  // Unset と等価

【VO-OPT-04 観点の検証 - TryFrom(valid) テスト】
  var result = RespondentName.TryFrom("太郎", out var vo);
  Assert.True(result);              // 返り値が true
  Assert.True(vo.IsSet);            // 設定済み状態
  Assert.True(vo.TryGetValue(out var v));
  Assert.Equal("太郎", v);      // 値が一致

【VO-OPT-05 観点の検証 - TryFrom(invalid) テスト】
  var result = RespondentName.TryFrom("", out var vo);
  Assert.False(result);             // 返り値が false
  Assert.False(vo.IsSet);           // Unset 状態

【VO-EQ 観点の検証】
  var vo1 = RespondentName.From("太郎");
  var vo2 = RespondentName.From("太郎");
  Assert.True(vo1.Equals(vo2));
  Assert.True(vo1 == vo2);

【VO-HC 観点の検証】
  var vo1 = RespondentName.From("太郎");
  var vo2 = RespondentName.From("太郎");
  Assert.Equal(vo1.GetHashCode(), vo2.GetHashCode());

【VO-TS 観点の検証】
  var vo = RespondentName.Unset();
  Assert.Equal("Unset", vo.ToString());

【VO-VLR 観点の検証】
  var vo = RespondentName.From("太郎");  // 正常
  Assert.Throws<ArgumentException>(() => RespondentName.From(""));    // 空文字列
  Assert.Throws<ArgumentException>(() => RespondentName.From("A" * 51));  // 51文字以上
```

---

## 6. テスト用実装の設定

### 6.1 前提環境

| 項目 | 内容 |
|------|------|
| **テスティングフレームワーク** | xUnit / NUnit |
| **Assertion** | Assert.True / Assert.Equal / Assert.Throws |
| **Mock/Stub** | 不要（ValueObject は依存性なし） |

### 6.2 テストクラス構成

```
tests/Advance.Unit.Tests/
└── ValueObjects/
	├── CarModelTests.cs
	├── RespondentAgeTests.cs
	├── RespondentNameTests.cs
	└── ...
```

---

## 7. 改版履歴

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2025-xx-xx | 初版作成（カスタム形式） |
| **2.0** | **2026-06-03** | **ValueObjectテンプレート v2.0 に移行。IOptionalValueObject対応版。VO-IS/OPT/EQ/NE/HC/OP/TS 観点を導入。RespondentName 固有の VO-VLR（値の長さ範囲）観点を追加。** |

---

## 補足：何が改善されたのか

### v1.0 → v2.0 の変更

**追加テスト観点**:
- **VO-OPT-01** ~ **VO-OPT-07**: IOptionalValueObject メソッドの 7 つのテスト観点

**重要な追加**:
- **VO-OPT-03: TryFrom(null) は true を返し、Unset を返す**  
  → これが核となる仕様。null を異常値ではなく「未設定状態」として扱う設計を検証

**テスト観点ID体系の整備**:
- VO-IS（IsSet プロパティ）
- **VO-OPT（IOptionalValueObject メソッド）← 新規**
- VO-EQ（Equals - 等価）
- VO-NE（Equals - 非等価）
- VO-HC（GetHashCode）
- VO-OP（==, != 演算子）
- VO-TS（ToString）
- **VO-VLR（値の長さ/形式チェック）← RespondentName 固有**

v1.0 では IOptionalValueObject の仕様を完全にカバーできていませんでしたが、v2.0 では対応しています。
