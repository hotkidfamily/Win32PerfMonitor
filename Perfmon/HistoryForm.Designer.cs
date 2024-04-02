namespace PerfMonitor
{
    partial class HistoryForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose (bool disposing)
        {
            if ( disposing && (components != null) )
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
        private void InitializeComponent ()
        {
            components = new System.ComponentModel.Container();
            LVHistory = new ListView();
            HistoryMenuStrip = new ContextMenuStrip(components);
            OpenToolStripMenuItem = new ToolStripMenuItem();
            DeleteToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            ModifyMarkerToolStripMenuItem = new ToolStripMenuItem();
            FreshToolStripMenuItem = new ToolStripMenuItem();
            HistoryMenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // LVHistory
            // 
            LVHistory.Anchor =  AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            LVHistory.FullRowSelect = true;
            LVHistory.GridLines = true;
            LVHistory.LabelEdit = true;
            LVHistory.Location = new Point(11, 11);
            LVHistory.Margin = new Padding(2);
            LVHistory.Name = "LVHistory";
            LVHistory.Size = new Size(900, 447);
            LVHistory.TabIndex = 0;
            LVHistory.UseCompatibleStateImageBehavior = false;
            LVHistory.View = View.Details;
            LVHistory.AfterLabelEdit += LVHistory_AfterLabelEdit;
            LVHistory.KeyDown += LVHistory_KeyDown;
            LVHistory.MouseClick += LVHistory_MouseClick;
            LVHistory.MouseDoubleClick += LVHistory_MouseDoubleClick;
            // 
            // HistoryMenuStrip
            // 
            HistoryMenuStrip.Items.AddRange(new ToolStripItem[] { OpenToolStripMenuItem, DeleteToolStripMenuItem, toolStripSeparator1, ModifyMarkerToolStripMenuItem, FreshToolStripMenuItem });
            HistoryMenuStrip.Name = "contextMenuStrip1";
            HistoryMenuStrip.Size = new Size(146, 98);
            // 
            // OpenToolStripMenuItem
            // 
            OpenToolStripMenuItem.Image = Properties.Resources.floppy;
            OpenToolStripMenuItem.Name = "OpenToolStripMenuItem";
            OpenToolStripMenuItem.Size = new Size(145, 22);
            OpenToolStripMenuItem.Text = "打开";
            OpenToolStripMenuItem.Click += OpenToolStripMenuItem_Click;
            // 
            // DeleteToolStripMenuItem
            // 
            DeleteToolStripMenuItem.Image = Properties.Resources.remove;
            DeleteToolStripMenuItem.Name = "DeleteToolStripMenuItem";
            DeleteToolStripMenuItem.Size = new Size(145, 22);
            DeleteToolStripMenuItem.Text = "删除";
            DeleteToolStripMenuItem.Click += DeleteToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(142, 6);
            // 
            // ModifyMarkerToolStripMenuItem
            // 
            ModifyMarkerToolStripMenuItem.Image = Properties.Resources.marker;
            ModifyMarkerToolStripMenuItem.Name = "ModifyMarkerToolStripMenuItem";
            ModifyMarkerToolStripMenuItem.Size = new Size(145, 22);
            ModifyMarkerToolStripMenuItem.Text = "修改备注(F2)";
            ModifyMarkerToolStripMenuItem.Click += ModifyMarkerToolStripMenuItem_Click;
            // 
            // FreshToolStripMenuItem
            // 
            FreshToolStripMenuItem.Image = Properties.Resources.refresh;
            FreshToolStripMenuItem.Name = "FreshToolStripMenuItem";
            FreshToolStripMenuItem.Size = new Size(145, 22);
            FreshToolStripMenuItem.Text = "调整表格(F5)";
            FreshToolStripMenuItem.Click += FreshToolStripMenuItem_Click;
            // 
            // HistoryForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(922, 469);
            Controls.Add(LVHistory);
            Margin = new Padding(2);
            Name = "HistoryForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "HistoryForm";
            KeyDown += HistoryForm_KeyDown;
            HistoryMenuStrip.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private ListView LVHistory;
        private ContextMenuStrip HistoryMenuStrip;
        private ToolStripMenuItem OpenToolStripMenuItem;
        private ToolStripMenuItem DeleteToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem ModifyMarkerToolStripMenuItem;
        private ToolStripMenuItem FreshToolStripMenuItem;
    }
}