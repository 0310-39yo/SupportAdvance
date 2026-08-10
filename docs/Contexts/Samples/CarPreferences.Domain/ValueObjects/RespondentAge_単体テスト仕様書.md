# 単体テスト仕様書テンプレート — RespondentAge

**プロジェクト:** SupportAdvance  
**テスト対象:** ValueObject（値オブジェクト）  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-07-08  
**対応観点ID:** VO-IS-01/02 | VO-OPT-01～07 | VO-EQ-01～04 | VO-NE-01～05 | VO-HC-01～04 | VO-OP-01～05 | VO-TS-01～03 | VO-VR-01～03

---

## 0. 本書の位置づけ

本書は、Domain層の ValueObject クラス `RespondentAge` が、技術仕様書および詳細設計書で定義された
**等価性比較・ハッシュ・文字列化・値の不変性・IsSet状態管理・オプショナル値操作・年齢範囲検証（0～150）** を満たすことを確認するテスト仕様書。

---

## 1. テスト目的

ValueObject の各メンバーが、以下の仕様を満たすことを確認する：

- **値の不変性**：オブジェクト生成後、内部状態が変更されない
- **等価性比較**：同じ値を持つ2つのオブジェクトは等価である
- **ハッシュ整合性**：Equals=true のオブジェクトは同一ハッシュ値
- **文字列化**：ToString が仕様通りの形式で返される
- **IsSet状態管理**：IsSet フラグが正しく機能する
- **オプショナル値操作**（IOptionalValueObject実装時）：Unset・From・TryFrom・TryGetValue の動作
- **値範囲検証**：年齢値が 0～150 の範囲内に収まる

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | `RespondentAge` |
| **名前空間** | `SupportAdvance.Contexts.Samples.CarPreferences.Domain.ValueObjects` |
| **依存仕様** | ValueObject技術仕様書 v1.1 + IOptionalValueObject技術仕様書 v1.3 |
| **前提** | ValueObjectComponentNormalizer のテストが完了していること |

---

## 2.X テスト用実装と DI 設定

このテンプレートでは、値オブジェクト（RespondentAge）のテストに使用するテスト用実装を明記します。

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
| VO-OPT-06 | TryGetValue(out value) は IsSet=true で true を返す（新規実装） | 正常系 | IOptionalValueObject v1.3 § 3.4 | Test Constructor |
| VO-OPT-07 | TryGetValue(out value) は IsSet=false で false を返す（新規実装） | 正常系 | IOptionalValueObject v1.3 § 3.4 | Test Constructor |

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

### 観点グループ VR：`値範囲検証`（RespondentAge固有）

| 観点ID | 観点（説明） | 分類 | 依存仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| VO-VR-01 | 値 0～150 は有効（Validate パス） | 正常系 | § 4.0 | Test Constructor |
| VO-VR-02 | 値が負数（-1 等）は ArgumentOutOfRangeException | 異常系 | § 4.0 | Test Constructor |
| VO-VR-03 | 値が 151 以上は ArgumentOutOfRangeException | 異常系 | § 4.0 | Test Constructor |

### 観点グループ GVC：`GetValueComponents()` メソッド

| 観点ID | 観点（説明） | 分類 | 依存仕様 | テスト用実装 |
|--------|------|------|---------|------------|
| VO-GVC-01 | IsSet=true の場合、ValueField を yield return する | 正常系 | § 4.0 | Test Constructor |
| VO-GVC-02 | IsSet=false の場合、何も yield return しない（空列挙） | 正常系 | § 4.0 | Test Constructor |
| VO-GVC-03 | GetEqualityComponents との連携で IsSet が先頭に付加されることを確認 | 正常系 | § 4.0 | Test Constructor |

---

## 4. テスト観点別の検証シナリオ

### 観点 VO-IS-01：IsSet=true で構築したオブジェクトは IsSet が true を返す

#### 4.1.1 テスト観点

ValueObject をコンストラクタで IsSet=true で初期化した場合、IsSet プロパティが true を返す。

#### 4.1.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.1.2.1 | 正常系 | 単一値（25）、IsSet=true |
| 4.1.2.2 | 正常系 | 複数値テスト（0, 50, 150）、IsSet=true |

