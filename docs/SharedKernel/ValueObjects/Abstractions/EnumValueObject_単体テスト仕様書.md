# 単体テスト仕様書 — EnumValueObject<TValue>

**プロジェクト:** SupportAdvance  
**テスト対象:** EnumValueObject<TValue>（選択肢型値オブジェクト）  
**テストレベル:** 単体テスト  
**版:** 1.0 / 2026-07-07

---

## 1. 本書の位置づけ

本書は、Domain層の EnumValueObject<TValue> 派生クラスが、技術仕様書および詳細設計書で定義された
**選択肢の妥当性検証・ビジネス名称管理・等価性比較・ハッシュ・文字列化・IsSet状態管理** を満たすことを確認するテスト仕様書。

### 📌 設計上の注釈

**IsSet フラグについて**：EnumValueObject は設計上 **常に IsSet=true** です。これは、EnumValueObject が選択肢から必ず 1 つを選択した状態を表現するためです。したがって、以下の観点は実装では「該当しない」ものとして扱われます：

- **VO-IS-02**：IsSet=false での動作（EnumValueObject は常に true）
- **VO-NE-02**：IsSet が異なる場合の非等価性（常に true なため比較対象外）
- **VO-HC-02**：IsSet が異なるときのハッシュ値差異（該当なし）
- **VO-TS-01**：IsSet=false での "Unset" 返却（常に true なため発生しない）

これらの観点の実装テストは、EnumValueObject の仕様に基づいて省略されていることをご了承ください。

---

## 2. テスト目的

EnumValueObject<TValue> の各メンバーが、以下の仕様を満たすことを確認する：

- **値の妥当性検証**：無効な選択肢値は例外をスロー
- **業務名称の管理**：内部値を業務用の日本語名に変換
- **等価性比較**：同じ選択肢値を持つ2つのオブジェクトは等価
- **ハッシュ整合性**：Equals=true のオブジェクトは同一ハッシュ値
- **文字列化**：ToString が業務名称またはビジネス値を返す
- **IsSet状態管理**：IsSet フラグが正しく機能する

---

## 3. テスト対象クラス

| 項目 | 内容 |
|------|------|
| **クラス名** | [対象EnumValueObject名：例 QuestionType, RespondentStatus] |
| **名前空間** | SupportAdvance.SharedKernel.ValueObjects |
| **依拠仕様** | EnumValueObject<TValue>技術仕様書 v1.0 |
| **前提** | ValueObjectComponentNormalizer のテストが完了していること |

---

## 4. テスト観点一覧

### 観点グループ IS：`IsSet` プロパティ

| 観点ID | 観点（説明） | 分類 | テスト用実装 |
|--------|------|------|------------|
| VO-IS-01 | IsSet=true で構築したオブジェクトは IsSet が true を返す | 正常系 | Test Constructor |
| VO-IS-02 | IsSet=false で構築したオブジェクトは IsSet が false を返す | 正常系 | Test Constructor |

### 観点グループ VAL：`Validate` — 選択肢値の妥当性検証

| 観点ID | 観点（説明） | 分類 | テスト用実装 |
|--------|------|------|------------|
| VO-VAL-01 | 有効な選択肢値でコンストラクタを呼び出すと成功 | 正常系 | Test Constructor |
| VO-VAL-02 | 無効な選択肢値でコンストラクタを呼び出すと ArgumentOutOfRangeException | 異常系 | Test Constructor |
| VO-VAL-03 | Validate メソッドが無効値を検出して例外をスロー | 異常系 | Test Validate |

### 観点グループ DN：`GetDisplayName` — ビジネス名称の取得

| 観点ID | 観点（説明） | 分類 | テスト用実装 |
|--------|------|------|------------|
| VO-DN-01 | 各選択肢値に対応する正しい日本語名を返す | 正常系 | Test DisplayName |
| VO-DN-02 | 異なる選択肢値で異なる日本語名を返す | 正常系 | Test DisplayName |
| VO-DN-03 | 全選択肢に対応する日本語名が定義されている | 正常系 | Test DisplayName |

