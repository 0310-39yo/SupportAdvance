# CreatedBy 詳細設計書

**バージョン:** 1.0  
**作成日:** 2026年08月10日  
**依拠技術仕様書:** CreatedBy技術仕様書 v1.0

---

## 1. クラス概要

### 1.1 クラス定義

| 項目 | 内容 |
|------|------|
| クラス名 | `CreatedBy` |
| 種別 | `sealed class` |
| 名前空間 | `SupportAdvance.SharedKernel.ValueObjects.Audit` |
| 実装インターフェース | `IEquatable<CreatedBy>` |
| 継承元 | `PrimitiveValueObject<long>` |
| 配置レイヤ | SharedKernel（Domain層ベース） |

### 1.2 責務

- **値の保持** — 作成者の従業員行ID（long）を不変で保持
- **検証** — 行IDの妥当性をチェック（0より大きい値のみ）
- **等価性判定** — 同じ行IDを持つ2つのインスタンスは等価
- **ファクトリメソッド** — From()、TryFrom()、TryFromDbValue() を提供

### 1.3 協調クラス

```
CreatedBy  ──uses──▶  PrimitiveValueObject<long>
                           └─ GetValueComponents()

Mapper  ──uses──▶  CreatedBy
                       └─ TryFromDbValue()（DB値変換）

Entity  ──has──▶  CreatedBy
                      └─ 監査情報として保持
```

---

## 2. プロパティ設計

### 2.1 `Value` プロパティ

| 項目 | 内容 |
|------|------|
| 型 | `long` |
| アクセス修飾子 | `public` |
| セッタ | なし（読み取り専用） |
| デフォルト値 | なし（コンストラクタで指定） |

**設計判断**

- long 値をそのまま返す（LocalDateTime と異なり、型変換なし）
- ドメイン層では Value プロパティを通じて行IDにアクセス
- Mapper での DB 変換時、Value で内部値を取得

### 2.2 `IsSet` プロパティ

| 項目 | 内容 |
|------|------|
| 型 | `bool` |
| アクセス修飾子 | `public` |
| セッタ | なし |
| 値 | 常に `true`（必須値） |

**設計判断**

- CreatedBy は必須値なため IsSet は常に true
- IOptionalValueObject を実装しない（必須パターンのため）

---

## 3. メソッド設計

### 3.1 `From(long value)` — ファクトリメソッド

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static CreatedBy From(long value)` |

**処理フロー**

```
入力値が 0 より大きいか確認
  ↓
Yes → 検証処理へ
No  → ArgumentException throw

検証処理：値の有効性チェック
  → 0 以下の値は不可
  ↓
検証成功 → new CreatedBy(value) でインスタンス返却
検証失敗 → ArgumentException throw
```

**設計判断**

- null 入力は呼び出し元で処理（From の責務外）
- Domain層での生成はこのメソッドを使用
- 検証失敗時は例外で即座に失敗

---

### 3.2 `TryFrom(long? input, out CreatedBy result)` — 型安全版

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static bool TryFrom(long? input, out CreatedBy result)` |

**処理フロー**

```
入力値が null か？
  ↓
Yes → result = null!, return false（null は失敗）
No  → From() 呼び出し

From() が成功
  ↓
result = インスタンス, return true

From() が例外
  ↓
result = null!, return false
```

**設計判断**

- null 入力は失敗を返す（CreatedBy は必須値）
- Application層の入力検証で使用
- 例外を投げない（try-catch 不要）

---

### 3.3 `TryFromDbValue(long? input, out CreatedBy result)` — DB値変換版

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static bool TryFromDbValue(long? input, out CreatedBy result)` |

**処理フロー**

```
入力値が null か？
  ↓
Yes → result = null!, return false（DB NULL は失敗）
No  → From() 呼び出し

From() が成功
  ↓
result = インスタンス, return true

例外発生
  ↓
result = null!, return false
```

**設計判断**

- DB から読み込んだ BIGINT をこのメソッドで CreatedBy に変換
- Mapper で呼び出される（Repository パターン）
- 例外を投げない

---

### 3.4 `Equals(object? obj)` と `Equals(CreatedBy? other)` — 等価性判定

| 項目 | 内容 |
|------|------|
| シグネチャ | `public override bool Equals(object? obj)`<br/>`public bool Equals(CreatedBy? other)` |

**処理フロー**

```
obj が null か？
  ↓
Yes → false 返却

obj が CreatedBy にキャスト可能か？
  ↓
Yes → Equals(CreatedBy?) 呼び出し
No  → false 返却

Equals(CreatedBy?)：
  other が null か？
    ↓
  Yes → false
  No  → this.Value == other.Value か？
    ↓
  Yes → true
  No  → false
```

**設計判断**

- IEquatable<CreatedBy> を実装（型安全）
- Value の equality で比較
- 同一インスタンスは ReferenceEquals で最適化

---

### 3.5 `GetHashCode()` — ハッシュコード

| 項目 | 内容 |
|------|------|
| シグネチャ | `public override int GetHashCode()` |

**処理フロー**

```
Value.GetHashCode() を返却
```

**設計判断**

- Equals=true のペアは同一ハッシュ値を返す
- HashSet/Dictionary での使用に対応

---

### 3.6 `ToString()` — 文字列化

| 項目 | 内容 |
|------|------|
| シグネチャ | `public override string ToString()` |

**処理フロー**

```
Value.ToString() を返却
```

**設計判断**

- long の既定フォーマット（10進数）で出力
- ログやデバッグ用

---

### 3.7 `GetValueComponents()` — ValueObject基底メソッド

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected override IEnumerable<object?> GetValueComponents()` |

**処理フロー**

```
yield return Value;
```

**設計判断**

- PrimitiveValueObject の Equals/GetHashCode で自動使用
- Value のみが等価性判定に影響

---

### 3.8 `Validate(long normalized)` — 値検証（PrimitiveValueObject内）

| 項目 | 内容 |
|------|------|
| シグネチャ | `public override void Validate(long normalized)` |

**処理フロー**

```
値が 0 より大きいか確認
  ↓
Yes → 検証成功（return）
No  → ArgumentException throw
```

**設計判断**

- コンストラクタで自動実行
- 負の数・0 は不可

---

## 4. コンストラクタ

### 4.1 private コンストラクタ

```csharp
private CreatedBy(long value) : base(value, true)
{
}
```

**設計意図:**
- ファクトリメソッド（From, TryFrom）のみで生成を許可
- 不正な値の混入を防止
- base(value, true) で PrimitiveValueObject に値と IsSet=true を渡す

---

## 5. レイヤ制約確認

| 確認項目 | 判定 | 備考 |
|----------|------|------|
| Application 層への依存 | ❌ 禁止 | DTO 介してアクセス |
| Infrastructure 層への依存 | ❌ 禁止 | 一方向（Mapper から TryFromDbValue を呼び出し） |
| ロガー依存 | ❌ 禁止 | ログは呼び出し元で |
| DB アクセス | ❌ 禁止 | 値の検証のみ |

---

## 6. 実装上の注意点

| 項目 | 説明 |
|------|------|
| **値の範囲** | 従業員行IDは 1 以上（0 と負数は不可） |
| **null 処理** | CreatedBy は必須値（null は失敗を返す） |
| **DB マッピング** | BIGINT (SQL Server) ← → long (C#) |
| **演算子オーバーロード** | == と != は基底クラスで実装（Equals 経由） |

---

## 7. 改版履歴

| 版 | 日付 | 内容 |
|----|------|------|
| 1.0 | 2026-08-10 | 初版作成。CreatedBy の詳細設計 |
