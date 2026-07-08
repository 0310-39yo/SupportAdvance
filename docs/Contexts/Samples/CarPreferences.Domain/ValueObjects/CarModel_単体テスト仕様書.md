# 単体テスト仕様書テンプレート — CarModel

**プロジェクト:** SupportAdvance  
**テスト対象:** ValueObject（値オブジェクト）  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-07-08  
**対応観点ID:** VO-IS-01/02 | VO-OPT-01～07 | VO-EQ-01～04 | VO-NE-01～05 | VO-HC-01～04 | VO-OP-01～05 | VO-TS-01～03 | VO-CV-01～07 | VO-VF-01～03

---

## 0. 本書の位置づけ

本書は、Domain層の ValueObject クラス `CarModel` が、技術仕様書および詳細設計書で定義された
**等価性比較・ハッシュ・文字列化・値の不変性・IsSet状態管理・オプショナル値操作** を満たすことを確認するテスト仕様書。

---

## 1. テスト目的

ValueObject の各メンバーが、以下の仕様を満たすことを確認する：

- **値の不変性**：オブジェクト生成後、内部状態が変更されない
- **等価性比較**：同じ値を持つ2つのオブジェクトは等価である
- **ハッシュ整合性**：Equals=true のオブジェクトは同一ハッシュ値
- **文字列化**：ToString が仕様通りの形式で返される
- **IsSet状態管理**：IsSet フラグが正しく機能する
- **オプショナル値操作**（IOptionalValueObject実装時）：Unset・From・TryFrom・TryGetValue の動作
- **列挙型ValueObject対応**：静的フィールド（Unknown, Sedan, SportUtility等）と値の範囲チェック（0～6）

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | `CarModel` |
| **名前空間** | `SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects` |
| **依存仕様** | ValueObject技術仕様書 v1.1 + IOptionalValueObject技術仕様書 v1.3 |
| **前提** | ValueObjectComponentNormalizer のテストが完了していること |

---

## 2.X テスト用実装と DI 設定

このテンプレートでは、値オブジェクト（CarModel）のテストに使用するテスト用実装を明記します。

| 実装名 | 役割 | 対応する観点ID | 説明 |
|--------|------|----------------|------|
| DIFixture | DI コンテナの構成 | 全観点 | 依存性注入による検証（不要な場合は N/A） |
| Test Constructor | テスト用コンストラクタ | VO-IS-01～02, VO-OPT-01～07 | 値オブジェクトの初期化検証 |
| Equals/GetHashCode | 標準メソッド | VO-EQ, VO-HC など | .NET の Equals・GetHashCode メソッド検証 |

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
| VO-OPT-06 | TryGetValue(out value) は IsSet=true で true を返す（基底クラスから継承） | 正常系 | IOptionalValueObject v1.3 § 3.4 | Test Constructor |
| VO-OPT-07 | TryGetValue(out value) は IsSet=false で false を返す（基底クラスから継承） | 正常系 | IOptionalValueObject v1.3 § 3.4 | Test Constructor |

### 観点グループ EQ：`Equals` — 等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | 依存仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| VO-EQ-01 | 同じ型・同じ値・同じ IsSet を持つ 2 つのオブジェクトは等価 | 正常系 | § 2.3 | Test Constructor, Equals/GetHashCode |
| VO-EQ-02 | 同一参照のオブジェクトは等価 | 正常系（自己参照） | § 2.3 | Test Constructor, Equals/GetHashCode |
| VO-EQ-03 | 複数コンポーネント（IsSet と ValueField）がすべて一致する場合は等価 | 正常系 | § 2.3 | Test Constructor, Equals/GetHashCode |
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
| VO-TS-02 | IsSet=true のとき、コンポーネントをカンマ区切りで連結 | 正常系 | § 2.6 | Test Constructor |
| VO-TS-03 | ToString 出力に IsSet の値そのものが含まれない | 正常系 | § 2.6 | Test Constructor |

### 観点グループ CV：`CarModel フィールド初期化`（列挙型ValueObject固有）

