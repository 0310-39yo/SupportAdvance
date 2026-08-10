# UpdatedBy 詳細設計書

**バージョン:** 1.0  
**作成日:** 2026年08月10日  
**依拠技術仕様書:** UpdatedBy技術仕様書 v1.0

---

## 1. クラス概要

### 1.1 クラス定義

| 項目 | 内容 |
|------|------|
| クラス名 | `UpdatedBy` |
| 種別 | `sealed class` |
| 名前空間 | `SupportAdvance.SharedKernel.ValueObjects.Audit` |
| 実装インターフェース | `IEquatable<UpdatedBy>` |
| 継承元 | `PrimitiveValueObject<long?>` |
| 配置レイヤ | SharedKernel（Domain層ベース） |

### 1.2 責務

- **値の保持** — 更新者の従業員行ID（long?）を不変で保持
- **IsSet状態管理** — 更新済み/未更新を IsSet フラグで管理
- **検証** — 設定時は 0 より大きい値のみ
- **null吸収** — TryFrom/TryFromDbValue で null を Unset に変換
- **等価性判定** — Value と IsSet が同一の2つのインスタンスは等価

### 1.3 協調クラス

```
UpdatedBy  ──uses──▶  PrimitiveValueObject<long?>
                           └─ GetValueComponents()

Mapper  ──uses──▶  UpdatedBy
                       └─ TryFromDbValue()（DB値変換）

Entity  ──has──▶  UpdatedBy
                      └─ 監査情報として保持
```

---

## 2. プロパティ設計

### 2.1 `Value` プロパティ

| 項目 | 内容 |
|------|------|
| 型 | `long?` |
| アクセス修飾子 | `public` |
| セッタ | なし（読み取り専用） |
| デフォルト値 | なし（コンストラクタで指定） |

**設計判断**

- IsSet=true のとき long の値、IsSet=false のとき null を返す
- Mapper での DB 変換時、Value で内部値を取得

### 2.2 `IsSet` プロパティ

| 項目 | 内容 |
|------|------|
| 型 | `bool` |
| アクセス修飾子 | `public` |
| セッタ | なし |
| 値 | true（更新済み）または false（未更新） |

**設計判断**

- オプション ValueObject なため IsSet フラグで状態を管理
- IOptionalValueObject パターン

### 2.3 `HasUpdated` プロパティ

| 項目 | 内容 |
|------|------|
| 型 | `bool` |
| アクセス修飾子 | `public` |
| セッタ | なし |
| 実装 | IsSet の別名 |

**設計判断**

- Domain層での可読性向上（`if (entity.UpdatedBy.HasUpdated)` の方が読みやすい）

---

## 3. メソッド設計

### 3.1 `From(long value)` — ファクトリメソッド

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static UpdatedBy From(long value)` |

**処理フロー**

```
入力値が 0 より大きいか確認
  ↓
Yes → 検証処理へ
No  → ArgumentException throw

検証処理：値の有効性チェック
  ↓
検証成功 → new UpdatedBy(value, isSet: true) でインスタンス返却
検証失敗 → ArgumentException throw
```

---

### 3.2 `Unset()` — 未設定インスタンス生成

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static UpdatedBy Unset()` |

**処理フロー**

```
new UpdatedBy(null, isSet: false) を返却
```

**設計判断**

- Value は null、IsSet は false で未更新状態を表現

---

### 3.3 `TryFrom(long? input, out UpdatedBy result)` — 型安全版

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static bool TryFrom(long? input, out UpdatedBy result)` |

**処理フロー**

```
入力値が null か？
  ↓
Yes → result = Unset(), return true（null は Unset に変換）
No  → From() 呼び出し

From() が成功
  ↓
result = インスタンス, return true

From() が例外
  ↓
result = Unset(), return false
```

**設計判断**

- null は失敗ではなく Unset に変換して成功を返す（重要）
- Application層の入力検証で使用

---

### 3.4 `TryFromDbValue(long? input, out UpdatedBy result)` — DB値変換版

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static bool TryFromDbValue(long? input, out UpdatedBy result)` |

**処理フロー**

```
入力値が null か？
  ↓
Yes → result = Unset(), return true（DB NULL → Unset に変換）
No  → From() 呼び出し

From() が成功
  ↓
result = インスタンス, return true

例外発生
  ↓
result = Unset(), return false
```

**設計判断**

- DB NULL を自動的に Unset に変換する（重要）
- Mapper で呼び出される

---

### 3.5 `Equals/GetHashCode` — 等価性判定

**処理フロー**

```
IsSet が異なる場合 → false
IsSet が同じで Value が異なる場合 → false
IsSet が同じで Value が同じ場合 → true
```

**設計判断**

- Value だけでなく IsSet も等価性判定に含める
- `UpdatedBy.From(1002)` と `UpdatedBy.Unset()` は異なる

---

## 4. コンストラクタ

### 4.1 private コンストラクタ

```csharp
private UpdatedBy(long? value, bool isSet) : base(value, isSet)
{
}
```

**設計意図:**
- ファクトリメソッドのみで生成を許可
- isSet フラグで未更新状態を管理

---

## 5. レイヤ制約確認

| 確認項目 | 判定 | 備考 |
|----------|------|------|
| Application 層への依存 | ❌ 禁止 | DTO 介してアクセス |
| Infrastructure 層への依存 | ❌ 禁止 | 一方向（Mapper から TryFromDbValue を呼び出し） |
| ロガー依存 | ❌ 禁止 | ログは呼び出し元で |
| DB アクセス | ❌ 禁止 | 値の検証のみ |

---

## 6. 改版履歴

| 版 | 日付 | 内容 |
|----|------|------|
| 1.0 | 2026-08-10 | 初版作成。UpdatedBy の詳細設計 |
