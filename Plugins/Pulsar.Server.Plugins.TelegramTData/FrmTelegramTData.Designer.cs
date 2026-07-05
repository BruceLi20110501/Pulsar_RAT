namespace Pulsar.Server.Plugins.TelegramTData
{
    partial class FrmTelegramTData
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmTelegramTData));
            lblStatus = new System.Windows.Forms.Label();
            lvwProcesses = new System.Windows.Forms.ListView();
            colPid = new System.Windows.Forms.ColumnHeader();
            colPath = new System.Windows.Forms.ColumnHeader();
            cmsProcess = new System.Windows.Forms.ContextMenuStrip(components);
            tsmiDownload = new System.Windows.Forms.ToolStripMenuItem();
            tsmiDownloadAll = new System.Windows.Forms.ToolStripMenuItem();
            btnRefresh = new System.Windows.Forms.Button();
            cmsProcess.SuspendLayout();
            SuspendLayout();
            // 
            // lblStatus
            // 
            lblStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            lblStatus.Location = new System.Drawing.Point(0, 372);
            lblStatus.Name = "lblStatus";
            lblStatus.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            lblStatus.Size = new System.Drawing.Size(640, 28);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "就绪";
            lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lvwProcesses
            // 
            lvwProcesses.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colPid, colPath });
            lvwProcesses.ContextMenuStrip = cmsProcess;
            lvwProcesses.Dock = System.Windows.Forms.DockStyle.Fill;
            lvwProcesses.FullRowSelect = true;
            lvwProcesses.GridLines = true;
            lvwProcesses.Location = new System.Drawing.Point(0, 36);
            lvwProcesses.Name = "lvwProcesses";
            lvwProcesses.Size = new System.Drawing.Size(640, 336);
            lvwProcesses.TabIndex = 1;
            lvwProcesses.UseCompatibleStateImageBehavior = false;
            lvwProcesses.View = System.Windows.Forms.View.Details;
            // 
            // colPid
            // 
            colPid.Text = "PID";
            colPid.Width = 100;
            // 
            // colPath
            // 
            colPath.Text = "TData 路径";
            colPath.Width = 520;
            // 
            // cmsProcess
            // 
            cmsProcess.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsmiDownload, tsmiDownloadAll });
            cmsProcess.Name = "cmsProcess";
            cmsProcess.Size = new System.Drawing.Size(227, 48);
            cmsProcess.Opening += cmsProcess_Opening;
            // 
            // tsmiDownload
            // 
            tsmiDownload.Name = "tsmiDownload";
            tsmiDownload.Size = new System.Drawing.Size(226, 22);
            tsmiDownload.Text = "下载选中的TData";
            tsmiDownload.Click += tsmiDownload_Click;
            // 
            // tsmiDownloadAll
            // 
            tsmiDownloadAll.Name = "tsmiDownloadAll";
            tsmiDownloadAll.Size = new System.Drawing.Size(226, 22);
            tsmiDownloadAll.Text = "下载全部TData";
            tsmiDownloadAll.Click += tsmiDownloadAll_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.Dock = System.Windows.Forms.DockStyle.Top;
            btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnRefresh.Location = new System.Drawing.Point(0, 0);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new System.Drawing.Size(640, 36);
            btnRefresh.TabIndex = 0;
            btnRefresh.Text = "刷新";
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // FrmTelegramTData
            // 
            ClientSize = new System.Drawing.Size(640, 400);
            Controls.Add(lvwProcesses);
            Controls.Add(btnRefresh);
            Controls.Add(lblStatus);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmTelegramTData";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            cmsProcess.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ListView lvwProcesses;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.ColumnHeader colPid;
        private System.Windows.Forms.ColumnHeader colPath;
        private System.Windows.Forms.ContextMenuStrip cmsProcess;
        private System.Windows.Forms.ToolStripMenuItem tsmiDownload;
        private System.Windows.Forms.ToolStripMenuItem tsmiDownloadAll;
    }
}
