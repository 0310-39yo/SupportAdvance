# 単体テスト仕様書テンプレート — PrimitiveValueObject

**プロジェクト:** SupportAdvance  
**テスト対象:** PrimitiveValueObject<TValue>（プリミティブ値オブジェクト）  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-07-03 / 加藤 正人

---

## 0. 本書の位置づけ

本書は、Domain層の PrimitiveValueObject<TValue> 抽象クラスが、技術仕様書および詳細設計書で定義された
**正規化・検証・フォーマット・等価性・ハッシュ管理・文字列化** を満たすことを確認するテスト仕様書。

---

## 1. テスト目的

PrimitiveValueObject<TValue> の各メンバーが、以下の仕様を満たすことを確認する：

- **正規化処理**：入力値が Normalize メソッドで適切に処理される
- **検証処理**：入力値が Validate メソッドで検証される
- **値の不変性**：オブジェクト生成後、内部状態が変更されない
- **等価性比較**：同じ値を持つ2つのオブジェクトは等価である
- **ハッシュ整合性**：Equals=true のオブジェクトは同一ハッシュ値
- **文字列化**：ToString が Format メソッドに基づいて返される
- **IsSet状態管理**：IsSet フラグが正しく機能する

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | PrimitiveValueObject<TValue>（抽象クラス） |
| **名前空間** | Advance.Domain.ValueObjects |
| **依拠仕様** | PrimitiveValueObject技術仕様書 |
| **前提** | ValueObject および ValueObjectComponentNormalizer のテストが完了していること |

---

## 2.X テスト用実装と DI 設定

このテンプレートでは、プリミティブ値オブジェクトのテストに使用するテスト用実装を明記します。

| 実装名 | 役割 | 対応する観点ID | 説明 |
|--------|------|----------------|------|
| Test Constructor | テスト用コンストラクタ | PVO-CT-01～05, PVO-FM-01～02 | 値オブジェクトの初期化・フォーマット検証 |
| Normalize Override | Normalize のオーバーライド実装 | PVO-NM-01～02 | 正規化ロジックの検証 |
| Validate Override | Validate のオーバーライド実装 | PVO-VL-01～02 | 検証ロジックの検証 |
| Equals/GetHashCode | 標準メソッド | PVO-EQ-01～06 等 | .NET の Equals・GetHashCode メソッド検証 |

---

## 3. テスト観点一覧

### 観点グループ CT：コンストラクタ

| 観点ID | 観点（説明） | 分類 | テスト用実装 |
|--------|------|------|---------|
| PVO-CT-01 | isSet=true で値設定用コンストラクタを呼び出すと IsSet が true を返す | 正常系 | Test Constructor |
| PVO-CT-02 | 未設定用コンストラクタを isSet=false で呼び出すと IsSet が false を返す | 正常系 | Test Constructor |
| PVO-CT-03 | Normalize が Validate より先に呼び出される | 正常系 | Normalize Override |
| PVO-CT-04 | Normalize で入力値が変換される | 正常系 | Normalize Override |
| PVO-CT-05 | isSet=false の場合、Normalize および Validate は呼び出されない | 正常系 | Test Constructor |
| PVO-CT-06 | Validate が例外をスロー場合、インスタンス構築が失敗する | 異常系 | Validate Override |

### 観点グループ TG：`TryGetValue`

| 観点ID | 観点（説明） | 分類 | テスト用実装 |
|--------|------|------|---------|
| PVO-TG-01 | isSet=true の場合、TryGetValue() は true を返す | 正常系 | Test Constructor |
| PVO-TG-02 | isSet=false の場合、TryGetValue() は false を返す | 正常系 | Test Constructor |

### 観点グループ NM：`Normalize`

| 観点ID | 観点（説明） | 分類 | テスト用実装 |
|--------|------|------|---------|
| PVO-NM-01 | Normalize をオーバーライドしない場合、入力値がそのまま保持される | 正常系 | Test Constructor |
| PVO-NM-02 | Normalize でカスタム処理を実装した場合、変換結果が保持される | 正常系 | Normalize Override |

### 観点グループ VL：`Validate`

| 観点ID | 観点（説明） | 分類 | テスト用実装 |
|--------|------|------|---------|
| PVO-VL-01 | Validate をオーバーライドしない場合、常に成功する | 正常系 | Test Constructor |
| PVO-VL-02 | Validate で例外をスロー場合、インスタンス構築が失敗する | 異常系 | Validate Override |

### 観点グループ FM：`Format`

| 観点ID | 観点（説明） | 分類 | テスト用実装 |
|--------|------|------|---------|
| PVO-FM-01 | Format をオーバーライドしない場合、ToString() は value.ToString() を返す | 正常系 | Test Constructor |
| PVO-FM-02 | Format でカスタムフォーマットを実装した場合、フォーマット結果を返す | 正常系 | Test Constructor |

### 観点グループ EC：`GetEqualityComponents`

