# 単体テスト仕様書テンプレート — ValueObject

**プロジェクト:** SupportAdvance  
**テスト対象:** ValueObject（値オブジェクト）  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-07-07

---

## 0. 本書の位置づけ

本書は、Domain層の ValueObject クラスが、技術仕様書および詳細設計書で定義された
**等価性比較・ハッシュ・文字列化・値の不変性・IsSet状態管理** を満たすことを確認するテスト仕様書。

---

## 1. テスト目的

ValueObject の各メンバーが、以下の仕様を満たすことを確認する：

- **値の不変性**：オブジェクト生成後、内部状態が変更されない
- **等価性比較**：同じ値を持つ2つのオブジェクトは等価である
- **ハッシュ整合性**：Equals=true のオブジェクトは同一ハッシュ値
- **文字列化**：ToString が仕様通りの形式で返される
- **IsSet状態管理**：IsSet フラグが正しく機能する

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | ValueObject（抽象クラス） |
| **名前空間** | Advance.Domain.ValueObjects |
| **依拠仕様** | ValueObject技術仕様書 |
| **前提** | ValueObjectComponentNormalizer のテストが完了していること |

---

## 3. テスト観点一覧

### 観点グループ IS：`IsSet` プロパティ

| 観点ID | 観点（説明） | 分類 | 検証方法 |
|--------|------|------|---------|
| VO-IS-01 | IsSet=true で構築したオブジェクトは IsSet が true を返す | 正常系 | Test Constructor |
| VO-IS-02 | IsSet=false で構築したオブジェクトは IsSet が false を返す | 正常系 | Test Constructor |

### 観点グループ EQ：`Equals` — 等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | 検証方法 |
|--------|------|------|---------|
| VO-EQ-01 | 同じ型・同じ値・同じ IsSet を持つ 2 つのオブジェクトは等価 | 正常系 | Equals/GetHashCode |
| VO-EQ-02 | 同一参照のオブジェクトは等価 | 正常系（自己参照） | Equals/GetHashCode |
| VO-EQ-03 | 複数コンポーネントがすべて一致する場合は等価 | 正常系 | Equals/GetHashCode |
| VO-EQ-04 | 両方が IsSet=false かつ値が同じ場合は等価 | 正常系 | Equals/GetHashCode |

### 観点グループ NE：`Equals` — 非等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | 検証方法 |
|--------|------|------|---------|
| VO-NE-01 | コンポーネント値が異なる場合は非等価 | 異常系 | Equals/GetHashCode |
| VO-NE-02 | IsSet が異なる場合は非等価 | 境界値テスト | Equals/GetHashCode |
| VO-NE-03 | 型が異なる場合は非等価（値が同じでも） | 異常系 | Equals/GetHashCode |
| VO-NE-04 | null との比較は非等価 | 例外/異常系 | Equals/GetHashCode |
| VO-NE-05 | 複数コンポーネントの一部が異なる場合は非等価 | 境界値テスト | Equals/GetHashCode |

### 観点グループ HC：`GetHashCode`

| 観点ID | 観点（説明） | 分類 | 検証方法 |
|--------|------|------|---------|
| VO-HC-01 | Equals=true の 2 つのオブジェクトは同一ハッシュ値 | 正常系 | Equals/GetHashCode |
| VO-HC-02 | IsSet が異なるとハッシュ値が異なる | 境界値テスト | Equals/GetHashCode |
| VO-HC-03 | コンポーネント値が異なるとハッシュ値が異なる | 境界値テスト | Equals/GetHashCode |
| VO-HC-04 | ハッシュ値は複数呼び出しで一貫している | 正常系（副作用なし） | Equals/GetHashCode |

### 観点グループ OP：`==` / `!=` 演算子

| 観点ID | 観点（説明） | 分類 | 検証方法 |
|--------|------|------|---------|
| VO-OP-01 | 等価なオブジェクトに == を適用すると true | 正常系 | Equals/GetHashCode |
| VO-OP-02 | 非等価なオブジェクトに == を適用すると false | 異常系 | Equals/GetHashCode |
| VO-OP-03 | 両辺が null のとき == は true | 準正常系（null チェック） | Equals/GetHashCode |
| VO-OP-04 | 片方のみ null のとき == は false | 異常系 | Equals/GetHashCode |
| VO-OP-05 | != は == の否定と一致 | 正常系 | Equals/GetHashCode |

### 観点グループ TS：`ToString`

| 観点ID | 観点（説明） | 分類 | 検証方法 |
|--------|------|------|---------|
| VO-TS-01 | IsSet=false のとき "Unset" を返す | 正常系 | Test Constructor |
| VO-TS-02 | IsSet=true のとき、コンポーネントをカンマ区切りで連結 | 正常系 | Test Constructor |
| VO-TS-03 | ToString 出力に IsSet の値そのものが含まれない | 正常系 | Test Constructor |

---

## 4. テスト観点別の検証シナリオ

### 観点 VO-IS-01：IsSet=true で構築したオブジェクトは IsSet が true を返す

#### 4.1.1 テスト観点

ValueObject をコンストラクタで IsSet=true で初期化した場合、IsSet プロパティが true を返す。

#### 4.1.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.1.2.1 | 正常系 | 単一コンポーネント、IsSet=true |
| 4.1.2.2 | 正常系 | 複数コンポーネント、IsSet=true |

#### 4.1.3 テストデータと期待結果