### 観点グループ EQ：`Equals` — 等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | テスト用実装 |
|--------|------|------|------------|
| VO-EQ-01 | 同じ選択肢値・同じ IsSet を持つ 2 つのオブジェクトは等価 | 正常系 | Test Constructor, Equals |
| VO-EQ-02 | 同一参照のオブジェクトは等価 | 正常系（自己参照） | Test Constructor, Equals |

### 観点グループ NE：`Equals` — 非等価と判定されるケース

| 観点ID | 観点（説明） | 分類 | テスト用実装 |
|--------|------|------|------------|
| VO-NE-01 | 選択肢値が異なる場合は非等価 | 異常系 | Test Constructor, Equals |
| VO-NE-02 | IsSet が異なる場合は非等価 | 境界値テスト | Test Constructor, Equals |
| VO-NE-03 | 型が異なる場合は非等価 | 異常系 | Test Constructor, Equals |
| VO-NE-04 | null との比較は非等価 | 例外/異常系 | Test Constructor, Equals |

### 観点グループ HC：`GetHashCode`

| 観点ID | 観点（説明） | 分類 | テスト用実装 |
|--------|------|------|------------|
| VO-HC-01 | Equals=true の 2 つのオブジェクトは同一ハッシュ値 | 正常系 | Equals, GetHashCode |
| VO-HC-02 | IsSet が異なるとハッシュ値が異なる | 境界値テスト | Equals, GetHashCode |
| VO-HC-03 | 選択肢値が異なるとハッシュ値が異なる | 境界値テスト | Equals, GetHashCode |

### 観点グループ OP：`==` / `!=` 演算子

| 観点ID | 観点（説明） | 分類 | テスト用実装 |
|--------|------|------|------------|
| VO-OP-01 | 等価なオブジェクトに == を適用すると true | 正常系 | Equals |
| VO-OP-02 | 非等価なオブジェクトに == を適用すると false | 異常系 | Equals |
| VO-OP-03 | 両辺が null のとき == は true | 準正常系（null チェック） | Equals |
| VO-OP-04 | 片方のみ null のとき == は false | 異常系 | Equals |
| VO-OP-05 | != は == の否定と一致 | 正常系 | Equals |

### 観点グループ TS：`ToString`

| 観点ID | 観点（説明） | 分類 | テスト用実装 |
|--------|------|------|------------|
| VO-TS-01 | IsSet=false のとき "Unset" を返す | 正常系 | Test Constructor |
| VO-TS-02 | IsSet=true のとき、GetDisplayName の結果を返す | 正常系 | Test ToString |
| VO-TS-03 | ToString 出力に IsSet の値そのものが含まれない | 正常系 | Test ToString |

---

## 5. テスト観点別の検証シナリオ

### 観点 VO-VAL-01：有効な選択肢値でコンストラクタを呼び出すと成功

#### 5.1.1 テスト観点

有効な選択肢値を使用してコンストラクタを呼び出した場合、インスタンスが正常に生成される。

#### 5.1.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 5.1.2.1 | 正常系 | 有効な選択肢値でインスタンス生成 |
| 5.1.2.2 | 正常系 | 複数の異なる有効値でそれぞれ生成 |

#### 5.1.3 前提条件

- EnumValueObject 派生クラスが定義されている
- 有効な選択肢値が明確に定義されている

#### 5.1.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 5.1.2.1 | QuestionType(1) — SingleChoice | 有効な選択肢値 |
| 5.1.2.2 | QuestionType(2), QuestionType(3) | 複数の有効値 |

#### 5.1.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 5.1.2.1 | インスタンス生成成功 | 例外なし |
| 5.1.2.2 | 各値でインスタンス生成成功 | 例外なし |

#### 5.1.6 判定基準

- [ ] 有効な選択肢値でコンストラクタが成功する
- [ ] IsSet=true のインスタンスが返される
- [ ] 複数の有効値すべてで生成可能

---

