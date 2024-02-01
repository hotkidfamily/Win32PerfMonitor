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
            PlotLink = new ScottPlot.FormsPlot();
            PlotPerf = new ScottPlot.FormsPlot();
            tableLayoutPanel1 = new TableLayoutPanel();
            PlotVMem = new ScottPlot.FormsPlot();
            BtnFull = new Button();
            BtnSlide = new Button();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // PlotMem
            // 
            PlotMem.Anchor =  AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            PlotMem.Location = new Point(4, 196);
            PlotMem.Margin = new Padding(4, 3, 4, 3);
            PlotMem.Name = "PlotMem";
            PlotMem.Size = new Size(890, 187);
            PlotMem.TabIndex = 1;
            // 
            // PlotLink
            // 
            PlotLink.Anchor =  AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            PlotLink.Location = new Point(4, 389);
            PlotLink.Margin = new Padding(4, 3, 4, 3);
            PlotLink.Name = "PlotLink";
            PlotLink.Size = new Size(890, 187);
            PlotLink.TabIndex = 2;
            // 
            // PlotPerf
            // 
            PlotPerf.Anchor =  AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            PlotPerf.Location = new Point(4, 582);
            PlotPerf.Margin = new Padding(4, 3, 4, 3);
            PlotPerf.Name = "PlotPerf";
            PlotPerf.Size = new Size(890, 189);
            PlotPerf.TabIndex = 3;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor =  AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(PlotMem, 0, 1);
            tableLayoutPanel1.Controls.Add(PlotLink, 0, 2);
            tableLayoutPanel1.Controls.Add(PlotPerf, 0, 3);
            tableLayoutPanel1.Controls.Add(PlotVMem, 0, 0);
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
            // PlotVMem
            // 
            PlotVMem.Anchor =  AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            PlotVMem.Location = new Point(4, 3);
            PlotVMem.Margin = new Padding(4, 3, 4, 3);
            PlotVMem.Name = "PlotVMem";
            PlotVMem.Size = new Size(890, 187);
            PlotVMem.TabIndex = 7;
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
        private ScottPlot.FormsPlot PlotMem;
        private ScottPlot.FormsPlot PlotLink;
        private ScottPlot.FormsPlot PlotPerf;
        private TableLayoutPanel tableLayoutPanel1;
        private Button BtnFull;
        private Button BtnSlide;
        private ScottPlot.FormsPlot PlotVMem;
    }
}