# 詳細設計書 — BusinessDayClock

**プロジェクト:** SupportAdvance  
**レイヤ:** Common  
**種別:** クラス詳細設計  
**依拠技術仕様書:** BusinessDayClock 技術仕様書 v1.1  
**版:** 1.3 / 2026-09-18

---

## 1. クラス概要

### 1.1 クラス定義

| 項目 | 内容 |
|------|------|
| クラス名 | `BusinessDayClock` |
| 種別 | `sealed class` |
| 名前空間 | `SupportAdvance.Common.Clocks` |
| 実装インターフェース | `IClock`、`IBusinessDayClockControl`、`IDisposable` |
| 継承元 | なし（`OffsetClock` とは独立） |
| 配置レイヤ | Common（`src/Common/Clocks/BusinessDayClock.cs`） |

同時に追加する型は次のとおり。

| 型 | 種別 | 配置 | 役割 |
|---|---|---|---|
| `IBusinessDayClockControl` | interface | `src/Common/Clocks/IBusinessDayClockControl.cs` | ON／OFF などの操作用の口 |
| `BusinessDayClockState` | internal sealed record | `src/Common/Clocks/BusinessDayClockState.cs` | 状態ファイルの内容 |
| `BusinessDayClockStateFile` | internal sealed class | `src/Common/Clocks/BusinessDayClockStateFile.cs` | 状態ファイルの読み書き |

### 1.2 責務

- 業務日のセッション（ON〜OFF）を管理する
- 業務日と実際の現在時刻から `JstNow`／`JstToday` を算出する
- 最後に終了した業務日を状態ファイルに保存し、次回の ON で翌日から始められるようにする
- 操作用の口（`IBusinessDayClockControl`）を提供する

### 1.3 協調クラス

```
ClockFactory  ──creates──▶  BusinessDayClock
  └─ ClockType = "BusinessDay" のとき生成

BusinessDayClock  ──uses──▶  TimeProvider（.NET 標準）
                                └─ GetUtcNow()：実際の現在時刻（テストでは偽の時刻に差し替え）
BusinessDayClock  ──uses──▶  BusinessDayClockStateFile
                                └─ Load() / Save(state) / Delete()

Infrastructure.DependencyInjection  ──registers──▶  IClock, IBusinessDayClockControl
App（WpfTrial）/ Program（WinTrial）  ──calls──▶  IBusinessDayClockControl.TurnOn / TurnOff
ViewModel（検証用ボタン）             ──calls──▶  IBusinessDayClockControl.TurnOn / TurnOff / Reset
```

---

## 2. プロパティ設計

### 2.1 `JstNow`

| 項目 | 内容 |
|------|------|
| 型 | `LocalDateTime` |
| アクセス修飾子 | `public` |
| セッタ | なし（読み取り専用） |
| デフォルト値 | なし |

**設計判断**

- 値は `ComputeBusinessDate(systemNow) ＋ systemNow の時刻部分` で毎回算出する（§3.6）。キャッシュしない（実時間で進むため）
- ロックの中で状態を読み取る

### 2.2 `JstToday`

| 項目 | 内容 |
|------|------|
| 型 | `LocalDateTime` |
| アクセス修飾子 | `public` |
| セッタ | なし（読み取り専用） |
| デフォルト値 | なし |

**設計判断**

- `CurrentBusinessDate` の 0 時。`JstNow` を取得してから日付部分を切り出すのではなく、同じ `systemNow` から業務日を 1 回だけ算出する（0 時ちょうどに呼ばれた場合のずれを防ぐため）

### 2.3 `IsOn`

| 項目 | 内容 |
|------|------|
| 型 | `bool` |
| アクセス修飾子 | `public` |
| セッタ | なし（読み取り専用） |
| デフォルト値 | `false`（未 ON） |

### 2.4 `CurrentBusinessDate`

| 項目 | 内容 |
|------|------|
| 型 | `DateOnly` |
| アクセス修飾子 | `public` |
| セッタ | なし（読み取り専用） |
| デフォルト値 | なし |

### 2.5 内部フィールド

| フィールド | 型 | 内容 |
|---|---|---|
| `_lock` | `Lock` | すべての状態の読み書きを保護する |
| `_timeProvider` | `TimeProvider` | 実際の現在時刻の取得元 |
| `_stateFile` | `BusinessDayClockStateFile` | 状態ファイルの読み書き |
| `_startDate` | `DateOnly` | 設定の開始日 |
| `_lastEndedDate` | `DateOnly?` | 最後に終了した業務日。なしの場合は `null` |
| `_isOn` | `bool` | ON 中かどうか |
| `_sessionDate` | `DateOnly` | ON したときの業務日（ON 中のみ有効） |
| `_onSystemDate` | `DateOnly` | ON したときの実際の日付（JST。ON 中のみ有効） |
| `_disposed` | `bool` | 破棄済みかどうか |

