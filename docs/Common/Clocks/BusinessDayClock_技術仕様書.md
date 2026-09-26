# 技術仕様書 — BusinessDayClock

**プロジェクト:** SupportAdvance  
**レイヤ:** Common  
**種別:** クラス設計原則  
**版:** 1.1 / 2026-09-18

---

## 1. 位置づけ

`BusinessDayClock` は SupportAdvance において、**クロックの ON／OFF のたびに業務日を 1 日ずつ進める、検証用の** `IClock` 実装クラスである。

日付処理を含む機能（日次の締め、期限切れ判定、在籍期間の判定など）を検証するとき、実際の暦日を待たずに「1 日目 → 2 日目 → 3 日目…」と業務日を進めながら操作を繰り返したい。そのための時計である。

本クラスの目的は以下に集約される。

- ON したときに業務日が始まり、OFF したときにその業務日が終わる、という「1 セッション＝1 業務日」の時計を提供する
- 次の ON では、前回 OFF した業務日の翌日から始める
- 時刻の部分は実際の現在時刻（JST）を使い、ON 中に 0 時を越えた場合は業務日も翌日に進める
- アプリケーションの起動・終了による自動の ON／OFF と、画面のボタンなどによる手動の ON／OFF の両方に対応する

> **📌 原則**  
> - `OffsetClock`（指定日時から実時間と同じ速さで進む時計）とは**独立したクラス**とし、継承も委譲もしない  
> - 検証（Development／Test 環境）専用。本番環境では `SystemClock` を使う  
> - 日時の取得はすべて `IClock` 経由で行う（CLAUDE.md「LocalDateTime 使用規則」）。`DateTime.Now` の直接使用は、Clock 実装の内部である本クラスに限り許可される

### 1.1 動作の概要

開始日を `2026-04-01` とした場合の例。

| # | 操作・出来事 | 実際の日時（JST） | `JstNow` が返す値 | 状態ファイルの「最後に終了した業務日」 |
|---|---|---|---|---|
| 0 | 一番最初の ON の前 | 9/18 09:50 | **2026-04-01**（基点日）09:50 | （なし） |
| 1 | 初回の ON | 9/18 10:00 | **2026-04-01** 10:00 | （なし） |
| 2 | ON のまま 0 時を越える | 9/19 00:01 | **2026-04-02** 00:01 | （なし） |
| 3 | OFF | 9/19 00:30 | 2026-04-02 00:30 | 2026-04-02 |
| 4 | OFF 中に参照 | 9/19 08:00 | 2026-04-02 08:00（日付は進まない） | 2026-04-02 |
| 5 | 2 回目の ON | 9/19 09:00 | **2026-04-03** 09:00 | 2026-04-02 |
| 6 | OFF → ON を繰り返す | … | 2026-04-04、04-05 …と 1 日ずつ進む | … |

### 1.2 状態の定義

| 状態 | 意味 | `JstNow` の日付部分 |
|---|---|---|
| 未 ON（初回） | 一番最初の ON の前（最後に終了した業務日がない） | **基点日**（設定の `BusinessDayStartDate`） |
| 未 ON（再起動後） | アプリを再起動し、まだ ON していない（最後に終了した業務日がある） | 最後に終了した業務日（OFF と同じ） |
| ON | 業務日のセッション中 | ON したときの業務日 ＋ ON 中に越えた 0 時の回数 |
| OFF | 業務日のセッションが終了した | 最後に終了した業務日（固定） |

いずれの状態でも、時刻の部分は実際の現在時刻（JST）とする。

> **📌 考え方**  
> 年月日は基点日から始まり、ON（前回の翌日から開始）と 0 時越えによってのみ増えていく。ON していない間は日付を進めない。時刻は常に実際の現在時刻とする。

---

## 2. メンバー仕様

### 2.1 JstNow

現在の業務日時を返す（`IClock` の実装）。

| 項目 | 内容 |
|------|------|
| シグネチャ | `LocalDateTime JstNow { get; }` |
| 戻り値 | 業務日（§1.2 の状態ごとの日付）＋実際の現在時刻（JST）。`DateTime.Kind` は `Unspecified` |
| 例外 | 例外を投げない |
| 用途 | Domain／Application 層の日時取得（`IClock` 経由） |

**設計判断**

- OFF 中や未 ON でも例外にしない。ログ出力（`FrameworkLoggingAdapter`）も `IClock` を使うため、起動処理中や終了処理中に例外が出るとアプリケーションが停止するため
- 実際の日時が過去に戻った（OS の日付変更など）場合、越えた 0 時の回数は 0 として扱い、業務日を戻さない

---

### 2.2 JstToday

現在の業務日の 0 時を返す（`IClock` の実装）。

