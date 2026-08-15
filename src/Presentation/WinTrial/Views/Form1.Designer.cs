namespace SupportAdvance.Presentation.WinTrial.Views
{
    partial class Form1
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
            sfButton1 = new Syncfusion.WinForms.Controls.SfButton();
            sfButton2 = new Syncfusion.WinForms.Controls.SfButton();
            textBoxExt1 = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
            label1 = new Label();
            ((System.ComponentModel.ISupportInitialize)textBoxExt1).BeginInit();
            SuspendLayout();
            // 
            // sfButton1
            // 
            sfButton1.FlatStyle = FlatStyle.Popup;
            sfButton1.Font = new Font("Segoe UI Semibold", 9F);
            sfButton1.Location = new Point(35, 35);
            sfButton1.Name = "sfButton1";
            sfButton1.Size = new Size(96, 28);
            sfButton1.TabIndex = 1;
            sfButton1.Text = "テストボタン";
            // 
            // sfButton2
            // 
            sfButton2.FlatStyle = FlatStyle.Popup;
            sfButton2.Font = new Font("Segoe UI Semibold", 9F);
            sfButton2.Location = new Point(137, 89);
            sfButton2.Name = "sfButton2";
            sfButton2.Size = new Size(96, 28);
            sfButton2.TabIndex = 2;
            sfButton2.Text = "テストボタン";
            // 
            // textBoxExt1
            // 
            textBoxExt1.BeforeTouchSize = new Size(100, 23);
            textBoxExt1.Location = new Point(31, 94);
            textBoxExt1.Name = "textBoxExt1";
            textBoxExt1.Size = new Size(100, 23);
            textBoxExt1.TabIndex = 3;
            textBoxExt1.Text = "textBoxExt1";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(246, 99);
            label1.Name = "label1";
            label1.Size = new Size(38, 15);
            label1.TabIndex = 4;
            label1.Text = "label1";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(484, 302);
            Controls.Add(label1);
            Controls.Add(textBoxExt1);
            Controls.Add(sfButton2);
            Controls.Add(sfButton1);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)textBoxExt1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Syncfusion.WinForms.Controls.SfButton sfButton1;
        private Syncfusion.WinForms.Controls.SfButton sfButton2;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExt1;
        private Label label1;
    }
}
