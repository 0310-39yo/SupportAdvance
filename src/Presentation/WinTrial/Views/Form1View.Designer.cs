namespace SupportAdvance.Presentation.WinTrial.Views
{
    partial class Form1View
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
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
            buttonAdv1 = new Syncfusion.Windows.Forms.ButtonAdv();
            buttonAdv2 = new Syncfusion.Windows.Forms.ButtonAdv();
            textBoxExt1 = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
            label1 = new Label();
            label2 = new Label();
            ((System.ComponentModel.ISupportInitialize)textBoxExt1).BeginInit();
            SuspendLayout();
            // 
            // buttonAdv1
            // 
            buttonAdv1.Font = new Font("Yu Gothic UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            buttonAdv1.Location = new Point(35, 35);
            buttonAdv1.Name = "buttonAdv1";
            buttonAdv1.Size = new Size(96, 28);
            buttonAdv1.TabIndex = 1;
            buttonAdv1.Text = "テストボタン";
            // 
            // buttonAdv2
            // 
            buttonAdv2.Font = new Font("Yu Gothic UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            buttonAdv2.Location = new Point(137, 89);
            buttonAdv2.Name = "buttonAdv2";
            buttonAdv2.Size = new Size(96, 28);
            buttonAdv2.TabIndex = 2;
            buttonAdv2.Text = "検索";
            // 
            // textBoxExt1
            // 
            textBoxExt1.BeforeTouchSize = new Size(100, 29);
            textBoxExt1.Location = new Point(31, 94);
            textBoxExt1.Name = "textBoxExt1";
            textBoxExt1.Size = new Size(100, 29);
            textBoxExt1.TabIndex = 3;
            textBoxExt1.Text = "textBoxExt1";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(246, 99);
            label1.Name = "label1";
            label1.Size = new Size(52, 21);
            label1.TabIndex = 4;
            label1.Text = "label1";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(174, 136);
            label2.Name = "label2";
            label2.Size = new Size(52, 21);
            label2.TabIndex = 5;
            label2.Text = "label2";
            // 
            // Form1View
            // 
            AutoScaleDimensions = new SizeF(9F, 21F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(484, 302);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(textBoxExt1);
            Controls.Add(buttonAdv2);
            Controls.Add(buttonAdv1);
            Font = new Font("Yu Gothic UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            Name = "Form1View";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form1View";
            WindowState = FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)textBoxExt1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdv1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdv2;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExt1;
        private Label label1;
        private Label label2;
    }
}