#### 4.1.3 前提条件

- ValueObject コンストラクタが利用可能
- IsSet プロパティが読み取り可能

#### 4.1.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.1.2.1 | From(25) | 有効な年齢 |
| 4.1.2.2 | From(0), From(150) | 境界値 |

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
| 4.2.2.2 | 正常系 | Unset() 複数回呼び出し |

#### 4.2.3 前提条件

- Unset() 静的メソッドが定義されている

#### 4.2.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.2.2.1 | RespondentAge.Unset() | 未設定状態 |
| 4.2.2.2 | RespondentAge.Unset() | 複数回呼び出し |

#### 4.2.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.2.2.1 | IsSet == false | 未設定フラグ |
| 4.2.2.2 | IsSet == false | 複数呼び出し |

#### 4.2.6 判定基準

- [ ] Unset() は IsSet=false のインスタンスを返す
- [ ] Unset インスタンスの ToString() は "Unset" を返す

---

### 観点 VO-OPT-01～07

（テンプレートと同様のシナリオをRespondentAgeの文脈で実施）

---

### 観点 VO-EQ-01～05、VO-NE-01～05、VO-HC-01～04、VO-OP-01～05、VO-TS-01～03

（テンプレートに同じ。RespondentAgeの値（0～150）を用いてテスト）

---

### 観点 VO-VR-01：値 0～150 は有効（Validate パス）

#### 4.50.1 テスト観点

Validate メソッドが、ValueField の範囲（0～150）をチェックしている。有効な値では例外が発生しない。

#### 4.50.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.50.2.1 | 正常系 | 値 0（最小） |
| 4.50.2.2 | 正常系 | 値 1（通常値） |
| 4.50.2.3 | 正常系 | 値 25（代表値） |
| 4.50.2.4 | 正常系 | 値 150（最大） |

#### 4.50.3 前提条件

- From() メソッドが Validate を呼び出している

#### 4.50.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.50.2.1 | RespondentAge.From(0) | 0 歳 |
| 4.50.2.2 | RespondentAge.From(1) | 1 歳 |
| 4.50.2.3 | RespondentAge.From(25) | 25 歳 |
| 4.50.2.4 | RespondentAge.From(150) | 150 歳（最大） |

#### 4.50.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.50.2.1 | インスタンス生成成功、IsSet==true | 0 が有効 |
| 4.50.2.2 | インスタンス生成成功、IsSet==true | 1 が有効 |
| 4.50.2.3 | インスタンス生成成功、IsSet==true | 25 が有効 |
| 4.50.2.4 | インスタンス生成成功、IsSet==true | 150 が有効 |

#### 4.50.6 判定基準

- [ ] 0～150 の値でインスタンス生成が成功する
- [ ] 例外は発生しない

---

### 観点 VO-VR-02：値が負数（-1 等）は ArgumentOutOfRangeException

#### 4.51.1 テスト観点

負数入力時に ArgumentOutOfRangeException が発生する。

#### 4.51.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.51.2.1 | 異常系 | 値 -1 |
| 4.51.2.2 | 異常系 | 値 -100 |

#### 4.51.3 前提条件

- Validate メソッドが -1 を範囲外と判定する

#### 4.51.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.51.2.1 | RespondentAge.From(-1) | 負数 |
| 4.51.2.2 | RespondentAge.From(-100) | 大きな負数 |

#### 4.51.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.51.2.1 | ArgumentOutOfRangeException | 例外発生 |
| 4.51.2.2 | ArgumentOutOfRangeException | 例外発生 |

#### 4.51.6 判定基準

- [ ] From(-1) で ArgumentOutOfRangeException が発生する
- [ ] TryFrom(-1, out _) で false を返す

---

### 観点 VO-VR-03：値が 151 以上は ArgumentOutOfRangeException

#### 4.52.1 テスト観点

150を超える値入力時に ArgumentOutOfRangeException が発生する。

#### 4.52.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.52.2.1 | 異常系 | 値 151 |
| 4.52.2.2 | 異常系 | 値 1000 |

#### 4.52.3 前提条件

- Validate メソッドが 151 以上を範囲外と判定する

