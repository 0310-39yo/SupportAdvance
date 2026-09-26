namespace SupportAdvance.Presentation.WinTrial.Views
{
    partial class LoginDialog
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
            components = new System.ComponentModel.Container();
            lblLoginId = new Label();
            txtLoginId = new TextBox();
            lblPassword = new Label();
            txtPassword = new TextBox();
            lblError = new Label();
            btnLogin = new Syncfusion.Windows.Forms.ButtonAdv();
            btnCancel = new Syncfusion.Windows.Forms.ButtonAdv();
            SuspendLayout();
            // 
            // lblLoginId
            // 
            lblLoginId.AutoSize = true;
            lblLoginId.Font = new Font("Yu Gothic UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            lblLoginId.Location = new Point(20, 20);
            lblLoginId.Name = "lblLoginId";
            lblLoginId.Size = new Size(75, 21);
            lblLoginId.TabIndex = 0;
            lblLoginId.Text = "ログインID:";
            // 
            // txtLoginId
            // 
            txtLoginId.Font = new Font("Yu Gothic UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            txtLoginId.Location = new Point(120, 20);
            txtLoginId.Name = "txtLoginId";
            txtLoginId.Size = new Size(250, 29);
            txtLoginId.TabIndex = 1;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Font = new Font("Yu Gothic UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            lblPassword.Location = new Point(20, 60);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(71, 21);
            lblPassword.TabIndex = 2;
            lblPassword.Text = "パスワード:";
            // 
            // txtPassword
            // 
            txtPassword.Font = new Font("Yu Gothic UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 128);
            txtPassword.Location = new Point(120, 60);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.Size = new Size(250, 29);
            txtPassword.TabIndex = 3;
            // 
            // lblError
            // 
            lblError.Font = new Font("Yu Gothic UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            lblError.ForeColor = Color.Red;
            lblError.Location = new Point(20, 100);
            lblError.Name = "lblError";
            lblError.Size = new Size(350, 40);
            lblError.TabIndex = 4;
            // 
            // btnLogin
            // 
            btnLogin.BeforeTouchSize = new Size(100, 35);
            btnLogin.Location = new Point(150, 145);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(100, 35);
            btnLogin.TabIndex = 5;
            btnLogin.Text = "ログイン";
            btnLogin.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.BeforeTouchSize = new Size(100, 35);
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(260, 145);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(100, 35);
            btnCancel.TabIndex = 6;
            btnCancel.Text = "キャンセル";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // LoginDialog
            // 
            AcceptButton = btnLogin;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(400, 200);
            Controls.Add(btnCancel);
            Controls.Add(btnLogin);
            Controls.Add(lblError);
            Controls.Add(txtPassword);
            Controls.Add(lblPassword);
            Controls.Add(txtLoginId);
            Controls.Add(lblLoginId);
            Font = new Font("Yu Gothic UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "LoginDialog";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ログイン";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblLoginId;
        private TextBox txtLoginId;
        private Label lblPassword;
        private TextBox txtPassword;
        private Label lblError;
        private Syncfusion.Windows.Forms.ButtonAdv btnLogin;
        private Syncfusion.Windows.Forms.ButtonAdv btnCancel;
    }
}
