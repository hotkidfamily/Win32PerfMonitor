namespace PerfMonitor
{
    partial class ProcsEnumForm
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
            textBoxPidOrName = new TextBox();
            LVProcss = new ListView();
            labelProcess = new Label();
            SuspendLayout();
            // 
            // textBoxPidOrName
            // 
            textBoxPidOrName.Location = new Point(12, 12);
            textBoxPidOrName.Name = "textBoxPidOrName";
            textBoxPidOrName.Size = new Size(485, 23);
            textBoxPidOrName.TabIndex = 0;
            textBoxPidOrName.KeyUp += textBoxPidOrName_KeyUp;
            // 
            // LVProcss
            // 
            LVProcss.FullRowSelect = true;
            LVProcss.GridLines = true;
            LVProcss.Location = new Point(12, 52);
            LVProcss.Name = "LVProcss";
            LVProcss.Size = new Size(776, 386);
            LVProcss.TabIndex = 1;
            LVProcss.UseCompatibleStateImageBehavior = false;
            LVProcss.View = View.Details;
            LVProcss.MouseDoubleClick += LVProcss_MouseDoubleClick;
            // 
            // labelProcess
            // 
            labelProcess.AutoSize = true;
            labelProcess.Location = new Point(539, 15);
            labelProcess.Name = "labelProcess";
            labelProcess.Size = new Size(40, 17);
            labelProcess.TabIndex = 2;
            labelProcess.Text = "Total:";
            // 
            // ProcsEnumForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(labelProcess);
            Controls.Add(LVProcss);
            Controls.Add(textBoxPidOrName);
            DoubleBuffered = true;
            MaximizeBox = false;
            Name = "ProcsEnumForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "ProcsEnum";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBoxPidOrName;
        private ListView LVProcss;
        private Label labelProcess;
    }
}