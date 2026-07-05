# 技術仕様書 — IClockValueObject

**プロジェクト:** SupportAdvance  
**レイヤ:** Domain 層  
**種別:** インターフェース設計原則  
**版:** 1.0 / 2026-07-03

---

## 1. 位置づけ

`IClockValueObject<TValue>` は、**現在時刻に依存した検証を行う ValueObject** の設計規約である。

本インターフェースの目的は以下に集約される。

- 時刻に基づいた値検証（例：未来日チェック）を Domain 層で定義すること
- **Domain 層は `IClock` を知らない** という原則を維持しながら、検証メソッドの引数として時刻情報を受け取ること
- 検証ロジックが必要な場合、UseCase が `IClock` を DI から取得し、検証メソッドに注入すること

> **📌 原則**  
> Domain 層は時刻取得の具体実装を知らない。`IClock` は Domain 層のメソッド引数でのみ受け取られる。

---

## 2. メンバー仕様

### 2.1 `ValidateWithClock` メソッド

時刻に依存した検証を行うインスタンスメソッド。

| 項目 | 内容 |
|------|------|
| シグネチャ | `void ValidateWithClock(TValue value, IClock clock)` |
| 検証内容 | 指定された値が、与えられた時刻を基準として有効か判定 |
| 成功時 | メソッドは return（void） |
| 失敗時 | `ArgumentException` またはその派生を throw |
| パラメータ | `value`: 検証対象の値、`clock`: 現在時刻を供給する IClock インスタンス |

**設計判断**

- **戻り値を持たない（void）**: 検証成功時は何も返さず、失敗時は例外を throw する
- **TValue**: 検証対象の値の型（例：`DateTime` for DateOfBirth）
- **IClock**: Domain 層の外（Application 層 / UseCase）から注入される

---

## 3. 実装パターンと例外保証

### 3.1 検証対象の例

| ValueObject | TValue | 検証ロジック | 例外条件 |
|---|---|---|---|
| DateOfBirth | DateTime | 未来日チェック | value.Date >= clock.Now.Date → ArgumentException |
| RegistrationAt | DateTime | 未来日チェック | value > clock.Now → ArgumentException |
| EventScheduledDate | DateTime | 過去日チェック | value.Date < clock.Now.Date → ArgumentException |

### 3.2 例外保証

| メソッド | 成功時 | 失敗時 |
|---|---|---|
| `ValidateWithClock` | 戻る（void） | ArgumentException（または派生） |

> **📌 注意**  
> 例外型は `ArgumentException` またはその派生クラス（`ArgumentOutOfRangeException` など）に限定される。`NullReferenceException` や `InvalidOperationException` は不許可。

---

## 4. 呼び出し側責務（Application / UseCase）

### 4.1 IClock の取得と注入

UseCase は以下の流れで `ValidateWithClock` を呼び出す。

```
1. DI から IClock インスタンスを取得（コンストラクタ注入）
2. ValueObject インスタンスを Domain 層で生成（時刻検証なし）
3. UseCase 内で ValidateWithClock を呼び出し（IClock を引数で渡す）
4. 検証成功 → 業務フロー継続
5. 検証失敗 → Application Exception にマップして呼び出し元に返す
```

### 4.2 DI 登録責務

`IClock` の DI 登録責務は **Infrastructure 層のみ** にある。Domain 層では IClock を登録してはならない。

---

## 5. Domain 層制約

`IClockValueObject<TValue>` を実装する ValueObject は以下を遵守する。

| 禁止（Domain 層が知ってはならないもの） | 許可（Domain 層が知ってよいもの） |
|---------------------------------------|----------------------------------|
| ❌ `DateTime.Now` / `UtcNow` 直接使用 | ✅ IClock インターフェース（メソッド引数） |
| ❌ `SystemClock` / `TickingClock` 生成 | ✅ IClock インスタンス受け取り |
| ❌ タイムゾーン判定 | ✅ 時刻値の大小比較 |

> **📌 原則**  
> Domain 層は **いかなる時刻実装も知ってはならない**。時刻が必要な場合、必ず IClock インターフェースを経由する。

---

## 6. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-07-03 | 加藤 正人 | 初版作成 |
