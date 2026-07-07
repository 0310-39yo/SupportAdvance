# 詳細設計書 — ValueObject

**プロジェクト:** SupportAdvance  
**レイヤ:** Domain 層  
**種別:** 基底抽象クラス詳細設計  
**依拠技術仕様書:** ValueObject技術仕様書 v1.0  
**版:** 1.0 / 2026-07-07

---

## 1. クラス概要

### 1.1 クラス定義

| 項目 | 内容 |
|------|------|
| クラス名 | `ValueObject` |
| 種別 | `abstract class` |
| 名前空間 | `SupportAdvance.SharedKernel.ValueObjects` |
| 実装インターフェース | `IEquatable<ValueObject>` |
| 配置レイヤ | Domain 層 |

### 1.2 責務

- すべての値オブジェクト派生クラスに対して、**値等価の比較基盤**を提供する
- **未設定状態（Unset）** を `null` に頼らず型で表現する仕組みを強制する
- 正規化処理を `ValueObjectComponentNormalizer` に委譲し、自身は比較・ハッシュ・文字列化のルールのみを保持する

### 1.3 協調クラス

```
ValueObject  ──uses──▶  ValueObjectComponentNormalizer
                             └─ Normalize(instance, components)

派生クラス  ──extends──▶  ValueObject
                              └─ override GetEqualityComponents()
```

---

## 2. プロパティ設計

### 2.1 `IsSet`

| 項目 | 内容 |
|------|------|
| 型 | `bool` |
| アクセス修飾子 | `public` |
| セッタ | `protected init` |
| デフォルト値 | `false` |

**設計判断**

- 派生クラスのコンストラクタ内で必ず明示的に設定する
- 設定を省略した場合は `false`（Unset）として扱われる
- `init` に限定することで、構築後の書き換えを禁止する

---

## 3. メソッド設計

### 3.1 `GetEqualityComponents()` — 抽象メソッド

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected abstract IEnumerable<object?> GetEqualityComponents()` |
| 実装義務 | 全派生クラスで必須オーバーライド |

**設計判断**

- `IsSet` は返さない。基底クラスが `ValueObjectComponentNormalizer` 経由で先頭に付加する
- 派生クラスは自身の値フィールドのみを列挙する

---

### 3.2 `Equals(object? obj)` — オーバーライド

| 項目 | 内容 |
|------|------|
| シグネチャ | `public override bool Equals(object? obj)` |

**処理フロー**

```
obj が null               → false を返す
obj が ValueObject でない → false を返す
                          → Equals(ValueObject?) に委譲
```

---

### 3.3 `Equals(ValueObject? other)` — IEquatable 実装

| 項目 | 内容 |
|------|------|
| シグネチャ | `public bool Equals(ValueObject? other)` |

**処理フロー**

```
1. other == null                            → false
2. ReferenceEquals(this, other)             → true
3. this.GetType() != other.GetType()        → false
4. 双方の GetEqualityComponents() を Normalizer で正規化
5. 正規化済みリストを順次 Equals 比較
   - 要素数が異なる                         → false
   - いずれかの要素が不一致                 → false
6. すべて一致                               → true
```

---

### 3.4 `GetHashCode()` — オーバーライド

| 項目 | 内容 |
|------|------|
| シグネチャ | `public override int GetHashCode()` |

**算出ルール**

```
初期値 = 17
各コンポーネント c に対して:
    hash = hash * 31 + (c?.GetHashCode() ?? 0)
```

**設計判断**

- `Equals` が `true` を返す 2 オブジェクトは必ず同じハッシュ値を返す
- 正規化済みコンポーネントに対して計算するため、`IsSet` は必ずハッシュに含まれる

---

### 3.5 `==` / `!=` 演算子

| 項目 | 内容 |
|------|------|
| シグネチャ | `public static bool operator ==(ValueObject? left, ValueObject? right)` |
| シグネチャ | `public static bool operator !=(ValueObject? left, ValueObject? right)` |

**処理フロー（`==`）**

```
left が null かつ right が null → true
left が null または right が null → false
それ以外 → left.Equals(right)
```

**`!=`** は `==` の否定とする。

---

### 3.6 `ToString()` — オーバーライド

| 項目 | 内容 |
|------|------|
| シグネチャ | `public override string ToString()` |

**処理フロー**

```
IsSet == false → "Unset" を返す
IsSet == true  → 正規化コンポーネントから IsSet を除いた要素を
                 string.Join(", ", ...) で連結して返す
```

**設計判断**

- デバッグ・ログ出力での可読性確保が目的であり、業務ロジックで依存してはならない

---

## 4. 派生クラスへの設計指示

### 4.1 実装義務

| 義務 | 内容 |
|------|------|
| `GetEqualityComponents()` のオーバーライド | 全派生クラスで必須 |
| コンストラクタでの `IsSet` 設定 | 省略禁止 |
| Unset 表現の提供 | 静的プロパティまたは静的ファクトリメソッドで `IsSet = false` インスタンスを公開する |

> **📌 設計判断**  
> `null` 返却を禁止する Domain 層の制約を確実に満たすため、Unset インスタンスの取得手段を派生クラスが必ず提供しなければならない。

### 4.2 禁止事項

| 禁止事項 | 理由 |
|----------|------|
| `null` を返す API の定義 | Domain 層は null を許容しない |
| `IsSet` を `GetEqualityComponents()` に含める | 二重付加となり比較が壊れる |
| 構築後の状態書き換え | 値オブジェクトは不変であること |

---

## 5. レイヤ制約確認

| 確認項目 | 判定 | 備考 |
|----------|------|------|
| Application 層への依存 | ❌ 禁止 | |
| Infrastructure 層への依存 | ❌ 禁止 | |
| ロガー（`ILogger` 等）への依存 | ❌ 禁止 | |
| `DateTime.Now` / `UtcNow` の直接使用 | ❌ 禁止 | 時刻が必要な場合は `IClock` 経由 |
| `ValueObjectComponentNormalizer` への依存 | ✅ 許可 | 同一 Domain 層内部 |

---

## 6. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-07-03 | 加藤 正人 | 初版作成 |