| パターン | 入力値 | 期待値 |
|---------|--------|--------|
| 4.1.2.1 | 単一値で IsSet=true を指定 | IsSet == true |
| 4.1.2.2 | 複数フィールドで IsSet=true を指定 | IsSet == true |

#### 4.1.4 判定基準

- [ ] コンストラクタで IsSet=true を指定した場合、IsSet プロパティが true を返す
- [ ] IsSet プロパティは読み取り専用で、外部から変更不可

---

### 観点 VO-EQ-01：同じ型・同じ値・同じ IsSet を持つ 2 つのオブジェクトは等価

#### 4.2.1 テスト観点

同一の値とIsSet状態を持つ2つのValueObjectインスタンスは、Equals メソッドで等価と判定される。

#### 4.2.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.2.2.1 | 正常系 | 同じ値、同じ IsSet=true |
| 4.2.2.2 | 正常系 | 同じ値、同じ IsSet=false |
| 4.2.2.3 | 正常系 | 複数コンポーネント、すべて一致 |

#### 4.2.3 テストデータと期待結果

| パターン | 入力値 | 期待値 |
|---------|--------|--------|
| 4.2.2.1 | 同値のインスタンスペア（IsSet=true） | obj1.Equals(obj2) == true |
| 4.2.2.2 | 同値のインスタンスペア（IsSet=false） | obj1.Equals(obj2) == true |
| 4.2.2.3 | 複数フィールド一致 | obj1.Equals(obj2) == true |

#### 4.2.4 判定基準

- [ ] 同じ値のValueObjectは Equals で true を返す
- [ ] 複数回の Equals 呼び出しで一貫性がある
- [ ] == 演算子でも true を返す

---

### 観点 VO-NE-02：IsSet が異なる場合は非等価

#### 4.3.1 テスト観点

IsSet フラグが異なる場合、たとえ値が同じでも非等価と判定される（境界値テスト）。

#### 4.3.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.3.2.1 | 境界値 | IsSet: true vs false |
| 4.3.2.2 | 境界値 | IsSet: false vs true（逆順） |

#### 4.3.3 テストデータと期待結果

| パターン | 入力値 | 期待値 |
|---------|--------|--------|
| 4.3.2.1 | 値は同じだが IsSet のみ異なる | obj1.Equals(obj2) == false |
| 4.3.2.2 | 値は同じだが IsSet のみ異なる（逆） | obj1.Equals(obj2) == false |

#### 4.3.4 判定基準

- [ ] IsSet フラグが異なると非等価と判定される
- [ ] 値が同じでも IsSet により区別される
- [ ] GetHashCode も異なる値を返す

---

### 観点 VO-HC-01：Equals=true の 2 つのオブジェクトは同一ハッシュ値

#### 4.4.1 テスト観点

Equals メソッドで true を返す2つのValueObjectは、GetHashCode で同一のハッシュ値を返す（ハッシュ整合性）。

#### 4.4.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.4.2.1 | 正常系 | 同値オブジェクトのハッシュ比較 |
| 4.4.2.2 | 正常系 | 複数のオブジェクトペアでハッシュ検証 |

#### 4.4.3 テストデータと期待結果

| パターン | 入力値 | 期待値 |
|---------|--------|--------|
| 4.4.2.1 | 同値のインスタンスペア | obj1.GetHashCode() == obj2.GetHashCode() |
| 4.4.2.2 | 複数ペア検証 | 全ペアでハッシュ値が同一 |

#### 4.4.4 判定基準

- [ ] Equals=true のオブジェクトペアは同一ハッシュ値を返す
- [ ] ハッシュ値は複数呼び出しで一貫している
- [ ] HashSet / Dictionary で正しく機能する

---

### 観点 VO-TS-01：IsSet=false のとき "Unset" を返す

#### 4.5.1 テスト観点

IsSet=false の ValueObject に対して ToString を呼び出した場合、"Unset" という文字列を返す。

#### 4.5.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.5.2.1 | 正常系 | IsSet=false での "Unset" 返却 |

#### 4.5.3 テストデータと期待結果

| パターン | 入力値 | 期待値 |
|---------|--------|--------|
| 4.5.2.1 | IsSet=false のインスタンス | ToString() == "Unset" |

#### 4.5.4 判定基準

- [ ] ToString() が正確に "Unset" を返す
- [ ] 大文字小文字が正確に一致

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

### 5.2 検証方法

```
【VO-IS 観点の検証】
  var vo = new OrderId("ORD-001", IsSet: true);
  Assert.True(vo.IsSet);

【VO-EQ 観点の検証】
  var vo1 = new OrderId("ORD-001", IsSet: true);
  var vo2 = new OrderId("ORD-001", IsSet: true);
  Assert.True(vo1.Equals(vo2));
  Assert.True(vo1 == vo2);

【VO-HC 観点の検証】
  var vo1 = new OrderId("ORD-001", IsSet: true);
  var vo2 = new OrderId("ORD-001", IsSet: true);
  Assert.Equal(vo1.GetHashCode(), vo2.GetHashCode());

【VO-TS 観点の検証】
  var vo = new OrderId("ORD-001", IsSet: false);
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
tests/SupportAdvance.Unit.Tests/
└── ValueObjects/
    ├── OrderIdTests.cs
    ├── SampleNameTests.cs
    ├── SampleAgeTests.cs
    └── ... （その他ValueObject）
```

---

## 7. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-07-03 | 加藤 正人 | 初版作成 |