**設計判断**

- `_lastEndedDate` は `DateOnly?` とする。Common 層は null 厳格性原則（Domain 層のルール）の対象外で、「なし」を表すのに最も素直なため。外部には公開しない

---

## 3. メソッド設計

### 3.1 `BusinessDayClock()` — コンストラクタ

| 項目 | 内容 |
|------|------|
| シグネチャ | `BusinessDayClock(DateOnly startDate, string? stateFilePath = null, TimeProvider? timeProvider = null)` |

**処理フロー**

```
1. _timeProvider ← timeProvider ?? TimeProvider.System
2. _stateFile ← new BusinessDayClockStateFile(stateFilePath ?? 既定のパス)
3. _startDate ← startDate
4. state ← _stateFile.Load()
     読めない（なし・壊れている）      → state = null
     state.StartDate != startDate       → state = null（設定の変更をやり直しの合図とみなす）
5. _lastEndedDate を決める
     state == null                      → null
     state.IsOn == true（異常終了の跡） → state.SessionDate
     それ以外                           → state.LastEndedDate
6. _isOn ← false（常に未 ON で開始する）
```

**設計判断**

- コンストラクタでは ON しない。ON のきっかけ（起動時の自動 ON）は Composition Root が決める（§6.3）
- 既定のパスは `Path.Combine(Environment.GetFolderPath(SpecialFolder.LocalApplicationData), "SupportAdvance", "BusinessDayClock.json")`
- `timeProvider` を引数にするのは単体テストのため（`FakeTimeProvider` で 0 時越えを再現する）

---

### 3.2 `TurnOn()` — `IBusinessDayClockControl` 実装

| 項目 | 内容 |
|------|------|
| シグネチャ | `void TurnOn()` |

**処理フロー**

```
lock:
  _isOn == true                → 何もしない
  それ以外:
    _sessionDate  ← _lastEndedDate?.AddDays(1) ?? _startDate
    _onSystemDate ← 実際の今日（JST）
    _isOn ← true
    _stateFile.Save(StartDate=_startDate, LastEndedDate=_lastEndedDate, IsOn=true, SessionDate=_sessionDate)
```

**設計判断**

- ON 時にも保存する。OFF せずに異常終了した場合に、次回「この業務日は終わった」とみなすため（技術仕様書 §5.3）

---

### 3.3 `TurnOff()` — `IBusinessDayClockControl` 実装

| 項目 | 内容 |
|------|------|
| シグネチャ | `void TurnOff()` |

**処理フロー**

```
lock:
  _isOn == false               → 何もしない
  それ以外:
    _lastEndedDate ← ComputeBusinessDate(実際の今)   ※ 0 時越えを反映した業務日
    _isOn ← false
    _stateFile.Save(StartDate=_startDate, LastEndedDate=_lastEndedDate, IsOn=false, SessionDate=null)
```

---

### 3.4 `Reset()` — `IBusinessDayClockControl` 実装

| 項目 | 内容 |
|------|------|
| シグネチャ | `void Reset()` |

**処理フロー**

```
lock:
  _lastEndedDate ← null
  _isOn ← false
  _stateFile.Delete()          ※ ファイルがない場合は何もしない
```

---

### 3.5 `Dispose()` — `IDisposable` 実装

| 項目 | 内容 |
|------|------|
| シグネチャ | `void Dispose()` |

**処理フロー**

```
_disposed == true → 何もしない
それ以外          → _disposed ← true
```

**設計判断**

- **Dispose で OFF しない。** DI コンテナーは、インスタンスとして登録したシングルトン（`AddSingleton<IClock>(instance)`）を破棄しないため、Dispose に OFF を頼ると呼ばれないことがある。OFF は Composition Root が明示的に呼ぶ（§6.3）
- 破棄後も `JstNow` は値を返す（終了処理中のログ出力のため）

---

### 3.6 `ComputeBusinessDate()` — private

| 項目 | 内容 |
|------|------|
| シグネチャ | `DateOnly ComputeBusinessDate(DateTime systemJstNow)` |

**処理フロー**

```
_isOn == true           → _sessionDate + max(0, systemJstNow の日付 - _onSystemDate) 日
_lastEndedDate != null  → _lastEndedDate          （OFF 中、または再起動後の未 ON：最後に終了した業務日で固定）
それ以外                → _startDate              （一番最初の ON の前：基点日）
```

