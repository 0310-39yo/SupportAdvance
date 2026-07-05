# 単体テスト仕様書テンプレート — ValueObjectComponentNormalizer

**プロジェクト:** SupportAdvance  
**テスト対象:** ValueObjectComponentNormalizer（値オブジェクトコンポーネント正規化器）  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-07-03 / 加藤 正人

---

## 0. 本書の位置づけ

本書は、Domain層の ValueObjectComponentNormalizer クラスが、技術仕様書および詳細設計書で定義された
**コンポーネント列挙の正規化・IsSet フラグの管理・重複排除** を満たすことを確認するテスト仕様書。

---

## 1. テスト目的

ValueObjectComponentNormalizer の Normalize メソッドが、以下の仕様を満たすことを確認する：

- **IsSet フラグの付加**：コンポーネント列挙の先頭に IsSet フラグを正確に付加する
- **重複排除**：IsSet フラグが既に先頭にある場合、重複させない
- **値の保持**：コンポーネント値を正確に保持する
- **null 対応**：null を含むコンポーネントを正確に扱う
- **空列挙対応**：components が null または空の場合、IsSet のみを返す

---

## 2. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | ValueObjectComponentNormalizer |
| **メソッド** | Normalize(ValueObject instance, IEnumerable<object?>? components) |
| **名前空間** | Advance.Domain.ValueObjects |
| **依拠仕様** | ValueObjectComponentNormalizer技術仕様書 |

---

## 3. テスト観点一覧

### 観点グループ A：`components` が `null` または空

| 観点ID | 観点（説明） | 分類 | 検証方法 |
|--------|------|------|---------|
| VCN-A-01 | instance.IsSet=true、components=null → IsSet のみを返す | 正常系 | Test Case |
| VCN-A-02 | instance.IsSet=true、components=empty → IsSet のみを返す | 正常系 | Test Case |
| VCN-A-03 | instance.IsSet=false、components=null → IsSet のみを返す | 正常系 | Test Case |

### 観点グループ B：先頭要素が `bool` かつ `IsSet` と一致

| 観点ID | 観点（説明） | 分類 | 検証方法 |
|--------|------|------|---------|
| VCN-B-01 | IsSet=true、components={ true, "abc" } → 重複しない | 正常系 | Test Case |
| VCN-B-02 | IsSet=false、components={ false, 42 } → 重複しない | 正常系 | Test Case |
| VCN-B-03 | IsSet=true、components={ false, "abc" } → IsSet を付加（先頭値不一致） | 境界値テスト | Test Case |

### 観点グループ C：通常ケース

| 観点ID | 観点（説明） | 分類 | 検証方法 |
|--------|------|------|---------|
| VCN-C-01 | IsSet=true、components={ "Tokyo" } → IsSet を先頭に付加 | 正常系 | Test Case |
| VCN-C-02 | IsSet=true、components={ "Tokyo", 100, 3.14 } → IsSet を先頭に付加 | 正常系 | Test Case |
| VCN-C-03 | IsSet=true、components={ null, "abc" } → null を含むまま処理 | 正常系 | Test Case |

---

## 4. テスト観点別の検証シナリオ

### 観点 VCN-A-01：instance.IsSet=true、components=null → IsSet のみを返す

#### 4.1.1 テスト観点

components が null の場合、IsSet フラグのみを返す。

#### 4.1.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.1.2.1 | 正常系 | IsSet=true での null 対応 |

#### 4.1.3 テストデータと期待結果

| パターン | 入力値 | 期待値 |
|---------|--------|--------|
| 4.1.2.1 | instance.IsSet=true, components=null | { true } |

#### 4.1.4 判定基準

- [ ] 要素数が 1 である
- [ ] 唯一の要素が IsSet と一致する

---

### 観点 VCN-A-02：instance.IsSet=true、components=empty → IsSet のみを返す

#### 4.2.1 テスト観点

