# WpfTrial MVVM／アーキテクチャ準拠 修正計画

作成日: 2026-09-26
対象: `src/Presentation/WpfTrial`、`src/Presentation/Shared`
根拠: 本セッションの準拠調査（SfNavigationDrawer 以外のコントロールの MVVM／MVVM Toolkit／クリーンアーキテクチャ検証）

## 方針

- 修正は Phase 単位でコミットを分け、各 Phase 完了時に `dotnet build` と既存テストが通ることを確認
- CLAUDE.md のルールにより、各 Phase の着手前にユーザー承認を得る
- ViewModel から `System.Windows.*` / Syncfusion 型への参照をゼロにする（単体テスト可能にする）

## Phase 1: 不具合・即時修正（低リスク）

| # | 内容 | 対象 |
|---|---|---|
| 1-1 | Ribbon の `SaveCommand` 未定義バインディングの解消。未実装ならボタン削除、または `[RelayCommand]` のプレースホルダを追加 | MainWindow.xaml / MainWindowViewModel.cs |
| 1-2 | `Form1ViewModel` の「従業員が見つかりません」時に `DepartmentNames` をクリア | Form1ViewModel.cs |
| 1-3 | `(AppSettings)appSettings` のダウンキャスト廃止。`IAppSettings` のまま使う（`ApplicationBuildType` が IAppSettings に無ければ追加を検討） | Form1ViewModel.cs / MainWindowViewModel.cs |
| 1-4 | `LoginWindowViewModel` の例外処理統一。予期しない例外は画面に `ex.Message` を出さず固定文言＋`LogError` | LoginWindowViewModel.cs |
| 1-5 | `ExecuteSampleUseCase` の `catch → throw;` 廃止、`Form1ViewModel.Dispose()` 削除、`EmptyView.xaml.cs` の不要 using 整理と XMLDoc 追加 | 各ファイル |

完了条件: ビルド成功、ログイン→メイン画面表示が従来どおり動作

## Phase 2: MainWindow の MVVM 化（最優先）

### 2-1 タブの ViewModel 化

- `TabItemData`（`UserControl` を保持）を廃止し、`TabItemViewModel : ObservableObject` を新設（`Header`、`ContentViewModel`）
- 画面種別は `EmptyTabContentViewModel` から開始し、画面ごとに `XxxViewModel` を追加していく構造にする
- View との対応付けは `App.xaml` または `MainWindow.xaml` の `DataTemplate DataType=...` で宣言（ViewModel-first）
- `MainWindowViewModel` から `using ...Views` と `new EmptyView()` を除去

### 2-2 ナビゲーションのコマンド化

- `NavigationItemSelected(object?)` を `OpenTab(string menuKey)` のような型付きメソッドにする（引数は `string` か専用の enum／record。Syncfusion の `NavigationItem` は受け取らない）
- `[RelayCommand] private void OpenTab(...)` として Command のみ公開（`public` メソッド併用をやめる）
- View 側の橋渡しは次のいずれか（Phase 開始時に実機で検証して選択）
  - (a) 添付ビヘイビア／`Behavior<SfNavigationDrawer>` で `ItemClicked` → Command（葉ノード判定はビヘイビア内に閉じる）
  - (b) 上記が困難な場合はコードビハインドを維持し、`viewModel.OpenTabCommand.Execute(item.Header)` 経由にする（最低限「Command 経由」を守る）
- `MainWindow.xaml.cs` の説明コメント（「RelayCommand はコンパイル時生成のため参照できない」）を訂正
- 2026-09-26 の SfNavigationDrawer 再検証計画の結論（`Items.Count` 判定）は維持

### 2-3 ObservableProperty の正規化

- `_selectedTab = x; OnPropertyChanged("SelectedTab")` を `SelectedTab = x` に変更
- `OpenTabs` を `[ObservableProperty]` から `public ObservableCollection<TabItemViewModel> OpenTabs { get; } = new();` へ
- `TabSelectionChanged` コマンドを廃止し、`partial void OnSelectedTabChanged(...)` でログ出力
- フィールド命名を `_camelCase` に統一（`LoginWindowViewModel`、`BusinessDayClockViewModel` の `loginId` 形式を修正）

### 2-4 重複・未使用コードの整理

- `MainWindowViewModel` の BizId 検索（`BizIdSearchInput`、`EmployeeFullName`、`DepartmentNames`、`SearchEmployeeByBizId`）と `ExecuteSampleUseCase` は MainWindow.xaml で未使用。削除し、`Form1ViewModel` に一本化
- 併せて、未使用の `IClock`／`GetEmployeeByBizIdIntegrationUseCase`／`BusinessDayClock` の注入も削除（MainWindow に BusinessDayClock パネルを出す場合は別途判断）
- クラスの責務コメントを実態に合わせて更新

完了条件: `MainWindowViewModel` に `System.Windows` と `Syncfusion` の using が無い（grep で確認）

## Phase 3: Presentation → Infrastructure 依存違反の解消

### 現状

`ICurrentUserService` が `src/Infrastructure/Services/` にあり、`LoginWindowViewModel`／`RealCurrentUserService`（WpfTrial、WinTrial 両方）が Infrastructure 名前空間を参照している。一方で Infrastructure のリポジトリ基底クラスもこれを使用（利用箇所は約 15 ファイル）。

