# 単体テスト仕様書 — EnumValueObject<TValue>

**プロジェクト:** Advance  
**対象:** Domain 層 / ValueObject 基底クラス  
**テスト対象:** `EnumValueObject<TValue>` の基本機能  
**版:** 1.0 / 2026-07-04

---

## 1. テスト対象の位置づけ

### 1.1 テスト対象オブジェクト

`EnumValueObject<TValue>` は、Domain 層における抽象基底クラスである。

- **テスト対象範囲**：`EnumValueObject<TValue>` の基本メンバー機能（IsSet、TryGetValue、Validate、GetDisplayName、等価性判定、ToString）
- **テスト実装方法**：具体的な派生クラス（例：OrderStatus）を通じた機能検証
- **テスト責務**：仕様書 §2「メンバー仕様」に定義される各メンバーの正確な動作確認

### 1.2 テスト対象外（派生クラス固有）

以下は、各派生クラスの個別テスト仕様書で検証する：

- From(int value) メソッドの値逆引きロジック
- Validate メソッドの範囲チェック内容（1～3 など、クラス固有の範囲）
- GetDisplayName メソッドの値変換ロジック（"下書き"→"承認済" など、表示内容）
- 派生クラスのコンストラクタ仕様（private 修飾、引数形式）

---

## 2. テストケース一覧

### 2.1 IsSet プロパティのテスト

**根拠:** 技術仕様書 §2.1

| テストID | 観点ID | テストケース | 前提条件 | 操作 | 期待結果 | 備考 |
|---|---|---|---|---|---|---|
| T-IsSet-001 | IsSet-001 | IsSet は常に true を返す | OrderStatus.Draft インスタンス生成済み | Draft.IsSet を読み取る | true を返す | EnumValueObject は常に値を保持するため |
| T-IsSet-002 | IsSet-002 | 複数インスタンスで IsSet は常に true | OrderStatus.Draft、Approved、Completed インスタンス生成済み | 全インスタンスで IsSet を読み取る | すべて true を返す | 選択肢すべてで同じ動作 |

**テスト実装例（テストメソッド名形式）:**
- `IsSet_常に_Trueを返す()`
- `IsSet_複数インスタンス_すべてTrueを返す()`

---

### 2.2 TryGetValue メソッドのテスト

**根拠:** 技術仕様書 §2.3

| テストID | 観点ID | テストケース | 前提条件 | 操作 | 期待結果 | 備考 |
|---|---|---|---|---|---|---|
| T-TryGetValue-001 | TryGetValue-001 | TryGetValue は常に true を返す | OrderStatus.Draft インスタンス生成済み | Draft.TryGetValue(out int value) を呼び出す | 戻り値が true、value に内部値（1）が格納される | IsSet=true のため常に成功 |
| T-TryGetValue-002 | TryGetValue-002 | out パラメータに正しい内部値が格納される | OrderStatus.Approved インスタンス生成済み | Approved.TryGetValue(out int value) を呼び出す | value=2（Approved に対応する値） | 派生クラスの静的フィールド値に対応 |
| T-TryGetValue-003 | TryGetValue-003 | 複数の選択肢で異なる内部値が取得できる | Draft、Approved、Completed インスタンス生成済み | 各インスタンスで TryGetValue を呼び出す | Draft→1、Approved→2、Completed→3 | 各選択肢の内部値を検証 |

**テスト実装例（テストメソッド名形式）:**
- `TryGetValue_常に_Trueを返す()`
- `TryGetValue_outパラメータ_正しい内部値を格納する()`
- `TryGetValue_複数選択肢_各値に対応した内部値を返す()`

---

### 2.3 Validate メソッドのテスト（派生クラス経由）

**根拠:** 技術仕様書 §2.4

> **注記：** Validate は protected abstract メソッドであり、派生クラスが実装する。
> 以下は「無効な値が指定された場合に例外がスローされる」という基本原則を検証する。
> 具体的な範囲チェック（1～3 など）は派生クラスのテスト仕様書に委譲する。

| テストID | 観点ID | テストケース | 前提条件 | 操作 | 期待結果 | 備考 |
|---|---|---|---|---|---|---|
| T-Validate-001 | Validate-001 | 無効な値で ArgumentOutOfRangeException がスローされる | OrderStatus.From(99) を呼び出す | From メソッド内で Validate が自動実行される | ArgumentOutOfRangeException がスローされる | 派生クラスで範囲チェック (1～3) |
| T-Validate-002 | Validate-002 | 有効な値では例外がスローされない | OrderStatus.From(1) を呼び出す | From メソッド内で Validate が自動実行される | インスタンスが正常に生成される | Draft に対応する有効値 |
| T-Validate-003 | Validate-003 | 境界値で Validate が正確に動作する | OrderStatus.From(3) と OrderStatus.From(4) を呼び出す | 各値で Validate が実行される | From(3) は成功、From(4) は ArgumentOutOfRangeException | 範囲の上限を検証 |