| 観点ID | 観点（説明） | 分類 | 依存仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| VO-CV-01 | Unknown フィールド (0) が IsSet=true で初期化される | 正常系 | § 3.0 | Test Constructor |
| VO-CV-02 | Sedan フィールド (1) が IsSet=true で初期化される | 正常系 | § 3.0 | Test Constructor |
| VO-CV-03 | SportUtility フィールド (2) が IsSet=true で初期化される | 正常系 | § 3.0 | Test Constructor |
| VO-CV-04 | Hatchback フィールド (3) が IsSet=true で初期化される | 正常系 | § 3.0 | Test Constructor |
| VO-CV-05 | Coupe フィールド (4) が IsSet=true で初期化される | 正常系 | § 3.0 | Test Constructor |
| VO-CV-06 | Minivan フィールド (5) が IsSet=true で初期化される | 正常系 | § 3.0 | Test Constructor |
| VO-CV-07 | Other フィールド (6) が IsSet=true で初期化される | 正常系 | § 3.0 | Test Constructor |

### 観点グループ VF：`値範囲・バリデーション`（列挙型ValueObject固有）

| 観点ID | 観点（説明） | 分類 | 依存仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| VO-VF-01 | ValueField が 0～6 の範囲内は Validate パス | 正常系 | § 4.0 | Test Constructor |
| VO-VF-02 | ValueField が負数は ArgumentOutOfRangeException | 異常系 | § 4.0 | Test Constructor |
| VO-VF-03 | ValueField が 7 以上は ArgumentOutOfRangeException | 異常系 | § 4.0 | Test Constructor |

### 観点グループ GVC：`GetValueComponents()` メソッド

| 観点ID | 観点（説明） | 分類 | 依存仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| VO-GVC-01 | IsSet=true の場合、ValueField を yield return する | 正常系 | § 4.0 | Test Constructor |
| VO-GVC-02 | IsSet=false の場合、何も yield return しない（空列挙） | 正常系 | § 4.0 | Test Constructor |
| VO-GVC-03 | GetEqualityComponents との連携で IsSet が先頭に付加されることを確認 | 正常系 | § 4.0 | Test Constructor |

### 観点グループ DN：`GetDisplayName()` メソッド（列挙型ValueObject固有）

| 観点ID | 観点（説明） | 分類 | 依存仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| VO-DN-01 | ValueField = 0 の場合「不明」を返す | 正常系 | § 5.0 | Test Constructor |
| VO-DN-02 | ValueField = 1～6 の場合、対応する業務名称を返す | 正常系 | § 5.0 | Test Constructor |
| VO-DN-03 | ValueField が範囲外の場合、ArgumentOutOfRangeException をスロー | 異常系 | § 5.0 | Test Constructor |

---

## 4. テスト観点別の検証シナリオ

### 観点 VO-IS-01：IsSet=true で構築したオブジェクトは IsSet が true を返す

#### 4.1.1 テスト観点

ValueObject をコンストラクタで IsSet=true で初期化した場合、IsSet プロパティが true を返す。

#### 4.1.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.1.2.1 | 正常系 | 単一値（0）、IsSet=true |
| 4.1.2.2 | 正常系 | 複数値テスト（1, 3, 6）、IsSet=true |

#### 4.1.3 前提条件

- ValueObject コンストラクタが利用可能
- IsSet プロパティが読み取り可能

#### 4.1.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.1.2.1 | new CarModel(0) | Unknown |
| 4.1.2.2 | new CarModel(3) | Hatchback |

※注：実際のコンストラクタがprivateの場合は、From()メソッド経由でテスト

#### 4.1.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.1.2.1 | IsSet == true | プロパティが true |
| 4.1.2.2 | IsSet == true | プロパティが true |

#### 4.1.6 判定基準

- [ ] From(value) で初期化した場合、IsSet プロパティが true を返す
- [ ] IsSet プロパティは読み取り専用で、外部から変更不可

---

### 観点 VO-IS-02：IsSet=false で構築したオブジェクトは IsSet が false を返す

#### 4.2.1 テスト観点