### 観点 VO-VAL-02：無効な選択肢値でコンストラクタを呼び出すと ArgumentOutOfRangeException

#### 5.2.1 テスト観点

無効な選択肢値を使用してコンストラクタを呼び出した場合、ArgumentOutOfRangeException が発生する。

#### 5.2.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 5.2.2.1 | 異常系 | 範囲外の値（低） |
| 5.2.2.2 | 異常系 | 範囲外の値（高） |
| 5.2.2.3 | 異常系 | ビジネスルール違反 |

#### 5.2.3 前提条件

- Validate メソッドが無効値を検出する

#### 5.2.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 5.2.2.1 | QuestionType(0) | 範囲外（0 は無効） |
| 5.2.2.2 | QuestionType(999) | 範囲外（999 は無効） |
| 5.2.2.3 | QuestionType(-1) | 負数 |

#### 5.2.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 5.2.2.1 | ArgumentOutOfRangeException | 例外発生 |
| 5.2.2.2 | ArgumentOutOfRangeException | 例外発生 |
| 5.2.2.3 | ArgumentOutOfRangeException | 例外発生 |

#### 5.2.6 判定基準

- [ ] 無効値でコンストラクタが ArgumentOutOfRangeException をスロー
- [ ] 例外メッセージが明確
- [ ] 全ての無効値パターンで例外をスロー

---

### 観点 VO-DN-01：各選択肢値に対応する正しい日本語名を返す

#### 5.3.1 テスト観点

各選択肢値に対応する正しい日本語名（ビジネス名称）が GetDisplayName または ToString で返される。

#### 5.3.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 5.3.2.1 | 正常系 | 値1 → 日本語名1 |
| 5.3.2.2 | 正常系 | 値2 → 日本語名2 |

#### 5.3.3 前提条件

- GetDisplayName が実装されている
- 各選択肢に対応する日本語名が定義されている

#### 5.3.4 テストデータ

| パターン | 入力値 | 期待する日本語名 |
|---------|--------|------------|
| 5.3.2.1 | QuestionType(1) | "単一選択" |
| 5.3.2.2 | QuestionType(2) | "複数選択" |

#### 5.3.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 5.3.2.1 | "単一選択" | ToString が正確な日本語名 |
| 5.3.2.2 | "複数選択" | ToString が正確な日本語名 |

#### 5.3.6 判定基準

- [ ] 各選択肢値が正確な日本語名を返す
- [ ] 日本語名は仕様書と一致する
- [ ] 複数の選択肢で異なる名称を返す

---

### 観点 VO-EQ-01：同じ選択肢値・同じ IsSet を持つ 2 つのオブジェクトは等価

#### 5.4.1 テスト観点

同一の選択肢値と IsSet 状態を持つ 2 つの EnumValueObject インスタンスは等価と判定される。

#### 5.4.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 5.4.2.1 | 正常系 | 同じ値、同じ IsSet=true |
| 5.4.2.2 | 正常系 | 同じ値、同じ IsSet=false |

#### 5.4.3 前提条件

- 2 つの異なるインスタンスを生成
- 選択肢値と IsSet が完全に一致

#### 5.4.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 5.4.2.1 | obj1 = QuestionType(1)<br/>obj2 = QuestionType(1) | 同じ選択肢値 |
| 5.4.2.2 | obj1 = QuestionType.Unset()<br/>obj2 = QuestionType.Unset() | Unset 状態 |

#### 5.4.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 5.4.2.1 | obj1.Equals(obj2) == true | Equals が true |
| 5.4.2.2 | obj1.Equals(obj2) == true | Unset 同士でも等価 |

#### 5.4.6 判定基準

- [ ] 同じ選択肢値の EnumValueObject は Equals で true を返す
- [ ] == 演算子でも true を返す
- [ ] 複数回の Equals 呼び出しで一貫性がある

---

### 観点 VO-NE-01：選択肢値が異なる場合は非等価

#### 5.5.1 テスト観点

異なる選択肢値を持つ 2 つの EnumValueObject は非等価と判定される。