**テスト実装例（テストメソッド名形式）:**
- `Validate_無効な値_ArgumentOutOfRangeExceptionを投げる()`
- `Validate_有効な値_インスタンス生成に成功する()`
- `Validate_境界値_上限超過時に例外を投げる()`

---

### 2.4 GetDisplayName メソッドのテスト（派生クラス経由）

**根拠:** 技術仕様書 §2.5

> **注記：** GetDisplayName は protected abstract メソッドであり、派生クラスが実装する。
> 以下は「メソッドが呼び出され、文字列が返される」という基本原則を検証する。
> 具体的な表示名（"下書き"、"承認済" など）は派生クラスのテスト仕様書に委譲する。

| テストID | 観点ID | テストケース | 前提条件 | 操作 | 期待結果 | 備考 |
|---|---|---|---|---|---|---|
| T-GetDisplayName-001 | GetDisplayName-001 | ToString() が GetDisplayName() の結果を返す | OrderStatus.Draft インスタンス生成済み | Draft.ToString() を呼び出す | "下書き"（日本語表示名）が返される | ToString から GetDisplayName を経由 |
| T-GetDisplayName-002 | GetDisplayName-002 | 各選択肢で異なる表示名が返される | Draft、Approved、Completed インスタンス生成済み | 各インスタンスで ToString() を呼び出す | Draft→"下書き"、Approved→"承認済"、Completed→"完了" | 選択肢ごとの表示内容 |
| T-GetDisplayName-003 | GetDisplayName-003 | 表示名は空文字列でない | 任意の OrderStatus インスタンス | ToString() を呼び出す | 空でない文字列が返される | 表示のための名称が存在 |

**テスト実装例（テストメソッド名形式）:**
- `ToString_GetDisplayName経由_表示名を返す()`
- `ToString_複数選択肢_各値に対応した表示名を返す()`
- `ToString_表示名_空文字列でない()`

---

### 2.5 GetEqualityComponents メソッドのテスト

**根拠:** 技術仕様書 §2.6

| テストID | 観点ID | テストケース | 前提条件 | 操作 | 期待結果 | 備考 |
|---|---|---|---|---|---|---|
| T-EqualityComp-001 | EqualityComp-001 | 同じ値の 2 つのインスタンスは等価 | OrderStatus.Draft のインスタンス 2 つ | instance1.Equals(instance2) を実行 | true を返す | 静的フィールド経由なら同じオブジェクト参照 |
| T-EqualityComp-002 | EqualityComp-002 | 異なる値の 2 つのインスタンスは非等価 | OrderStatus.Draft と Approved | Draft.Equals(Approved) を実行 | false を返す | 異なる選択肢 |
| T-EqualityComp-003 | EqualityComp-003 | 等価なインスタンスは同じハッシュコードを持つ | OrderStatus.Draft の参照 2 つ | GetHashCode() の結果を比較 | ハッシュコードが等しい | 等価性が成立 |
| T-EqualityComp-004 | EqualityComp-004 | 非等価なインスタンスは異なるハッシュコードを持つ（通常） | Draft と Approved | 各々の GetHashCode() を実行 | ハッシュコードが異なる（通常）| 異なる値オブジェクト |

**テスト実装例（テストメソッド名形式）:**
- `Equals_同じ値_Trueを返す()`
- `Equals_異なる値_Falseを返す()`
- `GetHashCode_等価_同じハッシュコードを返す()`
- `GetHashCode_非等価_異なるハッシュコードを返す()`

---

### 2.6 ToString メソッドのテスト

**根拠:** 技術仕様書 §2.7

| テストID | 観点ID | テストケース | 前提条件 | 操作 | 期待結果 | 備考 |
|---|---|---|---|---|---|---|
| T-ToString-001 | ToString-001 | ToString() は GetDisplayName() を呼び出す | OrderStatus インスタンス生成済み | ToString() を呼び出す | GetDisplayName() の返し値と同じ文字列が返される | IsSet=true のため "Unset" にはならない |
| T-ToString-002 | ToString-002 | ログ出力時に ToString() が使用できる | OrderStatus.Approved インスタンス | Console.WriteLine(status.ToString()) 相当を実行 | "承認済"（日本語表示名）がログに出力される | UI 層での直接利用を想定 |
| T-ToString-003 | ToString-003 | MessageBox に ToString() が直接渡せる | OrderStatus インスタンス | MessageBox.Show(status.ToString()) 相当を実行 | 日本語表示名がダイアログに表示される | WPF UI での利用想定 |