Unset() メソッド経由で初期化したオブジェクトの IsSet プロパティが false を返す。

#### 4.2.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.2.2.1 | 正常系 | Unset() の呼び出し |
| 4.2.2.2 | 正常系 | Unset() シングルトン確認 |

#### 4.2.3 前提条件

- Unset() 静的メソッドが定義されている

#### 4.2.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.2.2.1 | CarModel.Unset() | 未設定状態 |
| 4.2.2.2 | CarModel.Unset() | 複数回呼び出し検証用 |

#### 4.2.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.2.2.1 | IsSet == false | 未設定フラグ |
| 4.2.2.2 | ReferenceEquals == true | シングルトン |

#### 4.2.6 判定基準

- [ ] Unset() は IsSet=false のインスタンスを返す
- [ ] Unset() 複数呼び出しで同一インスタンスを返す（シングルトン）
- [ ] Unset インスタンスの ToString() は "Unset" を返す

---

### 観点 VO-OPT-01：Unset() は IsSet=false のインスタンスを返す

#### 4.3.1 テスト観点

IOptionalValueObject を実装した ValueObject に対して Unset() 静的メソッドを呼び出した場合、IsSet=false のインスタンスが返される。

#### 4.3.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.3.2.1 | 正常系 | Unset() の呼び出し |
| 4.3.2.2 | 正常系 | Unset().TryGetValue() の連鎖呼び出し |

#### 4.3.3 前提条件

- ValueObject が IOptionalValueObject を実装している

#### 4.3.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.3.2.1 | CarModel.Unset() | 未設定インスタンス取得 |
| 4.3.2.2 | CarModel.Unset().TryGetValue(out _) | 連鎖呼び出し検証 |

#### 4.3.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.3.2.1 | IsSet == false | 未設定状態 |
| 4.3.2.2 | result == false | TryGetValue で false 返却 |

#### 4.3.6 判定基準

- [ ] Unset() は IsSet=false のインスタンスを返す
- [ ] Unset インスタンスから値取得を試みると false を返す
- [ ] Unset インスタンスの ToString() は "Unset" を返す

---

### 観点 VO-OPT-02：From(value) は IsSet=true のインスタンスを返す

#### 4.4.1 テスト観点

IOptionalValueObject を実装した ValueObject に対して From() 静的メソッドを呼び出した場合、IsSet=true のインスタンスが返される。

#### 4.4.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.4.2.1 | 正常系 | 有効な値で From() を呼び出し |
| 4.4.2.2 | 正常系 | From() 後に TryGetValue() で値取得 |

#### 4.4.3 前提条件

- ValueObject が IOptionalValueObject を実装している
- From() メソッドが定義されている

#### 4.4.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.4.2.1 | CarModel.From(1) | 有効な値（Sedan） |
| 4.4.2.2 | CarModel.From(2) | 有効な値（SportUtility） |

#### 4.4.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.4.2.1 | IsSet == true | 設定済み状態 |
| 4.4.2.2 | result == 2 | 値が取得できる |

#### 4.4.6 判定基準

- [ ] From(value) は IsSet=true のインスタンスを返す
- [ ] From インスタンスから値取得を試みると true を返す
- [ ] 取得値は入力値と一致する

---

### 観点 VO-OPT-03：TryFrom(null) は true を返し、Unset インスタンスを返す

#### 4.5.1 テスト観点

IOptionalValueObject を実装した ValueObject に対して TryFrom(null) を呼び出した場合、true を返し、Unset インスタンスが out パラメータに設定される。

**これは「null を異常値ではなく未設定状態として扱う」という IOptionalValueObject の核となる仕様である。**

#### 4.5.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.5.2.1 | 正常系（null吸収） | null 入力で true + Unset 返却 |
| 4.5.2.2 | 正常系 | null → Unset の IsSet 確認 |

#### 4.5.3 前提条件

- ValueObject が IOptionalValueObject を実装している
- TryFrom(T?) メソッドが定義されている

