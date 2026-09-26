# SfNavigationDrawer 再検証計画

## 背景

`MainWindow.xaml` / `MainWindow.xaml.cs` に「SfNavigationDrawer は動的な NavigationItem
階層構築時に CollectionChanged ハンドラで NullReferenceException が発生する既知の不具合」
というコメントがあったが、プロジェクト内にエビデンス（エラーログ、Issue 番号、再現手順の
記録）が存在しなかった。

## 調査結果

### git 履歴

- コミット `877bd19`（2026-09-25 05:57）で GroupBar → SfNavigationDrawer に置換し、
  「初期化時の NullReferenceException を回避」する対策込みで実装。
- コミット `6521487`（2026-09-25 06:04）で SfNavigationDrawer を廃止し GroupBar に戻す。
- **問題が実際に発生した最初の実装（対策前のコード）は一度もコミットされておらず、
  何が真因だったか直接検証できない。**

### 877bd19 時点の実装

```xml
<navigationDrawer:SfNavigationDrawer ...>
    <navigationDrawer:SfNavigationDrawer.ContentView>
        <navigationDrawer:NavigationItemsView x:Name="NavigationItemsHost"/>
    </navigationDrawer:SfNavigationDrawer.ContentView>
</navigationDrawer:SfNavigationDrawer>
```

```csharp
// Loaded イベントまで構築を遅延し、Items.Add() ではなく ItemsSource へ一括代入
var customerItem = new NavigationItem { Header = "顧客管理", ItemsSource = new[] {...} };
NavigationItemsHost.ItemsSource = new[] { customerItem, orderItem };
```

### ローカル公式サンプル（`C:\Users\Public\Documents\Syncfusion\WPF\34.2.2\SampleBrowser\navigation\NavigationDrawer`）との比較

`NavigationDrawerDataBining.xaml`（MVVM データバインディングの公式パターン）:

```xml
<syncfusion:SfNavigationDrawer
    ItemsSource="{Binding Categories}"
    SelectedItem="{Binding CategorySelectedItem}">
    <syncfusion:SfNavigationDrawer.FooterItems>
        <syncfusion:NavigationItem Header="Settings">...</syncfusion:NavigationItem>
    </syncfusion:SfNavigationDrawer.FooterItems>
    <syncfusion:SfNavigationDrawer.ContentView>
        <Grid x:Name="contentViewGrid">...</Grid>  <!-- 実際の業務コンテンツ -->
    </syncfusion:SfNavigationDrawer.ContentView>
</syncfusion:SfNavigationDrawer>
```

リフレクションで確認した型情報（`Syncfusion.SfNavigationDrawer.WPF.dll` 34.2.8）：

| 型 | 基底クラス | 備考 |
|---|---|---|
| `SfNavigationDrawer` | - | `ContentPropertyAttribute` = `Items`（XAML直下ネストが公式に想定された書き方） |
| `NavigationItem` | `HeaderedItemsControl` | `Items`/`ItemsSource` を持つ通常の階層アイテム |
| `NavigationItemsView` | `ItemsControl` | 公開クラスだが公式サンプル・ドキュメントに一切登場しない |

### 判明した問題点

1. **`ContentView` はナビゲーション選択後に表示するメインコンテンツ領域**であり、
   ナビゲーション項目自体を配置する場所ではない。公式サンプルは `ContentView` に
   業務コンテンツ、`ItemsSource`/`FooterItems` にナビゲーション項目を分離している。
2. プロジェクトの実装は **`ContentView` の中に `NavigationItemsView` を配置してナビゲーション
   項目を表示しようとしていた** — 公式に文書化されたパターンから外れた API 誤用の疑いが強い。
3. `SfNavigationDrawer` の `ContentPropertyAttribute` が `Items` であることから、XAML で
   直下に `NavigationItem` をネストする書き方は公式に想定されている（公式サンプルの
   `FooterItems` 内で実際にネスト記述が問題なく使われている）。「XAML でネストすると
   CollectionChanged が壊れる」という一般化はこの事実と矛盾する。
4. 結論：**「Syncfusion 自体の既知の不具合」と断定する根拠はなく、`ContentView` の誤用が
   真因だった可能性が高い。**

## 再検証ステップ

### Step 1: 公式パターンでの再実装（試験実装、ブランチ上で検証のみ）

- `SfNavigationDrawer.ItemsSource` に `ObservableCollection<NavigationMenuItem>`
  （Header / Children を持つ ViewModel 側モデル）をバインド
