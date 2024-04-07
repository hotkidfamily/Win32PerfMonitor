namespace PerfMonitor
{
    partial class VisualForm
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
        private void InitializeComponent ()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VisualForm));
            PlotMem = new ScottPlot.FormsPlot();
            PlotUpLink = new ScottPlot.FormsPlot();
            PlotPerf = new ScottPlot.FormsPlot();
            tableLayoutPanel1 = new TableLayoutPanel();
            PlotVMem = new ScottPlot.FormsPlot();
            PlotDownlink = new ScottPlot.FormsPlot();
            RadioFull = new RadioButton();
            RadioSlide = new RadioButton();
            groupBox1 = new GroupBox();
            tableLayoutPanel1.SuspendLayout();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // PlotMem
            // 
            PlotMem.Anchor =  AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            PlotMem.Location = new Point(4, 157);
            PlotMem.Margin = new Padding(4, 3, 4, 3);
            PlotMem.Name = "PlotMem";
            PlotMem.Size = new Size(890, 148);
            PlotMem.TabIndex = 1;
            // 
            // PlotUpLink
            // 
            PlotUpLink.Anchor =  AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            PlotUpLink.Location = new Point(4, 311);
            PlotUpLink.Margin = new Padding(4, 3, 4, 3);
            PlotUpLink.Name = "PlotUpLink";
            PlotUpLink.Size = new Size(890, 148);
            PlotUpLink.TabIndex = 2;
            // 
            // PlotPerf
            // 
            PlotPerf.Anchor =  AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            PlotPerf.Location = new Point(4, 619);
            PlotPerf.Margin = new Padding(4, 3, 4, 3);
            PlotPerf.Name = "PlotPerf";
            PlotPerf.Size = new Size(890, 152);
            PlotPerf.TabIndex = 3;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor =  AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(PlotMem, 0, 1);
            tableLayoutPanel1.Controls.Add(PlotUpLink, 0, 2);
            tableLayoutPanel1.Controls.Add(PlotVMem, 0, 0);
            tableLayoutPanel1.Controls.Add(PlotPerf, 0, 4);
            tableLayoutPanel1.Controls.Add(PlotDownlink, 0, 3);
            tableLayoutPanel1.Location = new Point(12, 37);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 5;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableLayoutPanel1.Size = new Size(898, 774);
            tableLayoutPanel1.TabIndex = 4;
            // 
            // PlotVMem
            // 
            PlotVMem.Anchor =  AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            PlotVMem.Location = new Point(4, 3);
            PlotVMem.Margin = new Padding(4, 3, 4, 3);
            PlotVMem.Name = "PlotVMem";
            PlotVMem.Size = new Size(890, 148);
            PlotVMem.TabIndex = 7;
            // 
            // PlotDownlink
            // 
            PlotDownlink.Anchor =  AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            PlotDownlink.Location = new Point(4, 465);
            PlotDownlink.Margin = new Padding(4, 3, 4, 3);
            PlotDownlink.Name = "PlotDownlink";
            PlotDownlink.Size = new Size(890, 148);
            PlotDownlink.TabIndex = 8;
            // 
            // RadioFull
            // 
            RadioFull.AutoSize = true;
            RadioFull.Location = new Point(6, 13);
            RadioFull.Name = "RadioFull";
            RadioFull.Size = new Size(50, 21);
            RadioFull.TabIndex = 7;
            RadioFull.Text = "趋势";
            RadioFull.UseVisualStyleBackColor = true;
            RadioFull.CheckedChanged += RadioFull_CheckedChanged;
            // 
            // RadioSlide
            // 
            RadioSlide.AutoSize = true;
            RadioSlide.Checked = true;
            RadioSlide.Location = new Point(62, 13);
            RadioSlide.Name = "RadioSlide";
            RadioSlide.Size = new Size(50, 21);
            RadioSlide.TabIndex = 8;
            RadioSlide.TabStop = true;
            RadioSlide.Text = "实时";
            RadioSlide.UseVisualStyleBackColor = true;
            RadioSlide.CheckedChanged += RadioSlide_CheckedChanged;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(RadioFull);
            groupBox1.Controls.Add(RadioSlide);
            groupBox1.Location = new Point(795, 1);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(115, 40);
            groupBox1.TabIndex = 9;
            groupBox1.TabStop = false;
            groupBox1.Text = "图形";
            // 
            // VisualForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(922, 823);
            Controls.Add(groupBox1);
            Controls.Add(tableLayoutPanel1);
            Icon = (Icon) resources.GetObject("$this.Icon");
            MinimumSize = new Size(938, 795);
            Name = "VisualForm";
            Text = "VisualForm";
            KeyDown += VisualForm_KeyDown;
            tableLayoutPanel1.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private ScottPlot.FormsPlot PlotMem;
        private ScottPlot.FormsPlot PlotUpLink;
        private ScottPlot.FormsPlot PlotPerf;
        private ScottPlot.FormsPlot PlotVMem;
        private TableLayoutPanel tableLayoutPanel1;
        private ScottPlot.FormsPlot PlotDownlink;
        private RadioButton RadioFull;
        private RadioButton RadioSlide;
        private GroupBox groupBox1;
    }
}