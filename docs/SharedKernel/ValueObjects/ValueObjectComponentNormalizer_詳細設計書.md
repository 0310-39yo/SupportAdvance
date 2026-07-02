# 詳細設計書 — ValueObjectComponentNormalizer

**プロジェクト:** SupportAdvance  
**レイヤ:** Domain 層（内部ユーティリティ）  
**種別:** 等価性正規化ユーティリティ詳細設計  
**依拠技術仕様書:** ValueObjectComponentNormalizer技術仕様書 v1.0  
**版:** 1.0 / 2026-07-03

---

## 1. クラス概要

### 1.1 クラス定義

| 項目 | 内容 |
|------|------|
| クラス名 | `ValueObjectComponentNormalizer` |
| 種別 | `internal static class` |
| 名前空間 | `SupportAdvance.SharedKernel.ValueObjects` |
| 配置レイヤ | Domain 層 |

### 1.2 責務

- `ValueObject.GetEqualityComponents()` の戻り値に `IsSet` を**重複なく先頭へ付加**し、正規化された列挙を返す
- 本クラス自身は状態を持たず、副作用のない純粋な変換処理のみを行う

### 1.3 公開範囲

`internal` とし、`ValueObject` クラスからのみ呼び出される。Application / Infrastructure 層からの直接参照は禁止する。

### 1.4 呼び出し元と目的

| 呼び出し元（ValueObject 内） | 目的 |
|-----------------------------|------|
| `Equals(ValueObject? other)` | 両オブジェクトのコンポーネントを正規化し要素比較を行う |
| `GetHashCode()` | 正規化済みコンポーネントで 31 進法ハッシュを計算する |
| `ToString()` | `IsSet` を除いたコンポーネントをカンマ区切り文字列に整形する |

---

## 2. メソッド設計

### 2.1 `Normalize` — 唯一のメソッド

| 項目 | 内容 |
|------|------|
| シグネチャ | `internal static IEnumerable<object?> Normalize(ValueObject instance, IEnumerable<object?>? components)` |
| 戻り値 | `IsSet` を先頭に含む正規化済みコンポーネント列挙 |

---

## 3. 分岐設計

### 3.1 分岐判定の優先順位

```
Normalize(instance, components)
    │
    ├─ [分岐 A] components が null または空
    │       → { instance.IsSet } のみ返す
    │
    ├─ [分岐 B] components の先頭が bool かつ 値 == instance.IsSet
    │       → components をそのまま列挙（重複付加しない）
    │
    └─ [分岐 C] 上記以外（通常ケース）
            → { instance.IsSet } + components を連結して列挙
```

### 3.2 各分岐の詳細

#### 分岐 A：`components` が `null` または空

| 項目 | 内容 |
|------|------|
| 判定条件 | `components == null` または `!components.Any()` |
| 戻り値 | `yield return instance.IsSet` のみ |
| 想定ケース | 派生クラスが `GetEqualityComponents()` で何も返さない場合 |

#### 分岐 B：先頭が `bool` かつ `IsSet` と一致

| 項目 | 内容 |
|------|------|
| 判定条件 | 先頭要素が `bool` 型 かつ その値 `== instance.IsSet` |
| 戻り値 | `components` をそのまま `yield return` |
| 想定ケース | 派生クラスが意図的に `IsSet` を先頭に含めている場合 |

**設計判断**

値が一致する場合のみ「既に付加済み」とみなす。値が異なる `bool` が先頭にある場合は分岐 C として処理し、`IsSet` を正しく先頭に付加する。

#### 分岐 C：それ以外（通常ケース）

| 項目 | 内容 |
|------|------|
| 判定条件 | 分岐 A・B のいずれにも該当しない |
| 戻り値 | `instance.IsSet` を先頭に `yield return` し、続けて `components` を列挙 |
| 想定ケース | 派生クラスが値フィールドのみを `GetEqualityComponents()` で返す（最も一般的） |

### 3.3 分岐まとめ

| 分岐 | 判定条件 | `IsSet` 付加 | `components` 展開 |
|------|---------|-------------|------------------|
| A | `null` または空 | 付加する | なし |
| B | 先頭 = `bool` かつ値 `== IsSet` | 付加しない（既存を使用） | そのまま展開 |
| C | 上記以外 | 先頭に付加する | 後続に展開 |

---

## 4. 制約・禁止事項

### 4.1 副作用禁止

| 禁止事項 | 理由 |
|----------|------|
| ログ出力 | 副作用を持たない純粋変換処理であること |
| `instance` の状態書き換え | 値オブジェクトの不変性を破壊するため |
| DB・ファイル・ネットワークアクセス | Domain 層は技術実装を知らない |

### 4.2 依存関係

| 依存先 | 可否 | 備考 |
|--------|------|------|
| `ValueObject` | ✅ 許可 | 同一 Domain 層内部 |
| Application 層 | ❌ 禁止 | 越境禁止 |
| Infrastructure 層 | ❌ 禁止 | 越境禁止 |
| `ILogger` 等フレームワーク型 | ❌ 禁止 | Domain は技術非依存 |

### 4.3 null 安全性

- `components` 引数が `null` の場合、例外をスローしない（分岐 A として正常処理する）
- `components` 内の個々の要素が `null` であることは許容する（ハッシュ計算時は `0` として扱う）

---

## 5. レイヤ制約確認

| 確認項目 | 判定 | 備考 |
|----------|------|------|
| `internal` スコープの維持 | ✅ 必須 | Domain 外からの参照を遮断する |
| Application 層からの直接参照 | ❌ 禁止 | |
| Infrastructure 層からの直接参照 | ❌ 禁止 | |
| `DateTime.Now` 等の時刻直接取得 | ❌ 禁止 | 本クラスでは時刻を扱わない |

---

## 6. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-07-03 | 加藤 正人 | 初版作成 |
