using System.Collections.Specialized;
using System.ComponentModel;
using SupportAdvance.Presentation.Shared.Icons;
using SupportAdvance.Presentation.Shared.ViewModels;
using SupportAdvance.Presentation.Shared.ViewModels.Tabs;
using SupportAdvance.Presentation.WinTrial.Icons;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;

namespace SupportAdvance.Presentation.WinTrial.Views;

/// <summary>
/// メインウィンドウ（WinForms + MVVM Toolkit）
/// </summary>
/// <remarks>
/// <para>【責務】<see cref="MainWindowViewModel"/> とナビゲーション・タブ（MDI 子フォーム）の同期のみ。タブの管理は ViewModel が担当</para>
/// <para>【設計】WinForms には ItemsSource 相当が無いため、<see cref="MainWindowViewModel.OpenTabs"/> の変更を MDI 子フォームの生成・破棄に反映し、
/// 子フォームのアクティブ化・終了を <see cref="MainWindowViewModel.SelectedTab"/>／<c>CloseTabCommand</c> に反映する</para>
/// <para>【設計】ナビゲーションは階層を持てる <see cref="TreeViewAdv"/> を左側に固定配置する（項目名は WpfTrial の階層メニューと同じ）。
/// Syncfusion の NavigationDrawer は「閉じるスライドパネル」で階層を持てず、リサイズで項目が消えたため使用しない</para>
/// <para>【動作】子項目のクリック（またはキーボードの Enter）でタブを開く。親項目のクリックは展開／折りたたみのみ</para>
/// <para>【折りたたみ】上部のボタンで、ナビゲーションを幅の細い縦バー（ボタンのみ）と展開表示に切り替える（WpfTrial の Compact／Expanded に相当。WinForms 側はアイコンを持たないため、折りたたみ時は項目を隠す）</para>
/// </remarks>
public partial class MainWindow : Form
{
    private static readonly (string Parent, AppIcon ParentIcon, (string Name, AppIcon Icon)[] Children)[] NavigationMenu =
    [
        ("顧客管理", AppIcon.Customers, [(MainWindowViewModel.CustomerListMenuName, AppIcon.List), ("顧客登録", AppIcon.Register)]),
        ("受注管理", AppIcon.Orders, [("受注一覧", AppIcon.List), ("受注登録", AppIcon.Register)])
    ];

    // タブの高さは、タブ内のアイコンの大きさ（TabIconSize）で決まる（高さ ≒ アイコンの大きさ + 7）。文字の大きさはタブの高さにほぼ影響しない
    private const int TabIconSize = 24;
    private const float TabFontSize = 12f;

    private const int NavigationIconSize = 20;
    private const float NavigationFontSize = 11f;

    // ナビゲーションの項目の上下の余白の合計（小さいほど、項目の間隔が詰まる）
    private const int NavigationItemVerticalPadding = 4;

    // Office2016White の明るい背景で見やすいよう、黒に近い濃い色で描く（ForeColor は、テーマ適用前の値になるため使わない）
    private static readonly Color NavigationIconColor = Color.FromArgb(0x1A, 0x1A, 0x1A);
    private const int CollapsedNavigationIndent = 4;

    private const int DefaultExpandedNavigationWidth = 220;
    private const int CollapsedNavigationWidth = 36;

    private readonly MainWindowViewModel _viewModel;
    private readonly Dictionary<TabItemViewModel, Form> _tabForms = new();
    private readonly int _expandedNavigationIndent;
    private readonly OpeningView _openingView;
    private int _expandedNavigationWidth = DefaultExpandedNavigationWidth;
    private bool _isSynchronizing;
    private bool _isNavigationCollapsed;