| 観点ID | 観点（説明） | 分類 | テスト用実装 |
|--------|------|------|---------|
| PVO-EC-01 | isSet=true の場合、GetEqualityComponents は IsSet と値を返す | 正常系 | Test Constructor |
| PVO-EC-02 | isSet=false の場合、GetEqualityComponents は IsSet のみを返す | 正常系 | Test Constructor |
| PVO-EC-03 | GetEqualityComponents の結果が正規化される | 正常系 | Test Constructor |

### 観点グループ EQ：等価性

| 観点ID | 観点（説明） | 分類 | テスト用実装 |
|--------|------|------|---------|
| PVO-EQ-01 | 同じ型・同じ値・IsSet=true の 2 つのオブジェクトは等価 | 正常系 | Equals/GetHashCode |
| PVO-EQ-02 | 同じ型・IsSet=false の 2 つのオブジェクトは等価 | 正常系 | Equals/GetHashCode |
| PVO-EQ-03 | 同じ型・IsSet=true・値が異なる場合は非等価 | 異常系 | Equals/GetHashCode |
| PVO-EQ-04 | IsSet が異なる場合は非等価 | 境界値テスト | Equals/GetHashCode |
| PVO-EQ-05 | Equals=true のオブジェクトは同一ハッシュ値 | 正常系 | Equals/GetHashCode |
| PVO-EQ-06 | Normalize で結果が同じになる場合、等価と判定される | 正常系 | Equals/GetHashCode |

### 観点グループ TS：`ToString`

| 観点ID | 観点（説明） | 分類 | テスト用実装 |
|--------|------|------|---------|
| PVO-TS-01 | isSet=false の場合、ToString() は "Unset" を返す | 正常系 | Test Constructor |
| PVO-TS-02 | Format をオーバーライドしない場合、ToString() は value.ToString() を返す | 正常系 | Test Constructor |
| PVO-TS-03 | Format でカスタムフォーマットを実装した場合、フォーマット結果を返す | 正常系 | Test Constructor |

---

## 4. テスト観点別の検証シナリオ

### 観点 PVO-CT-01：isSet=true で値設定用コンストラクタを呼び出すと IsSet が true を返す

#### 4.1.1 テスト観点

PrimitiveValueObject のコンストラクタで isSet=true を指定した場合、IsSet プロパティが true を返す。

#### 4.1.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.1.2.1 | 正常系 | 文字列値、isSet=true |
| 4.1.2.2 | 正常系 | 数値、isSet=true |

#### 4.1.3 テストデータと期待結果

| パターン | 入力値 | 期待値 |
|---------|--------|--------|
| 4.1.2.1 | new TestPrimitiveVO("Tokyo", isSet: true) | IsSet == true |
| 4.1.2.2 | new TestPrimitiveVO(42, isSet: true) | IsSet == true |

#### 4.1.4 判定基準

- [ ] isSet=true でコンストラクタを呼び出した場合、IsSet が true を返す
- [ ] IsSet プロパティは読み取り専用

---

### 観点 PVO-CT-03：Normalize が Validate より先に呼び出される

#### 4.2.1 テスト観点

コンストラクタ実行時、Normalize メソッドが Validate メソッドより先に呼び出される順序が重要である。

#### 4.2.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.2.2.1 | 正常系 | メソッド呼び出し順序の検証 |

#### 4.2.3 テストデータと期待結果

| パターン | 期待値 |
|---------|--------|
| 4.2.2.1 | Normalize が Validate より先に呼び出される |

#### 4.2.4 判定基準

- [ ] メソッド呼び出し順序が正確である
- [ ] 正規化してから検証される

---

### 観点 PVO-CT-04：Normalize で入力値が変換される

#### 4.3.1 テスト観点

Normalize をオーバーライドした具体クラスで、入力値が適切に変換される。

#### 4.3.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.3.2.1 | 正常系 | Trim 処理での変換 |

#### 4.3.3 テストデータと期待結果

| パターン | 入力値 | 期待値 |
|---------|--------|--------|
| 4.3.2.1 | "  Tokyo  " で isSet=true | TryGetValue で "Tokyo" を取得 |

#### 4.3.4 判定基準

- [ ] Normalize での変換結果が正確である
- [ ] 保持された値が変換後の値である

---

### 観点 PVO-NM-02：Normalize でカスタム処理を実装した場合、変換結果が保持される

#### 4.4.1 テスト観点

Normalize メソッドをカスタム実装した場合、その処理結果が値として保持される。

#### 4.4.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.4.2.1 | 正常系 | Trim + ToUpper 処理 |

#### 4.4.3 テストデータと期待結果

| パターン | 入力値 | 期待値 |
|---------|--------|--------|
| 4.4.2.1 | "  tokyo  " で isSet=true | TryGetValue で "TOKYO" を取得 |

#### 4.4.4 判定基準

- [ ] カスタム処理の結果が正確である
- [ ] 複数の処理（Trim・ToUpper）が適用される

---

### 観点 PVO-VL-02：Validate で例外をスロー場合、インスタンス構築が失敗する

#### 4.5.1 テスト観点

Validate メソッドで例外がスローされる場合、コンストラクタもそれを伝播して失敗する。

#### 4.5.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.5.2.1 | 異常系 | 長さ検証で例外発生 |

