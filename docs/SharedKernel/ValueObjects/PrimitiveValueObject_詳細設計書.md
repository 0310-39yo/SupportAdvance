# 詳細設計書 — PrimitiveValueObject

**プロジェクト:** SupportAdvance  
**レイヤ:** Domain 層  
**種別:** ジェネリック基底抽象クラス詳細設計  
**依拠技術仕様書:** PrimitiveValueObject技術仕様書 v1.0  
**版:** 1.0 / 2026-07-03

---

## 1. クラス概要

### 1.1 クラス定義

| 項目 | 内容 |
|------|------|
| クラス名 | `PrimitiveValueObject<TValue>` |
| 種別 | `abstract class` |
| 継承元 | `ValueObject` |
| 名前空間 | `SupportAdvance.SharedKernel.ValueObjects` |
| 配置レイヤ | Domain 層 |

### 1.2 責務

- スカラ値を保持する値オブジェクトに共通する正規化・検証・等価性・文字列化の基盤を提供する
- 派生クラスは `Normalize` / `Validate` / `Format` のオーバーライドのみで業務固有ロジックを表現する
- `ValueField` の初期化・保護を一元管理し、派生クラスが直接フィールド操作を行わないよう強制する

### 1.3 継承関係

```
ValueObject  ◀──extends──  PrimitiveValueObject<TValue>  ◀──extends──  派生クラス
                                  │
                                  ├─ Normalize(TValue) → TValue
                                  ├─ Validate(TValue)  → void
                                  ├─ Format(TValue)    → string
                                  └─ PrimitiveEqualityComponents() → IEnumerable<object?>
```

### 1.4 型パラメータ

| パラメータ | 制約 | 説明 |
|-----------|------|------|
| `TValue` | なし | 保持する値の型。`string`・数値型・日付型等を想定 |

---

## 2. フィールド設計

### 2.1 `ValueField`

| 項目 | 内容 |
|------|------|
| 型 | `TValue` |
| 修飾子 | `protected readonly` |
| 初期化タイミング | 値設定用コンストラクタ内、`Normalize` 完了後 |
| 未初期化時の値 | `default`（`IsSet = false` のとき） |

**設計判断**

- `protected` とし、派生クラスからの読み取りのみを許可する
- 外部からの取得は `TryGetValue` 経由に限定する
- `readonly` により構築後の書き換えを禁止する

---

## 3. コンストラクタ設計

### 3.1 未設定用コンストラクタ

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected PrimitiveValueObject(bool isSet)` |
| 用途 | Unset インスタンスの構築 |

**処理フロー**

```
base(isSet) を呼び出す（ValueObject の IsSet を設定）
ValueField は未初期化のまま
```

---

### 3.2 値設定用コンストラクタ

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected PrimitiveValueObject(TValue value, bool isSet)` |
| 用途 | 値を保持するインスタンスの構築 |

**処理フロー**

```
1. base(isSet) を呼び出す（ValueObject の IsSet を設定）
2. IsSet == true の場合:
   a. ValueField = Normalize(value)
   b. Validate(ValueField)
3. IsSet == false の場合:
   → ValueField は未初期化のまま
```

**設計判断**

- `Normalize` → `Validate` の順序は不変とする
- `Validate` が例外をスローした場合、インスタンスは構築されない
- `IsSet = false` のケースで `value` 引数は無視される

---

## 4. メソッド設計

### 4.1 `TryGetValue(out TValue value)` — 仮想メソッド

| 項目 | 内容 |
|------|------|
| シグネチャ | `public virtual bool TryGetValue(out TValue value)` |

**処理フロー**

```
IsSet == true  → value = ValueField; return true
IsSet == false → value = default!;   return false
```

**設計判断**

- `virtual` とすることで派生クラスが取得ロジックを拡張可能にする
- `default!` の `!` は null 許容参照型の警告抑制のためのもので、呼び出し元は `false` 返却時に `value` を参照してはならない

---

### 4.2 `Normalize(TValue input)` — 保護仮想メソッド

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected virtual TValue Normalize(TValue input)` |
| デフォルト実装 | `return input` |

**設計判断**

- 副作用を持たない純粋変換処理のみを行う
- 文字列のトリム・大文字小文字変換・フォーマット統一等を想定する
- 例外スローは禁止。入力値の妥当性検証は `Validate` で行う

---

### 4.3 `Validate(TValue normalized)` — 保護仮想メソッド

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected virtual void Validate(TValue normalized)` |
| デフォルト実装 | 何もしない |