#### 5.5.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 5.5.2.1 | 異常系 | 値1 vs 値2 |
| 5.5.2.2 | 異常系 | 値2 vs 値1 |

#### 5.5.3 前提条件

- 2 つ以上の異なる有効な選択肢値が存在

#### 5.5.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 5.5.2.1 | obj1 = QuestionType(1)<br/>obj2 = QuestionType(2) | 異なる値 |
| 5.5.2.2 | obj1 = QuestionType(2)<br/>obj2 = QuestionType(1) | 異なる値（逆順） |

#### 5.5.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 5.5.2.1 | obj1.Equals(obj2) == false | 非等価 |
| 5.5.2.2 | obj1.Equals(obj2) == false | 非等価 |

#### 5.5.6 判定基準

- [ ] 異なる選択肢値は非等価と判定される
- [ ] GetHashCode も異なる値を返す
- [ ] != 演算子で true を返す

---

### 観点 VO-NE-02：IsSet が異なる場合は非等価

#### 5.6.1 テスト観点

IsSet フラグが異なる場合、選択肢値が同じでも非等価と判定される。

#### 5.6.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 5.6.2.1 | 境界値 | IsSet=true vs IsSet=false |

#### 5.6.3 前提条件

- Unset() メソッドが実装されている
- IsSet が比較に含まれる

#### 5.6.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 5.6.2.1 | obj1 = QuestionType(1)<br/>obj2 = QuestionType.Unset() | IsSet のみ異なる |

#### 5.6.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 5.6.2.1 | obj1.Equals(obj2) == false | 非等価 |

#### 5.6.6 判定基準

- [ ] IsSet フラグが異なると非等価と判定される
- [ ] 値が同じでも IsSet により区別される
- [ ] GetHashCode も異なる値を返す

---

### 観点 VO-HC-01：Equals=true の 2 つのオブジェクトは同一ハッシュ値

#### 5.7.1 テスト観点

Equals で true を返す 2 つの EnumValueObject は同一のハッシュ値を返す。

#### 5.7.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 5.7.2.1 | 正常系 | 同値ペアのハッシュ比較 |

#### 5.7.3 前提条件

- 複数の同値インスタンスを生成

#### 5.7.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 5.7.2.1 | obj1 = QuestionType(1)<br/>obj2 = QuestionType(1) | 同値ペア |

#### 5.7.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 5.7.2.1 | obj1.GetHashCode() == obj2.GetHashCode() | ハッシュ値が同一 |

#### 5.7.6 判定基準

- [ ] Equals=true のオブジェクトペアは同一ハッシュ値を返す
- [ ] ハッシュ値は複数呼び出しで一貫している
- [ ] HashSet / Dictionary で正しく機能する

---

### 観点 VO-TS-01：IsSet=false のとき "Unset" を返す

#### 5.8.1 テスト観点

IsSet=false の EnumValueObject に対して ToString を呼び出した場合、"Unset" という文字列を返す。

#### 5.8.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 5.8.2.1 | 正常系 | IsSet=false での "Unset" 返却 |

#### 5.8.3 前提条件

- Unset() メソッドが実装されている

#### 5.8.4 テストデータ

| パターン | 入力値 | 説明 |
|---------|--------|------|
| 5.8.2.1 | QuestionType.Unset().ToString() | Unset 状態のToString |

#### 5.8.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 5.8.2.1 | "Unset" | 文字列が "Unset" |

#### 5.8.6 判定基準

- [ ] ToString() が正確に "Unset" を返す
- [ ] 大文字小文字が正確に一致

---

### 観点 VO-TS-02：IsSet=true のとき、GetDisplayName の結果を返す

#### 5.9.1 テスト観点

IsSet=true の EnumValueObject に対して ToString を呼び出した場合、GetDisplayName で返される日本語名（ビジネス名称）を返す。

#### 5.9.2 テストパターン