時刻の部分は、どの場合も `systemJstNow` の時刻をそのまま使う。

**設計判断**

- ON 中に何日も放置した場合（例：金曜に ON して月曜に参照）は、越えた 0 時の回数だけ業務日が進む
- 実際の日付が ON 時より前に戻った場合は 0 日とし、業務日を戻さない

### 3.7 実際の現在時刻（JST）の取得 — private

```
TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), Tokyo Standard Time).DateTime
  → DateTime.SpecifyKind(…, DateTimeKind.Unspecified)
```

`SystemClock` と同じ方法で JST に変換する。

---

## 4. 関連クラスへの設計指示

### 4.1 `IBusinessDayClockControl`

```csharp
public interface IBusinessDayClockControl
{
    bool IsOn { get; }
    DateOnly CurrentBusinessDate { get; }
    void TurnOn();
    void TurnOff();
    void Reset();
}
```

### 4.2 `BusinessDayClockStateFile`（internal）

| メソッド | 内容 |
|---|---|
| `BusinessDayClockState? Load()` | ファイルがない・JSON として読めない場合は `null`（例外を外に出さない） |
| `void Save(BusinessDayClockState state)` | フォルダーがなければ作成し、`{パス}.tmp` に書いてから `File.Move(tmp, path, overwrite: true)` で置き換える |
| `void Delete()` | ファイルがあれば削除 |

状態ファイルの形式（`System.Text.Json`、日付は `yyyy-MM-dd`）:

```json
{
  "startDate": "2026-04-01",
  "lastEndedDate": "2026-04-02",
  "isOn": false,
  "sessionDate": null
}
```

### 4.3 `IClockSettings` ／ `ClockSettings` への追加

| プロパティ | 型 | 既定値 |
|---|---|---|
| `BusinessDayStartDate` | `string?` | `null` |
| `BusinessDayStateFilePath` | `string?` | `null`（既定のパスを使用） |

### 4.4 `ClockFactory` への追加

```
settings.ClockType（大文字化）
  "BUSINESSDAY" → CreateBusinessDayClock(settings)

CreateBusinessDayClock:
  BusinessDayStartDate が未指定・空       → ArgumentException
  DateOnly.ParseExact(値, "yyyy-MM-dd")   → 形式が不正なら FormatException
  new BusinessDayClock(startDate, settings.BusinessDayStateFilePath)
```

### 4.5 禁止事項

| 禁止事項 | 理由 |
|----------|------|
| `OffsetClock` の継承・流用 | 独立したクロックとする方針（技術仕様書 §3.2） |
| `JstNow`／`JstToday` での例外送出 | ログ出力が止まり、アプリケーションが停止するため |
| `DateTime.Now` の直接使用 | テストで時刻を差し替えられなくなるため。実際の時刻は `TimeProvider` から取得する |
| Dispose での OFF | §3.5 のとおり呼ばれない場合があるため |

---

## 5. レイヤ制約確認

| 確認項目 | 判定 | 備考 |
|----------|------|------|
| Application 層への依存 | ❌ 禁止 | 依存しない |
| Infrastructure 層への依存 | ❌ 禁止 | 依存しない。DI 登録は Infrastructure 側が本クラスを参照する（正しい方向） |
| ロガー（`ILogger` 等）への依存 | ❌ 禁止 | Common は Crosscutting より内側のため。状態ファイルの異常は戻り値（`null`）で扱う |
| `DateTime.Now` / `UtcNow` の直接使用 | ❌ 禁止 | Clock 実装の内部では許可されているが、テストのため `TimeProvider` を使う |
| 外部 NuGet パッケージ | ❌ 禁止 | `TimeProvider`・`System.Text.Json`・`System.IO` はすべて .NET 標準 |
| Presentation からの利用 | ✅ 許可 | Presentation → Common は許可された依存（`IBusinessDayClockControl` を使う） |

---

## 6. 状態遷移と組み込み

### 6.1 状態遷移

```
            TurnOn（開始日から）
  [未 ON] ────────────────────▶ [ON] ◀──────────────┐
     ▲                           │ │                 │ TurnOn（最後に終了した業務日の翌日から）
     │ Reset                     │ │ 0 時越え：業務日＋1 │
     │                           │ └───┘               │
     │                    TurnOff│                     │
     │                           ▼                     │
     └──────────────────────── [OFF] ─────────────────┘
                 Reset
```

- `未 ON` の `JstNow` の日付は、一番最初の ON の前なら基点日、再起動後なら最後に終了した業務日
- `ON` での `TurnOn`、`未 ON`／`OFF` での `TurnOff` は何もしない（べき等）
- `Reset` はどの状態からでも `未 ON` に戻る

