namespace SupportAdvance.Presentation.WinTrial.Views {
    partial class MainWindow {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            components = new System.ComponentModel.Container();
            var treeNodeAdvStyleInfo1 = new Syncfusion.Windows.Forms.Tools.TreeNodeAdvStyleInfo();
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            ribbon = new Syncfusion.Windows.Forms.Tools.RibbonControlAdv();
            navigationSplitter = new Splitter();
            navigationPanel = new Panel();
            navigationTree = new Syncfusion.Windows.Forms.Tools.TreeViewAdv();
            navigationToggleButton = new Button();
            tabbedMdiManager = new Syncfusion.Windows.Forms.Tools.TabbedMDIManager(components);
            statusStripEx = new Syncfusion.Windows.Forms.Tools.StatusStripEx();
            ((System.ComponentModel.ISupportInitialize)ribbon).BeginInit();
            navigationPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)navigationTree).BeginInit();
            SuspendLayout();
            // 
            // ribbon
            // 
            ribbon.Dock = Syncfusion.Windows.Forms.Tools.DockStyleEx.Top;
            ribbon.Font = new Font("Segoe UI", 8.25F);
            ribbon.Location = new Point(0, 0);
            ribbon.MenuButtonFont = new Font("Segoe UI", 8.25F);
            ribbon.MenuButtonText = "";
            ribbon.MenuColor = Color.FromArgb(0, 114, 198);
            ribbon.Name = "ribbon";
            ribbon.OfficeColorScheme = Syncfusion.Windows.Forms.Tools.ToolStripEx.ColorScheme.Managed;
            // 
            // ribbon.OfficeMenu
            // 
            ribbon.OfficeMenu.Name = "OfficeMenu";
            ribbon.OfficeMenu.Size = new Size(12, 65);
            ribbon.QuickPanelImageLayout = PictureBoxSizeMode.StretchImage;
            ribbon.RibbonHeaderImage = Syncfusion.Windows.Forms.Tools.RibbonHeaderImage.None;
            ribbon.SelectedTab = null;
            ribbon.ShowRibbonDisplayOptionButton = true;
            ribbon.Size = new Size(1184, 60);
            ribbon.SystemText.QuickAccessDialogDropDownName = "Start menu";
            ribbon.SystemText.RenameDisplayLabelText = "&Display Name:";
            ribbon.TabIndex = 1;
            //
            // navigationSplitter（ナビゲーションの幅をマウスのドラッグで変えるための境界。幅の保持は MainWindow.cs）
            //
            navigationSplitter.Dock = DockStyle.Left;
            navigationSplitter.Location = new Point(200, 60);
            navigationSplitter.MinExtra = 200;
            navigationSplitter.MinSize = 120;
            navigationSplitter.Name = "navigationSplitter";
            navigationSplitter.Size = new Size(5, 679);
            navigationSplitter.TabIndex = 3;
            navigationSplitter.TabStop = false;
            // 
            // navigationPanel
            // 
            navigationPanel.Controls.Add(navigationTree);
            navigationPanel.Controls.Add(navigationToggleButton);
            navigationPanel.Dock = DockStyle.Left;
            navigationPanel.Location = new Point(0, 60);
            navigationPanel.Name = "navigationPanel";
            navigationPanel.Size = new Size(200, 679);
            navigationPanel.TabIndex = 2;
            // 
            // navigationTree
            // 
            treeNodeAdvStyleInfo1.CheckBoxTickThickness = 1;
            treeNodeAdvStyleInfo1.CheckColor = Color.FromArgb(109, 109, 109);
            treeNodeAdvStyleInfo1.EnsureDefaultOptionedChild = true;
            treeNodeAdvStyleInfo1.IntermediateCheckColor = Color.FromArgb(109, 109, 109);
            treeNodeAdvStyleInfo1.OptionButtonColor = Color.FromArgb(109, 109, 109);
            treeNodeAdvStyleInfo1.SelectedOptionButtonColor = Color.FromArgb(210, 210, 210);
            navigationTree.BaseStylePairs.AddRange(new Syncfusion.Windows.Forms.Tools.StyleNamePair[] { new Syncfusion.Windows.Forms.Tools.StyleNamePair("Standard", treeNodeAdvStyleInfo1) });
            navigationTree.Dock = DockStyle.Fill;
            // 
            // 
            // 
            navigationTree.HelpTextControl.BaseThemeName = null;
            navigationTree.HelpTextControl.Location = new Point(0, 0);
            navigationTree.HelpTextControl.Name = "";
            navigationTree.HelpTextControl.TabIndex = 0;
            navigationTree.HideSelection = false;
            navigationTree.InactiveSelectedNodeForeColor = SystemColors.ControlText;
            navigationTree.Location = new Point(0, 28);
            navigationTree.MetroColor = Color.FromArgb(22, 165, 220);
            navigationTree.Name = "navigationTree";
            navigationTree.SelectedNodeForeColor = SystemColors.HighlightText;
            navigationTree.Size = new Size(200, 651);
            navigationTree.TabIndex = 1;
            navigationTree.ThemeStyle.TreeNodeAdvStyle.CheckBoxTickThickness = 0;
            navigationTree.ThemeStyle.TreeNodeAdvStyle.EnsureDefaultOptionedChild = true;
            // 
            // 
            // 
            navigationTree.ToolTipControl.BaseThemeName = null;
            navigationTree.ToolTipControl.Location = new Point(0, 0);
            navigationTree.ToolTipControl.Name = "";
            navigationTree.ToolTipControl.TabIndex = 0;
            // 
            // navigationToggleButton
            // 
            navigationToggleButton.Dock = DockStyle.Top;
            navigationToggleButton.FlatStyle = FlatStyle.Flat;
            navigationToggleButton.Location = new Point(0, 0);
            navigationToggleButton.Name = "navigationToggleButton";
            navigationToggleButton.Size = new Size(200, 28);
            navigationToggleButton.TabIndex = 0;
            navigationToggleButton.Text = "◀";
            navigationToggleButton.UseVisualStyleBackColor = true;
            // 
            // tabbedMdiManager
            // 
            tabbedMdiManager.AttachedTo = null;
            tabbedMdiManager.CloseButtonBackColor = Color.White;
            tabbedMdiManager.CloseButtonToolTip = "";
            tabbedMdiManager.DropDownButtonToolTip = "";
            tabbedMdiManager.ImageSize = new Size(16, 16);
            // 
            // statusStripEx
            // 
            statusStripEx.BackColor = SystemColors.Control;
            statusStripEx.BeforeTouchSize = new Size(1184, 22);
            statusStripEx.Dock = Syncfusion.Windows.Forms.Tools.DockStyleEx.Bottom;
            statusStripEx.Location = new Point(0, 739);
            statusStripEx.MetroColor = Color.FromArgb(135, 206, 255);
            statusStripEx.Name = "statusStripEx";
            statusStripEx.Size = new Size(1184, 22);
            statusStripEx.TabIndex = 0;
            statusStripEx.Text = "Ready";
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1184, 761);
            Controls.Add(navigationSplitter);
            Controls.Add(navigationPanel);
            Controls.Add(ribbon);
            Controls.Add(statusStripEx);
            IsMdiContainer = true;
            Name = "MainWindow";
            Text = "MainWindow";
            ((System.ComponentModel.ISupportInitialize)ribbon).EndInit();
            navigationPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)navigationTree).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdv ribbon;
        private Splitter navigationSplitter;
        private Panel navigationPanel;
        private Button navigationToggleButton;
        private Syncfusion.Windows.Forms.Tools.TreeViewAdv navigationTree;
        private Syncfusion.Windows.Forms.Tools.TabbedMDIManager tabbedMdiManager;
        private Syncfusion.Windows.Forms.Tools.StatusStripEx statusStripEx;
    }
}
