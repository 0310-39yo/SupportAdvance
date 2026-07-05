# 技術仕様書 — EnumValueObject<TValue>

**プロジェクト:** SupportAdvance  
**レイヤ:** Domain 層  
**種別:** 抽象基底クラス設計原則  
**版:** 1.0 / 2026-07-04

---

## 1. 位置づけ

`EnumValueObject<TValue>` は SupportAdvance プロジェクトの Domain 層において、**定義済みの選択肢から 1 つを選択する値オブジェクト**の基底クラスである。

本クラスの目的は以下に集約される。

- OrderStatus（注文状態）、WorkDivision（業務区分）、DesignWorkClassification（設計作業分類）のような「固定選択肢」を表現する
- 「任意の値」ではなく「選択肢に限定された値」として、業務ルールを型で表現する
- ToString() が業務名称（表示名）を返すことで、UI 層での直接利用を実現する

> **📌 原則**  
> Domain 層の ValueObject は null を許容しない。EnumValueObject は常に値を保持する。

---

## 2. メンバー仕様

### 2.1 IsSet プロパティ

値が設定されているか判定するブール値フラグ。

| 項目 | 内容 |
|------|------|
| シグネチャ | `public bool IsSet { get; protected init; }` |
| 戻り値 | true（常に。IsSet=true のみが有効状態） |
| 例外 | 例外を投げない |
| 用途 | 等価性判定、ToString 判定、**0 vs null の識別** |

**設計判断**

- EnumValueObject は常に値を持つため、IsSet は常に true
- IOptionalValueObject を実装しない（具体型で static abstract メソッドを提供）
- ValueObject 基底との整合性を保つため、IsSet プロパティは継承元から取得

---

### 2.2 ValueField プロパティ

選択肢に対応する内部値（通常は int）。

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected readonly TValue ValueField` |
| 戻り値 | TValue 型の値 |
| 例外 | 例外を投げない |
| 用途 | 派生クラスが GetDisplayName() で業務名称に変換する際の参照 |

**設計判断**

- protected readonly であり、派生クラスからのみ参照可能
- Validate 検証済みの値を保持
- 代理メソッド TryGetValue() で外部から安全に取得

---

### 2.3 TryGetValue メソッド

選択肢を表す内部値を取得するメソッド。

| 項目 | 内容 |
|------|------|
| シグネチャ | `public bool TryGetValue(out TValue value)` |
| 戻り値 | IsSet=true なら true（value に ValueField を格納）、false なら false（value は default） |
| 例外 | 例外を投げない |
| 用途 | Application 層や Presentation 層で、安全に内部値を取得して API レスポンス等に変換 |

**設計判断**

- EnumValueObject は常に IsSet=true なため、戻り値は常に true
- 将来的な拡張性（オプショナル化）を考慮し、try パターンを採用
- out パラメータで値を返す（LINQ Where 句での利用を考慮）

---

### 2.4 Validate メソッド（抽象）

派生クラスが実装し、選択肢の妥当性を検証。

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected abstract void Validate(TValue value)` |
| 戻り値 | なし |
| 例外 | 無効な value の場合、ArgumentOutOfRangeException をスロー |
| 用途 | コンストラクタ内で自動実行。選択肢の範囲チェック等 |

**設計判断**

- protected abstract であり、派生クラスが必ず実装する
- コンストラクタから呼び出され、オブジェクト生成時の整合性を保証
- 例外スロー時、オブジェクト生成は失敗（不完全な状態での回避）

---

### 2.5 GetDisplayName メソッド（抽象）