components が空列挙の場合、IsSet フラグのみを返す。

#### 4.2.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.2.2.1 | 正常系 | IsSet=true での空列挙対応 |

#### 4.2.3 テストデータと期待結果

| パターン | 入力値 | 期待値 |
|---------|--------|--------|
| 4.2.2.1 | instance.IsSet=true, components=Enumerable.Empty<object?>() | { true } |

#### 4.2.4 判定基準

- [ ] 要素数が 1 である
- [ ] IsSet のみが返される

---

### 観点 VCN-B-01：IsSet=true、components={ true, "abc" } → 重複しない

#### 4.3.1 テスト観点

components の先頭要素が bool で、かつ IsSet と一致する場合、重複して付加されない。

#### 4.3.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.3.2.1 | 正常系 | IsSet=true、先頭=true での非重複 |

#### 4.3.3 テストデータと期待結果

| パターン | 入力値 | 期待値 |
|---------|--------|--------|
| 4.3.2.1 | instance.IsSet=true, components={ true, "abc" } | { true, "abc" } |

#### 4.3.4 判定基準

- [ ] 要素数が 2 である
- [ ] 要素が { true, "abc" } と一致する
- [ ] true が重複しない

---

### 観点 VCN-B-02：IsSet=false、components={ false, 42 } → 重複しない

#### 4.4.1 テスト観点

components の先頭要素が bool で false、かつ IsSet=false の場合、重複して付加されない。

#### 4.4.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.4.2.1 | 正常系 | IsSet=false、先頭=false での非重複 |

#### 4.4.3 テストデータと期待結果

| パターン | 入力値 | 期待値 |
|---------|--------|--------|
| 4.4.2.1 | instance.IsSet=false, components={ false, 42 } | { false, 42 } |

#### 4.4.4 判定基準

- [ ] 要素数が 2 である
- [ ] 要素が { false, 42 } と一致する
- [ ] false が重複しない

---

### 観点 VCN-B-03：IsSet=true、components={ false, "abc" } → IsSet を付加（先頭値不一致）

#### 4.5.1 テスト観点

components の先頭要素が bool だが、IsSet と値が異なる場合、IsSet を先頭に付加する（境界値テスト）。

#### 4.5.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.5.2.1 | 境界値 | IsSet=true だが components={ false, ... } |

#### 4.5.3 テストデータと期待結果

| パターン | 入力値 | 期待値 |
|---------|--------|--------|
| 4.5.2.1 | instance.IsSet=true, components={ false, "abc" } | { true, false, "abc" } |

#### 4.5.4 判定基準

- [ ] 要素数が 3 である
- [ ] 要素が { true, false, "abc" } と一致する
- [ ] IsSet が先頭に付加される

---

### 観点 VCN-C-01：IsSet=true、components={ "Tokyo" } → IsSet を先頭に付加

#### 4.6.1 テスト観点

components の先頭要素が string（bool ではない）場合、IsSet を先頭に付加する。

#### 4.6.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.6.2.1 | 正常系 | 通常ケース（先頭が string） |

#### 4.6.3 テストデータと期待結果

| パターン | 入力値 | 期待値 |
|---------|--------|--------|
| 4.6.2.1 | instance.IsSet=true, components={ "Tokyo" } | { true, "Tokyo" } |

#### 4.6.4 判定基準

- [ ] 要素数が 2 である
- [ ] IsSet が先頭に付加される
- [ ] 元の値が保持される

---

### 観点 VCN-C-02：IsSet=true、components={ "Tokyo", 100, 3.14 } → IsSet を先頭に付加

#### 4.7.1 テスト観点

複数の異なる型のコンポーネントに対して、IsSet を先頭に正確に付加する。

#### 4.7.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.7.2.1 | 正常系 | 複数コンポーネント（string, int, double） |

#### 4.7.3 テストデータと期待結果

