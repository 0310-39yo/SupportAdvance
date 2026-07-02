# 技術仕様書 — IOptionalValueObject

**プロジェクト:** SupportAdvance  
**レイヤ:** Domain 層  
**種別:** インターフェース設計原則  
**版:** 1.0 / 2026-07-03

---

## 1. 位置づけ

`IOptionalValueObject<TSelf, TValue>` は、**null 許容の外部入力を型で安全に吸収する ValueObject** の設計規約である。

本インターフェースの目的は以下に集約される。

- API / JSON / DB などの外部入力で `null` が返される場合、Domain 層に入る前に型で吸収すること
- Domain 層では `null` を一切許容しない原則を維持すること
- 未設定状態を `Unset` インスタンスで安全に表現すること

> **📌 原則**  
> C# 11 以上で `static abstract` メンバーが使用可能なため、静的メソッド（`Unset`, `From`, `TryFrom`）を **インターフェースで定義できる**。

---

## 2. メンバー仕様

### 2.1 `TryGetValue` インスタンスメソッド

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

### 2.2 `Unset` 静的メソッド（static abstract）

未設定インスタンスを返す。

| 項目 | 内容 |
|------|------|
| シグネチャ | `static abstract TSelf Unset()` |
| 戻り値 | `IsSet = false` なインスタンス |
| 目的 | Null Object パターンの実装 |

**設計判断**

- Domain 層は `null` を許容しない
- 値がない状態を表現するため、Unset インスタンスを返す
- C# 11 以上で `static abstract` に対応

### 2.3 `From` 静的メソッド（static abstract）

値から ValueObject インスタンスを生成し、値を検証する。

| 項目 | 内容 |
|------|------|
| シグネチャ | `static abstract TSelf From(TValue value)` |
| 戻り値 | 生成されたインスタンス |
| 失敗時 | `ArgumentException` / `ArgumentOutOfRangeException` / `FormatException` を throw |
| 用途 | 信頼できる値源（DB など）からの生成 |

**設計判断**

- **値を検証する**（例：範囲チェック、フォーマットチェック）
- null を受け入れない（`null` の場合は例外）
- 検証失敗時は `ArgumentException` またはその派生を throw
- **呼び出し元の責務**：DB の NULL を変換する場合は `From()` に null を渡さず、呼び出し元（Infrastructure 層）が null チェックを行い `Unset()` を直接使用する

### 2.4 `TryFrom` 静的メソッド（static abstract）

値から ValueObject インスタンスを安全に生成する。

| 項目 | 内容 |
|------|------|
| シグネチャ | `static abstract bool TryFrom(TValue? input, out TSelf result)` |
| 戻り値 | 成功時 `true`、失敗時 `false` |
| 例外 | **例外を投げない** |
| 用途 | 外部入力（API / JSON）の値変換 |

**設計判断**

- **null 入力を正常値として扱う**（`null` → `Unset()` に変換）
- null 以外の値は `From()` に委譲し、バリデーション失敗時は `false` を返す
- 例外を投げないため、C# の `TryPattern` に準拠
- **用途は外部入力（UI / API / JSON）専用**；DB からの値変換には `From()` を使用する

---

## 3. null 処理の原則

### 3.1 From() vs TryFrom()

| メソッド | 用途 | null 入力 | 処理 | 戻り値 |
|--------|------|---------|------|--------|
| `From(value)` | DB など信頼できるソース | null | 例外 throw | - |
| `TryFrom(value?)` | UI / API / JSON など外部入力 | null | Unset に変換 | true |

**設計判断**

- `From()` は信頼できる値源向け：null は不正値として拒否。**DB の NULL は呼び出し元（Infrastructure 層）が `Unset()` を直接使用する**
- `TryFrom()` は外部入力専用：null は有効な「未設定」として吸収

### 3.2 Domain 層での未設定判定

Domain 層では `null` チェック禁止。必ず `TryGetValue()` / `IsSet` で判定する。

---

## 4. Domain 層制約

| ❌ 禁止（Domain が知ってはならないもの） | ✅ 許可（Domain が知ってよいもの） |
|---------------------------------------|----------------------------------|
| ❌ `null` チェック（`== null` など） | ✅ `TryGetValue()` / `IsSet` での判定 |
| ❌ `null` 値返却 | ✅ `Unset()` インスタンス返却 |
| ❌ ログ・DB アクセス | ✅ 値の妥当性検証 |

---

## 5. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-07-03 | 加藤 正人 | 初版作成 |
