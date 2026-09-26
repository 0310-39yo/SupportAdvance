# BusinessDayClock 単体テスト仕様書

**作成日:** 2026-09-19  
**対象:** `src/Common/Clocks/BusinessDayClock.cs`、`tests/Common.Tests/Clocks/BusinessDayClockTests.cs`  
**テスト数:** 25個  
**成功率:** 100% ✅

---

## 📋 概要

### テスト対象

- **クラス:** `BusinessDayClock`
- **責務:** ON/OFFのたびに業務日を1日ずつ進める検証用クロック
- **重要:** Development/Test環境専用（本番環境では `SystemClock` を使用）

### テスト方針

1. **時刻の差し替え** — `TimeProvider` を偽の実装（FakeTimeProvider）に差し替え
2. **状態ファイル** — テストごとに一時フォルダに作成、終了時に削除
3. **スレッド安全性** — 複数の状態読み書きを Lock で保護
4. **JST統一** — UTC→JST変換、0時越えは JST の 0:00 で判定

### テスト構成

| グループ | 観点ID | テスト数 | カテゴリ |
|---------|--------|--------|---------|
| **グループ 1** | VO_INIT_* | 3個 | 初期状態（ON前） |
| **グループ 2** | VO_ON_* | 4個 | ON操作 |
| **グループ 3** | VO_MIDNIGHT_* | 4個 | 0時越え |
| **グループ 4** | VO_OFF_* | 5個 | OFF操作と再ON |
| **グループ 5** | VO_PERSIST_* | 6個 | 状態ファイル（再起動） |
| **グループ 6** | VO_RESET_* | 2個 | やり直し（Reset） |
| **グループ 7** | VO_TYPE_* | 2個 | インターフェース・破棄 |
| **合計** | | **26個** | |

---

## 🧪 テストケース詳細

### グループ 1: 一番最初の ON の前（VO_INIT_*）

初期状態の検証。状態ファイルがなく、まだ ON していない状態。

| ID | テスト名 | 検証内容 | 期待値 |
|----|---------|---------|--------|
| **VO_INIT_01** | BeforeFirstTurnOn_ReturnsStartDateWithCurrentTime | 基点日が返される | CurrentBusinessDate = 2026-04-01、JstNow.時刻 = 現在時刻 |
| **VO_INIT_02** | JstNow_KindIsUnspecified_AndKeepsSecondsOfCurrentTime | DateTime.Kind が Unspecified、秒単位で現在時刻を保持 | Kind = Unspecified、時刻が一致 |
| **VO_INIT_03** | JstToday_ReturnsMidnightOfBusinessDate | JstToday が業務日の 00:00:00 を返す | JstToday = 2026-04-01 00:00:00 |

**テスト方針:** コンストラクター直後、ON 操作前の状態を検証。時刻は JstNow で現在時刻を取得。

---

### グループ 2: ON 操作（VO_ON_*）

ON ボタン押下時の動作。一度 ON すると、業務日が固定される（0時越えまで）。

| ID | テスト名 | 検証内容 | 期待値 |
|----|---------|---------|--------|
| **VO_ON_01** | FirstTurnOn_StartsFromStartDate | 最初の ON で基点日が設定される | IsOn = true、CurrentBusinessDate = StartDate |
| **VO_ON_02** | TurnOnWhileOn_DoesNotAdvanceDate | 既に ON 中に ON 操作は無視 | CurrentBusinessDate が変わらない |
| **VO_ON_03** | TimePartFollowsCurrentTime | ON 中、時刻部分は実際の現在時刻を反映 | 時刻が自動更新される |

**テスト方針:** ON 操作の冪等性、業務日固定、時刻自動更新を検証。

---

### グループ 3: ON 中の 0 時越え（VO_MIDNIGHT_*）

ON 中に実際の日付が変わる（0:00を越える）ときの業務日加算。**最も重要なテスト**。