#### 4.5.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.5.2.1 | CarModel.TryFrom((int?)null, out var result) | null 入力 |
| 4.5.2.2 | result.IsSet | 返却インスタンス確認 |

#### 4.5.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.5.2.1 | true | 返り値が true（正常処理） |
| 4.5.2.2 | false | Unset 状態（IsSet=false） |

#### 4.5.6 判定基準

- [ ] TryFrom(null) は true を返す（例外ではなく正常系）
- [ ] 返却インスタンスは IsSet=false である
- [ ] result.Equals(CarModel.Unset()) == true

---

### 観点 VO-OPT-04：TryFrom(valid) は true を返し、設定済みインスタンスを返す

#### 4.6.1 テスト観点

IOptionalValueObject を実装した ValueObject に対して TryFrom(有効値) を呼び出した場合、true を返し、設定済みインスタンスが out パラメータに設定される。

#### 4.6.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.6.2.1 | 正常系 | 有効値入力で true + 設定済み 返却 |
| 4.6.2.2 | 正常系 | 返却インスタンスから値取得 |

#### 4.6.3 前提条件

- ValueObject が IOptionalValueObject を実装している
- TryFrom(T?) メソッドが定義されている

#### 4.6.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.6.2.1 | CarModel.TryFrom(1, out var result) | 有効な値（Sedan） |
| 4.6.2.2 | result.TryGetValue(out var value) | 値取得 |

#### 4.6.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.6.2.1 | true | 返り値が true |
| 4.6.2.2 | (1, true) | 値が 1 で取得成功 |

#### 4.6.6 判定基準

- [ ] TryFrom(有効値) は true を返す
- [ ] 返却インスタンスは IsSet=true である
- [ ] result.TryGetValue(out value) で value == 1

---

### 観点 VO-OPT-05：TryFrom(invalid) は false を返し、Unset インスタンスを返す

#### 4.7.1 テスト観点

IOptionalValueObject を実装した ValueObject に対して TryFrom(無効値) を呼び出した場合、false を返し、Unset インスタンスが out パラメータに設定される。

検証失敗時は例外ではなく、false + Unset という安全な結果を返す（TryPattern）。

#### 4.7.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.7.2.1 | 異常系（検証失敗） | 無効値入力で false + Unset 返却 |
| 4.7.2.2 | 異常系 | 返却インスタンスの IsSet 確認 |

#### 4.7.3 前提条件

- ValueObject が IOptionalValueObject を実装している
- Validate() メソッドで無効値を検出できる

#### 4.7.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.7.2.1 | CarModel.TryFrom(-1, out var result) | 無効な値（負数） |
| 4.7.2.2 | CarModel.TryFrom(7, out var result) | 無効な値（7以上） |

#### 4.7.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.7.2.1 | false | 返り値が false（検証失敗） |
| 4.7.2.2 | false | 返却インスタンスは Unset |

#### 4.7.6 判定基準

- [ ] TryFrom(無効値) は false を返す（例外ではない）
- [ ] 返却インスタンスは IsSet=false である
- [ ] 例外は発生しない（TryPattern）

---

### 観点 VO-OPT-06：TryGetValue(out value) は IsSet=true で true を返す

#### 4.8.1 テスト観点

IOptionalValueObject を実装した ValueObject が IsSet=true の場合、TryGetValue() は true を返し、値が out パラメータに設定される。

#### 4.8.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.8.2.1 | 正常系 | IsSet=true で TryGetValue() 呼び出し |
| 4.8.2.2 | 正常系 | out パラメータに値が設定される |

#### 4.8.3 前提条件

- ValueObject が IOptionalValueObject を実装している
- インスタンスが IsSet=true 状態である

#### 4.8.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.8.2.1 | CarModel.From(2).TryGetValue(out var val) | IsSet=true インスタンス |
| 4.8.2.2 | val | 返却値確認 |

#### 4.8.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.8.2.1 | true | 返り値が true |
| 4.8.2.2 | 2 | val が 2 である |

#### 4.8.6 判定基準

- [ ] IsSet=true のインスタンスから TryGetValue() は true を返す
- [ ] out パラメータに正しい値が設定される

