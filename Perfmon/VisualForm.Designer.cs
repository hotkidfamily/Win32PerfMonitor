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
            formsPlotProcCPU = new ScottPlot.FormsPlot();
            formsPlotProcMem = new ScottPlot.FormsPlot();
            formsPlotLink = new ScottPlot.FormsPlot();
            formsPlotCpuPerf = new ScottPlot.FormsPlot();
            tableLayoutPanel1 = new TableLayoutPanel();
            BtnFull = new Button();
            BtnSlide = new Button();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // formsPlotProcCPU
            // 
            formsPlotProcCPU.Anchor =  AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            formsPlotProcCPU.Location = new Point(4, 3);
            formsPlotProcCPU.Margin = new Padding(4, 3, 4, 3);
            formsPlotProcCPU.Name = "formsPlotProcCPU";
            formsPlotProcCPU.Size = new Size(890, 187);
            formsPlotProcCPU.TabIndex = 0;
            // 
            // formsPlotProcMem
            // 
            formsPlotProcMem.Anchor =  AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            formsPlotProcMem.Location = new Point(4, 196);
            formsPlotProcMem.Margin = new Padding(4, 3, 4, 3);
            formsPlotProcMem.Name = "formsPlotProcMem";
            formsPlotProcMem.Size = new Size(890, 187);
            formsPlotProcMem.TabIndex = 1;
            // 
            // formsPlotLink
            // 
            formsPlotLink.Anchor =  AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            formsPlotLink.Location = new Point(4, 389);
            formsPlotLink.Margin = new Padding(4, 3, 4, 3);
            formsPlotLink.Name = "formsPlotLink";
            formsPlotLink.Size = new Size(890, 187);
            formsPlotLink.TabIndex = 2;
            // 
            // formsPlotCpuPerf
            // 
            formsPlotCpuPerf.Anchor =  AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            formsPlotCpuPerf.Location = new Point(4, 582);
            formsPlotCpuPerf.Margin = new Padding(4, 3, 4, 3);
            formsPlotCpuPerf.Name = "formsPlotCpuPerf";
            formsPlotCpuPerf.Size = new Size(890, 189);
            formsPlotCpuPerf.TabIndex = 3;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor =  AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(formsPlotProcCPU, 0, 0);
            tableLayoutPanel1.Controls.Add(formsPlotProcMem, 0, 1);
            tableLayoutPanel1.Controls.Add(formsPlotLink, 0, 2);
            tableLayoutPanel1.Controls.Add(formsPlotCpuPerf, 0, 3);
            tableLayoutPanel1.Location = new Point(12, 37);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.Size = new Size(898, 774);
            tableLayoutPanel1.TabIndex = 4;
            // 
            // BtnFull
            // 
            BtnFull.Anchor =  AnchorStyles.Top | AnchorStyles.Right;
            BtnFull.Location = new Point(719, 8);
            BtnFull.Margin = new Padding(2);
            BtnFull.Name = "BtnFull";
            BtnFull.Size = new Size(71, 24);
            BtnFull.TabIndex = 5;
            BtnFull.Text = "历史";
            BtnFull.UseVisualStyleBackColor = true;
            BtnFull.Click += BtnFull_Click;
            // 
            // BtnSlide
            // 
            BtnSlide.Anchor =  AnchorStyles.Top | AnchorStyles.Right;
            BtnSlide.Location = new Point(818, 8);
            BtnSlide.Margin = new Padding(2);
            BtnSlide.Name = "BtnSlide";
            BtnSlide.Size = new Size(71, 24);
            BtnSlide.TabIndex = 6;
            BtnSlide.Text = "实时";
            BtnSlide.UseVisualStyleBackColor = true;
            BtnSlide.Click += BtnSlide_Click;
            // 
            // VisualForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(922, 823);
            Controls.Add(BtnSlide);
            Controls.Add(BtnFull);
            Controls.Add(tableLayoutPanel1);
            Icon = (Icon) resources.GetObject("$this.Icon");
            MinimumSize = new Size(938, 795);
            Name = "VisualForm";
            Text = "VisualForm";
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private ScottPlot.FormsPlot formsPlotProcCPU;
        private ScottPlot.FormsPlot formsPlotProcMem;
        private ScottPlot.FormsPlot formsPlotLink;
        private ScottPlot.FormsPlot formsPlotCpuPerf;
        private TableLayoutPanel tableLayoutPanel1;
        private Button BtnFull;
        private Button BtnSlide;
    }
}