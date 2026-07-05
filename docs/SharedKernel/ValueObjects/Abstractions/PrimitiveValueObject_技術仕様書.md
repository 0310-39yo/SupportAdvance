# 技術仕様書 — PrimitiveValueObject

**プロジェクト:** SupportAdvance  
**レイヤ:** Domain 層  
**種別:** ジェネリック基底抽象クラス設計原則  
**依拠技術仕様書:** ValueObject技術仕様書 v1.0  
**版:** 1.0 / 2026-07-03

---

## 1. 位置づけ

`PrimitiveValueObject<TValue>` は、`ValueObject` を継承するジェネリック抽象クラスである。

スカラ値（文字列・数値・日付等）を保持する値オブジェクトに共通する以下の責務を集約し、派生クラスの実装を単純化する。

- 単一の型付き値（`TValue`）の保持と正規化・検証の強制
- 未設定状態（Unset）の一元管理
- 表示用フォーマットの拡張点の提供
- 等価性比較コンポーネントの標準実装

> **📌 原則**  
> Domain 層は `null` を一切許容しない。未設定状態は `IsSet = false` によって型で表現する。

---

## 2. メンバー仕様

### 2.1 `ValueField` フィールド

正規化済みの内部値を格納する `readonly` フィールド。

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected readonly TValue ValueField` |
| 説明 | `IsSet = true` 時に `Normalize()` 済みの値を保持。`IsSet = false` 時は未初期化（`default`）のまま |

> **📌 注意**  
> `IsSet = false` の場合、`ValueField` は未初期化である。`ValueField` への直接アクセスは `IsSet` を確認した上で行うこと。外部からのアクセスは `TryGetValue` 経由に限定する。

---

### 2.2 `TryGetValue(out TValue value)` 仮想メソッド

`IsSet` の状態に応じて内部値を取得する。

| 項目 | 内容 |
|------|------|
| シグネチャ | `public virtual bool TryGetValue(out TValue value)` |
| 戻り値 | 成功時 `true`、未設定時 `false` |

| `IsSet` | `value` の設定 | 戻り値 |
|---------|--------------|--------|
| `true` | `ValueField` を代入 | `true` |
| `false` | `default!` を代入 | `false` |

---

### 2.3 `Normalize(TValue input)` 保護仮想メソッド

入力値の素朴な正規化を行う。派生クラスでオーバーライドして使用する。

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected virtual TValue Normalize(TValue input)` |
| 戻り値 | `TValue` — 正規化後の値 |
| デフォルト実装 | 入力値をそのまま返す（処理なし） |

> **📌 原則**  
> 本メソッドは副作用を持たない純粋変換処理でなければならない。例外のスローは `Validate` の責務であり、`Normalize` では行わない。

---

### 2.4 `Validate(TValue normalized)` 保護仮想メソッド

正規化済みの値に対して業務ルールを検証する。派生クラスでオーバーライドして使用する。

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected virtual void Validate(TValue normalized)` |
| 戻り値 | なし（`void`） |
| デフォルト実装 | 何もしない（処理なし） |

> **📌 原則**  
> 検証に失敗した場合、派生クラスは例外をスローする。例外の種別は派生クラスの業務ルールに従う。`Validate` はログ出力・状態変更を行ってはならない。

---

### 2.5 `Format(TValue value)` 保護仮想メソッド

表示用文字列を生成する。`ToString()` から呼び出される。

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected virtual string Format(TValue value)` |
| 戻り値 | `string` — 表示用に整形された文字列 |
| デフォルト実装 | `value.ToString()` を返す |

---

### 2.6 `PrimitiveEqualityComponents()` 保護仮想メソッド

等価性比較に使用するコンポーネントを列挙する。

| 条件 | 戻り値 |
|------|--------|
| 常に | `IsSet` を先頭に返す |
| `IsSet == true` のとき | 続けて `ValueField` を返す |
| `IsSet == false` のとき | `IsSet` のみ |

> **📌 注意**  
> 本メソッドは `IsSet` を先頭に含めて返す。`ValueObjectComponentNormalizer` の分岐 B に該当するため、`IsSet` の二重付加は発生しない。

---

### 2.7 `GetEqualityComponents()` オーバーライド

`ValueObject` の抽象メソッドの実装。`PrimitiveEqualityComponents()` に委譲する。

| 項目 | 内容 |
|------|------|
| シグネチャ | `protected override IEnumerable<object?> GetEqualityComponents()` |
| 処理 | `PrimitiveEqualityComponents()` を呼び出す |

---

### 2.8 `ToString()` オーバーライド

人が読めるデバッグ・表示用文字列を返す。

| 条件 | 戻り値 |
|------|--------|
| `IsSet == true` | `Format(ValueField)` の結果 |
| `IsSet == false` | `"Unset"` |

---

## 3. 設計制約・禁止事項

### 3.1 Domain 層制約

`PrimitiveValueObject<TValue>` は Domain 層に属するため、以下を知ってはならない。

| 禁止（Domain 層が知ってはならないもの） | 許可（Domain 層が知ってよいもの） |
|---------------------------------------|----------------------------------|
| ❌ ログ出力実装 | ✅ 純粋な値計算・正規化 |
| ❌ DB / ORM | ✅ 型による状態表現（Unset / IsSet） |
| ❌ フレームワーク型（`ILogger` 等） | ✅ 業務ルール検証（例外スロー） |
| ❌ 時刻取得（`DateTime.Now` / `UtcNow`） | ✅ `IClock` インターフェース（DI 注入） |

### 3.2 null 禁止原則

- コンストラクタで `IsSet` を明示的に設定すること
- `IsSet = false` のインスタンスを返す場合、`null` を返さず Unset インスタンスを返す
- `TryGetValue` の `false` 返却時、呼び出し元は `value` を使用してはならない

### 3.3 正規化・検証の順序保証

`Normalize` → `Validate` の順序は必ず守る。順序を入れ替えてはならない。

### 3.4 `ValueField` の直接参照禁止

派生クラス外部からの `ValueField` への直接アクセスは禁止する。外部アクセスは `TryGetValue` 経由に限定する。

---

## 4. 実装ガイドライン

### 4.1 派生クラスの実装義務

- Unset を表す静的プロパティまたは静的ファクトリメソッドを提供する
- 業務ルールがある場合は `Normalize` および `Validate` をオーバーライドする
- 表示形式が `ToString()` と異なる場合は `Format` をオーバーライドする

### 4.2 派生クラスの禁止事項

| 禁止事項 | 理由 |
|----------|------|
| `GetEqualityComponents()` の再オーバーライド | `PrimitiveEqualityComponents()` への委譲を破壊するため |
| `Normalize` 内での例外スロー | 例外スローは `Validate` の責務 |
| `IsSet = false` 時の `ValueField` 参照 | 未初期化値への参照となるため |

### 4.3 実装例（参考）

```csharp
// 派生クラスの例：正規化・検証をオーバーライドする場合
protected override string Normalize(string input) => input.Trim();
protected override void Validate(string normalized)
{
    if (normalized.Length > 100) throw new DomainException("...");
}
```

---

## 5. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-07-03 | 加藤 正人 | 初版作成 |