| 項目 | 内容 |
|------|------|
| シグネチャ | `LocalDateTime JstToday { get; }` |
| 戻り値 | `JstNow` の日付部分 ＋ `00:00:00` |
| 例外 | 例外を投げない |
| 用途 | 日付単位の判定 |

---

### 2.3 TurnOn

業務日のセッションを開始する。

| 項目 | 内容 |
|------|------|
| シグネチャ | `void TurnOn()` |
| 戻り値 | なし |
| 例外 | 状態ファイルの書き込みに失敗した場合 `IOException` など（§5.3） |
| 用途 | アプリケーション起動時（自動）、画面のボタン（手動） |

**設計判断**

- 開始する業務日は「最後に終了した業務日の翌日」。最後に終了した業務日がない場合は、設定の開始日
- すでに ON の場合は何もしない（べき等）。自動 ON と手動 ON が重なっても業務日が進みすぎないようにするため

---

### 2.4 TurnOff

業務日のセッションを終了する。

| 項目 | 内容 |
|------|------|
| シグネチャ | `void TurnOff()` |
| 戻り値 | なし |
| 例外 | 状態ファイルの書き込みに失敗した場合 `IOException` など（§5.3） |
| 用途 | アプリケーション終了時（自動）、画面のボタン（手動） |

**設計判断**

- OFF した時点の業務日（0 時越えを反映した日）を「最後に終了した業務日」として状態ファイルに保存する
- すでに OFF（または未 ON）の場合は何もしない（べき等）

---

### 2.5 Reset

業務日の進行を最初からやり直す。

| 項目 | 内容 |
|------|------|
| シグネチャ | `void Reset()` |
| 戻り値 | なし |
| 例外 | 状態ファイルの削除に失敗した場合 `IOException` など |
| 用途 | 検証をはじめからやり直すとき（画面のボタン） |

**設計判断**

- 「最後に終了した業務日」を消し、状態を「未 ON」に戻す。次の ON は設定の開始日から始まる
- ON 中に呼ばれた場合も、セッションを破棄して未 ON に戻す（OFF の保存はしない）

---

### 2.6 IsOn

ON 中かどうかを示す。

| 項目 | 内容 |
|------|------|
| シグネチャ | `bool IsOn { get; }` |
| 戻り値 | ON 中の場合は `true`。未 ON・OFF の場合は `false` |
| 例外 | 例外を投げない |
| 用途 | 画面のボタンの有効／無効の切り替え |

---

### 2.7 CurrentBusinessDate

現在の業務日（日付のみ）を示す。

| 項目 | 内容 |
|------|------|
| シグネチャ | `DateOnly CurrentBusinessDate { get; }` |
| 戻り値 | `JstNow` の日付部分と同じ値 |
| 例外 | 例外を投げない |
| 用途 | 画面への「現在の業務日」の表示 |

---

### 2.8 IBusinessDayClockControl（操作用インターフェース）

`TurnOn`／`TurnOff`／`Reset`／`IsOn`／`CurrentBusinessDate` を公開する、操作用のインターフェース。

| 項目 | 内容 |
|------|------|
| 定義場所 | `SupportAdvance.Common.Clocks` |
| 実装 | `BusinessDayClock` |
| 用途 | Composition Root（自動の ON／OFF）と ViewModel（ボタン）からの操作 |

**設計判断**

- 利用側が `IClock` を `BusinessDayClock` にキャストしなくて済むよう、操作用の口を分ける（インターフェース分離）
- `ClockType` が `BusinessDay` 以外のときは DI に登録しない。利用側は「登録されていれば操作ボタンを表示する」という形で扱う

---

## 3. 設計制約・禁止事項

### 3.1 レイヤ制約（CLAUDE.md 準拠）

| ❌ 禁止（知ってはならないもの） | ✅ 許可（知ってよいもの） |
|-------------------------------|--------------------------|
| ❌ SharedKernel・Domain・Application・Infrastructure・Presentation の型 | ✅ `IClock`、`LocalDateTime`、`IClockSettings`（同じ Common） |
| ❌ 外部 NuGet パッケージ（Common は依存ゼロ） | ✅ .NET 標準ライブラリ（`System.IO`、`System.Text.Json`） |
| ❌ `IAppLogging` などのロガー（Crosscutting は Common の外側） | ✅ `DateTime.Now`（Clock 実装の内部に限る） |

### 3.2 実装上の禁止事項

- **`OffsetClock` を継承・流用しない。** 利用者の要望として、独立したクロックとするため。また、`OffsetClock` の変更が本クラスの動作に影響しないようにするため
- **本番環境で使わない。** 業務日が実際の暦日と一致しなくなり、監査列（`created_at` など）に実在しない日付が記録されるため。`appsettings.Production.json` では `ClockType` を `System` にする
- **`JstNow`／`JstToday` で例外を投げない。** 理由は §2.1
- **複数のプロセスで同じ状態ファイルを共有しない。** 後から書き込んだプロセスの状態で上書きされるため（排他制御は行わない）