---

### 観点 VO-OPT-07：TryGetValue(out value) は IsSet=false で false を返す

#### 4.9.1 テスト観点

IOptionalValueObject を実装した ValueObject が IsSet=false の場合、TryGetValue() は false を返す。

#### 4.9.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.9.2.1 | 正常系 | IsSet=false で TryGetValue() 呼び出し |
| 4.9.2.2 | 正常系 | out パラメータは default 値のまま |

#### 4.9.3 前提条件

- ValueObject が IOptionalValueObject を実装している
- インスタンスが IsSet=false 状態である

#### 4.9.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.9.2.1 | CarModel.Unset().TryGetValue(out var val) | IsSet=false インスタンス |
| 4.9.2.2 | val | デフォルト値確認 |

#### 4.9.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.9.2.1 | false | 返り値が false |
| 4.9.2.2 | default(int) | val が 0 (default) |

#### 4.9.6 判定基準

- [ ] IsSet=false の インスタンスから TryGetValue() は false を返す
- [ ] out パラメータはデフォルト値のまま

---

### 観点 VO-EQ-01：同じ型・同じ値・同じ IsSet を持つ 2 つのオブジェクトは等価

#### 4.10.1 テスト観点

同じ値を持つ 2 つの CarModel オブジェクトは等価である。

#### 4.10.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.10.2.1 | 正常系 | 単一値テスト（From(1) vs Sedan） |
| 4.10.2.2 | 正常系 | 複数パターン（0, 2, 6） |

#### 4.10.3 前提条件

- Equals メソッドが実装されている
- 複数コンポーネント（IsSet, ValueField）が一致する

#### 4.10.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.10.2.1 | CarModel.From(1).Equals(CarModel.Sedan) | 同一値 |
| 4.10.2.2 | CarModel.From(0).Equals(CarModel.Unknown) | Unknown テスト |

#### 4.10.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.10.2.1 | true | 等価である |
| 4.10.2.2 | true | 等価である |

#### 4.10.6 判定基準

- [ ] From(1).Equals(Sedan) == true
- [ ] Equals は IsSet と ValueField の両方を比較している

---

### 観点 VO-EQ-02：同一参照のオブジェクトは等価

#### 4.11.1 テスト観点

同一参照のオブジェクト同士は常に等価である（自己参照）。

#### 4.11.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.11.2.1 | 正常系（自己参照） | 同一参照テスト |

#### 4.11.3 前提条件

- Equals メソッドが自己参照チェックを実装している

#### 4.11.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.11.2.1 | var x = CarModel.Sedan; x.Equals(x) | 自己参照 |

#### 4.11.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.11.2.1 | true | 自己参照は常に等価 |

#### 4.11.6 判定基準

- [ ] 自己参照のオブジェクトは Equals で true を返す

---

### 観点 VO-EQ-03：複数コンポーネント（IsSet と ValueField）がすべて一致する場合は等価

#### 4.12.1 テスト観点

IsSet と ValueField の両方が一致すれば等価である。

#### 4.12.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.12.2.1 | 正常系 | IsSet=true, ValueField=3 |
| 4.12.2.2 | 正常系 | IsSet=false, ValueField=0 |

#### 4.12.3 前提条件

- 複数コンポーネント（IsSet, ValueField）が一致している

#### 4.12.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.12.2.1 | CarModel.From(3).Equals(CarModel.From(3)) | Hatchback 同一 |
| 4.12.2.2 | CarModel.Unset().Equals(CarModel.Unset()) | Unset 同一 |

#### 4.12.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.12.2.1 | true | コンポーネント一致 |
| 4.12.2.2 | true | Unset は特殊ケース |

#### 4.12.6 判定基準

- [ ] 複数コンポーネントがすべて一致すれば等価

---

### 観点 VO-EQ-04：両方が IsSet=false かつ値が同じ場合は等価

#### 4.13.1 テスト観点

IsSet=false の 2 つのインスタンス同士は等価で あり、値の内容は参照されない。