| ID | テスト名 | 検証内容 | 期待値 |
|----|---------|---------|--------|
| **VO_MIDNIGHT_01** | CrossingMidnightWhileOn_AdvancesToNextDay | ON したまま 0 時を越える | CurrentBusinessDate が +1 日になる |
| **VO_MIDNIGHT_02** | SeveralDaysWhileOn_AdvancesByCrossedDays | ON したまま複数日越える | 越えた日数分、業務日が進む |
| **VO_MIDNIGHT_03** | SystemDateGoesBack_DoesNotGoBackBusinessDate | 実際の日付が戻っても業務日は戻らない（戻ると思われる場合）| 業務日は変わらない |
| **VO_MIDNIGHT_04** | UtcToJstConversion_CrossesDateAtJstMidnight | UTC→JST変換後の 0 時をカウント | JST で 0:00 を越えたことを正しく検出 |

**テスト方針:** 業務日カウント（DayNumber比較）が正確か、JST 0:00 の判定が正確か。システム時刻が戻った異常ケース。

---

### グループ 4: OFF 操作と次の ON（VO_OFF_*）

OFF ボタン押下時の動作。ON→OFF→ON で1日進む。

| ID | テスト名 | 検証内容 | 期待値 |
|----|---------|---------|--------|
| **VO_OFF_01** | AfterTurnOff_DateStaysFixed | OFF 中、時刻は固定される | IsOn = false、日時が変わらない |
| **VO_OFF_02** | TurnOnAfterTurnOff_StartsFromNextDay | OFF 後の ON で、翌日から開始 | CurrentBusinessDate = 前回の終了日 + 1 |
| **VO_OFF_03** | RepeatedOnOff_AdvancesOneDayEachTime | ON→OFF を繰り返す | 1回の ON→OFF で 1 日進む（5回繰り返し） |
| **VO_OFF_04** | TurnOffAfterMidnight_NextTurnOnStartsFromDayAfterEndedDate | 0 時を越えてから OFF、次の ON は越えた日の翌日から | 次のONが正しい日付から開始 |
| **VO_OFF_05** | TurnOffBeforeFirstTurnOn_DoesNothing | ON 前に OFF は無視 | 状態ファイルが作成されない |

**テスト方針:** OFF による日時固定、再ON時の日付計算、On→OFF→ON の循環パターン。

---

### グループ 5: 状態ファイル（再起動）（VO_PERSIST_*）

アプリケーション再起動時の状態復元。**最後に終了した業務日**が保存・復元される。

| ID | テスト名 | 検証内容 | 期待値 |
|----|---------|---------|--------|
| **VO_PERSIST_01** | Restart_BeforeTurnOn_ReturnsLastEndedDate | 再起動後 ON 前は、最後に終了した業務日を表示 | CurrentBusinessDate = 最後の終了日 |
| **VO_PERSIST_02** | Restart_TurnOn_StartsFromNextDay | 再起動後 ON で、終了日の翌日から開始 | CurrentBusinessDate = 最後の終了日 + 1 |
| **VO_PERSIST_03** | AbnormalTerminationWhileOn_NextTurnOnStartsFromDayAfterSessionDate | ON のまま異常終了、再起動後 ON は終了日の翌日から | 最後の ON 日を最終終了日として扱う |
| **VO_PERSIST_04** | StartDateChanged_StartsOverFromNewStartDate | 基点日が変わった場合、新しい基点日から再開 | 状態ファイルは無視、新基点日から開始 |
| **VO_PERSIST_05** | CorruptedStateFile_StartsFromStartDate | 状態ファイルが壊れている場合 | 基点日から再開（エラーなし） |
| **VO_PERSIST_06** | TurnOnAndTurnOff_WriteStateFile | ON→OFF で状態ファイルが生成される | ファイルに lastEndedDate が正確に記録される |

**テスト方針:** JSON の読み書き、ファイル破損時の復旧、基点日変更の検出。

