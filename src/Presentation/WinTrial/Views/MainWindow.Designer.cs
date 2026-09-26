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
            ribbon = new Syncfusion.Windows.Forms.Tools.RibbonControlAdv();
            navigationPanel = new Panel();
            navigationToggleButton = new Button();
            navigationTree = new Syncfusion.Windows.Forms.Tools.TreeViewAdv();
            tabbedMdiManager = new Syncfusion.Windows.Forms.Tools.TabbedMDIManager();
            statusStripEx = new Syncfusion.Windows.Forms.Tools.StatusStripEx();

            // ribbon（上部）
            ribbon.Dock = Syncfusion.Windows.Forms.Tools.DockStyleEx.Top;
            ribbon.Location = new Point(0, 0);
            ribbon.Name = "ribbon";
            ribbon.Size = new Size(800, 60);
            ribbon.TabIndex = 1;

            // navigationPanel（左側。折りたたみ可能なナビゲーション領域。幅の切り替えは MainWindow.cs）
            navigationPanel.Dock = DockStyle.Left;
            navigationPanel.Location = new Point(0, 60);
            navigationPanel.Name = "navigationPanel";
            navigationPanel.Size = new Size(200, 365);
            navigationPanel.TabIndex = 2;
            navigationPanel.Controls.Add(navigationTree);
            navigationPanel.Controls.Add(navigationToggleButton);

            // navigationToggleButton（ナビゲーションの展開／折りたたみ）
            navigationToggleButton.Dock = DockStyle.Top;
            navigationToggleButton.FlatStyle = FlatStyle.Flat;
            navigationToggleButton.Name = "navigationToggleButton";
            navigationToggleButton.Size = new Size(200, 28);
            navigationToggleButton.TabIndex = 0;
            navigationToggleButton.Text = "◀";
            navigationToggleButton.UseVisualStyleBackColor = true;

            // navigationTree（階層メニュー。項目は MainWindow.cs で構築）
            navigationTree.Dock = DockStyle.Fill;
            navigationTree.HideSelection = false;
            navigationTree.Name = "navigationTree";
            navigationTree.TabIndex = 1;

            // statusStripEx（下部）
            statusStripEx.Dock = Syncfusion.Windows.Forms.Tools.DockStyleEx.Bottom;
            statusStripEx.Location = new Point(0, 425);
            statusStripEx.Name = "statusStripEx";
            statusStripEx.Size = new Size(800, 25);
            statusStripEx.TabIndex = 0;
            statusStripEx.Text = "Ready";

            // MainWindow
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            IsMdiContainer = true;
            Controls.Add(navigationPanel);
            Controls.Add(ribbon);
            Controls.Add(statusStripEx);
            Name = "MainWindow";
            Text = "MainWindow";
            ResumeLayout(false);
            PerformLayout();

            // TabbedMDIManager を MainWindow にアタッチ
            tabbedMdiManager.AttachToMdiContainer(this);
        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdv ribbon;
        private Panel navigationPanel;
        private Button navigationToggleButton;
        private Syncfusion.Windows.Forms.Tools.TreeViewAdv navigationTree;
        private Syncfusion.Windows.Forms.Tools.TabbedMDIManager tabbedMdiManager;
        private Syncfusion.Windows.Forms.Tools.StatusStripEx statusStripEx;
    }
}
