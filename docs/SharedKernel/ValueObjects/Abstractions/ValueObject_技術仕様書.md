# 技術仕様書 — ValueObject

**プロジェクト:** SupportAdvance  
**レイヤ:** Domain 層  
**種別:** 基底抽象クラス設計原則  
**版:** 1.0 / 2026-07-03

---

## 1. 位置づけ

`ValueObject` は SupportAdvance プロジェクトにおいて、Domain 層に属するすべての値オブジェクトの基底抽象クラスである。

本クラスの目的は以下の 2 点に集約される。

- 未設定状態（Unset）の型安全な表現
- 等価性比較のルールを統一し、参照等価ではなく値等価に基づいた比較を実現すること

> **📌 原則**  
> Domain 層は `null` を一切許容しない。未設定状態は `IsSet = false` によって型で表現する。

---

## 2. メンバー仕様

### 2.1 `IsSet` プロパティ

未設定状態を示す `bool` フラグ。派生クラスはコンストラクタ内で明示的に設定しなければならない。

| 項目 | 内容 |
|------|------|
| シグネチャ | `public bool IsSet { get; protected init; }` |
| 戻り値 | `true` = 値が設定済み、`false` = 未設定（Unset） |

**設計判断**

- `protected init` により、派生クラスのコンストラクタ内での設定のみ許可
- 構築後の書き換え禁止

---

### 2.2 `GetEqualityComponents()` 抽象メソッド

派生クラスが等価性比較に使用するコンポーネントを列挙する抽象メソッド。各派生クラスは必ずオーバーライドする。

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected abstract IEnumerable<object?> GetEqualityComponents()` |
| 戻り値 | `IEnumerable<object?>` — 比較対象のコンポーネント列挙 |

**設計判断**

- `IsSet` を含めない。基底クラスが `ValueObjectComponentNormalizer` 経由で先頭に付加するため
- 派生クラスは自身の値フィールドのみを列挙する

---

### 2.3 `Equals(object? obj)` メソッド — オーバーライド

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

### 2.4 `Equals(ValueObject? other)` メソッド — IEquatable 実装

本クラスは `IEquatable<ValueObject>` を実装する。

| 項目 | 内容 |
|------|------|
| シグネチャ | `public bool Equals(ValueObject? other)` |

**処理フロー（等価性比較）**

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

**設計判断**

- `IEquatable<ValueObject>` を実装することで、ジェネリックコレクション（`List<T>` 等）における等価性比較がボックス化を避けられる

---

### 2.5 `GetHashCode()` メソッド — オーバーライド

正規化されたコンポーネントに対して 31 進法ハッシュを計算する。

```
hash = components.Aggregate(17, (current, obj) => current * 31 + (obj?.GetHashCode() ?? 0))
```

**設計判断**

- `Equals` が `true` ならば必ず同じハッシュ値を返す

---

### 2.6 `==` / `!=` 演算子オーバーロード

`null` 安全な等価性比較を提供する。

| 演算子 | 振る舞い |
|--------|---------|
| `==` | 両者が `null` → `true`；片方が `null` → `false`；それ以外は `left.Equals(right)` |
| `!=` | `==` の否定 |

---

### 2.7 `ToString()` メソッド — オーバーライド

人が読めるデバッグ用文字列を返す。

| 条件 | 戻り値 |
|------|--------|
| `IsSet == false` | `"Unset"` を返す |
| `IsSet == true` | 正規化コンポーネント（`IsSet` を除く）をカンマ区切りで連結した文字列 |

---

## 3. 設計制約・禁止事項

### 3.1 Domain 層制約

`ValueObject` は Domain 層に属するため、以下を知ってはならない。

| 禁止（Domain 層が知ってはならないもの） | 許可（Domain 層が知ってよいもの） |
|---------------------------------------|----------------------------------|
| ❌ ログ出力実装 | ✅ 純粋な値計算 |
| ❌ DB / ORM | ✅ 型による状態表現（Unset / IsSet） |
| ❌ フレームワーク型（`ILogger` 等） | ✅ 値の比較・ハッシュ計算 |
| ❌ 時刻取得（`DateTime.Now` / `UtcNow`） | ✅ `IClock` インターフェース（DI 注入） |

### 3.2 null 禁止原則

Domain 層は `null` を一切許容しない。

- コンストラクタで `IsSet` を明示的に設定すること
- `ValueObject` を返す API が「値なし」を意味する場合、`null` を返さず Unset インスタンスを返す
- `GetEqualityComponents()` から `null` 要素を返すことは許容する

### 3.3 依存関係ルール遵守

本クラスが依存できる方向は Domain 層の内部のみである。Application / Infrastructure への依存は禁止。

---

## 4. 実装ガイドライン

### 4.1 派生クラスの実装義務

- `GetEqualityComponents()` を必ずオーバーライドする
- コンストラクタで `IsSet` を設定する（未設定の場合は `false` が保持される）
- Unset を表す静的ファクトリまたは静的プロパティを提供する

### 4.2 最小限の実装例

```csharp
// IsSet = true の派生例
protected override IEnumerable<object?> GetEqualityComponents()
{
    yield return Value;  // IsSet は基底クラスが付加するため不要
}
```

---

## 5. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-07-03 | 加藤 正人 | 初版作成 |