### 方針

- `ICurrentUserService`（インターフェースのみ）を **Application 汎用層**（`src/Application/Abstractions/Services/`、名前空間 `SupportAdvance.Application.Abstractions.Services`）へ移動
  - 既存の `ISequenceProvider`（`Abstractions/Identifiers`）と同じ流儀
  - 汎用 Application 層は「インターフェース定義のみ」というルールに合致
  - `Infrastructure.csproj` は既に `Application.csproj` を参照済みで、Presentation → Application も許可された依存方向。プロジェクト参照の追加は不要
  - Infrastructure が Application のインターフェースを実装する形になり、CLAUDE.md の依存ルールに沿う
- 移動先を Common にしない理由: 「現在のユーザー」はアプリケーションの文脈を表す契約であり、Common（Clock や Settings などの基盤ユーティリティ）より Application の役割に合うため
- 名前空間変更に伴い using を一括置換（Infrastructure リポジトリ、各 Context Infrastructure、テスト、WinTrial、WpfTrial）
- `SystemCurrentUserService` は Infrastructure に残す（Application のインターフェースの実装のため許可される）
- `ICurrentUserService` の XMLDoc（実装先の記述）を更新

### 検証

- `grep -r "SupportAdvance.Infrastructure" src/Presentation --include=*.cs`（App.xaml.cs 以外でヒットしないこと）
- 全ソリューションのビルド、Infrastructure／Employee／Department のテスト実行

## Phase 4: 検証の自動化

- `tests/Presentation/` に `WpfTrial.Tests` プロジェクトを新設（現状は空）
  - `MainWindowViewModel`: 同一ヘッダー選択時に既存タブを選ぶ／新規タブ追加／`SelectedTab` 通知
  - `LoginWindowViewModel`: 入力検証、成功時に `LoginSucceeded` 発火、失敗時にパスワードクリア（Use Case はモック）
  - `Form1ViewModel`: 数値以外／未検出／例外の各分岐
- `Architecture.Tests` に追加
  - Presentation の型（`App` 以外）が `SupportAdvance.Infrastructure*` に依存しない
  - `*ViewModel` が `System.Windows.*`／`Syncfusion.*` に依存しない
- 既存の 888 テスト＋新規テストが全て合格

## Phase 5: 任意の改善（低優先）

- 各 View の要素ごとの `FontFamily`／`FontSize` 重複指定を削除し、App.xaml の暗黙スタイルに一本化
- `LoginWindowViewModel.IsNotLoading` を廃止し、`InverseBooleanConverter` か `!IsLoading` 相当のバインディングへ
- Window の ViewModel 紐付けを DataTemplate／`d:DataContext` 設計時サポートへ整理
- `TabItemViewModel` が `Header` 変更を通知するか（ObservableObject 化で対応済み）確認

## Phase 6: WinTrial の準拠調査（WpfTrial 改修完了後）

### 背景

WinTrial は Phase 3 で `ICurrentUserService` の名前空間変更に追随するのみで、ViewModel／View の中身は未調査。ただし `LoginDialogViewModel` と `Services/RealCurrentUserService.cs` が Infrastructure 名前空間を参照していることは確認済み（Phase 3 で解消）。

### 調査観点

- WinForms における MVVM の位置づけ（データバインディング、MVP との使い分け）と、プロジェクトの基本方針との整合
- ViewModel から `System.Windows.Forms` 型への依存が無いこと
- MVVM Toolkit の使い方（`ObservableProperty`／`RelayCommand`）の正確性
- Form のコードビハインドにビジネスロジックが無く、Use Case 呼び出しは ViewModel 経由であること
- Presentation → Infrastructure／Domain の依存が Program.cs のみであること
- `Presentation.Shared` の共有 ViewModel（`BusinessDayClockViewModel`）の使われ方
- WpfTrial との重複コード（Form1 相当の画面、ログイン処理）の有無

### 成果物

- 調査結果を `docs/Assistance/Reports/yyyyMMdd_WinTrial_準拠調査報告.md` に保存（WpfTrial 調査と同じ観点・重大度で整理）
- 必要な修正は別途 Plans に修正計画を作成し、承認後に着手

### 完了条件

- WinTrial についても Phase 4 の NetArchTest（Presentation → Infrastructure 禁止、ViewModel の UI 型非依存）が適用対象に含まれていること

## リスクと留意点

| リスク | 対策 |
|---|---|
| Syncfusion `SfNavigationDrawer` のイベントをビヘイビア化できない | 2-2 (b) にフォールバック |
| `ICurrentUserService` 移動による広範囲の namespace 変更 | Phase 3 を単独コミットにし、置換後に全ビルド＋全テスト |
| ViewModel-first の DataTemplate で TabControlExt の表示が崩れる | 既存の SfNavigationDrawer 検証手順（スモークテスト）で実機確認 |
| WinTrial にも同種の違反がある | 本計画は WpfTrial 中心。WinTrial は Phase 3 の namespace 変更のみ追随し、MVVM 観点の調査は Phase 6 で実施 |

## 実施順序

Phase 1 → 2 → 3 → 4（Phase 4 のテストは Phase 2・3 と並行して追加してもよい）→ 5 → 6

各 Phase 後に、ログイン → メイン → ナビゲーション選択 → タブ追加・切り替えの手動スモークテストを実施する。