**テスト実装例（テストメソッド名形式）:**
- `ToString_GetDisplayName経由_表示名を返す()`
- `ToString_ログ出力_表示名が出力される()`
- `ToString_UI表示_日本語が表示される()`

---

### 2.7 静的メンバーのテスト（派生クラス経由）

**根拠:** 技術仕様書 §3「設計制約・禁止事項」

| テストID | 観点ID | テストケース | 前提条件 | 操作 | 期待結果 | 備考 |
|---|---|---|---|---|---|---|
| T-Static-001 | Static-001 | 静的フィールドが正しく初期化されている | OrderStatus クラス定義済み | OrderStatus.Draft、.Approved、.Completed を参照 | 各々が正しい EnumValueObject インスタンス | static readonly フィールド定義 |
| T-Static-002 | Static-002 | From メソッドが値から逆引きできる | OrderStatus.From(1) を呼び出す | From メソッド内で switch 式が実行される | OrderStatus.Draft が返される | 値 1 → Draft |
| T-Static-003 | Static-003 | From メソッドが無効な値で例外をスロー | OrderStatus.From(99) を呼び出す | Validate が From 内で実行 | ArgumentOutOfRangeException がスローされる | 無効な値のハンドリング |
| T-Static-004 | Static-004 | From メソッドが複数の値に対応 | OrderStatus.From(1, 2, 3) を各々呼び出す | 各値が対応する選択肢に逆引きされる | Draft、Approved、Completed がそれぞれ返される | 全選択肢の逆引き |

**テスト実装例（テストメソッド名形式）:**
- `StaticField_定義済み_正しくInitializeされている()`
- `From_有効な値_対応する選択肢を返す()`
- `From_無効な値_例外を投げる()`
- `From_複数値_各値に対応した選択肢を返す()`

---

### 2.8 派生クラスの制約チェック

**根拠:** 技術仕様書 §3「設計制約・禁止事項」

| テストID | 観点ID | テストケース | 前提条件 | 操作 | 期待結果 | 備考 |
|---|---|---|---|---|---|---|
| T-Constraint-001 | Constraint-001 | sealed クラスはさらに継承できない | OrderStatus クラス生成 | OrderStatus を親にして派生クラスを定義しようとする | コンパイルエラー（またはリフレクション検証で確認） | sealed 修飾子の強制 |
| T-Constraint-002 | Constraint-002 | コンストラクタは private である | OrderStatus インスタンス | new OrderStatus(1) を直接呼び出す | コンパイルエラー（アクセス不可） | 静的フィールド経由のみ |
| T-Constraint-003 | Constraint-003 | TryFrom は実装されない | OrderStatus クラス定義 | TryFrom メソッドを探す | メソッドが存在しない（IOptionalValueObject 非実装） | null許容不対応 |

**テスト実装例（テストメソッド名形式）:**
- `Sealed_派生禁止_コンパイルエラーになる()`
- `Constructor_Private_外部からインスタンス化不可()`
- `TryFrom_未実装_存在しない()`

---

## 3. テスト実装の責務区分

### 3.1 本テスト仕様書で検証する範囲（EnumValueObject 基底）

- IsSet プロパティの常時 true 返却
- TryGetValue の try パターン実装と out パラメータの正確性
- Validate の自動実行と例外スロー基本動作
- GetDisplayName の被呼び出し機構（ToString 経由）
- GetEqualityComponents による等価性判定の基礎
- ToString の IsSet チェック形式（形式上の "Unset" 対応）
- 派生クラス制約の設計制約（sealed、private コンストラクタ）

### 3.2 派生クラス個別テスト仕様書に委譲する範囲

各派生クラス（OrderStatus、WorkDivision など）の個別テスト仕様書では、以下を検証：

- **Validate メソッドの具体範囲**: OrderStatus では 1～3、他のクラスでは適切な範囲
- **GetDisplayName の具体的な値変換**: OrderStatus では 1→"下書き"、2→"承認済"、3→"完了"
- **From メソッドの逆引きロジック**: 各値から対応する静的フィールドへの正確な割り当て
- **コンストラクタの具体シグネチャ**: private OrderStatus(int value) など

---

## 4. テスト実装のガイドライン

### 4.1 テストメソッド命名規則

本テスト仕様書のテストメソッドは、以下の形式で実装される：

```csharp
[Fact]
public void [操作]_[条件]_[期待結果]()
{
    // Arrange（前提条件の準備）
    
    // Act（操作の実行）
    
    // Assert（期待結果の検証）
}
```