**設計判断**

- 正規化済みの値のみを受け取る。正規化前の値への参照は行わない
- 検証失敗時は例外をスローする。例外型は派生クラスの業務ルールに従う
- ログ出力・状態変更を行ってはならない

---

### 4.4 `Format(TValue value)` — 保護仮想メソッド

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected virtual string Format(TValue value)` |
| デフォルト実装 | `return value?.ToString() ?? string.Empty` |

**設計判断**

- `ToString()` から呼び出される表示用の拡張点である
- デバッグ用途に加え、UI 表示ロジックを持たない範囲での表現整形を許容する
- 業務ロジック（判定・変換）をここに持ち込んではならない

---

### 4.5 `PrimitiveEqualityComponents()` — 保護仮想メソッド

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected virtual IEnumerable<object?> PrimitiveEqualityComponents()` |

**処理フロー**

```
yield return IsSet          （常に先頭に返す）
IsSet == true の場合:
    yield return ValueField （続けて返す）
```

**設計判断**

- `IsSet` を先頭に含めて返すことで、`ValueObjectComponentNormalizer` の分岐 B（先頭が `bool` かつ値が `IsSet` と一致）に該当し、二重付加を防ぐ
- `virtual` とすることで派生クラスが複数コンポーネントを持つ場合に拡張可能にする

---

### 4.6 `GetEqualityComponents()` — オーバーライド

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected override IEnumerable<object?> GetEqualityComponents()` |
| 処理 | `return PrimitiveEqualityComponents()` に委譲 |

**設計判断**

- `ValueObject` の抽象メソッドを実装し、等価性比較の基点を `PrimitiveEqualityComponents` に統一する
- 派生クラスはこのメソッドを再オーバーライドせず、`PrimitiveEqualityComponents` をオーバーライドして拡張する

---

### 4.7 `ToString()` — オーバーライド

| 項目 | 内容 |
|------|------|
| シグネチャ | `public override string ToString()` |

**処理フロー**

```
IsSet == true  → Format(ValueField) を返す
IsSet == false → "Unset" を返す
```

**設計判断**

- 基底クラス `ValueObject.ToString()` を再オーバーライドし、`Format` による派生クラス固有の表示を実現する
- `IsSet = false` の場合は基底クラスと同様に `"Unset"` を返し、一貫性を保つ

---

## 5. 派生クラスへの設計指示

### 5.1 実装義務

| 義務 | 内容 |
|------|------|
| Unset 表現の提供 | 静的プロパティまたは静的ファクトリメソッドで `IsSet = false` インスタンスを公開する |
| コンストラクタの保護 | コンストラクタは `protected` または `private` に限定し、静的ファクトリ経由で構築させる |

> **📌 設計判断**  
> `null` 返却禁止の Domain 層制約を確実に満たすため、Unset インスタンスの取得手段を必ず提供することを義務とする。

### 5.2 禁止事項

| 禁止事項 | 理由 |
|----------|------|
| `GetEqualityComponents()` の再オーバーライド | `PrimitiveEqualityComponents()` への委譲構造を破壊するため |
| `Normalize` 内での例外スロー | 例外スローは `Validate` の責務であり、責務が混在するため |
| `IsSet = false` 時の `ValueField` 参照 | 未初期化値への参照となるため |
| `null` を返す API の定義 | Domain 層は null を許容しない |

---

## 6. レイヤ制約確認

| 確認項目 | 判定 | 備考 |
|----------|------|------|
| Application 層への依存 | ❌ 禁止 | |
| Infrastructure 層への依存 | ❌ 禁止 | |
| ロガー（`ILogger` 等）への依存 | ❌ 禁止 | |
| `DateTime.Now` / `UtcNow` の直接使用 | ❌ 禁止 | 時刻が必要な場合は `IClock` 経由 |
| `ValueObject` への依存 | ✅ 許可 | 継承元（同一 Domain 層） |
| `ValueObjectComponentNormalizer` への依存 | ✅ 許可 | `ValueObject` 経由で間接利用、同一 Domain 層 |

---

## 7. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-07-03 | 加藤 正人 | 初版作成 |
