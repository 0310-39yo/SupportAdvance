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
            navigationDrawer = new Syncfusion.Windows.Forms.Tools.NavigationDrawer();
            tabbedMdiManager = new Syncfusion.Windows.Forms.Tools.TabbedMDIManager();
            statusStripEx = new Syncfusion.Windows.Forms.Tools.StatusStripEx();

            // ribbon（上部）
            ribbon.Dock = Syncfusion.Windows.Forms.Tools.DockStyleEx.Top;
            ribbon.Location = new Point(0, 0);
            ribbon.Name = "ribbon";
            ribbon.Size = new Size(800, 60);
            ribbon.TabIndex = 1;

            // navigationDrawer（左側）
            navigationDrawer.Dock = DockStyle.Left;
            navigationDrawer.Location = new Point(0, 60);
            navigationDrawer.Name = "navigationDrawer";
            navigationDrawer.Size = new Size(200, 365);
            navigationDrawer.TabIndex = 2;

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
            Controls.Add(navigationDrawer);
            Controls.Add(ribbon);
            Controls.Add(statusStripEx);
            Name = "MainWindow";
            Text = "MainWindow";
            Load += MainWindow_Load;
            ResumeLayout(false);
            PerformLayout();

            // TabbedMDIManager を MainWindow にアタッチ
            tabbedMdiManager.AttachToMdiContainer(this);
        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdv ribbon;
        private Syncfusion.Windows.Forms.Tools.NavigationDrawer navigationDrawer;
        private Syncfusion.Windows.Forms.Tools.TabbedMDIManager tabbedMdiManager;
        private Syncfusion.Windows.Forms.Tools.StatusStripEx statusStripEx;
    }
}
