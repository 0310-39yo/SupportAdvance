# XMLドキュメントコメント ガイド

新しくソースを書くときの **XMLドキュメントコメント（`///`）の書き方の基準とテンプレート**。

- **対象**: `src/` 配下のすべての C# コード（テストコードは対象外。テストは [テスト観点ID] の規約に従う）
- **既存コードの扱い**: 一括で書き直す必要はない。ファイルを修正するときに、触ったメンバーをこのガイドに合わせる（ボーイスカウトルール）
- **根拠**: 既存 186 ファイル・1,087 コメントブロックを調べた結果（[20260918_XMLドキュメントコメント現状調査報告.md](../Reports/20260918_XMLドキュメントコメント現状調査報告.md)）と、Microsoft の推奨事項（C# 言語仕様「ドキュメント コメント」、.NET API ドキュメントの書き方）
- **文体**: コメントの文は**すべて体言止め**。文の区切りには「。」を使い、**最後の文には付けない**（→ [§4](#4-文体のルール体言止めと決まった言い回し)）

---

## 目次

1. [5つの原則](#1-5つの原則)
2. [IntelliSense でどう見えるか](#2-intellisense-でどう見えるか)
3. [タグの使い分け](#3-タグの使い分け)
4. [文体のルール（体言止め）と決まった言い回し](#4-文体のルール体言止めと決まった言い回し)
5. [【見出し】で使う言葉（プロジェクト固有）](#5-見出しで使う言葉プロジェクト固有)
6. [要素別テンプレート](#6-要素別テンプレート)
7. [`<inheritdoc/>` の使い方](#7-inheritdoc-の使い方)
8. [避けるべき書き方（実コードの例）](#8-避けるべき書き方実コードの例)
9. [レビュー用チェックリスト](#9-レビュー用チェックリスト)
10. [ビルド設定（適用済み）](#10-ビルド設定適用済み)

---

## 1. 5つの原則

| # | 原則 | 理由 |
|---|---|---|
| 1 | **`<summary>` は1〜2文の要約だけにする**。詳しい説明は `<remarks>` に書く | 補完一覧のツールチップには `<summary>` しか表示されない。長い `<summary>` は1行目しか読まれない |
| 2 | **名前を言い換えただけの文は書かない**。名前から分からないこと（意図・約束事・制約・根拠）を書く | `GetHashCode` に「ハッシュコードの取得」と書いても、読む人は何も新しく知ることができない |
| 3 | **約束事（コントラクト）をはっきり書く**: null／Unset の意味、例外、副作用、時刻（JST）、単位、スレッド安全性 | 呼び出す人が実装を読まずに正しく使えるようにする。これがドキュメントコメントを書く最大の目的 |
| 4 | **型名・メンバー名は `<see cref="..."/>` で参照する**（普通の文字列で書かない） | 名前を変更すると一緒に変わる。F12 で定義に移動できる。参照先がなければビルド警告（CS1574）で気づける。Identity BC → Authentication BC の名前変更のときのように、文字列で書いた名前は古いまま残ってしまう |
| 5 | **正しい XML として書く**。`<` `>` `&` を地の文にそのまま書かない | XML として壊れたコメントは IntelliSense に**何も表示されない**。現在はビルドエラーで検出（→ [§10](#10-ビルド設定適用済み)） |

---

## 2. IntelliSense でどう見えるか

コメントは「ソースを読む人」だけでなく「**IntelliSense で見る人**」にも向けて書く。後者のほうがずっと多い。

| 表示される場所 | 表示されるもの |
|---|---|
| 補完一覧のツールチップ | `<summary>`（と例外の型名） |
| シグネチャヘルプ（`(` を入力したとき） | メソッドの `<summary>` と、入力中の引数の `<param>` |
| クイックヒント（マウスを乗せたとき） | `<summary>` を中心に、`<returns>`・`<remarks>`・`<exception>` など |

### ⚠ 改行は無視される

IntelliSense は `///` の改行を**すべて空白1つにまとめて**表示する。プロジェクトでよく使われている下の書き方は、

```csharp
/// <summary>
/// ローカル認証 Use Case
///
/// 【責務】
/// - ログインID・パスワードでユーザーを認証
/// - UserAuthSession を生成してセッションを確立
/// </summary>
```

ツールチップでは次のように **1つの長い文** になってしまう。

> ローカル認証 Use Case 【責務】 - ログインID・パスワードでユーザーを認証 - UserAuthSession を生成してセッションを確立

**対策**: 段落は `<para>`、箇条書きは `<list>` で表す。どちらも IntelliSense で改行されて表示される。

```csharp
/// <summary>
/// ログインID とパスワードによるユーザー認証と、認証セッションの記録を行うユースケース
/// </summary>
/// <remarks>
/// <para>【責務】認証（本人確認）のみ。権限の確認（認可）は Presentation 層の担当</para>
/// <para>【副作用】成功・失敗どちらの場合も <see cref="UserAuthSession"/> を 1 件保存</para>
/// </remarks>
```

---

## 3. タグの使い分け

### 3-1. 要素ごとに必要なタグ

✅＝必須　○＝あてはまる場合は必須　△＝任意　―＝使わない

| 要素 | summary | remarks | param | typeparam | returns | value | exception | example |
|---|---|---|---|---|---|---|---|---|
| クラス／record／struct | ✅ | △ | ○ ※1 | ○ | ― | ― | ― | △ |
| インターフェース | ✅ | △ | ― | ○ | ― | ― | ― | △ |
| enum 型 | ✅ | △ | ― | ― | ― | ― | ― | ― |
| enum の値 | ✅ ※2 | ― | ― | ― | ― | ― | ― | ― |
| コンストラクター | ✅ | △ | ✅ | ― | ― | ― | ○ | ― |
| メソッド | ✅ | △ | ✅ | ○ | ○ ※3 | ― | ○ | △ |
| プロパティ | ✅ | △ | ― | ― | ― | △ ※4 | ○ | ― |
| 定数／static readonly | ✅ | △ | ― | ― | ― | ― | ― | ― |
| イベント | ✅ | △ | ― | ― | ― | ― | ― | ― |
| 演算子 | ✅ | ― | ✅ | ― | ✅ | ― | ○ | ― |
| override／インターフェースの実装 | `<inheritdoc/>` を使う（→ [§7](#7-inheritdoc-の使い方)） ||||||||

- ※1 プライマリコンストラクターの引数は、**型の** `<param>` に書く
- ※2 短い説明でも、`<summary>` は必ず 3 行形式（→ [§4-1](#4-1-体言止めに統一する)）
- ※3 `void`／`Task` を返すメソッドには書かない
- ※4 取りうる値の意味（null、Unset、空、単位）を書くときに使う。**プロパティには `<returns>` を使わない**

### 3-2. タグの一覧

| タグ | 用途 | このプロジェクトでのルール |
|---|---|---|
| `<summary>` | 要約 | 1〜2文。体言止め。最後の文に「。」なし。**必ず 3 行形式**（1 行形式は禁止） |
| `<remarks>` | 詳しい説明 | `<para>【見出し】…</para>` の形で書く（→ [§5](#5-見出しで使う言葉プロジェクト固有)） |
| `<param name="x">` | 引数の説明 | 型名を繰り返さない。**意味・単位・null のときの扱い**を書く |
| `<typeparam name="T">` | 型引数の説明 | 制約の**理由**を書く |
| `<returns>` | 戻り値の説明 | 「見つからないとき」「Unset のとき」の値まで書く |
| `<value>` | プロパティの値の説明 | `<returns>` の代わりに使う |
| `<exception cref="T">` | 送出する例外 | **「〜の場合」の形で送出する条件を書く**。自分で `throw` している例外はすべて書く |
| `<see cref="T"/>` | 型・メンバーへのリンク | ジェネリック型は `cref="Entity{TId}"`（`<>` ではなく `{}`） |
| `<see langword="null"/>` | キーワード | `null`／`true`／`false` は必ずこれで書く |
| `<paramref name="x"/>` | 同じメソッドの引数を指す | 引数名が変わると警告で気づける |
| `<typeparamref name="T"/>` | 型引数を指す | |
| `<c>…</c>` | 1行のコード・列名・SQL | `<c>t_user_auth_sessions.row_id</c>` のように使う |
| `<code>…</code>` | 複数行のコード | `<example>` の中に書く。**Markdown の ``` は使わない** |
| `<para>` | 段落の区切り | `<remarks>` の中で使う |
| `<list type="bullet\|number\|table">` | 箇条書き・番号付きリスト・表 | 処理の流れは `number` にする |
| `<seealso cref="T"/>` | 関連する型 | 対になる型を示す（Entity ↔ Mapper ↔ DbModel など） |
| `<inheritdoc/>` | コメントを引き継ぐ | → [§7](#7-inheritdoc-の使い方) |

### 3-3. XML で特別な意味を持つ文字の書き方

| 書きたいもの | ❌ そのまま書く | ✅ 正しい書き方 |
|---|---|---|
| ジェネリック型の参照 | `Entity<TId>` | `<see cref="Entity{TId}"/>` |
| 参照ではない型の表記 | `List<string>` | `<c>List&lt;string&gt;</c>` |
| 比較演算子 | `a < b` | `a &lt; b`、または「a が b より小さい場合」と書く |
| `&` | `A & B` | `A &amp; B` |

### 3-4. cref は依存の方向に従う（このプロジェクト独自のルール）

`<see cref>` は**そのプロジェクトから参照できる型**しか指定できない。依存の方向で外側にある型（例: Domain から見た Application の `IQueryService`）は cref にせず、`<c>IQueryService</c>` と書く。**cref を書くためだけに `using` や `ProjectReference` を増やしてはいけない。**

---

## 4. 文体のルール（体言止め）と決まった言い回し

### 4-1. 体言止めに統一する

コメントの文は、`<summary>`・`<remarks>`・`<param>`・`<returns>`・`<value>`・`<exception>` のどれでも、**すべて体言止め**（名詞で終える）にする。
句点は**文の区切りにだけ使い、最後の文には付けない**。

| ルール | ✅ 良い例 | ❌ 悪い例 |
|---|---|---|
| 名詞で終える | 従業員の退職処理 | 従業員を退職させる／従業員を退職させます |
| 動作は「〜の〇〇」「〜処理」「〜の実行」などの名詞にする | ログインID に対応する認証情報の非同期取得 | ログインID に対応する認証情報を非同期に取得する |
| 条件は「〜の場合」で終える | `retiredOn` が未来日の場合 | `retiredOn` が未来日のとき送出される |
| 文が 2 つ以上あるときも、それぞれを体言止めにする | 在職中は Unset。null なし | 在職中は Unset。null にはならない |
| 文の区切りには「。」を使う | 在職中は Unset。null なし | 在職中は Unset、null なし／在職中は Unset null なし |
| 最後の文には「。」を付けない | 退職日（JST） | 退職日（JST）。 |
| 最後の文に「。」なしは、1 文だけの場合も同じ | 退職日（JST） | 退職日（JST）。 |

### `<summary>` は必ず 3 行形式

`<summary>` は、中身が 1 行でも開始タグ・本文・終了タグをそれぞれ別の行に書く。1 行形式（`/// <summary>…</summary>`）は使わない。

```csharp
// ✅ 良い例
/// <summary>
/// ログイン中のユーザーのログインID
/// </summary>

// ❌ 悪い例
/// <summary>ログイン中のユーザーのログインID</summary>
```

- 理由: 本文の追加・修正で差分が 1 行に収まり、すべての `<summary>` の見た目がそろうため
- `<param>`／`<returns>`／`<value>`／`<exception>` などの他のタグは 1 行形式のままでよい

- **無理に体言止めにしない**: 「〜ない」「〜できる」など、否定や可能を名詞にすると不自然になる場合は、「〜不可」「〜可能」「〜なし」を使う。
  - 例: 「null にはならない」→「null になることのない値」のような回りくどい形ではなく、「null なし」
  - 例: 「例外は送出しない」→「例外の送出なし」
  - 例: 「変更してはいけない」→「変更禁止」
- 用語はそろえる: 集約ルート／値オブジェクト／ユースケース／リポジトリ／DB モデル／ドメインイベント
- 日時には必ず **JST** かどうかを書く。時間の長さには単位（ミリ秒・秒）を書く

### 4-2. `<summary>` の書き出し

| 要素 | 形 | 例 |
|---|---|---|
| クラス | 「〜を表す［種類］」／「〜を行う［役割］」 | 従業員を表す集約ルート |
| インターフェース | 「〜の提供」／「〜のための抽象」 | 集約を ID で読み取る問い合わせサービスの抽象 |
| コンストラクター | 「`<see cref="X"/>` クラスの新しいインスタンスの初期化」 | |
| メソッド | 「〜の〇〇」「〜処理」 | 従業員の退職処理 |
| 非同期メソッド | 「〜の非同期〇〇」 | ログインID に対応する認証情報の非同期取得 |
| Try 系メソッド | 「〜の試行」 | 日時（JST）からの `<see cref="UpdatedAt"/>` 生成の試行 |
| bool を返すメソッド | 「〜かどうかの判定」 | 指定日時に在職しているかどうかの判定 |
| ファクトリーメソッド | 「〜を持つ〇〇の生成」 | 指定日時を持つ `<see cref="UpdatedAt"/>` の生成 |
| プロパティ | 名詞句 | 退職日（JST） |
| bool のプロパティ | 「〜かどうかを示す値」 | 更新済みかどうかを示す値 |
| イベント | 「〜時に発生するイベント」 | ログイン成功時に発生するイベント |
| 定数 | 何を表す値か | システム処理用に予約した従業員行ID |

### 4-3. 決まった言い回し

```text
<returns>
  見つかった［X］。見つからない場合は <see langword="null"/>
  ［条件］の場合は <see langword="true"/>、それ以外の場合は <see langword="false"/>
  該当なしの場合は空のコレクション（<see langword="null"/> なし）
  未設定の場合は <see cref="X.Unset"/>（<see langword="null"/> なし）
</returns>

<exception cref="ArgumentNullException"><paramref name="x"/> が <see langword="null"/> の場合</exception>
<exception cref="ArgumentException"><paramref name="x"/> が空文字または空白のみの場合</exception>
<exception cref="ArgumentOutOfRangeException"><paramref name="x"/> が 1 未満の場合</exception>
<exception cref="InvalidOperationException">［状態］での呼び出しの場合</exception>
```

---

## 5. 【見出し】で使う言葉（プロジェクト固有）

`【責務】` などの見出しは、このプロジェクトのコメントの良いところで、読みやすい。**見出しは続けて使い、書く場所を `<summary>` から `<remarks>` の `<para>` に移す。** 見出しの後の文も体言止めにする。

```csharp
/// <remarks>
/// <para>【責務】…の管理</para>
/// <para>【重要】…の場合は例外</para>
/// </remarks>
```

使う見出しは下の表から選ぶ。**1つの `<remarks>` に書くのは、本当に必要な4〜5個までにする。**

| 見出し | 書くこと | 主に使う要素 | |
|---|---|---|---|
| 【責務】 | 何を受け持つか／**何を受け持たないか** | 型 | 既存 |
| 【設計】 | なぜこの形にしたか。採用しなかった案 | 型・メソッド | 既存 |
| 【用途】 | どの層・どの場面で使うか | 型・メソッド | 既存 |
| 【呼び出し元】 | 想定している呼び出し元 | メソッド | 既存 |
| 【ライフサイクル】 | 生成・復元・終了の流れ | 集約・Entity | 既存 |
| 【値の根拠】 | その値を選んだ理由（マジックナンバーの説明） | 定数 | 既存 |
| 【重要】 | 知らないと不具合になること | すべて | 既存 |
| 【注意】 | 間違えやすい点 | すべて | 既存 |
| **【不変条件】** | いつでも必ず成り立つこと | 集約・Entity・値オブジェクト | **追加** |
| **【事前条件】** | 呼び出す前に満たしている必要があること | メソッド | **追加** |
| **【副作用】** | DB への書き込み、ドメインイベントの発行、状態の変更、ログ出力 | メソッド | **追加** |
| **【null契約】** | null と Unset をどう扱うか（null 厳格性原則） | 値オブジェクト・Mapper・DTO | **追加** |
| **【時刻】** | JST かどうか、`IClock` から取るか、`DateTime.Kind` | 日時を扱う型・メソッド | **追加** |
| **【スレッド安全性】** | 複数スレッドから同時に呼べるか、どのスレッドで発生するか | 共有される型・イベント | **追加** |
| **【DB】** | 対応するテーブル・列の型・NULL 可否・外部キー | DB モデル | **追加** |
| **【参照】** | 関連する設計ガイドのパス | 型 | **追加** |

> **履歴（「2026-09-xx に変更」など）はコメントに書かない。** 変更の履歴は Git が管理する。ただし「2026-09-18 時点でシーケンスの値は○○」のように、**値の根拠となった日付**は書いてよい（`WellKnownIds` の書き方は良い例）。

---

## 6. 要素別テンプレート

各テンプレートでは、`［…］` の部分を置き換える。実例は、既存のクラスをこのガイドに合わせて書き直したもの。

### 6-1. クラス（基本形）

```csharp
/// <summary>
/// ［何を表すか／何を行うか］［種類］
/// </summary>
/// <remarks>
/// <para>【責務】［受け持つこと］。［受け持たないこと］は <see cref="［担当する型］"/> の担当</para>
/// <para>【設計】［この形にした理由］</para>
/// </remarks>
/// <seealso cref="［対になる型］"/>
public sealed class ［ClassName］
```

### 6-2. 集約ルート／Entity

```csharp
/// <summary>
/// 従業員を表す集約ルート。在職期間・部署への所属・ロール・権限を一貫性を保って管理する単位
/// </summary>
/// <remarks>
/// <para>【集約ID】<see cref="EmployeeRowId"/>（シーケンスで採番する long 値）</para>
/// <para>【ライフサイクル】新規作成は <see cref="Create"/>、DB からの復元は <see cref="Reconstruct"/>、
/// 退職は <see cref="RetireEmployee"/></para>
/// <para>【不変条件】<see cref="DepartmentMemberships"/> の要素は、すべてこの従業員の所属</para>
/// <para>【Context 間参照】他の BC への公開は <see cref="IEmployee"/> としてのみ（<c>IQueryService</c> 経由）</para>
/// </remarks>
public sealed class Employee : AggregateRoot<EmployeeRowId>, IEmployee
```

### 6-3. 値オブジェクト

```csharp
/// <summary>
/// ［業務上の意味］を表す値オブジェクト
/// </summary>
/// <remarks>
/// <para>【制約】［桁数・文字の種類・範囲など、Validate で確認する内容］</para>
/// <para>【正規化】［Normalize で行う変換。変換なしの場合は省略］</para>
/// <para>【null契約】必須／任意。［null を受け取った場合の扱い。Unset か、失敗か］</para>
/// </remarks>
public sealed class ［Name］ : PrimitiveValueObject<［T］>
```

**実例**（`UpdatedAt`）:

```csharp
/// <summary>
/// エンティティの最終更新日時（JST）を表す値オブジェクト
/// </summary>
/// <remarks>
/// <para>【null契約】未更新の状態は <see langword="null"/> ではなく <see cref="Unset"/>（<see cref="HasUpdated"/> が
/// <see langword="false"/>）で表現。Domain 層での null 確認は不要</para>
/// <para>【時刻】値は <see cref="LocalDateTime"/>（JST）で保持。DB の <c>DateTime</c> との変換は
/// Infrastructure（Mapper・Repository）の担当。この型は <c>DateTime</c> を公開しない</para>
/// <para>【参照】docs/Assistance/Guides/null厳格性設計ガイド.md</para>
/// </remarks>
/// <seealso cref="CreatedAt"/>
/// <seealso cref="DeletedAt"/>
public sealed class UpdatedAt : PrimitiveValueObject<LocalDateTime?>, IEquatable<UpdatedAt>
```

### 6-4. ユースケース

```csharp
/// <summary>
/// ［業務上の操作］を行うユースケース
/// </summary>
/// <remarks>
/// <para>【処理の流れ】</para>
/// <list type="number">
/// <item><description>［手順1］</description></item>
/// <item><description>［手順2］</description></item>
/// </list>
/// <para>【副作用】［保存するもの・発行するイベント］</para>
/// <para>【責務】［対象外のこと。例: 認可は対象外］</para>
/// </remarks>
```

**実例**（`AuthenticateLocalUserUseCase`）:

```csharp
/// <summary>
/// ログインID とパスワードによるユーザー認証と、認証セッションの記録を行うユースケース
/// </summary>
/// <remarks>
/// <para>【処理の流れ】</para>
/// <list type="number">
/// <item><description><see cref="ILoginCredentialsQuery"/> によるログインID からの認証情報の取得</description></item>
/// <item><description>認証情報が有効かどうかの確認</description></item>
/// <item><description><see cref="IPasswordHashService"/> によるパスワードの照合</description></item>
/// <item><description><see cref="UserAuthSession"/> の作成と保存</description></item>
/// </list>
/// <para>【副作用】<b>失敗した場合もセッションを 1 件保存</b>（不正ログインの追跡用）。
/// 記録する従業員は、ログインID が存在しない場合は <see cref="WellKnownIds.UnknownUserEmployeeRowId"/>、
/// それ以外の場合はログイン対象アカウントの従業員</para>
/// <para>【責務】認証のみ。権限の確認（認可）と Employee BC の参照は対象外</para>
/// </remarks>
public sealed class AuthenticateLocalUserUseCase
```

### 6-5. インターフェース（リポジトリ・問い合わせサービス）

コードの例は `<example><code>` に書く。`<code>` の中でも `<` は `&lt;` と書く必要がある。

```csharp
/// <summary>
/// 集約を ID で読み取る、Context 間共通の問い合わせサービスの抽象
/// </summary>
/// <typeparam name="TAggregate">読み取る集約。Domain の Entity ではなく、Application 層で公開するインターフェース（例: <see cref="IEmployee"/>）</typeparam>
/// <typeparam name="TId">集約ID の型</typeparam>
/// <remarks>
/// <para>【用途】他の BC の集約の<b>最新の状態</b>の同期的な読み取り。変更の通知にはドメインイベントを使用</para>
/// <para>【参照】CLAUDE.md「Context間のデータ共有パターン」</para>
/// </remarks>
/// <example>
/// <code>
/// services.AddScoped&lt;IQueryService&lt;IEmployee, EmployeeRowId&gt;, EmployeeQueryService&gt;();
/// </code>
/// </example>
public interface IQueryService<TAggregate, TId>
```

インターフェースのメンバーには**呼び出す側から見た約束事**を書く。実装クラスは `<inheritdoc/>` で引き継ぐ。

```csharp
/// <summary>
/// 集約ID に対応する集約の非同期取得
/// </summary>
/// <param name="id">取得する集約の ID</param>
/// <returns>見つかった集約。存在しない場合、または論理削除済みの場合は <see langword="null"/></returns>
Task<TAggregate?> GetByIdAsync(TId id);
```

> 「論理削除済みの場合」は、実装がそうなっているときだけ書く。**書いた約束事は、すべての実装が守らなければならない。**

### 6-6. ジェネリックな基底クラス（プライマリコンストラクター）

```csharp
/// <summary>
/// 監査列を自動で設定する、リポジトリの基底クラス
/// </summary>
/// <typeparam name="TEntity">扱うエンティティ</typeparam>
/// <typeparam name="TDbModel">対応する DB モデル。監査列はプロパティ名によるリフレクションで検索</typeparam>
/// <typeparam name="TId">エンティティの ID。<see cref="RowId"/> の派生型のみ</typeparam>
/// <param name="mapper">エンティティと DB モデルの相互変換を行うマッパー</param>
/// <param name="currentUser">監査列（<c>created_by</c> など）に記録する操作者の情報</param>
/// <param name="clock">監査列（<c>updated_at</c>）に記録する現在時刻（JST）の取得元</param>
/// <exception cref="ArgumentNullException">いずれかの引数が <see langword="null"/> の場合</exception>
/// <remarks>
/// <para>【責務】監査列（<c>*_at</c>／<c>*_by</c>）の設定。マッパーの担当は業務データの変換のみ</para>
/// <para>【注意】DB モデルのプロパティ名が <c>CreatedBy</c>／<c>UpdatedAt</c>／<c>UpdatedBy</c>／<c>DeletedBy</c> と異なる場合、エラーなしで設定されないまま</para>
/// </remarks>
public abstract class RepositoryBase<TEntity, TDbModel, TId>(
    IEntityMapper<TEntity, TDbModel, TId> mapper, ICurrentUserService currentUser, IClock clock)
```

### 6-7. DB モデル

`[Table]`／`[Column]` 属性で分かること（テーブル名・列名）は**書かない**。属性では分からない「意味・NULL の意味・外部キー」を書く。

```csharp
/// <summary>
/// ［テーブルの論理名］テーブルの 1 行を表す DB モデル
/// </summary>
/// <remarks>
/// <para>【対応】Domain 側は <see cref="［Entity］"/>。変換は <see cref="［Mapper］"/> の担当</para>
/// <para>【時刻】日時の列はすべて JST の <see cref="DateTime"/>（Kind=Unspecified）。<see cref="LocalDateTime"/> への変換はマッパーの担当</para>
/// </remarks>
[Table("t_xxx")]
public class ［Name］DbModel
{
    /// <summary>
    /// ログアウト日時（JST）
    /// </summary>
    /// <value><see langword="null"/> の場合、ログアウト操作なしで終了したセッション（異常終了など）</value>
    [Column("logged_out_at")]
    public DateTime? LoggedOutAt { get; set; }

    /// <summary>
    /// ローカル認証で使用した認証情報の行ID
    /// </summary>
    /// <value>AD 認証の場合は <see langword="null"/></value>
    /// <remarks>【DB】FK → <c>m_login_credentials.row_id</c></remarks>
    [Column("login_credentials_row_id")]
    public long? LoginCredentialsRowId { get; set; }
}
```

### 6-8. DTO（Request／Response）

```csharp
/// <summary>
/// ［ユースケース名］への入力
/// </summary>
/// <remarks>
/// <para>【null契約】外部からの入力のため null 許容。値オブジェクトへの変換（TryFrom）はユースケースの担当</para>
/// </remarks>
public sealed class ［Name］Request
{
    /// <summary>
    /// ログインID
    /// </summary>
    /// <value>必須。前後の空白を除いて 50 文字以内</value>
    public string LoginId { get; init; } = string.Empty;
}
```

### 6-9. ドメインイベント

```csharp
/// <summary>
/// ［起きたこと］を表すドメインイベント
/// </summary>
/// <remarks>
/// <para>【発行元】<see cref="［Aggregate.Method］"/></para>
/// <para>【発行のタイミング】集約の状態変更の直後に登録、リポジトリでの保存後に配信</para>
/// <para>【購読者】［想定している購読者。例: 監査ログ、人事システム連携］</para>
/// </remarks>
public sealed class ［Name］Event : IDomainEvent
```

### 6-10. 例外クラス

```csharp
/// <summary>
/// ［どんな失敗か］の場合に送出される例外
/// </summary>
/// <remarks>
/// <para>【送出元】<see cref="［型.メソッド］"/></para>
/// <para>【主な原因】［接続の失敗、タイムアウトなど］。元の例外は <see cref="Exception.InnerException"/></para>
/// <para>【対処】［呼び出し元の対応方法。再試行の可否］</para>
/// </remarks>
public class ［Name］Exception : InvalidOperationException
{
    /// <summary>
    /// エラーメッセージを指定した、<see cref="［Name］Exception"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="message">エラーの内容を説明するメッセージ</param>
    public ［Name］Exception(string message) : base(message) { }

    /// <summary>
    /// エラーメッセージと原因の例外を指定した、<see cref="［Name］Exception"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="message">エラーの内容を説明するメッセージ</param>
    /// <param name="innerException">この例外の原因になった例外（<c>SqlException</c> など）</param>
    public ［Name］Exception(string message, Exception? innerException) : base(message, innerException) { }
}
```

### 6-11. static クラスと定数

定数には、**その値を選んだ理由（【値の根拠】）と、よく似た定数との違い**を書く。値そのもの（`= "Debug"`）はコメントに書かない（ソースを見れば分かり、書くと値を変えたときに古くなる）。

```csharp
/// <summary>
/// ［何を表す値か］
/// </summary>
/// <remarks>
/// <para>【用途】［使用箇所］</para>
/// <para>【値の根拠】［この値を選んだ理由］</para>
/// <para>【重要】［よく似た定数との違い。使い分けの基準］</para>
/// </remarks>
public const long ［Name］ = ［value］;
```

**実例**（`WellKnownIds.UnknownUserEmployeeRowId`）:

```csharp
/// <summary>
/// 従業員マスタで本人を特定できない人（存在しないログインID でのログイン試行者など）を表す、予約済みの従業員行ID
/// </summary>
/// <remarks>
/// <para>【用途】<c>AuthenticateLocalUserUseCase</c> での、ログインID が見つからなかった失敗の記録</para>
/// <para>【重要】<see cref="SystemUserEmployeeRowId"/>（システム自身による自動処理）とは別の意味。
/// こちらは「実在するが、特定できない人」</para>
/// <para>【値の根拠】<c>s_row_id_sequence</c> が通過済みの範囲の空き番号（2026-09-18 に確認）。
/// 循環しない設定（is_cycling=0）のため、今後の採番との重複なし。
/// 対応する人物行ID は <see cref="UnknownUserPersonRowId"/></para>
/// </remarks>
public const long UnknownUserEmployeeRowId = 2147483649;
```

### 6-12. enum

```csharp
/// <summary>
/// ［分類の対象］の区分
/// </summary>
/// <remarks>
/// <para>【重要】数値は DB に保存されるため、既存の値の変更・並べ替えは禁止</para>
/// </remarks>
public enum ［Name］
{
    /// <summary>
    /// ID とパスワードによるローカル認証
    /// </summary>
    LocalAuth = 0,

    /// <summary>
    /// Windows Active Directory 認証
    /// </summary>
    WindowsAD = 1,
}
```

### 6-13. コンストラクター

```csharp
// ① DI で依存を受け取るコンストラクター（引数の説明は短くてよい。例外は必ず書く）
/// <summary>
/// <see cref="［ClassName］"/> クラスの新しいインスタンスの初期化
/// </summary>
/// <param name="［x］">［用途］</param>
/// <exception cref="ArgumentNullException">いずれかの引数が <see langword="null"/> の場合</exception>

// ② private コンストラクター（ファクトリーメソッドから呼ばれる）
/// <summary>
/// 検証済みの値によるインスタンスの初期化。外部からの生成は <see cref="From"/>／<see cref="Unset"/> を使用
/// </summary>
```

> コンストラクターには `<returns>` を書かない（`UpdatedAt` の private コンストラクターに書かれている）。

### 6-14. メソッド

#### 基本形

```csharp
/// <summary>
/// ［処理の内容］
/// </summary>
/// <param name="［x］">［意味］。［単位・JST かどうか・null の場合の扱い］</param>
/// <returns>［戻り値の意味］。［見つからない場合・Unset の場合の値］</returns>
/// <exception cref="［T］">［送出する条件］の場合</exception>
/// <remarks>
/// <para>【事前条件】［呼び出し前に満たしている必要があること］</para>
/// <para>【副作用】［状態の変更・DB への書き込み・イベントの発行］</para>
/// </remarks>
```

**実例**（`Employee.RetireEmployee`）:

```csharp
/// <summary>
/// 従業員の退職処理と、<see cref="EmployeeRetiredEvent"/> の発行
/// </summary>
/// <param name="retiredOn">退職日（JST）。この日時以降、<see cref="IsActive"/> の結果は <see langword="false"/></param>
/// <exception cref="InvalidOperationException">設定済みの退職日が <paramref name="retiredOn"/> 以前の場合（その時点で退職済みの場合）</exception>
/// <remarks>
/// <para>【副作用】<see cref="RetiredOn"/> の更新と、ドメインイベント 1 件の登録。DB への保存はリポジトリでの保存時</para>
/// </remarks>
public void RetireEmployee(LocalDateTime retiredOn)
```

#### 非同期メソッド

```csharp
/// <summary>
/// ［処理の内容］の非同期実行
/// </summary>
/// <param name="cancellationToken">処理の取り消し用トークン</param>
/// <returns>非同期操作を表すタスク。結果は［結果の意味］</returns>
/// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> による取り消しの場合</exception>
```

#### Try 系メソッド

```csharp
/// <summary>
/// 日時（JST）からの <see cref="UpdatedAt"/> 生成の試行。例外の送出なし
/// </summary>
/// <param name="input">更新日時（JST）。<see langword="null"/> は「未更新」（DB の値は Infrastructure が変換して渡す）</param>
/// <param name="result">
/// 成功した場合は生成したインスタンス（<paramref name="input"/> が <see langword="null"/> の場合は <see cref="Unset"/>）。
/// 失敗した場合は <see langword="null"/>（使用禁止）
/// </param>
/// <returns>成功した場合は <see langword="true"/>。値が検証に通らなかった場合は <see langword="false"/></returns>
public static bool TryFrom(LocalDateTime? input, out UpdatedAt result)
```

#### ファクトリーメソッド（From／Unset／Reconstruct）

```csharp
/// <summary>
/// 指定日時を持つ <see cref="UpdatedAt"/> の生成
/// </summary>
/// <param name="value">更新日時（JST）。通常は <see cref="IClock.JstNow"/> から取得した値</param>
/// <returns>設定済み（<see cref="HasUpdated"/> が <see langword="true"/>）のインスタンス</returns>
/// <exception cref="ArgumentException"><paramref name="value"/> が <see cref="LocalDateTime.MinValue"/> または <see cref="LocalDateTime.MaxValue"/> の場合</exception>

/// <summary>
/// 未更新の状態を表す <see cref="UpdatedAt"/> の生成
/// </summary>
/// <returns><see cref="HasUpdated"/> が <see langword="false"/> のインスタンス（<see langword="null"/> なし）</returns>

/// <summary>
/// DB から読み込んだ値による <see cref="Employee"/> の復元
/// </summary>
/// <remarks>
/// <para>【用途】Infrastructure 層のマッパーからの呼び出し専用。新規作成は <see cref="Create"/></para>
/// <para>【注意】検証とドメインイベントの発行なし（保存済みの値を信頼）</para>
/// </remarks>
```

> `Reconstruct` の【注意】は、実装がそうなっている場合だけ書く。

#### override（Equals／GetHashCode／ToString）

```csharp
/// <inheritdoc/>
public override bool Equals(object? obj) => Equals(obj as UpdatedAt);

/// <inheritdoc/>
public override int GetHashCode() => HashCode.Combine(ValueField, IsSet);

// ToString は出力の形式に意味がある場合だけ、自分で書く
/// <summary>
/// デバッグ用の文字列表現
/// </summary>
/// <returns><c>Employee(RowId=…, TypeDivision=…, BizId=…)</c> 形式の文字列。画面表示やデータの解析には使用禁止</returns>
public override string ToString()
```

### 6-15. プロパティ

```csharp
/// <summary>
/// 退職日（JST）
/// </summary>
/// <value>在職中の場合は <see cref="RetiredOn.Unset"/>（<see langword="null"/> なし）</value>
public RetiredOn RetiredOn { get; private set; }

/// <summary>
/// 更新済みかどうかを示す値
/// </summary>
/// <value>更新済みの場合は <see langword="true"/>。<see cref="ValueObject.IsSet"/> の、業務上の意味に合わせた別名</value>
public bool HasUpdated => IsSet;

/// <summary>
/// この従業員の所属部署の一覧
/// </summary>
/// <value>読み取り専用。所属なしの場合は空（<see langword="null"/> なし）</value>
/// <remarks>
/// <para>【注意】所属の終了日（<c>EndOn</c>）と雇用の終了日（<see cref="RetiredOn"/>）は別管理。
/// 異動による <see cref="RetiredOn"/> の変更なし</para>
/// </remarks>
public IReadOnlyCollection<DepartmentMembership> DepartmentMemberships => _departmentMemberships.AsReadOnly();
```

### 6-16. フィールド

| 種類 | コメント |
|---|---|
| DI で受け取った `private readonly` フィールド | **原則として書かない**（名前と型を見れば分かる） |
| 意味・単位・不変条件がある `private` フィールド | `<summary>` を書く |
| `protected` フィールド | **必ず書く**（派生クラスから使う API になるため） |
| `private const` | public の定数と同じように書く（【値の根拠】など） |
| `[ObservableProperty]` を付けたフィールド | **必ず書く**。生成されるプロパティにそのまま引き継がれ、画面にバインドするときに参照される |

```csharp
/// <summary>
/// 保持している値。<see cref="ValueObject.IsSet"/> が <see langword="true"/> の場合のみ有効
/// </summary>
protected readonly TValue ValueField;

/// <summary>
/// ログインID の入力値
/// </summary>
[ObservableProperty]
private string loginId = string.Empty;

/// <summary>
/// ログイン処理中かどうかを示す値。<see langword="true"/> の間は入力欄とボタンが無効
/// </summary>
[ObservableProperty]
private bool isLoading;
```

### 6-17. イベント

```csharp
/// <summary>
/// ログイン成功時に発生するイベント
/// </summary>
/// <remarks>
/// <para>【用途】View でのウィンドウを閉じる処理のきっかけ</para>
/// <para>【スレッド安全性】UI スレッドで発生</para>
/// </remarks>
public event EventHandler? LoginSucceeded;
```

### 6-18. `[RelayCommand]` を付けたメソッド

生成される `LoginCommand` のコメントは「`Login` を実行するコマンド」となり、このメソッドへのリンクになる。そのため、**メソッドのコメントを、コマンドの説明として書く。**

```csharp
/// <summary>
/// 入力されたログインID とパスワードによる認証。成功した場合は <see cref="LoginSucceeded"/> の発生
/// </summary>
/// <remarks>
/// <para>【副作用】失敗した場合は <c>ErrorMessage</c> の設定と、パスワード入力欄のクリア。例外の送出なし</para>
/// </remarks>
[RelayCommand]
public async Task Login()
```

### 6-19. 演算子

```csharp
/// <summary>
/// 2 つの <see cref="AuthMethod"/> が等しいかどうかの判定
/// </summary>
/// <param name="left">比較する 1 つ目の値</param>
/// <param name="right">比較する 2 つ目の値</param>
/// <returns>両方とも <see langword="null"/> の場合、または値が等しい場合は <see langword="true"/></returns>
public static bool operator ==(AuthMethod? left, AuthMethod? right)
```

### 6-20. 拡張メソッド（DI への登録）

```csharp
/// <summary>
/// Authentication BC の Application 層のサービスの、DI コンテナーへの登録
/// </summary>
/// <param name="services">登録先のサービスコレクション</param>
/// <returns>メソッドチェーン用の <paramref name="services"/> 自身</returns>
/// <remarks>
/// <para>【登録内容】ユースケース（Scoped）。Infrastructure の実装は対象外（Composition Root で別途登録）</para>
/// </remarks>
public static IServiceCollection AddAuthenticationApplication(this IServiceCollection services)
```

---

## 7. `<inheritdoc/>` の使い方

| 場面 | 書き方 |
|---|---|
| インターフェースのメンバーを実装する | `/// <inheritdoc/>` |
| `Equals`／`GetHashCode` を override する | `/// <inheritdoc/>` |
| 基底クラスの抽象メンバーを override する | `/// <inheritdoc/>` |
| 実装に固有の補足を加える | `/// <inheritdoc/>` の後に `/// <remarks>…</remarks>` を追加する |
| 別のメンバーのコメントを借りる | `/// <inheritdoc cref="Other"/>` |

```csharp
/// <inheritdoc/>
/// <remarks>
/// <para>【実装】PBKDF2-SHA256（反復 10,000 回、Salt 16 バイト）による照合。
/// ハッシュの比較は処理時間が一定の方法（タイミング攻撃への対策）</para>
/// <para>【注意】ハッシュの形式が不正な場合も、例外の送出なしで <see langword="false"/></para>
/// </remarks>
public bool VerifyPassword(string plainPassword, string passwordHash)
```

> **ルール**: 約束事は**インターフェース側に書く**。実装側に同じ文をコピーしない（`AppSettings` は良い例）。

---

## 8. 避けるべき書き方（実コードの例）

### 8-1. XML として壊れている（7 か所）

> 2026-09-18 に 7 か所とも修正済み。現在はビルドエラー（CS1570）で検出される。

`Entity<TId>` や、Markdown のコードブロックの中の `IQueryService<IEmployee, EmployeeRowId>` が XML のタグと解釈され、**コメント全体が IntelliSense に表示されない**。

| ファイル | 原因 |
|---|---|
| [IQueryService.cs:5](../../../src/Application/Queries/IQueryService.cs) | ``` の中の `<…>` |
| [IQueryServiceWithBizId.cs:5](../../../src/Application/Queries/IQueryServiceWithBizId.cs) | 同上 |
| [IEmployeeQueryService.cs:3](../../../src/Application/Queries/IEmployeeQueryService.cs) | 同上 |
| [EmployeeQueryService.cs:10](../../../src/Contexts/Employee/Employee.Application/Queries/EmployeeQueryService.cs) | 同上 |
| [IEntityMapper.cs:7](../../../src/Infrastructure/Mappers/IEntityMapper.cs) | 地の文の `<…>` |
| [RepositoryBase.cs:9](../../../src/Infrastructure/Repositories/RepositoryBase.cs) | `Entity<TId>` |
| [MultiTableRepositoryBase.cs:8](../../../src/Infrastructure/Repositories/MultiTableRepositoryBase.cs) | 同上 |

→ [§3-3](#3-3-xml-で特別な意味を持つ文字の書き方) のとおり、`{}`／`&lt;`／`<example><code>` に直す。

### 8-2. コメントが実装と食い違っている

```csharp
/// <summary>
/// 部署コード（参考情報）          ← ❌ 実際に入るのは TypeDivision.ToString()（従業員種別区分）
/// </summary>
public string Division { get; }    // EmployeeRetiredEvent.cs:21
```

> 2026-09-18 に修正済み（`従業員種別区分（参考情報）` に変更）。

コメントが間違っていると、コメントがない場合よりも悪い結果になる。**実装を変えたら、コメントも同じコミットで直す。**

### 8-3. 名前を言い換えているだけ・同じことを繰り返している

```csharp
/// <summary>
/// ハッシュコードを取得する
/// 【責務】オブジェクトのハッシュコードを取得する   ← ❌ summary と同じ文
/// </summary>
/// <returns>オブジェクトのハッシュコード</returns>
public override int GetHashCode()                    // UpdatedAt.cs:153
```

→ `/// <inheritdoc/>` の1行にする。

### 8-4. タグの使い方が間違っている

| 誤り | 例 | 正しい書き方 |
|---|---|---|
| プロパティに `<returns>` を使っている | `UpdatedAt.Value`、`HasUpdated`（9 か所） | `<value>` |
| コンストラクターに `<returns>` を書いている | `UpdatedAt` の private コンストラクター | 削除する |
| 空の `<param>` | `PrimitiveValueObject(bool isSet)` | 意味を書く |
| 例外の条件を書いていない | `<exception cref="InvalidOperationException">認証失敗</exception>` | 「ログインID が存在しない場合、アカウントが無効な場合、またはパスワードが一致しない場合」 |
| `throw` しているのに `<exception>` がない | `Employee.RetireEmployee`、`AddDepartmentMembership`（2026-09-18 修正済み） | 書き足す |
| 引数があるのに `<param>` がない | `Employee.RetireEmployee`、`RepositoryBase` のメソッド（2026-09-18 修正済み） | 書き足す |

### 8-5. 値をそのまま書いている

```csharp
/// <summary>
/// DebugBuild = "Debug"      ← ❌ 値を変えると古くなる。意味が分からない
/// </summary>
public const string DebugBuild = "Debug";
```

→ 次のように修正（2026-09-18 修正済み）

```csharp
/// <summary>
/// <see cref="IAppSettings.ApplicationBuildType"/> における Debug ビルドを表す値
/// </summary>
public const string DebugBuild = "Debug";
```

### 8-6. 文体が体言止めになっていない

既存コードには「〜する」「〜します」「〜を取得」などが混ざっている。触ったメンバーは体言止めに直す（最後の文の「。」は付けない。既存コードの約 98% はすでに句点なし）。

| ❌ 既存 | ✅ 体言止め |
|---|---|
| 従業員を退職させる | 従業員の退職処理 |
| 現在のJST日時を取得します | 現在の日時（JST） |
| ローカル認証を実行 | ローカル認証の実行 |
| Validate は、基礎クラスのコンストラクタで自動実行される | Validate は基底クラスのコンストラクターで自動実行 |

### 8-7. コメントがない

> 2026-09-18 にすべて追加済み（CS1591 は 0 件）。あわせて、コメントはあるが `<param>`／`<returns>`／`<exception>` が足りなかった約 260 件も補完済み。

当時 public なのにコメントがなかったもの: 型 8、メソッド 77、プロパティ・フィールド 20、定数 6。多いのは次のファイル。
`AuthMethod.cs`（11）、`BizId.cs`（8）、`BizCode.cs`（7）、`LoginId.cs`（6）、`FrameworkLoggingAdapter.cs`（6）、`RealCurrentUserService.cs`（WinTrial・WpfTrial とも 6）、`PasswordBoxAssistant.cs`（6）。
型そのものにコメントがないもの: `IClock`、`AppSettings` など。

---

## 9. レビュー用チェックリスト

**全体**
- [ ] public／protected のすべてのメンバーにコメントがある（override とインターフェースの実装は `<inheritdoc/>`）
- [ ] `<summary>` は1〜2文で、それだけ読めば意味が分かる
- [ ] 名前を言い換えただけの文になっていない
- [ ] 型名は `<see cref>`、`null`／`true`／`false` は `<see langword>` で書いている
- [ ] 地の文に `<` `>` `&` をそのまま書いていない
- [ ] **すべての文が体言止めで、文の区切りだけに「。」があり、最後の文には「。」がない**
- [ ] `<summary>` はすべて 3 行形式（`/// <summary>` ／ 本文 ／ `/// </summary>`）

**メソッド**
- [ ] すべての引数に `<param>` がある（型名の繰り返しではなく、意味・単位・null の扱いを書いている）
- [ ] 戻り値が「見つからない場合・空の場合・Unset の場合」まで書いてある
- [ ] 自分で `throw` している例外が、すべて `<exception>` に**条件付きで**書いてある
- [ ] DB への書き込み・イベントの発行などの副作用を【副作用】に書いている

**このプロジェクト固有**
- [ ] 日時には JST かどうかを書いている
- [ ] 値オブジェクト・DTO・DB モデルに、null と Unset の扱い（【null契約】）を書いている
- [ ] cref のためだけに、依存の方向に反する参照を追加していない
- [ ] 定数に【値の根拠】を書いている
- [ ] 実装とコメントが食い違っていない（実装と同じコミットで直している）

---

## 10. ビルド設定（適用済み）

2026-09-18 に `src/` 配下の全プロジェクトに適用済み。コメントの間違い（壊れた XML、存在しない cref、引数名の間違い）は**ビルドエラー**になる。

```xml
<!-- src/Directory.Build.props -->
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <!-- コメントの間違いはエラーにする -->
  <WarningsAsErrors>$(WarningsAsErrors);CS1570;CS1572;CS1573;CS1574;CS1734</WarningsAsErrors>
  <!-- コメントがないこと（CS1591）は、当面は警告として残す -->
</PropertyGroup>
```

| 警告 | 内容 |
|---|---|
| CS1570 | XML として壊れている |
| CS1572 | 存在しない引数の `<param>` がある |
| CS1573 | 一部の引数にしか `<param>` がない |
| CS1574 | `cref` の参照先が見つからない |
| CS1591 | public なメンバーにコメントがない |
| CS1734 | `<paramref>` の引数名が間違っている |

> **注意**: `Directory.Build.props` は最も近い 1 つしか自動で読み込まれない。そのため `src/Contexts` と `src/Presentation` の `Directory.Build.props` は、先頭の `<Import>` で `src/Directory.Build.props` を取り込んでいる。新しく `src/` 配下に `Directory.Build.props` を作る場合も、同じ `<Import>` を入れること。
>
> CS1591（コメントなし）は警告のまま。新しい public メンバーにコメントを書き忘れると、ビルドの警告に出る。

> `.editorconfig` の `resharper_xmldoc_*` の設定（子要素をインデントしない、`summary`／`remarks`／`para` などの前で改行する）は、このガイドのテンプレートと合っている。

---

## 更新履歴

| 日付 | 内容 |
|---|---|
| 2026-09-26 | 日時の DB 変換を Infrastructure に移す方針（値オブジェクトから `DateTime` を排除）に合わせて、§6-3 の `UpdatedAt` の実例（【時刻】）、§6-14 の Try 系の例（`TryFrom(LocalDateTime?)`）、§4-2 の表の例を修正 |
| 2026-09-18 | `<summary>` は必ず 3 行形式とするルールを追加（§3-1・§3-2・§4-1・チェックリスト）。すべての例を 3 行形式に変更 |
| 2026-09-18 | §8 の実例に修正済みの旨を追記。§10 のビルド設定を適用済みに変更（子の Directory.Build.props からの Import を追記） |
| 2026-09-18 | 句点のルールを「文の区切りには「。」、最後の文には付けない」に決定。すべての例から最後の「。」を削除 |
| 2026-09-18 | 文体を体言止めに統一。§4 を改訂し、すべてのテンプレートと実例を体言止めに変更。§8-6（文体の誤り例）とチェックリストの項目を追加 |
| 2026-09-18 | 初版。既存コード（186 ファイル）の調査に基づいてルールとテンプレートを作成 |
