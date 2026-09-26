using System.Collections.Specialized;
using System.ComponentModel;
using SupportAdvance.Presentation.Shared.ViewModels;
using SupportAdvance.Presentation.Shared.ViewModels.Tabs;
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
/// </remarks>
public partial class MainWindow : Form
{
    private static readonly (string Parent, string[] Children)[] NavigationMenu =
    [
        ("顧客管理", [MainWindowViewModel.CustomerListMenuName, "顧客登録"]),
        ("受注管理", ["受注一覧", "受注登録"])
    ];

    private readonly MainWindowViewModel _viewModel;
    private readonly Dictionary<TabItemViewModel, Form> _tabForms = new();
    private bool _isSynchronizing;

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

        // テーマは Office2016DarkGray（WpfTrial の Office2019White とは別。WinForms 側の既存の指定を維持）
        SkinManager.SetVisualStyle(this, "Office2016DarkGray");

        AddNavigationNodes();
        navigationTree.NodeMouseClick += NavigationTree_NodeMouseClick;
        navigationTree.KeyDown += NavigationTree_KeyDown;

        _viewModel.OpenTabs.CollectionChanged += OpenTabs_CollectionChanged;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void AddNavigationNodes()
    {
        foreach (var (parent, children) in NavigationMenu)
        {
            var parentNode = new TreeNodeAdv(parent);

            // 子項目は Tag に項目名を持たせる（親項目は Tag なし）
            foreach (var child in children)
            {
                parentNode.Nodes.Add(new TreeNodeAdv(child) { Tag = child });
            }

            navigationTree.Nodes.Add(parentNode);
        }

        navigationTree.ExpandAll();
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
