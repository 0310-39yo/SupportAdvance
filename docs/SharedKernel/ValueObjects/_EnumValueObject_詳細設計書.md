# 詳細設計書 — EnumValueObject<TSelf, TValue>

**プロジェクト:** SupportAdvance  
**レイヤ:** Domain 層  
**種別:** 抽象基底クラス詳細設計  
**依拠技術仕様書:** EnumValueObject<TSelf, TValue> 技術仕様書 v1.0  
**版:** 1.0 / 2026-07-04

---

## 1. クラス概要

### 1.1 クラス定義

| 項目 | 内容 |
|------|------|
| クラス名 | `EnumValueObject<TSelf, TValue>` |
| 種別 | `public abstract class` |
| 名前空間 | `Domain.ValueObjects.Base.Enum` |
| 実装インターフェース | なし |
| 継承元 | `ValueObject` |
| 配置レイヤ | Domain 層（値オブジェクト基底） |
| ジェネリック制約 | `where TSelf : EnumValueObject<TSelf, TValue> where TValue : struct` |

---

### 1.2 責務

- **選択肢型値オブジェクトの共通処理を提供する**：Validate → GetDisplayName の標準フロー
- **業務名称（表示名）を管理する**：GetDisplayName() を抽象メソッドで派生クラスに委譲
- **等価性判定を実装する**：IsSet と ValueField に基づく同一性判定
- **値の安全な取得を保証する**：TryGetValue メソッドで out パラメータ経由の取得

---

### 1.3 協調クラス

```
EnumValueObject<TSelf, TValue>
  ├─ extends ──▶ ValueObject
  │              └─ IsSet プロパティ
  │              └─ GetEqualityComponents() 抽象メソッド
  │
  └─ (派生クラス例)
     ├─ OrderStatus
     │  └─ override Validate(int value)
     │  └─ override GetDisplayName() → string
     ├─ WorkDivision
     │  └─ override Validate(int value)
     │  └─ override GetDisplayName() → string
     └─ DesignWorkClassification
        └─ override Validate(int value)
        └─ override GetDisplayName() → string
```

---

## 2. フィールド・プロパティ設計

### 2.1 `ValueField`

選択肢に対応する内部値（スカラ値）。

| 項目 | 内容 |
|------|------|
| 型 | `protected readonly TValue` |
| アクセス修飾子 | `protected` |
| セッタ | なし（readonly） |
| デフォルト値 | 派生クラスのコンストラクタで設定 |

**設計判断**

- `protected readonly` で派生クラスからの読み取りを許可し、外部からの変更は禁止
- TValue は struct 制約により、値型に限定（int, short, DateTime 等）
- 初期化はコンストラクタの base(value) 呼び出し時に実施

---

### 2.2 `IsSet` プロパティ（継承）

ValueObject 基底から継承するプロパティ。EnumValueObject では常に true。

| 項目 | 内容 |
|------|------|
| 型 | `public bool` |
| アクセス修飾子 | `public { get; protected init; }` |
| セッタ | protected init |
| デフォルト値 | true（コンストラクタで設定） |

**設計判断**

- EnumValueObject では常に true に設定（Unset 状態不要）
- ValueObject の protected init を利用し、基底クラスのコンストラクタから初期化
- 形式的には bool ですが、実装上は常に true

---

## 3. コンストラクタ設計

### 3.1 保護されたコンストラクタ（値設定）

```csharp
protected EnumValueObject(TValue value)
{
  IsSet = true;
  ValueField = value;
  Validate(value);
}
```

**責務**

- 派生クラスから呼び出され、値を設定してオブジェクトを初期化
- IsSet を true で初期化
- Validate() を呼び出して妥当性チェック

**処理フロー**

1. IsSet = true で初期化
2. ValueField に value を代入
3. Validate(value) で値の検証を実行
4. Validate が例外をスローした場合、オブジェクト生成は失敗

**設計判断**

- protected アクセスにより、派生クラスのみが呼び出し可能
- 派生クラスは private コンストラクタで本コンストラクタを呼び出す
- 例：`private OrderStatus(int value) : base(value) { }`

---

## 4. メソッド設計

### 4.1 TryGetValue

```csharp
public bool TryGetValue(out TValue value)
{
  if (!IsSet)
  {
    value = default;
    return false;
  }
  value = ValueField;
  return true;
}
```

**責務**

- 選択肢の内部値を安全に取得

**処理フロー**

1. IsSet が false なら、value を default に設定して false を返す
2. IsSet が true なら、value に ValueField を代入して true を返す

**設計判断**

- EnumValueObject では常に IsSet=true なため、戻り値は常に true
- ただし、将来的な拡張性（オプショナル化）を考慮し、try パターンを採用
- Application 層で使用：`if (status.TryGetValue(out int value)) { ... }`

---

### 4.2 Validate（抽象メソッド）

```csharp
protected abstract void Validate(TValue value);
```

**責務**

- 派生クラスが実装し、選択肢の妥当性を検証

**実装責任**

- 派生クラスが override して、有効な選択肢の範囲チェック等を実装
- 無効な value の場合、ArgumentOutOfRangeException をスロー

**実装例**

```csharp
protected override void Validate(int value)
{
  if (value is < 1 or > 3)
    throw new ArgumentOutOfRangeException(nameof(value));
}
```

**設計判断**

