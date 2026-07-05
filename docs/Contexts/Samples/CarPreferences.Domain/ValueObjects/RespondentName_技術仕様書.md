# RespondentName 技術仕様書

**バージョン:** 1.0  
**作成日:** 2025年  
**責務:** アンケート回答者の名前を管理する値オブジェクト  

---

## 1. 概要

### 1.1 クラス説明

`RespondentName` は、アンケート回答者の名前を表す**値オブジェクト**です。ValueObject の本質である不変性により、スレッドセーフな設計を実現しています。

- **継承**: `PrimitiveValueObject<string>`
- **シール**: `sealed class`（拡張不可）
- **用途**: ドメインモデルのエンティティにおいて、回答者の名前を型安全に表現する
- **特性**: ValueObject の不変性 → 値による等価性、依存性の低い設計、テスト容易性の向上

### 1.2 責務

| 責務 | 説明 |
|---|---|
| **文字列値の安全な保管** | string 値をラップし、ビジネスルール検証を自動実行 |
| **null 安全性の提供** | nullable と non-nullable の両シナリオに対応 |
| **未設定状態の表現** | null ではなく型で未設定状態（Unset）を表現（Option パターン） |
| **等価性判定** | 名前の内容が同一なら等価（値オブジェクト） |
| **ハッシング対応** | HashMap や HashSet への格納に対応 |

### 1.3 制約・ビジネスルール

| ルール | 詳細 |
|---|---|
| **Null チェック** | `From()` へ null を渡すと `ArgumentNullException` をスロー |
| **未設定状態の共有** | `Unset()` で生成した複数のインスタンスは等価（値が等しい） |
| **イミュータビリティ** | 一度生成されたインスタンスは変更不可（ValueObject） |

---

## 2. プロパティ & メソッド

### 2.1 プロパティ

#### `IsSet : bool { get; }` (PrimitiveValueObject から継承)

**役割:** 値が設定されているかを判定

```csharp
var respondentName = RespondentName.From("田中太郎");
Assert.True(respondentName.IsSet);  // true

var unset = RespondentName.Unset();
Assert.False(unset.IsSet);  // false
```

#### `Value : string { get; }` (PrimitiveValueObject から継承)

**役割:** 保持する文字列値を読み取り専用で取得

```csharp
var respondentName = RespondentName.From("山田花子");
string name = respondentName.Value;  // "山田花子"
```

**注意:** `IsSet = false` の場合、`Value` は default(string) です。

### 2.2 ファクトリメソッド

#### `Unset() : RespondentName`

**役割:** 未設定状態の RespondentName インスタンスを生成

```csharp
var unset = RespondentName.Unset();
Assert.False(unset.IsSet);
Assert.Equal("Unset", unset.ToString());
```

**実行フロー:**
1. プライベートコンストラクタ `RespondentName(bool isSet)` を呼び出し
2. `isSet = false` を渡す

#### `From(string value) : RespondentName`

**役割:** 指定された文字列から RespondentName を生成

```csharp
var respondentName = RespondentName.From("佐藤次郎");
Assert.True(respondentName.IsSet);
Assert.Equal("佐藤次郎", respondentName.Value);
```

**例外:**
- `ArgumentNullException` : `value` が null の場合

**実行フロー:**
1. `ArgumentNullException.ThrowIfNull(value)` で null チェック
2. プライベートコンストラクタ `RespondentName(string value, bool isSet)` を呼び出し
3. `isSet = true` を渡す
4. 基本クラスのコンストラクタで自動的に `Validate()` が実行される

#### `TryFrom(string? input, out RespondentName result) : bool`

**役割:** null 安全な生成、失敗時は false を返す

```csharp
// 成功例
bool success = RespondentName.TryFrom("鈴木太郎", out var respondentName);
Assert.True(success);
Assert.True(respondentName.IsSet);

// null 入力は Unset を返す（失敗ではない）
bool success2 = RespondentName.TryFrom(null, out var unset);
Assert.True(success2);
Assert.False(unset.IsSet);
```

**実行フロー:**
1. `input is null` → `Unset()` を生成して true を返す
2. `From(input)` を試行 → 成功時 true、例外時 false を返す

---

## 3. 等価性メソッド

### 3.1 Equals メソッド

#### `Equals(RespondentName? other) : bool`

**役割:** 指定された RespondentName と等価かどうかを判定

```csharp
var a = RespondentName.From("太郎");
var b = RespondentName.From("太郎");
Assert.True(a.Equals(b));  // 値が同じなら等価

var c = RespondentName.From("花子");
Assert.False(a.Equals(c));  // 値が異なれば非等価

// 未設定状態同士は等価
var u1 = RespondentName.Unset();
var u2 = RespondentName.Unset();
Assert.True(u1.Equals(u2));
```

**実装:** 基本クラス `ValueObject` の `Equals(ValueObject?)` に処理を委譲

---

## 4. 文字列化

### 4.1 ToString メソッド