| パターン | 分類 | 説明 |
|---------|------|------|
| 5.9.2.1 | 正常系 | 設定済みでの日本語名返却 |
| 5.9.2.2 | 正常系 | 複数の異なる値で異なる名称 |

#### 5.9.3 前提条件

- GetDisplayName が正確に実装されている

#### 5.9.4 テストデータ

| パターン | 入力値 | 期待する ToString 結果 |
|---------|--------|------------------------|
| 5.9.2.1 | QuestionType(1).ToString() | "単一選択" |
| 5.9.2.2 | QuestionType(2).ToString() | "複数選択" |

#### 5.9.5 期待結果

| パターン | 期待値 | 検証項目 |
|---------|--------|---------|
| 5.9.2.1 | "単一選択" | 日本語名 |
| 5.9.2.2 | "複数選択" | 異なる日本語名 |

#### 5.9.6 判定基準

- [ ] ToString() が GetDisplayName の結果と一致
- [ ] 各選択肢値で正確な日本語名を返す
- [ ] "Unset" との使い分けが正確

---

## 6. 判定基準

### 6.1 全般的な判定基準

| 項目 | 基準 |
|------|------|
| **値の妥当性** | 有効な値のみを受け入れ、無効な値では例外をスロー |
| **業務名称管理** | 各選択肢値が正確な日本語名を返す |
| **等価性** | Equals の結果が論理的に一貫していること |
| **ハッシュ整合性** | Equals=true のペアが同一ハッシュ値を返す |
| **文字列化** | ToString が仕様通りの形式を返す |
| **IsSet管理** | IsSet フラグが正しく機能する |

### 6.2 検証方法

```csharp
【VO-VAL-01 観点の検証 - 有効な値】
  var qt = new QuestionType(1);
  Assert.True(qt.IsSet);

【VO-VAL-02 観点の検証 - 無効な値】
  Assert.Throws<ArgumentOutOfRangeException>(() => new QuestionType(999));

【VO-DN-01 観点の検証 - 日本語名】
  var qt1 = new QuestionType(1);
  Assert.Equal("単一選択", qt1.ToString());
  
  var qt2 = new QuestionType(2);
  Assert.Equal("複数選択", qt2.ToString());

【VO-EQ-01 観点の検証】
  var qt1 = new QuestionType(1);
  var qt2 = new QuestionType(1);
  Assert.True(qt1.Equals(qt2));
  Assert.True(qt1 == qt2);

【VO-NE-01 観点の検証】
  var qt1 = new QuestionType(1);
  var qt2 = new QuestionType(2);
  Assert.False(qt1.Equals(qt2));
  Assert.True(qt1 != qt2);

【VO-HC-01 観点の検証】
  var qt1 = new QuestionType(1);
  var qt2 = new QuestionType(1);
  Assert.Equal(qt1.GetHashCode(), qt2.GetHashCode());

【VO-TS-01 観点の検証】
  var qt = QuestionType.Unset();
  Assert.Equal("Unset", qt.ToString());

【VO-TS-02 観点の検証】
  var qt1 = new QuestionType(1);
  Assert.Equal("単一選択", qt1.ToString());
```

---

## 7. テスト用実装の設定

### 7.1 前提環境

| 項目 | 内容 |
|------|------|
| **テスティングフレームワーク** | xUnit / NUnit |
| **Assertion** | Assert.True / Assert.Equal / Assert.Throws |
| **Mock/Stub** | 不要（EnumValueObject は依存性なし） |

### 7.2 テストクラス構成

```
tests/Unit.Tests/
└── ValueObjects/
    ├── QuestionTypeTests.cs
    ├── RespondentStatusTests.cs
    ├── PriorityTests.cs
    └── [その他 EnumValueObject 派生テスト]
```

---

## 8. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-07-07 | Claude Code | 初版作成 — EnumValueObject 用テスト仕様書 |
| 1.1 | 2026-07-07 | Claude Code | テスト実装完了：null 比較テスト（VO-OP-03, VO-OP-04）、型差異テスト（VO-NE-03）を追加。IsSet 設計注釈を追加。 |
