namespace Pulsar.Server.Plugins.QQKey
{
    partial class FrmQQKey
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
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmQQKey));
            picAvatar = new System.Windows.Forms.PictureBox();
            lblQQ = new System.Windows.Forms.Label();
            lblKey = new System.Windows.Forms.Label();
            lblStatus = new System.Windows.Forms.Label();
            btnMail = new System.Windows.Forms.Button();
            btnQZone = new System.Windows.Forms.Button();
            btnWeiyun = new System.Windows.Forms.Button();
            lblNickname = new System.Windows.Forms.Label();
            panelTop = new System.Windows.Forms.Panel();
            panelInfo = new System.Windows.Forms.Panel();
            panelButtons = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)picAvatar).BeginInit();
            panelTop.SuspendLayout();
            panelInfo.SuspendLayout();
            panelButtons.SuspendLayout();
            SuspendLayout();
            // 
            // picAvatar
            // 
            picAvatar.Location = new System.Drawing.Point(20, 20);
            picAvatar.Name = "picAvatar";
            picAvatar.Size = new System.Drawing.Size(100, 100);
            picAvatar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            picAvatar.TabIndex = 0;
            picAvatar.TabStop = false;
            // 
            // lblQQ
            // 
            lblQQ.AutoSize = true;
            lblQQ.Cursor = System.Windows.Forms.Cursors.Hand;
            lblQQ.Font = new System.Drawing.Font("微软雅黑", 10F);
            lblQQ.Location = new System.Drawing.Point(10, 10);
            lblQQ.Name = "lblQQ";
            lblQQ.Size = new System.Drawing.Size(71, 20);
            lblQQ.TabIndex = 1;
            lblQQ.Text = "QQ号：--";
            lblQQ.Click += lblQQ_Click;
            // 
            // lblKey
            // 
            lblKey.AutoSize = true;
            lblKey.Cursor = System.Windows.Forms.Cursors.Hand;
            lblKey.Font = new System.Drawing.Font("微软雅黑", 9F);
            lblKey.Location = new System.Drawing.Point(10, 70);
            lblKey.Name = "lblKey";
            lblKey.Size = new System.Drawing.Size(71, 17);
            lblKey.TabIndex = 2;
            lblKey.Text = "QQKey：--";
            lblKey.Click += lblKey_Click;
            // 
            // lblStatus
            // 
            lblStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            lblStatus.Location = new System.Drawing.Point(0, 298);
            lblStatus.Name = "lblStatus";
            lblStatus.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            lblStatus.Size = new System.Drawing.Size(520, 28);
            lblStatus.TabIndex = 3;
            lblStatus.Text = "就绪";
            lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnMail
            // 
            btnMail.Dock = System.Windows.Forms.DockStyle.Left;
            btnMail.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnMail.Location = new System.Drawing.Point(0, 0);
            btnMail.Name = "btnMail";
            btnMail.Size = new System.Drawing.Size(173, 40);
            btnMail.TabIndex = 4;
            btnMail.Text = "打开邮箱";
            btnMail.UseVisualStyleBackColor = true;
            btnMail.Click += btnMail_Click;
            // 
            // btnQZone
            // 
            btnQZone.Dock = System.Windows.Forms.DockStyle.Left;
            btnQZone.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnQZone.Location = new System.Drawing.Point(173, 0);
            btnQZone.Name = "btnQZone";
            btnQZone.Size = new System.Drawing.Size(174, 40);
            btnQZone.TabIndex = 5;
            btnQZone.Text = "打开QQ空间";
            btnQZone.UseVisualStyleBackColor = true;
            btnQZone.Click += btnQZone_Click;
            // 
            // btnWeiyun
            // 
            btnWeiyun.Dock = System.Windows.Forms.DockStyle.Fill;
            btnWeiyun.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnWeiyun.Location = new System.Drawing.Point(347, 0);
            btnWeiyun.Name = "btnWeiyun";
            btnWeiyun.Size = new System.Drawing.Size(173, 40);
            btnWeiyun.TabIndex = 6;
            btnWeiyun.Text = "打开微云";
            btnWeiyun.UseVisualStyleBackColor = true;
            btnWeiyun.Click += btnWeiyun_Click;
            // 
            // lblNickname
            // 
            lblNickname.AutoSize = true;
            lblNickname.Font = new System.Drawing.Font("微软雅黑", 9F);
            lblNickname.Location = new System.Drawing.Point(10, 40);
            lblNickname.Name = "lblNickname";
            lblNickname.Size = new System.Drawing.Size(54, 17);
            lblNickname.TabIndex = 7;
            lblNickname.Text = "昵称：--";
            // 
            // panelTop
            // 
            panelTop.Controls.Add(picAvatar);
            panelTop.Controls.Add(panelInfo);
            panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            panelTop.Location = new System.Drawing.Point(0, 0);
            panelTop.Name = "panelTop";
            panelTop.Size = new System.Drawing.Size(520, 140);
            panelTop.TabIndex = 8;
            // 
            // panelInfo
            // 
            panelInfo.Controls.Add(lblQQ);
            panelInfo.Controls.Add(lblNickname);
            panelInfo.Controls.Add(lblKey);
            panelInfo.Dock = System.Windows.Forms.DockStyle.Right;
            panelInfo.Location = new System.Drawing.Point(140, 0);
            panelInfo.Name = "panelInfo";
            panelInfo.Size = new System.Drawing.Size(380, 140);
            panelInfo.TabIndex = 0;
            // 
            // panelButtons
            // 
            panelButtons.Controls.Add(btnWeiyun);
            panelButtons.Controls.Add(btnQZone);
            panelButtons.Controls.Add(btnMail);
            panelButtons.Dock = System.Windows.Forms.DockStyle.Top;
            panelButtons.Location = new System.Drawing.Point(0, 140);
            panelButtons.Name = "panelButtons";
            panelButtons.Size = new System.Drawing.Size(520, 40);
            panelButtons.TabIndex = 9;
            // 
            // FrmQQKey
            // 
            ClientSize = new System.Drawing.Size(520, 326);
            Controls.Add(panelButtons);
            Controls.Add(panelTop);
            Controls.Add(lblStatus);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmQQKey";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "NT-QQkey提取";
            ((System.ComponentModel.ISupportInitialize)picAvatar).EndInit();
            panelTop.ResumeLayout(false);
            panelInfo.ResumeLayout(false);
            panelInfo.PerformLayout();
            panelButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.PictureBox picAvatar;
        private System.Windows.Forms.Label lblQQ;
        private System.Windows.Forms.Label lblKey;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnMail;
        private System.Windows.Forms.Button btnQZone;
        private System.Windows.Forms.Button btnWeiyun;
        private System.Windows.Forms.Label lblNickname;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Panel panelInfo;
        private System.Windows.Forms.Panel panelButtons;
    }
}