**役割:** インスタンスを文字列で表現

```csharp
var respondentName = RespondentName.From("鈴木");
string str = respondentName.ToString();  // "鈴木"

var unset = RespondentName.Unset();
string unsetStr = unset.ToString();  // "Unset"
```

**実装:** 基本クラス `PrimitiveValueObject` から継承

---

## 5. 使用例

### 5.1 基本的な使用方法

```csharp
// 名前を生成
var name = RespondentName.From("佐藤太郎");
Console.WriteLine($"名前: {name.Value}");  // 名前: 佐藤太郎

// 未設定状態
var unsetName = RespondentName.Unset();
if (!unsetName.IsSet)
{
	Console.WriteLine("名前は未設定です");
}
```

### 5.2 Try パターンの使用

```csharp
// ユーザー入力を安全に処理
private bool TryCreateRespondent(string? nameInput)
{
	// TryFrom で null 安全に処理
	if (!RespondentName.TryFrom(nameInput, out var respondentName))
	{
		Console.WriteLine("名前の処理に失敗しました");
		return false;
	}

	// 成功時
	if (respondentName.IsSet)
	{
		Console.WriteLine($"回答者: {respondentName.Value}");
	}
	else
	{
		Console.WriteLine("名前が設定されていません");
	}

	return true;
}
```

### 5.3 エンティティでの使用

```csharp
public class Respondent  // ドメインエンティティ
{
	public RespondentId Id { get; }
	public RespondentName Name { get; }  // ValueObject として保持
	public RespondentAge Age { get; }
	public CreatedAt CreatedAt { get; }
	public UpdatedAt UpdatedAt { get; }

	public Respondent(RespondentId id, RespondentName name, RespondentAge age)
	{
		Id = id ?? throw new ArgumentNullException(nameof(id));
		Name = name ?? throw new ArgumentNullException(nameof(name));
		Age = age ?? throw new ArgumentNullException(nameof(age));
		CreatedAt = CreatedAt.From(DateTime.UtcNow);
		UpdatedAt = UpdatedAt.From(DateTime.UtcNow);
	}
}

// 使用
var respondentName = RespondentName.From("田中花子");
var respondent = new Respondent(
	new RespondentId(Guid.NewGuid()),
	respondentName,
	RespondentAge.From(30)
);
```

---

## 6. 設計上の決定理由

### 6.1 PrimitiveValueObject<string> の選択

- **利点:** 汎用的な ValueObject パターンを提供
- **実装の統一:** 他の ValueObject（CreatedAt, UpdatedAt など）との一貫性
- **検証基盤:** `Validate()` や `Normalize()` の拡張ポイント

### 6.2 IOptionalValueObject の実装

- **null 安全性:** 従来の nullable string の問題を回避
- **Option パターン:** 未設定状態を型で表現（Unset メソッドで対応）
- **Domain-Driven Design:** ビジネスロジックが未設定状態を明示的に扱える

### 6.3 TryFrom の実装戦略

- **安全性:** null 入力を例外ではなく成功結果として処理
- **親切設計:** `TryFrom(string?)` で null を渡して `Unset` を得ることで、Option パターンをサポート
- **予測可能性:** *Try* パターンに従い、false のみで失敗を示す

---

## 7. DDD における位置付け

### 7.1 Value Object として

- **不変性:** インスタンス作成後、状態変化なし
- **値による等価性:** 名前の内容が同じなら、別インスタンスでも等価
- **キャスト不要:** 文字列型を直接使用するより、ドメイン意図が明確

### 7.2 Aggregate Root との関係

- `Respondent` エンティティの属性として保持
- `RespondentId` と同じく、エンティティの識別や属性管理に必須
- 独立した生命周期を持たない（Aggregate に依存）

---

## 8. API 互換性

- **最小限のパブリックAPI:** Unset / From / TryFrom / Equals のみ
- **隠蔽性:** プライベートコンストラクタで直接インスタンス化を防止
- **将来の拡張:** `Validate()` や `Normalize()` をオーバーライドしてビジネスルール追加可能

---

## 9. テスト容易性

- **単位テスト対象:** From / TryFrom / Equals / ToString
- **null ハンドリング:** 複数の null シナリオをテスト
- **不変性検証:** インスタンス作成後の変更不可を確認
- **等価性テスト:** 値による等価性を検証

---

## 10. 参考資料

- [PrimitiveValueObject<T>](../../../SharedKernel/ValueObjects/Abstractions/PrimitiveValueObject.cs)
- [IOptionalValueObject<TSelf, TValue>](../../../SharedKernel/ValueObjects/IOptionalValueObject.cs)
- [ValueObject 基本クラス](../../../SharedKernel/ValueObjects/Abstractions/ValueObject.cs)
- 関連 ValueObject: [CreatedAt](../../../SharedKernel/ValueObjects/Audit/CreatedAt_技術仕様書.md), [RespondentAge](../RespondentAge_技術仕様書.md)