- protected abstract であり、派生クラスは必ず実装する
- コンストラクタから自動実行されるため、オブジェクト生成時の整合性を保証
- 例外が発生した場合、オブジェクトは生成されない（不完全な状態を回避）

---

### 4.3 GetDisplayName（抽象メソッド）

```csharp
protected abstract string GetDisplayName();
```

**責務**

- 派生クラスが実装し、ValueField に対応する業務名称を返す

**実装責任**

- 派生クラスが override して、switch 式で ValueField → 日本語名を変換

**実装例**

```csharp
protected override string GetDisplayName()
{
  return ValueField switch
  {
    1 => "下書き",
    2 => "承認済",
    3 => "完了",
    _ => string.Empty
  };
}
```

**設計判断**

- IsSet=true のみで呼び出されるため、Unset 時の処理は不要
- switch 式で簡潔に実装（効率的な分岐）
- 戻り値は常に string（空文字列の可能性もある）

---

### 4.4 GetEqualityComponents（オーバーライド）

```csharp
protected override IEnumerable<object?> GetEqualityComponents()
{
  yield return IsSet;
  if (IsSet)
  {
    yield return ValueField;
  }
}
```

**責務**

- ValueObject 基底の Equals / GetHashCode で使用するコンポーネントを列挙

**処理フロー**

1. IsSet を最初に yield return
2. IsSet が true なら ValueField も yield return

**設計判断**

- IsSet と ValueField の両方をコンポーネントとして列挙
- ValueObjectComponentNormalizer により、IsSet の重複排除が自動実行
- 等価性判定は「同じ選択肢か（ValueField が一致するか）」のみに依存

---

### 4.5 ToString（オーバーライド）

```csharp
public override string ToString()
{
  return IsSet ? GetDisplayName() : "Unset";
}
```

**責務**

- オブジェクトを文字列で表現（ログ出力、UI 表示）

**処理フロー**

1. IsSet が true なら GetDisplayName() の結果を返す
2. IsSet が false なら "Unset" を返す

**設計判断**

- IsSet は常に true なため、戻り値は常に GetDisplayName() の結果
- "Unset" は形式上の出力（実装上は到達しない）
- UI 層で MessageBox.Show(orderStatus.ToString()) のように直接利用可能

---

## 5. 派生クラスへの設計指示

### 5.1 クラス定義

```csharp
public sealed class OrderStatus
  : EnumValueObject<OrderStatus, int>
{
  // ...
}
```

**必須事項**

- `sealed` 修飾子を使用し、さらなる継承を禁止
- `EnumValueObject<TSelf, TValue>` を継承（TSelf = 自身のクラス名）
- TValue は struct 型（int, short, byte 等）に限定

---

### 5.2 選択肢の定義

```csharp
public static readonly OrderStatus Draft = new(1);
public static readonly OrderStatus Approved = new(2);
public static readonly OrderStatus Completed = new(3);
```

**必須事項**

- 各選択肢を `public static readonly` フィールドで定義
- 値を引数で指定してインスタンスを生成
- フィールド名は PascalCase（英語）で命名

---

### 5.3 コンストラクタ

```csharp
private OrderStatus(int value)
  : base(value)
{
}
```

**必須事項**

- `private` コンストラクタのみ（protected は不可）
- base(value) で基底クラスに値を渡す
- 追加の初期化処理は不要（Validate は基底で自動実行）

---

### 5.4 From メソッド

```csharp
public static OrderStatus From(int value)
{
  return value switch
  {
    1 => Draft,
    2 => Approved,
    3 => Completed,
    _ => throw new ArgumentOutOfRangeException(nameof(value))
  };
}
```

**必須事項**

- `public static` メソッド
- 値（int）から選択肢インスタンスを取得
- switch 式で簡潔に実装
- 無効な value の場合 ArgumentOutOfRangeException をスロー

**用途**

- Application 層で、API リクエストボディの int 値から VO を生成
- 例：`var status = OrderStatus.From(request.StatusCode);`

---

### 5.5 Validate メソッド（オーバーライド）

```csharp
protected override void Validate(int value)
{
  if (value is < 1 or > 3)
    throw new ArgumentOutOfRangeException(nameof(value));
}
```

**必須事項**

- `protected override` メソッド
- 有効な選択肢の範囲チェック
- 無効な場合 ArgumentOutOfRangeException をスロー

---

### 5.6 GetDisplayName メソッド（オーバーライド）

```csharp
protected override string GetDisplayName()
{
  return ValueField switch
  {
    1 => "下書き",
    2 => "承認済",
    3 => "完了",
    _ => string.Empty
  };
}
```

**必須事項**

- `protected override` メソッド
- switch 式で ValueField → 業務名称を変換
- 日本語テキスト（ローカライズ対応可能）

---

## 6. レイヤ制約確認

### Domain 層の制約

- ✅ ValueObject は null を許容しない → EnumValueObject は IsSet=true のみ
- ✅ 業務ルールを型で表現 → From() の検証で無効な選択肢を排除
- ✅ DB・ORM・外部システムを知らない → Validate は純粋な値チェック
- ✅ ログ実装を知らない → ToString() は Business ロジックに依存しない

### Application 層での利用

```csharp
public class UpdateOrderStatusUseCase
{
  public void Execute(int statusCode)
  {
    var status = OrderStatus.From(statusCode);  // 検証済み VO
    // status.ToString() で UI 層に渡せる
  }
}
```

---

## 7. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-07-04 | 加藤 正人 | 初版作成（SupportAdvance 版）