---

## 4. 実装ガイドライン

### 4.1 実装義務

- `IClock`、`IBusinessDayClockControl`、`IDisposable` を実装する
- すべての状態の読み書きを 1 つのロックで保護する（ボタン操作とログ出力が別スレッドから同時に呼ばれるため）
- 状態ファイルは、一時ファイルに書いてから置き換える方法で書き込む（書き込み途中の異常終了で壊れないようにするため）
- 状態ファイルが読めない（存在しない・壊れている・開始日が設定と異なる）場合は、エラーにせず「最後に終了した業務日なし」として扱う

### 4.2 最小限の実装例（設計意図の説明）

```csharp
// 設計例：業務日の計算（ON 中）
// 業務日 ＝ ON したときの業務日 ＋ ON してから越えた 0 時の回数（負の場合は 0）
var crossedDays = Math.Max(0, DateOnly.FromDateTime(systemNow).DayNumber - onSystemDate.DayNumber);
var businessDate = sessionStartDate.AddDays(crossedDays);
return new LocalDateTime(businessDate.ToDateTime(TimeOnly.FromDateTime(systemNow)));
```

```csharp
// 設計例：Composition Root での自動の ON／OFF
var control = host.Services.GetService<IBusinessDayClockControl>();
control?.TurnOn();                   // 起動時（未登録＝BusinessDay 以外なら何もしない）
// ...
control?.TurnOff();                  // 終了時（App.OnExit など）
```

---

## 5. 設定と状態ファイル

### 5.1 設定（appsettings.*.json の `AppSettings:ClockSettings`）

`ClockSettings` に次の 2 項目を追加する。既存の項目（`StartTime`、`OffsetDateTime` など）は使わない（`OffsetClock`・`TickingClock` と独立させるため）。

| 項目 | 型 | 必須 | 既定値 | 内容 |
|---|---|---|---|---|
| `ClockType` | string | ✅ | — | `"BusinessDay"` を指定 |
| `BusinessDayStartDate` | string（`yyyy-MM-dd`） | ✅ | — | 初回の ON で始まる業務日 |
| `BusinessDayStateFilePath` | string | — | `%LOCALAPPDATA%\SupportAdvance\BusinessDayClock.json` | 状態ファイルのパス |

```json
"ClockSettings": {
  "ClockType": "BusinessDay",
  "BusinessDayStartDate": "2026-04-01"
}
```

### 5.2 状態ファイル

| 項目 | 内容 |
|---|---|
| 形式 | JSON（UTF-8） |
| 保存する値 | 設定の開始日、最後に終了した業務日、ON 中かどうか、ON 中の場合はその業務日 |
| 書き込むタイミング | `TurnOn`、`TurnOff`、`Reset`（削除） |
| やり直し方 | `Reset` を呼ぶ、状態ファイルを削除する、または `BusinessDayStartDate` を変更する（保存された開始日と設定が異なる場合は、状態を破棄して最初から始める） |

### 5.3 異常時の扱い

| 状況 | 扱い |
|---|---|
| 状態ファイルが存在しない | 最後に終了した業務日なし（開始日から始める） |
| 状態ファイルが壊れている | 同上。壊れたファイルは次の書き込みで上書き |
| 状態ファイルの開始日が設定と異なる | 同上（設定の変更をやり直しの合図とみなす） |
| ON のまま異常終了した（OFF が保存されていない） | 次の起動時、保存されていた「ON 中の業務日」を最後に終了した業務日とみなす。ON 中に 0 時を越えていた分は反映されない |
| 状態ファイルの書き込みに失敗した | `TurnOn`／`TurnOff` から例外を送出する（検証用のため、状態の不整合を黙って続けない） |

---

## 6. 関連ドキュメント

- [BusinessDayClock 詳細設計書](BusinessDayClock_詳細設計書.md)
- [LocalDateTime_タイムゾーン_ガイド.md](../../Assistance/Guides/LocalDateTime_タイムゾーン_ガイド.md)
- [OffsetClock 単体テスト仕様書](OffsetClock_単体テスト仕様書.md)（独立した別のクロック）
- [ClockFactory 単体テスト仕様書](ClockFactory_単体テスト仕様書.md)

---

## 7. 版管理

| 版 | 日付 | 変更内容 |
|----|------|---------|
| 1.0 | 2026-09-18 | 初版作成 |
| 1.1 | 2026-09-18 | 一番最初の ON の前は基点日を返すことを明記。未 ON を「初回」と「再起動後」に分けて定義（§1.1・§1.2） |
