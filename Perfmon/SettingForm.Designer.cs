namespace PerfMonitor
{
    partial class SettingForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingForm));
            tabControl1 = new TabControl();
            tabPageCommon = new TabPage();
            label1 = new Label();
            comboBox1 = new ComboBox();
            tabPageMonitor = new TabPage();
            tabControl1.SuspendLayout();
            tabPageCommon.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPageCommon);
            tabControl1.Controls.Add(tabPageMonitor);
            tabControl1.Location = new Point(12, 12);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(776, 310);
            tabControl1.TabIndex = 0;
            // 
            // tabPageCommon
            // 
            tabPageCommon.Controls.Add(label1);
            tabPageCommon.Controls.Add(comboBox1);
            tabPageCommon.Location = new Point(4, 26);
            tabPageCommon.Name = "tabPageCommon";
            tabPageCommon.Padding = new Padding(3);
            tabPageCommon.Size = new Size(768, 280);
            tabPageCommon.TabIndex = 0;
            tabPageCommon.Text = "Common";
            tabPageCommon.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 31);
            label1.Name = "label1";
            label1.Size = new Size(65, 17);
            label1.TabIndex = 1;
            label1.Text = "Language";
            // 
            // comboBox1
            // 
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "中文", "English" });
            comboBox1.Location = new Point(87, 28);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(121, 25);
            comboBox1.TabIndex = 0;
            // 
            // tabPageMonitor
            // 
            tabPageMonitor.Location = new Point(4, 26);
            tabPageMonitor.Name = "tabPageMonitor";
            tabPageMonitor.Padding = new Padding(3);
            tabPageMonitor.Size = new Size(768, 376);
            tabPageMonitor.TabIndex = 1;
            tabPageMonitor.Text = "Monitor";
            tabPageMonitor.UseVisualStyleBackColor = true;
            // 
            // SettingForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 331);
            Controls.Add(tabControl1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "SettingForm";
            Text = "SettingForm";
            tabControl1.ResumeLayout(false);
            tabPageCommon.ResumeLayout(false);
            tabPageCommon.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPageCommon;
        private TabPage tabPageMonitor;
        private Label label1;
        private ComboBox comboBox1;
    }
}