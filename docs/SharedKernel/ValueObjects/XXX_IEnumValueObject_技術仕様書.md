# 技術仕様書 — IEnumValueObject

**プロジェクト:** SupportAdvance  
**レイヤ:** Domain 層  
**種別:** インターフェース設計原則  
**版:** 1.0 / 2026-07-03

---

## 1. 位置づけ

`IEnumValueObject<TSelf, TValue>` は、**列挙型（Enum Like）をドメイン制約付き ValueObject として表現** するための設計規約である。

本インターフェースの目的は以下に集約される。

- Enum 値に対してドメイン検証（有効性チェック）を組み込むこと
- 未設定状態（Unset）を型で安全に表現すること
- 外部入力（JSON / API / DB）の値検証を `TryFrom` メソッドで行うこと

> **📌 原則**  
> プロジェクトは .NET 10.0 を使用し、C# 11 以降の `static abstract` メンバーをサポートします。
> 静的メソッド（`Unset`, `From`, `TryFrom`）は **インターフェースで定義され、実装クラスで必ず実装** される契約です。

---

## 2. メンバー仕様

### 2.1 `TryGetValue` メソッド（例外なし）

値を安全に取得するインスタンスメソッド。

| 項目 | 内容 |
|------|------|
| シグネチャ | `bool TryGetValue(out TValue value)` |
| 戻り値 | 成功時 `true`、未設定時 `false` |
| 例外 | **例外を投げない** |
| パラメータ | `out TValue value`: 取得した値（失敗時は default） |

**設計判断**

- 値が設定されている場合（`IsSet == true`）のみ `true` を返す
- 例外を投げないため、C# の `TryPattern` に準拠

### 2.2 `GetValue` メソッド（例外あり）

値を取得し、未設定の場合は例外を throw するインスタンスメソッド。

| 項目 | 内容 |
|------|------|
| シグネチャ | `TValue GetValue()` |
| 戻り値 | 設定されている場合の値 |
| 失敗時 | `ArgumentException` を throw |
| 用途 | 値が必ず存在することが分かっている場合 |

**設計判断**

- 値が未設定の場合、呼び出し側の不具合を検出するため `ArgumentException` を throw する
- パフォーマンスが重要な場合は `TryGetValue` の使用を推奨

---

## 3. 静的メソッド（インターフェース定義 / C# 11 以降）

`IEnumValueObject<TSelf, TValue>` を実装する場合、以下の **3 つの静的メソッドをインターフェースで定義し、実装クラスで必ず実装** する。

### 3.1 `Unset` 静的メソッド

未設定インスタンスを返す。

| 項目 | 内容 |
|------|------|
| シグネチャ | `static abstract TSelf Unset()` |
| 戻り値 | `IsSet = false` なインスタンス |
| 目的 | Null Object パターンの実装 |

**設計判断**

- Domain 層は `null` を許容しない
- 値がない状態を表現するため、Unset インスタンスを返す
- `default` キーワードで実装可（enum の default は 0）

### 3.2 `From` 静的メソッド

値から enum インスタンスを生成し、値を検証する。

| 項目 | 内容 |
|------|------|
| シグネチャ | `static abstract TSelf From(TValue value)` |
| 戻り値 | 生成されたインスタンス |
| 失敗時 | `ArgumentOutOfRangeException` を throw |
| 用途 | 信頼できる値源（DB など）からの生成 |

**設計判断**

- **値を検証する**（例：enum に存在しない値は例外）
- null を受け入れない（`null` の場合は例外）
- 検証失敗時は `ArgumentOutOfRangeException` を throw

### 3.3 `TryFrom` 静的メソッド

値から enum インスタンスを安全に生成する。

| 項目 | 内容 |
|------|------|
| シグネチャ | `static abstract bool TryFrom(TValue? input, out TSelf result)` |
| 戻り値 | 成功時 `true`、失敗時 `false` |
| 例外 | **例外を投げない** |
| 用途 | 外部入力（API / JSON）の値変換 |

**設計判断**

- **null 入力を正常値として扱う**（`null` → `Unset()` に変換）
- 無効な値は `false` を返し、`result` に `Unset()` を設定
- 例外を投げないため、C# の `TryPattern` に準拠

---

## 4. Domain 層制約

| ❌ 禁止（Domain が知ってはならないもの） | ✅ 許可（Domain が知ってよいもの） |
|---------------------------------------|----------------------------------|
| ❌ `null` 値返却 | ✅ `TryGetValue()` / `GetValue()` |
| ❌ ログ・DB アクセス | ✅ `Unset()` インスタンス |
| | ✅ enum 値の大小比較 |

---

## 5. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-07-03 | 加藤 正人 | 初版作成 |