### 6.2 DI 登録（`src/Infrastructure/DependencyInjection.cs`）

```
var clockInstance = ClockFactory.CreateClock(clockSettings);
services.AddSingleton<IClock>(clockInstance);
if (clockInstance is IBusinessDayClockControl control)
    services.AddSingleton(control);           ← 追加
```

### 6.3 Composition Root での自動の ON／OFF

| アプリ | ON（起動時） | OFF（終了時） |
|---|---|---|
| WpfTrial | `App.OnStartup` の `_host.Start()` の直後 | `App.OnExit` の `_host.StopAsync()` の前 |
| WinTrial | `Program.cs` のホスト開始の直後 | `Application.Run(...)` から戻った後 |

```
control = host.Services.GetService<IBusinessDayClockControl>()   ※ BusinessDay 以外なら null
control?.TurnOn()
...
control?.TurnOff()
```

> **注意（WinTrial）**: WinForms の `System.ServiceExtensions.GetService<T>` と名前が衝突するため、`ServiceProviderServiceExtensions.GetService<IBusinessDayClockControl>(host.Services)` と静的メソッドとして呼び出す（既存の `GetRequiredService` の呼び出しと同じ書き方）。

### 6.4 手動の ON／OFF（検証用のボタン）

WpfTrial と WinTrial の両方に画面を作る。ボタンの処理は `Presentation.Shared` の共通 ViewModel に 1 つだけ実装し、各アプリは画面（View）だけを持つ。

**共通 ViewModel：`BusinessDayClockViewModel`**

| 項目 | 内容 |
|---|---|
| 配置 | `src/Presentation/Shared/ViewModels/BusinessDayClockViewModel.cs` |
| 基底クラス | `ObservableObject`（CommunityToolkit.Mvvm。`Shared.csproj` に 8.4.2 を追加。WPF／WinForms のどちらにも依存しない） |
| コンストラクター | `BusinessDayClockViewModel(IBusinessDayClockControl? control = null)`。未登録の場合は既定値の `null` が渡される |
| `IsAvailable` | `control` が `null` でない場合は `true`。`false` の場合、各画面はパネルごと非表示 |
| `IsOn` | 現在 ON 中かどうか |
| `CurrentBusinessDateText` | 現在の業務日（`yyyy/MM/dd`） |
| `StatusText` | 「ON 中」／「OFF 中」 |
| `TurnOnCommand` | `TurnOn` を呼ぶ。実行可能なのは `IsAvailable` かつ OFF 中 |
| `TurnOffCommand` | `TurnOff` を呼ぶ。実行可能なのは `IsAvailable` かつ ON 中 |
| `ResetCommand` | `Reset` を呼ぶ。実行可能なのは `IsAvailable` |
| `Refresh()` | 表示を再取得する。各コマンドの実行後に呼ぶ |

- 業務日は ON 中に 0 時を越えると変わるが、表示の自動更新（タイマー）は行わない。コマンド実行時と画面の表示時に再取得する
- DI 登録：各アプリの `AddWpfTrialModules`／`AddWinTrialModules` で Scoped 登録する

**各アプリの画面**

| アプリ | 画面 | 組み込み方 |
|---|---|---|
| WpfTrial | `MainWindow` | `MainWindowViewModel` が `BusinessDayClockViewModel` を受け取り、`BusinessDayClock` プロパティで公開する。XAML の最下段に「業務日: yyyy/MM/dd（ON 中）」の表示と 3 つのボタンを追加し、`IsAvailable` が `false` の場合は非表示 |
| WinTrial | `Form1` | `Form1ViewModel` が同様に `BusinessDayClock` プロパティで公開する。`Form1` のコンストラクターで、フォーム下部にラベルと 3 つのボタンを配置し、`Button.Command` とデータバインドでつなぐ（デザイナーのファイルは変更しない） |

---

## 7. 改版履歴

| 版 | 日付 | 作成者 | 変更内容 |
|----|------|--------|---------|
| 1.0 | 2026-09-18 | Claude | 初版作成 |
| 1.1 | 2026-09-18 | Claude | 未 ON 時の業務日の説明を技術仕様書 v1.1 に合わせて補足（§3.6・§6.1） |
| 1.2 | 2026-09-18 | Claude | §6.4 を確定。共通 ViewModel を Presentation.Shared に置き、WpfTrial と WinTrial の両方に画面を作る |
| 1.3 | 2026-09-18 | Claude | 実装に合わせて §6.3 に WinTrial での GetService の呼び出し方の注意を追記 |