- 階層表示は `HierarchicalDataTemplate` 相当の `ItemTemplate` 定義、または
  `NavigationItem.ItemsSource` への再帰バインディングで対応可否を確認
- `ContentView` には **TabControlExt（既存のタブ領域）をそのまま配置**し、
  ナビゲーション項目とは分離する
- コードビハインドでの `new NavigationItem()` 手続き型構築はやめ、MVVM バインディングに統一
  （`Presentation/CLAUDE.md` の「ビジネスロジックを書かない」方針にも合致）

### Step 2: 動作検証

- 顧客管理／受注管理の 2 階層メニューが初期表示・展開・選択で例外なく動作するか確認
- `dotnet build` でのビルド確認、実機での画面起動確認

### Step 3: 判定

- **成功した場合**: SfNavigationDrawer への移行を正式採用し、MainWindow.xaml のコメントを
  「GroupBar から SfNavigationDrawer に移行済み、理由は○○」に更新
- **失敗した場合**: 再現した具体的な例外・スタックトレースを記録した上で GroupBar 継続を
  決定し、コメントを「公式パターンで再検証済み、なお NullReferenceException が発生するため
  GroupBar を継続採用」という検証済みの記述に更新（今回のような未検証の断定を避ける）

## 対象ファイル

- `src/Presentation/WpfTrial/Views/MainWindow.xaml`
- `src/Presentation/WpfTrial/Views/MainWindow.xaml.cs`
- `src/Presentation/WpfTrial/ViewModels/MainWindowViewModel.cs`
- `src/Presentation/WpfTrial/WpfTrial.csproj`（`Syncfusion.SfNavigationDrawer.WPF` 参照を追加）

## 検証結果（2026-09-26 実施）

### 実装内容

公式パターンに準拠し、以下の通り実装した：

- `SfNavigationDrawer` 直下に `NavigationItem` を XAML で宣言（`ContentPropertyAttribute`
  が `Items` であることを利用した公式の書き方）
- `SfNavigationDrawer.ContentView` には `TabControlExt`（既存のタブ領域）のみを配置し、
  ナビゲーション項目とは完全に分離
- コードビハインドでの `new NavigationItem()` 手続き型構築、`Loaded` までの遅延構築は廃止
- `NavigationPane.ItemClicked` イベントで葉ノードのみ `ViewModel.NavigationItemSelected()`
  を呼び出す方式に変更（`GroupBar.SelectedItemChanged` と同等の役割）

### スモークテストによる動作確認

MainWindow と同一の XAML 構造（`SfNavigationDrawer` + 階層 `NavigationItem` +
`ContentView` 内 `TabControlExt`）を持つ最小テストハーネスを作成し、実機で検証した
（ログイン認証を経ずに検証するため、一時的な単体 WPF プロジェクトとして作成・実行後に削除）。

- `InitializeComponent()` が例外なく完了（XAML パース成功）
- `OnContentRendered`（テンプレート適用後）が例外なく発火
- 親ノード（「顧客管理」「受注管理」）の `IsExpanded = true` による展開が例外なく成功
- 全ての葉ノード（顧客一覧／顧客登録／受注一覧／受注登録）のクリックで例外なくタブが追加
- ウィンドウが 3 秒間クラッシュせず生存

**結論：公式パターンに準拠した実装では、コメントが主張していた「初期化時の
NullReferenceException」は再現しなかった。** 当初の疑いどおり、旧実装
（`ContentView` 内への `NavigationItemsView` の誤配置）が真因だった可能性が高いことが
裏付けられた。

### 検証中に判明した別の実装上の注意点

`NavigationItem.HasItems` プロパティは、XAML で子アイテムを宣言していても常に `false` を
返す（`Items.Count` は正しく子の数を返す）。これは Syncfusion 側の仕様／既知の挙動と
みられる。そのため：

- ❌ `item.HasItems` で親子判定すると、親ノード（見出し）クリック時にも葉ノード扱いされ
  誤ってタブが開いてしまう
- ✅ `item.Items.Count > 0` で判定することで、親ノードクリック時はタブを開かず、
  葉ノードクリック時のみタブを開く正しい動作になることを確認済み

`MainWindow.xaml.cs` の `NavigationPane_ItemClicked` はこの判定方式で実装済み。

## 判定

**Step 3 の判定：成功。SfNavigationDrawer への移行を正式採用した。**

## 備考

- ソースコード変更は本計画に基づき実施済み（コミット前提でユーザー承認済み）。