    /// <summary>
    /// <see cref="MainWindow"/> クラスの新しいインスタンスの初期化
    /// </summary>
    /// <param name="viewModel">バインドする ViewModel</param>
    /// <exception cref="ArgumentNullException"><paramref name="viewModel"/> が <see langword="null"/> の場合</exception>
    public MainWindow(MainWindowViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;

        InitializeComponent();

        // MDI の子フォームをタブとして表示する（デザイナーの再生成で消えないよう、コードで接続する）
        tabbedMdiManager.AttachToMdiContainer(this);

        // テーマは Office2016White 
        SkinManager.SetVisualStyle(this, "Office2016White");

        // タブ（TabbedMDIManager）は、テーマの青い背景を使わず、白地に濃い文字の選択タブと、灰色の他のタブにする
        // （テーマの適用後に指定しないと、テーマの色で上書きされる）
        tabbedMdiManager.ActiveTabBackColor = Color.White;
        tabbedMdiManager.ActiveTabForeColor = Color.FromArgb(0x1A, 0x1A, 0x1A);
        tabbedMdiManager.TabBackColor = Color.FromArgb(0xE0, 0xE0, 0xE0);
        tabbedMdiManager.TabForeColor = Color.FromArgb(0x40, 0x40, 0x40);
        tabbedMdiManager.TabPanelBackColor = Color.FromArgb(0xF0, 0xF0, 0xF0);
        tabbedMdiManager.TabPanelBorderColor = Color.FromArgb(0xB0, 0xB0, 0xB0);

        // 各タブに閉じるボタンを表示する（選択中のタブだけでなく、すべてのタブ）。中ボタンのクリックでも閉じられる
        tabbedMdiManager.CloseButtonVisible = true;
        tabbedMdiManager.ShowCloseButton = true;
        tabbedMdiManager.ShowCloseButtonForActiveTabOnly = false;
        tabbedMdiManager.CloseOnMiddleButtonClick = true;

        // タブの高さと文字の大きさ（既定の高さ 23 では窮屈なため、大きくする）
        tabbedMdiManager.ImageSize = new Size(TabIconSize, TabIconSize);
        tabbedMdiManager.TabFont = new Font("Yu Gothic UI", TabFontSize, FontStyle.Regular, GraphicsUnit.Point, 128);
        tabbedMdiManager.ActiveTabFont = new Font("Yu Gothic UI", TabFontSize, FontStyle.Regular, GraphicsUnit.Point, 128);

        AddNavigationNodes();
        navigationTree.NodeMouseClick += NavigationTree_NodeMouseClick;
        navigationTree.KeyDown += NavigationTree_KeyDown;
        _expandedNavigationIndent = navigationTree.Indent;
        navigationToggleButton.Click += (_, _) => SetNavigationCollapsed(!_isNavigationCollapsed);

        // ドラッグで変えた展開時の幅を保持する（折りたたんだ後に展開したときに、その幅へ戻すため）
        navigationSplitter.SplitterMoved += (_, _) =>
        {
            if (!_isNavigationCollapsed)
            {
                _expandedNavigationWidth = navigationPanel.Width;
            }
        };
        SetNavigationCollapsed(false);

        // オープニング画面（タブは使わない）。MDI の領域（MdiClient）には MDI の子フォームしか追加できないため、
        // フォームの残りの領域を埋める（Dock = Fill）形で、MDI の領域の手前に重ねる。タブが 1 つも無い間だけ表示する
        _openingView = new OpeningView(_viewModel.Opening) { Visible = _viewModel.IsOpeningViewVisible };
        Controls.Add(_openingView);
        _openingView.BringToFront();

        _viewModel.OpenTabs.CollectionChanged += OpenTabs_CollectionChanged;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void AddNavigationNodes()
    {
        // アイコンは Fluent UI System Icons から、高 DPI に合わせた大きさの画像を作成して ImageList に登録する
        var iconPixelSize = (int)Math.Round(NavigationIconSize * DeviceDpi / 96.0);
        var imageList = new ImageList
        {
            ImageSize = new Size(iconPixelSize, iconPixelSize),
            ColorDepth = ColorDepth.Depth32Bit
        };
        var imageIndexes = new Dictionary<AppIcon, int>();

        int GetImageIndex(AppIcon icon)
        {
            if (!imageIndexes.TryGetValue(icon, out var index))
            {
                using var bitmap = FluentIconFont.CreateBitmap(icon, iconPixelSize, NavigationIconColor);
                imageList.Images.Add(bitmap);

                // ImageList は、ハンドルが作られるまで元の画像を参照し続ける。
                // 画像を破棄する前にハンドルを作らせないと、描画時に「パラメーターが有効ではありません」になる
                _ = imageList.Handle;

                index = imageList.Images.Count - 1;
                imageIndexes[icon] = index;
            }

            return index;
        }

        navigationTree.LeftImageList = imageList;
        navigationTree.Font = new Font(navigationTree.Font.FontFamily, NavigationFontSize);
        // 項目の高さは、アイコンと文字の高いほうに、上下の余白（合計 NavigationItemVerticalPadding）を足した大きさにする
        navigationTree.ItemHeight = Math.Max(iconPixelSize, navigationTree.Font.Height) + NavigationItemVerticalPadding;

        foreach (var (parent, parentIcon, children) in NavigationMenu)
        {
            var parentNode = new TreeNodeAdv(parent) { LeftImageIndices = [GetImageIndex(parentIcon)] };

            // 子項目は Tag に項目名を持たせる（親項目は Tag なし）
            foreach (var (name, icon) in children)
            {
                parentNode.Nodes.Add(new TreeNodeAdv(name) { Tag = name, LeftImageIndices = [GetImageIndex(icon)] });
            }

            navigationTree.Nodes.Add(parentNode);
        }

        navigationTree.ExpandAll();
    }

    /// <summary>
    /// ナビゲーションの折りたたみ／展開
    /// </summary>
    /// <param name="collapsed"><see langword="true"/> で幅の細い縦バー（アイコンのみ）、<see langword="false"/> で展開表示</param>
    /// <remarks>
    /// <para>【動作】折りたたみ時は、パネルの幅を狭めて項目名を隠し、アイコンだけを縦に並べる（展開／折りたたみのボタンと線は消し、字下げを小さくする）</para>
    /// <para>【幅】展開時の幅は、境界（<c>navigationSplitter</c>）のドラッグで変えられる。折りたたみ中はドラッグ不可</para>
    /// </remarks>
    private void SetNavigationCollapsed(bool collapsed)
    {
        _isNavigationCollapsed = collapsed;

        navigationTree.ShowPlusMinus = !collapsed;
        navigationTree.ShowLines = !collapsed;
        navigationTree.ShowRootLines = !collapsed;
        navigationTree.Indent = collapsed ? CollapsedNavigationIndent : _expandedNavigationIndent;

        navigationPanel.Width = collapsed ? CollapsedNavigationWidth : _expandedNavigationWidth;

        // 折りたたみ中は、ドラッグで幅を変えられないようにする
        navigationSplitter.Enabled = !collapsed;
        navigationToggleButton.Text = collapsed ? "▶" : "◀";
    }

    private void NavigationTree_NodeMouseClick(object? sender, TreeViewAdvMouseClickEventArgs e)
    {
        OpenTabFor(e.Node);
    }

    private void NavigationTree_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && navigationTree.SelectedNode is { } node)
        {
            OpenTabFor(node);
            e.Handled = true;
        }
    }

    private void OpenTabFor(TreeNodeAdv node)
    {
        // 親項目（Tag なし）は展開／折りたたみのみ。子項目のみタブを開く
        if (node.Tag is string menuName)
        {
            _viewModel.OpenTabCommand.Execute(menuName);
        }
    }

    private void OpenTabs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            foreach (var tab in _tabForms.Keys.ToList())
            {
                CloseTabForm(tab);
            }

            return;
        }

        if (e.OldItems is not null)
        {
            foreach (TabItemViewModel tab in e.OldItems)
            {
                CloseTabForm(tab);
            }
        }

        if (e.NewItems is not null)
        {
            foreach (TabItemViewModel tab in e.NewItems)
            {
                OpenTabForm(tab);
            }
        }
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.IsOpeningViewVisible))
        {
            _openingView.Visible = _viewModel.IsOpeningViewVisible;
            return;
        }

        if (e.PropertyName != nameof(MainWindowViewModel.SelectedTab))
        {
            return;
        }

        if (_viewModel.SelectedTab is { } tab && _tabForms.TryGetValue(tab, out var form))
        {
            form.Activate();
        }
    }

    private void OpenTabForm(TabItemViewModel tab)
    {
        // 画面の種類は ContentViewModel の型で決まる（新しい画面を追加する場合はここに分岐を追加）
        Form form = tab.ContentViewModel switch
        {
            Form1ViewModel form1ViewModel => new Form1View(form1ViewModel),
            EmptyTabContentViewModel => new EmptyTabForm(),
            _ => throw new NotSupportedException($"未対応のタブ内容です: {tab.ContentViewModel.GetType().Name}")
        };

        form.Text = tab.Header;
        form.MdiParent = this;

        // 最大化した状態（Form1View のデザイナーの既定、または直前の子フォームが最大化のときの既定）で表示すると、
        // 2 つ目以降の子フォームが MDI の領域全体を覆い、TabbedMDIManager のタブのバーが隠れる。
        // 通常の状態にして、タブのバーの下に配置させる（位置と大きさは TabbedMDIManager が決める）
        form.WindowState = FormWindowState.Normal;

        // 子フォームのアクティブ化・終了を ViewModel に反映
        form.Activated += (_, _) =>
        {
            if (!_isSynchronizing)
            {
                _viewModel.SelectedTab = tab;
            }
        };
        form.FormClosed += (_, _) =>
        {
            if (_isSynchronizing)
            {
                return;
            }

            _tabForms.Remove(tab);
            _viewModel.CloseTabCommand.Execute(tab);
        };

        _tabForms[tab] = form;
        form.Show();
    }

    private void CloseTabForm(TabItemViewModel tab)
    {
        if (!_tabForms.Remove(tab, out var form))
        {
            return;
        }

        _isSynchronizing = true;
        try
        {
            form.Close();
        }
        finally
        {
            _isSynchronizing = false;
        }
    }
}