選択肢に対応する業務名称（日本語表示名）を返すメソッド。

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected abstract string GetDisplayName()` |
|戻り値 | ValueField に対応する業務名称（例："下書き"、"承認済"） |
| 例外 | 例外を投げない |
| 用途 | ToString() から呼び出され、UI 層での直接表示を実現 |

**設計判断**

- IsSet=true のみで呼び出されるため、unset 時の処理は不要
- switch 式で簡潔に実装（ValueField → 業務名称の変換）
- ログ出力や画面表示に直接利用可能（ローカライズ対応も可能）

---

### 2.6 GetEqualityComponents メソッド（オーバーライド）

等価性判定用のコンポーネント列挙。

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected override IEnumerable<object?> GetEqualityComponents()` |
| 戻り値 | IsSet と ValueField を列挙 |
| 例外 | 例外を投げない |
| 用途 | ValueObject 基底の Equals / GetHashCode で使用 |

**設計判断**

- IsSet と ValueField の両方を列挙（ValueObjectComponentNormalizer で IsSet 重複排除）
- 等価性判定は「同じ選択肢か」のみに依存

---

### 2.7 ToString メソッド（オーバーライド）

業務名称を文字列で返すメソッド。

| 項目 | 内容 |
|------|------|
| シグネチャ | `public override string ToString()` |
| 戻り値 | IsSet=true なら GetDisplayName() の結果、false なら "Unset" |
| 例外 | 例外を投げない |
| 用途 | ログ出力、デバッグ出力、UI 層での表示 |

**設計判断**

- "Unset" は形式上の出力（実装上 IsSet は常に true）
- UI 層で MessageBox.Show(orderStatus.ToString()) のように直接利用可能
- ローカライズが必要な場合は、GetDisplayName() をオーバーライド

---

## 3. 設計制約・禁止事項

### 派生クラスの実装ルール

1. **sealed 修飾子の使用**
   - OrderStatus, WorkDivision 等は必ず sealed クラスとし、さらなる継承を禁止
   - EnumValueObject の派生クラスは「最終形」とする

2. **コンストラクタの構成**
   - private コンストラクタのみ（protected は不可）
   - 値設定用と Unset 用の 2 種類のオーバーロードは不要（IsSet=true のみ）
   - 例：`private OrderStatus(int value) : base(value) { }`

3. **静的メンバー・静的メソッド**
   - 各選択肢を static readonly フィールドで定義
     ```csharp
     public static readonly OrderStatus Draft = new(1);
     public static readonly OrderStatus Approved = new(2);
     ```
   - From(int value) メソッドで値から逆引き
     ```csharp
     public static OrderStatus From(int value) => value switch { 1 => Draft, 2 => Approved, ... };
     ```
   - TryFrom は実装不要（IOptionalValueObject 非実装）

4. **Validate / GetDisplayName の実装**
   - Validate: 有効な選択肢の範囲チェック（例：1～3）
   - GetDisplayName: switch 式で ValueField → 日本語名変換

5. **null 許容性**
   - Domain 層コードでは null を許容しない
   - API レスポンス（Application 層）で null が必要なら、IOptionalValueObject を別途実装

---

## 4. 実装パターン例（OrderStatus）

```csharp
public sealed class OrderStatus
  : EnumValueObject<int>
{
  // 選択肢を static readonly で定義
  public static readonly OrderStatus Draft = new(1);
  public static readonly OrderStatus Approved = new(2);
  public static readonly OrderStatus Completed = new(3);

  // private コンストラクタ（値設定）
  private OrderStatus(int value)
    : base(value)
  {
  }

  // 値から逆引き
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

  // Validate: 有効な選択肢範囲チェック
  protected override void Validate(int value)
  {
    if (value is < 1 or > 3)
      throw new ArgumentOutOfRangeException(nameof(value));
  }

  // GetDisplayName: 業務名称への変換
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
}
```

**使用例**：
```csharp
var status = OrderStatus.Draft;
Console.WriteLine(status);  // → "下書き"

status.TryGetValue(out int value);
value  // → 1

var status2 = OrderStatus.From(2);
status2.ToString();  // → "承認済"

var status3 = OrderStatus.From(99);  // → ArgumentOutOfRangeException スロー
```

---

## 5. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-07-04 | 加藤 正人 | 初版作成 |