#### 4.52.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 4.52.2.1 | RespondentAge.From(151) | 151 歳 |
| 4.52.2.2 | RespondentAge.From(1000) | 1000 歳 |

#### 4.52.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 4.52.2.1 | ArgumentOutOfRangeException | 例外発生 |
| 4.52.2.2 | ArgumentOutOfRangeException | 例外発生 |

#### 4.52.6 判定基準

- [ ] From(151) で ArgumentOutOfRangeException が発生する
- [ ] TryFrom(151, out _) で false を返す

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
| **値範囲検証** | 0～150 の範囲チェックが正しく機能すること |

### 5.2 検証方法

```
【VO-IS 観点の検証】
  var vo = RespondentAge.From(25);
  Assert.True(vo.IsSet);

【VO-OPT-03 観点の検証 - TryFrom(null) テスト】
  var result = RespondentAge.TryFrom((int?)null, out var unset);
  Assert.True(result);              // 返り値が true
  Assert.False(unset.IsSet);        // Unset 状態
  Assert.Equal(RespondentAge.Unset(), unset);

【VO-OPT-04 観点の検証 - TryFrom(valid) テスト】
  var result = RespondentAge.TryFrom(25, out var vo);
  Assert.True(result);              // 返り値が true
  Assert.True(vo.IsSet);            // 設定済み状態
  Assert.True(vo.TryGetValue(out var v));
  Assert.Equal(25, v);

【VO-OPT-05 観点の検証 - TryFrom(invalid) テスト】
  var result = RespondentAge.TryFrom(-1, out var vo);
  Assert.False(result);             // 返り値が false
  Assert.False(vo.IsSet);           // Unset 状態

【VO-EQ 観点の検証】
  var vo1 = RespondentAge.From(25);
  var vo2 = RespondentAge.From(25);
  Assert.True(vo1.Equals(vo2));
  Assert.True(vo1 == vo2);

【VO-HC 観点の検証】
  var vo1 = RespondentAge.From(25);
  var vo2 = RespondentAge.From(25);
  Assert.Equal(vo1.GetHashCode(), vo2.GetHashCode());

【VO-TS 観点の検証】
  var vo = RespondentAge.Unset();
  Assert.Equal("Unset", vo.ToString());

【VO-VR 観点の検証】
  // 有効値
  Assert.NotNull(RespondentAge.From(0));
  Assert.NotNull(RespondentAge.From(150));

  // 無効値（負数）
  Assert.Throws<ArgumentOutOfRangeException>(() => RespondentAge.From(-1));

  // 無効値（超過）
  Assert.Throws<ArgumentOutOfRangeException>(() => RespondentAge.From(151));

  // TryFromでの安全な処理
  Assert.False(RespondentAge.TryFrom(-1, out _));
  Assert.False(RespondentAge.TryFrom(151, out _));
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
| **2.0** | **2026-07-08** | **IOptionalValueObject対応版：Unset・From・TryFrom・TryGetValue 観点を追加。テスト観点 7個追加（VO-OPT-01～07）。値範囲検証観点を追加（VO-VR-01～03）。v1.0 は廃版へ。** |

---

## 補足：何が改善されたのか

### v1.0 → v2.0 の変更

**追加テスト観点**:
- **VO-OPT-01** ~ **VO-OPT-07**: IOptionalValueObject メソッドの 7 つのテスト観点
- **VO-VR-01** ~ **VO-VR-03**: 値範囲検証（0～150）の 3 つのテスト観点

**重要な追加**:
- **VO-OPT-03: TryFrom(null) は true を返し、Unset を返す**  
  → null を異常値ではなく「未設定状態」として扱う設計を検証

**テスト観点ID体系の整備**:
- VO-IS（IsSet プロパティ）
- **VO-OPT（IOptionalValueObject メソッド）← 新規**
- VO-EQ（Equals - 等価）
- VO-NE（Equals - 非等価）
- VO-HC（GetHashCode）
- VO-OP（==, != 演算子）
- VO-TS（ToString）
- **VO-VR（値範囲検証）← RespondentAge固有**

v1.0 では IOptionalValueObject の仕様をカバーできていませんでしたが、v2.0 では対応しています。