**例：**
```csharp
[Fact]
public void IsSet_常に_Trueを返す()
{
    // Arrange: OrderStatus.Draft の静的フィールド参照
    var draft = OrderStatus.Draft;
    
    // Act: IsSet プロパティを読み取る
    var result = draft.IsSet;
    
    // Assert: true を検証
    Assert.True(result);
}
```

### 4.2 前提条件の確認（Arrange）

各テストケースは、派生クラス（OrderStatus など）の実装が完了している前提で設計されている。

- OrderStatus が sealed クラスか確認
- OrderStatus.Draft、Approved、Completed が static readonly フィールドとして定義されているか確認
- OrderStatus.From(int value) メソッドが存在するか確認

### 4.3 派生クラス固有テストの実装時期

本テスト仕様書は EnumValueObject 基底クラスの汎用性を検証するが、以下の具体テストは派生クラスの個別テスト仕様書内で実装：

- OrderStatus の具体的な Validate 範囲（1～3）チェック
- OrderStatus の具体的な GetDisplayName ロジック（"下書き" など）検証
- OrderStatus.From(1) → Draft の逆引き確認

---

## 5. テストケースの実装順序

以下の順序で実装することを推奨：

1. **基本プロパティのテスト** (IsSet、TryGetValue)
2. **メソッド実装テスト** (Validate、GetDisplayName、ToString)
3. **等価性・ハッシュのテスト** (Equals、GetHashCode)
4. **静的メンバーのテスト** (From、静的フィールド)
5. **設計制約チェック** (sealed、private コンストラクタ)

---

## 6. テストカバレッジ目標

| 項目 | 目標 | 根拠 |
|---|---|---|
| **メソッドカバレッジ** | 100%（IsSet、TryGetValue、ToString、Equals、GetHashCode、GetEqualityComponents） | 基底クラスの全メンバーを検証 |
| **ブランチカバレッジ** | 100%（if / switch 文など） | 条件分岐の全パターンをテスト |
| **マテリアルカバレッジ** | 全テストケース実装 | 本仕様書に列挙された全観点を網羅 |

---

## 7. テスト環境・依存関係

| 項目 | 仕様 |
|---|---|
| **テストフレームワーク** | xUnit |
| **アサーション ライブラリ** | Xunit.Assert または FluentAssertions |
| **モック フレームワーク** | 不要（値オブジェクトのため副作用なし） |
| **テスト対象のコンパイル** | EnumValueObject<TValue> が正常にコンパイルされていること |

---

## 8. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-07-04 | Claude Code | 初版作成 / テクニカル仕様書 1.0 から自動生成 |

---

## 付録 A：テストデータ（OrderStatus 例）

以下は、テスト実装時に使用する具体的なテストデータ例である：

### A.1 有効な選択肢

```csharp
OrderStatus.Draft       // 内部値: 1, 表示名: "下書き"
OrderStatus.Approved    // 内部値: 2, 表示名: "承認済"
OrderStatus.Completed   // 内部値: 3, 表示名: "完了"
```

### A.2 無効な値

```csharp
OrderStatus.From(0)     // → ArgumentOutOfRangeException
OrderStatus.From(4)     // → ArgumentOutOfRangeException
OrderStatus.From(-1)    // → ArgumentOutOfRangeException
OrderStatus.From(99)    // → ArgumentOutOfRangeException
```

### A.3 From メソッドの逆引き

```csharp
OrderStatus.From(1)  // → OrderStatus.Draft
OrderStatus.From(2)  // → OrderStatus.Approved
OrderStatus.From(3)  // → OrderStatus.Completed
```

---

## 付録 B：テスト実装チェックリスト

テスト実装完了時に、以下をチェックしてください：

- ✅ IsSet プロパティが常に true を返す（T-IsSet-001, 002）
- ✅ TryGetValue が常に true を返す（T-TryGetValue-001, 002, 003）
- ✅ Validate が無効値で例外をスロー（T-Validate-001, 002, 003）
- ✅ GetDisplayName が ToString 経由で呼ばれる（T-GetDisplayName-001, 002, 003）
- ✅ Equals / GetHashCode で等価性判定が正確（T-EqualityComp-001～004）
- ✅ ToString が GetDisplayName の結果を返す（T-ToString-001, 002, 003）
- ✅ 静的フィールドが正しく初期化（T-Static-001, 002, 003, 004）
- ✅ sealed 修飾子が強制される（T-Constraint-001）
- ✅ コンストラクタが private（T-Constraint-002）
- ✅ TryFrom が実装されない（T-Constraint-003）
- ✅ テストカバレッジ 100%（メソッド・ブランチ）
