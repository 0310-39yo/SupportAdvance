using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;

namespace SupportAdvance.Presentation.WinTrial.Views {
    /// <summary>
    /// メインウィンドウ
    /// </summary>
    public partial class MainWindow : Form {
        /// <summary>
        /// メインウィンドウの初期化
        /// </summary>
        public MainWindow() {
            InitializeComponent();

            // --- テーマ統一（最重要）---
            SkinManager.SetVisualStyle(this, "Office2016DarkGray");
        }

        private void MainWindow_Load(object sender, EventArgs e) {
        }
    }
}