#### 4.13.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.13.2.1 | 正常系 | Unset() 同士 |

#### 4.13.3 前提条件

- Unset() がシングルトンで返される

#### 4.13.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.13.2.1 | CarModel.Unset().Equals(CarModel.Unset()) | 両方 Unset |

#### 4.13.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.13.2.1 | true | IsSet=false で等価 |

#### 4.13.6 判定基準

- [ ] IsSet=false 同士は等価

---

### 観点 VO-NE-01：コンポーネント値が異なる場合は非等価

#### 4.14.1 テスト観点

ValueField が異なる 2 つのオブジェクトは非等価である。

#### 4.14.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.14.2.1 | 異常系 | ValueField が異なる（1 vs 2） |
| 4.14.2.2 | 異常系 | ValueField が異なる（0 vs 6） |

#### 4.14.3 前提条件

- 両方のインスタンスが IsSet=true である

#### 4.14.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.14.2.1 | CarModel.Sedan.Equals(CarModel.SportUtility) | 1 vs 2 |
| 4.14.2.2 | CarModel.Unknown.Equals(CarModel.Other) | 0 vs 6 |

#### 4.14.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.14.2.1 | false | 非等価 |
| 4.14.2.2 | false | 非等価 |

#### 4.14.6 判定基準

- [ ] ValueField が異なれば非等価

---

### 観点 VO-NE-02：IsSet が異なる場合は非等価

#### 4.15.1 テスト観点

IsSet フラグが異なる 2 つのオブジェクトは非等価である。

#### 4.15.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.15.2.1 | 境界値テスト | IsSet=true vs IsSet=false（値が同じ） |

#### 4.15.3 前提条件

- 1 つが IsSet=true、もう 1 つが IsSet=false

#### 4.15.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.15.2.1 | CarModel.Sedan.Equals(CarModel.Unset()) | IsSet 異なる |

#### 4.15.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.15.2.1 | false | IsSet 異なれば非等価 |

#### 4.15.6 判定基準

- [ ] IsSet が異なれば非等価

---

### 観点 VO-NE-03, VO-NE-04, VO-NE-05

これらの観点については、テンプレートに従い、以下の通り：

**VO-NE-03** （型が異なる）：CarModel と string を比較すれば false
**VO-NE-04** （null 比較）：CarModel.Sedan.Equals(null) は false
**VO-NE-05** （複数コンポーネントの一部が異なる）：IsSet と ValueField の一部だけ異なるケース

---

### 観点 VO-HC-01：Equals=true の 2 つのオブジェクトは同一ハッシュ値

#### 4.20.1 テスト観点

Equals で true と判定された 2 つのオブジェクトは同一ハッシュ値を返す。

#### 4.20.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.20.2.1 | 正常系 | Sedan 同一インスタンス |
| 4.20.2.2 | 正常系 | From(1) と Sedan |

#### 4.20.3 前提条件

- GetHashCode メソッドが実装されている
- Equals=true のペアである

#### 4.20.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.20.2.1 | CarModel.Sedan.GetHashCode() == CarModel.Sedan.GetHashCode() | ハッシュ同一 |
| 4.20.2.2 | CarModel.From(1).GetHashCode() == CarModel.Sedan.GetHashCode() | ハッシュ同一 |

#### 4.20.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.20.2.1 | true | ハッシュ一致 |
| 4.20.2.2 | true | ハッシュ一致 |

#### 4.20.6 判定基準

- [ ] Equals=true なら GetHashCode が同一

---

### 観点 VO-HC-02, VO-HC-03, VO-HC-04

その他のハッシュコード観点については、テンプレート同様：

**VO-HC-02**：IsSet が異なるとハッシュ異なる
**VO-HC-03**：ValueField が異なるとハッシュ異なる
**VO-HC-04**：ハッシュ値は複数呼び出しで一貫している

---

### 観点 VO-OP, VO-TS

== / != 演算子と ToString については、テンプレートに従い確認。

---

### 観点 VO-CV-01～VO-CV-07：CarModel フィールド初期化

#### 4.50.1 テスト観点