| パターン | 入力値 | 期待値 |
|---------|--------|--------|
| 4.7.2.1 | instance.IsSet=true, components={ "Tokyo", 100, 3.14 } | { true, "Tokyo", 100, 3.14 } |

#### 4.7.4 判定基準

- [ ] 要素数が 4 である
- [ ] IsSet が先頭に付加される
- [ ] 全コンポーネントが順序を保持する

---

### 観点 VCN-C-03：IsSet=true、components={ null, "abc" } → null を含むまま処理

#### 4.8.1 テスト観点

components に null が含まれる場合でも、null を正確に処理する（例外をスローしない）。

#### 4.8.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 4.8.2.1 | 正常系 | null を含むコンポーネント |

#### 4.8.3 テストデータと期待結果

| パターン | 入力値 | 期待値 |
|---------|--------|--------|
| 4.8.2.1 | instance.IsSet=true, components={ null, "abc" } | { true, null, "abc" } |

#### 4.8.4 判定基準

- [ ] 要素数が 3 である
- [ ] null が保持される
- [ ] 例外がスローされない

---

## 5. 判定基準

### 5.1 全般的な判定基準

| 項目 | 基準 |
|------|------|
| **IsSet 付加** | IsSet フラグが正確に先頭に付加されること |
| **重複排除** | 先頭値が IsSet と一致する場合、重複しないこと |
| **値の保持** | コンポーネント値が正確に保持されること |
| **null 対応** | null を含むコンポーネントを正確に処理すること |
| **空列挙対応** | components が null または空の場合、IsSet のみを返すこと |
| **例外安全性** | 不正な入力でも例外をスローしないこと |

### 5.2 検証方法

```
【VCN-A-01 観点の検証】
  var vo = new MockValueObject { IsSet = true };
  var result = ValueObjectComponentNormalizer.Normalize(vo, null);
  Assert.Single(result);
  Assert.Equal(true, result.First());

【VCN-B-01 観点の検証 - 重複排除】
  var vo = new MockValueObject { IsSet = true };
  var components = new object?[] { true, "abc" };
  var result = ValueObjectComponentNormalizer.Normalize(vo, components);
  Assert.Equal(2, result.Count());
  Assert.Equal(new object[] { true, "abc" }, result);

【VCN-B-03 観点の検証 - 先頭値不一致】
  var vo = new MockValueObject { IsSet = true };
  var components = new object?[] { false, "abc" };
  var result = ValueObjectComponentNormalizer.Normalize(vo, components);
  Assert.Equal(3, result.Count());
  Assert.Equal(new object[] { true, false, "abc" }, result);

【VCN-C-01 観点の検証 - 通常ケース】
  var vo = new MockValueObject { IsSet = true };
  var components = new object?[] { "Tokyo" };
  var result = ValueObjectComponentNormalizer.Normalize(vo, components);
  Assert.Equal(2, result.Count());
  Assert.Equal(new object[] { true, "Tokyo" }, result);

【VCN-C-03 観点の検証 - null 対応】
  var vo = new MockValueObject { IsSet = true };
  var components = new object?[] { null, "abc" };
  var result = ValueObjectComponentNormalizer.Normalize(vo, components);
  Assert.Equal(3, result.Count());
  Assert.Null(result.ElementAt(1));
  Assert.Equal("abc", result.ElementAt(2));
```

---

## 6. テスト用実装の設定

### 6.1 前提環境

| 項目 | 内容 |
|------|------|
| **テスティングフレームワーク** | xUnit / NUnit |
| **Assertion** | Assert.Single / Assert.Equal / Assert.Null |
| **Mock/Stub** | MockValueObject（テスト用実装） |

### 6.2 テストクラス構成

```
tests/SupportAdvance.Unit.Tests/
└── Common/
    └── ValueObjectComponentNormalizerTests.cs
```

---

## 7. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-07-03 | 加藤 正人 | 初版作成 |