#### 4.5.3 テストデータと期待結果

| パターン | 入力値 | 期待値 |
|---------|--------|--------|
| 4.5.2.1 | "abcdef"（長さ 6）で Validate が長さ>5 で例外 | 例外がスローされる |

#### 4.5.4 判定基準

- [ ] 検証失敗時に例外がスローされる
- [ ] インスタンスが構築されない

---

### 観点 PVO-EQ-06：Normalize で結果が同じになる場合、等価と判定される

#### 4.6.1 テスト観点

異なる入力値でも Normalize の処理結果が同じ場合、等価と判定される（正規化に基づく等価性）。

#### 4.6.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.6.2.1 | 正常系 | Trim 処理による同値化 |

#### 4.6.3 テストデータと期待結果

| パターン | 入力値 | 期待値 |
|---------|--------|--------|
| 4.6.2.1 | "  Tokyo  " と "Tokyo" | obj1.Equals(obj2) == true |

#### 4.6.4 判定基準

- [ ] Normalize 後の値が等価である
- [ ] 正規化ロジックが等価性判定に反映される

---

### 観点 PVO-FM-01：Format をオーバーライドしない場合、ToString() は value.ToString() を返す

#### 4.7.1 テスト観点

Format をオーバーライドしていない具体クラスで、ToString() が value.ToString() と同じ結果を返す。

#### 4.7.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.7.2.1 | 正常系 | デフォルト実装での ToString |

#### 4.7.3 テストデータと期待結果

| パターン | 入力値 | 期待値 |
|---------|--------|--------|
| 4.7.2.1 | 値 42 で isSet=true | ToString() == "42" |

#### 4.7.4 判定基準

- [ ] デフォルト実装の結果が正確である
- [ ] value.ToString() と一致する

---

### 観点 PVO-FM-02：Format でカスタムフォーマットを実装した場合、フォーマット結果を返す

#### 4.8.1 テスト観点

Format メソッドをカスタム実装した場合、その処理結果が ToString() で返される。

#### 4.8.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.8.2.1 | 正常系 | "[{value}]" フォーマット |

#### 4.8.3 テストデータと期待結果

| パターン | 入力値 | 期待値 |
|---------|--------|--------|
| 4.8.2.1 | 値 "Tokyo" で isSet=true | ToString() == "[Tokyo]" |

#### 4.8.4 判定基準

- [ ] カスタムフォーマットが正確である
- [ ] フォーマット文字列が適用される

---

### 観点 PVO-TS-01：isSet=false の場合、ToString() は "Unset" を返す

#### 4.9.1 テスト観点

isSet=false の PrimitiveValueObject に対して ToString() を呼び出した場合、"Unset" という文字列を返す。

#### 4.9.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.9.2.1 | 正常系 | IsSet=false での "Unset" 返却 |

#### 4.9.3 テストデータと期待結果

| パターン | 入力値 | 期待値 |
|---------|--------|--------|
| 4.9.2.1 | isSet=false で構築 | ToString() == "Unset" |

#### 4.9.4 判定基準

- [ ] ToString() が正確に "Unset" を返す
- [ ] 大文字小文字が正確に一致

---

## 5. 判定基準

### 5.1 全般的な判定基準

| 項目 | 基準 |
|------|------|
| **不変性** | オブジェクト生成後、プロパティが変更されないこと |
| **正規化** | Normalize が正確に処理されること |
| **検証** | Validate で正確に検証されること |
| **等価性** | Equals の結果が論理的に一貫していること |
| **ハッシュ整合性** | Equals=true のペアが同一ハッシュ値を返すこと |
| **文字列化** | ToString が Format に基づいて返されること |

### 5.2 検証方法

```
【PVO-CT-01 観点の検証】
  var vo = new TestPrimitiveVO("Tokyo", isSet: true);
  Assert.True(vo.IsSet);

【PVO-CT-04 観点の検証 - Normalize テスト】
  var vo = new TestPrimitiveVO("  Tokyo  ", isSet: true);
  Assert.True(vo.TryGetValue(out var value));
  Assert.Equal("Tokyo", value);

【PVO-NM-02 観点の検証】
  var vo = new TestPrimitiveVO("  tokyo  ", isSet: true);
  Assert.True(vo.TryGetValue(out var value));
  Assert.Equal("TOKYO", value);

【PVO-EQ-01 観点の検証】
  var vo1 = new TestPrimitiveVO("Tokyo", isSet: true);
  var vo2 = new TestPrimitiveVO("Tokyo", isSet: true);
  Assert.True(vo1.Equals(vo2));

【PVO-FM-02 観点の検証】
  var vo = new TestPrimitiveVO("Tokyo", isSet: true);
  Assert.Equal("[Tokyo]", vo.ToString());
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
tests/SupportAdvance.Unit.Tests/
└── ValueObjects/
    ├── SampleNameTests.cs
    ├── SampleAgeTests.cs
    ├── SampleIdTests.cs
    └── ... （その他PrimitiveValueObject）
```

---

## 7. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-07-03 | 加藤 正人 | 初版作成 |