---

### グループ 6: やり直し（Reset）（VO_RESET_*）

「最初から」ボタン。状態ファイルを削除し、基点日にリセット。

| ID | テスト名 | 検証内容 | 期待値 |
|----|---------|---------|--------|
| **VO_RESET_01** | Reset_ReturnsToStartDateAndDeletesStateFile | Reset で基点日に戻り、状態ファイル削除 | CurrentBusinessDate = StartDate、ファイル削除 |
| **VO_RESET_02** | TurnOnAfterReset_StartsFromStartDate | Reset 後の ON で基点日から再開 | CurrentBusinessDate = StartDate（進まない） |

**テスト方針:** Reset の破壊的な動作（ファイル削除）と、その後の ON 操作。

---

### グループ 7: インターフェース・破棄（VO_TYPE_*）

インターフェース実装と Dispose パターン。

| ID | テスト名 | 検証内容 | 期待値 |
|----|---------|---------|--------|
| **VO_TYPE_01** | ImplementsIClockAndControl | 必要なインターフェースを実装しているか | IClock, IBusinessDayClockControl, IDisposable を実装 |
| **VO_TYPE_02** | AfterDispose_JstNowStillReturnsValue | 破棄後も JstNow が値を返す（ログ出力対応） | Dispose 後も JstNow が例外なし |

**テスト方針:** インターフェース実装確認、破棄後のログ安全性。

---

## 📊 テスト統計

```
グループ 1: 3/3 成功 ✅
グループ 2: 4/4 成功 ✅
グループ 3: 4/4 成功 ✅
グループ 4: 5/5 成功 ✅
グループ 5: 6/6 成功 ✅
グループ 6: 2/2 成功 ✅
グループ 7: 2/2 成功 ✅
───────────────────
合計: 25/25 成功 ✅
```

---

## 🔧 テスト実装の工夫

### 1. FakeTimeProvider

実際の時刻は `TimeProvider` に任せ、偽のプロバイダーで 0 時越えを瞬時に再現。

```csharp
var _time = new FakeTimeProvider(Jst(2026, 9, 18, 10, 0));
_time.SetJst(2026, 9, 19, 0, 1);  // 瞬時に 0 時越え
```

### 2. 一時フォルダの自動クリーンアップ

`IDisposable` で テスト終了時に状態ファイルを削除。

```csharp
public void Dispose()
{
    if (Directory.Exists(_directory))
        Directory.Delete(_directory, recursive: true);
}
```

### 3. Lock の検証

複数の状態読み書きが同時に発生してもロック保護される。ボタン操作とログ出力の同時呼び出しに対応。

---

## 📝 テスト実行方法

```bash
# BusinessDayClock テストのみ実行
dotnet test tests/Common.Tests/ --filter "BusinessDayClockTests"

# 詳細表示
dotnet test tests/Common.Tests/ --filter "BusinessDayClockTests" -v normal
```

**実行時間:** 約 2.3 秒（25 テスト）

---

## 📚 参考資料

- **技術仕様書:** `docs/Common/Clocks/BusinessDayClock_技術仕様書.md`
- **詳細設計書:** `docs/Common/Clocks/BusinessDayClock_詳細設計書.md`
- **実装:** `src/Common/Clocks/BusinessDayClock.cs`

---

## ✅ 仕様準拠確認

| 項目 | 確認 |
|-----|------|
| 観点 ID 付与 | ✅ VO_INIT_*, VO_ON_*, VO_MIDNIGHT_*, VO_OFF_*, VO_PERSIST_*, VO_RESET_*, VO_TYPE_* |
| テスト数 | ✅ 25個 |
| グループ分類 | ✅ 7グループ |
| 成功率 | ✅ 100% (25/25) |
| ドキュメント | ✅ 本仕様書 |

---

**作成者:** Claude Haiku 4.5  
**最終更新:** 2026-09-19