CarModel の 7 つの静的フィールド（Unknown～Other）がそれぞれ IsSet=true で初期化されている。

#### 4.50.2 テストパターン

| フィールド | ValueField | IsSet | 説明 |
|-----------|-----------|-------|------|
| Unknown | 0 | true | - |
| Sedan | 1 | true | - |
| SportUtility | 2 | true | - |
| Hatchback | 3 | true | - |
| Coupe | 4 | true | - |
| Minivan | 5 | true | - |
| Other | 6 | true | - |

#### 4.50.3 テストデータ

各フィールドについて、IsSet と ValueField を確認。

参考テストメソッド例：
```csharp
[Fact]
public void Unknown_IsSet_ReturnsTrue() => Assert.True(CarModel.Unknown.IsSet);

[Fact]
public void Sedan_ValueField_Returns1() => Assert.Equal(1, CarModel.Sedan.ValueField);
```

---

### 観点 VO-VF-01～VO-VF-03：値範囲・バリデーション

#### 4.60.1 テスト観点

Validate メソッドが、ValueField の範囲（0～6）をチェックしている。

#### 4.60.2 パターン

| パターン | 入力値 | 期待動作 |
|---------|--------|---------|
| VO-VF-01 | 0～6 | Validate パス |
| VO-VF-02 | 負数 | ArgumentOutOfRangeException |
| VO-VF-03 | 7 以上 | ArgumentOutOfRangeException |

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
| **null吸収** | TryFrom(null) が true + Unset を返すこと |
| **値操作安全性** | TryGetValue が例外ではなく bool で結果を示すこと |

### 5.2 検証方法

```
【VO-IS 観点の検証】
  var vo = CarModel.From(3);
  Assert.True(vo.IsSet);

【VO-OPT-03 観点の検証 - TryFrom(null) テスト】
  var result = CarModel.TryFrom((int?)null, out var unset);
  Assert.True(result);              // 返り値が true
  Assert.False(unset.IsSet);        // Unset 状態
  Assert.Equal(CarModel.Unset(), unset);  // Unset と等価

【VO-OPT-04 観点の検証 - TryFrom(valid) テスト】
  var result = CarModel.TryFrom(1, out var vo);
  Assert.True(result);              // 返り値が true
  Assert.True(vo.IsSet);            // 設定済み状態
  Assert.True(vo.TryGetValue(out var v));
  Assert.Equal(1, v);      // 値が一致

【VO-OPT-05 観点の検証 - TryFrom(invalid) テスト】
  var result = CarModel.TryFrom(-1, out var vo);
  Assert.False(result);             // 返り値が false
  Assert.False(vo.IsSet);           // Unset 状態

【VO-EQ 観点の検証】
  var vo1 = CarModel.From(1);
  var vo2 = CarModel.Sedan;
  Assert.True(vo1.Equals(vo2));
  Assert.True(vo1 == vo2);

【VO-HC 観点の検証】
  var vo1 = CarModel.From(1);
  var vo2 = CarModel.Sedan;
  Assert.Equal(vo1.GetHashCode(), vo2.GetHashCode());

【VO-TS 観点の検証】
  var vo = CarModel.Unset();
  Assert.Equal("Unset", vo.ToString());
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
tests/Contexts/Samples/CarPreferences.Domain.Tests/
└── ValueObjects/
	├── CarModelTests.cs
	├── RespondentAgeTests.cs
	└── RespondentNameTests.cs
```

---

## 7. 改版履歴

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-07-04 | 初版作成（基本的なValueObject テスト観点） |
| **2.0** | **2026-07-08** | **IOptionalValueObject対応版：Unset・From・TryFrom・TryGetValue 観点を追加。テスト観点 7個追加（VO-OPT-01～07）。v1.0 は廃版へ。** |

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
- VO-CV（ValueObject フィールド初期化）← 列挙型固有
- VO-VF（値範囲・バリデーション）← 列挙型固有

v1.0 では IOptionalValueObject の仕様をムカバーできていませんでしたが、v2.0 では対応しています。

