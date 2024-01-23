using CsvHelper;
using CsvHelper.Configuration;
using ScottPlot;
using System.Globalization;
using System.Windows.Forms;

namespace PerfMonitor
{
    public partial class VisualForm : Form
    {
        private readonly string _csvPath;

        private static readonly string TAB_HEADER_CPU = "CPU";
        private static readonly string TAB_HEADER_MEMORY = "Memory";
        private static readonly string TAB_HEADER_UPLINK = "UpLink";
        private static readonly string TAB_HEADER_SYSTEM = "System";
        private ScottPlot.Plottable.DataLogger _sysLogger = default!;
        private ScottPlot.Plottable.DataLogger _procLogger = default!;
        private ScottPlot.Plottable.DataLogger _procPerfLogger = default!;
        private ScottPlot.Plottable.DataLogger _memLogger = default!;
        private ScottPlot.Plottable.DataLogger _uplinkLogger = default!;
        private ScottPlot.Plottable.DataLogger _downlinkLogger = default!;

        class RunStatusItemMap : ClassMap<RunStatusItem>
        {
            public RunStatusItemMap()
            {
                Map(m => m.Pid).Name("Pid");
                Map(m => m.Cpu).Name("Cpu");
                Map(m => m.ProcName).Name("ProcName");
                Map(m => m.VMem).Name("VMem");
                Map(m => m.PhyMem).Name("PhyMem");
                Map(m => m.TotalMem).Name("TotalMem");
                Map(m => m.DownLink).Name("DownLink");
                Map(m => m.UpLink).Name("UpLink");
                Map(m => m.TotalLinkFlow).Name("TotalLinkFlow");
                Map(m => m.ExcuteSeconds).Name("ExcuteSeconds");
                Map(m => m.ExcuteStatus).Name("ExcuteStatus");
                Map(m => m.SysCpu).Name("SysCpu");
                Map(m => m.CpuPerf).Name("CpuPerf");
            }
        }

        public VisualForm(string path, string descriptor)
        {
            InitializeComponent();
            ConstructTabControl();
            _csvPath = path;
            _ = UpdateInfo();
            Text += descriptor;
        }

        private void ConstructTabControl()
        {
            ScottPlot.PixelPadding padding = new(85, 4, 4, 2);

            formsPlotProcCPU.Name = TAB_HEADER_CPU;
            formsPlotProcCPU.Plot.YLabel("CPU (%)");
            formsPlotProcCPU.Plot.YAxis.SetBoundary(-5, 200);
            formsPlotProcCPU.Plot.XAxis.SetBoundary(0);
            formsPlotProcCPU.Configuration.RightClickDragZoom = false;
            formsPlotProcCPU.Configuration.MiddleClickDragZoom = false;
            formsPlotProcCPU.Configuration.LeftClickDragPan = false;
            formsPlotProcCPU.Plot.ManualDataArea(padding);
            _procLogger = formsPlotProcCPU.Plot.AddDataLogger();
            _procLogger.LineWidth = 3;
            _procLogger.ViewSlide(width: 200);


            formsPlotProcMem.Name = TAB_HEADER_MEMORY;
            formsPlotProcMem.Plot.YLabel("Memory (MB)");
            formsPlotProcMem.Plot.YAxis.SetBoundary(-5);
            formsPlotProcMem.Plot.XAxis.SetBoundary(0);
            formsPlotProcMem.Configuration.RightClickDragZoom = false;
            formsPlotProcMem.Configuration.MiddleClickDragZoom = false;
            formsPlotProcMem.Configuration.LeftClickDragPan = false;
            formsPlotProcMem.Plot.ManualDataArea(padding);
            _memLogger = formsPlotProcMem.Plot.AddDataLogger();
            _memLogger.LineWidth = 3;
            _memLogger.ViewSlide(width: 200);


            formsPlotUpLink.Name = TAB_HEADER_UPLINK;
            formsPlotUpLink.Plot.YLabel("Traffic (Kb/s)");
            formsPlotUpLink.Plot.YAxis.SetBoundary(-5);
            formsPlotUpLink.Plot.XAxis.SetBoundary(0);
            formsPlotUpLink.Plot.Legend().Location = Alignment.UpperRight;
            formsPlotUpLink.Configuration.RightClickDragZoom = false;
            formsPlotUpLink.Configuration.MiddleClickDragZoom = false;
            formsPlotUpLink.Configuration.LeftClickDragPan = false;
            formsPlotUpLink.Plot.ManualDataArea(padding);
            _uplinkLogger = formsPlotUpLink.Plot.AddDataLogger();
            _uplinkLogger.LineWidth = 3;
            _uplinkLogger.ViewSlide(width: 200);
            _uplinkLogger.Label = "Up";
            _downlinkLogger = formsPlotUpLink.Plot.AddDataLogger();
            _downlinkLogger.LineWidth = 3;
            _downlinkLogger.ViewSlide(width: 200);
            _downlinkLogger.Label = "Down";


            formsPlotSysCpu.Name = TAB_HEADER_SYSTEM;
            formsPlotSysCpu.Plot.YLabel("System CPU (%)");
            formsPlotSysCpu.Plot.YAxis.SetBoundary(-5, 200);
            formsPlotSysCpu.Plot.XAxis.SetBoundary(0);
            formsPlotSysCpu.Plot.Legend().Location = Alignment.UpperRight;
            formsPlotSysCpu.Configuration.RightClickDragZoom = false;
            formsPlotSysCpu.Configuration.MiddleClickDragZoom = false;
            formsPlotSysCpu.Configuration.LeftClickDragPan = false;
            formsPlotSysCpu.Plot.ManualDataArea(padding);
            _sysLogger = formsPlotSysCpu.Plot.AddDataLogger();
            _sysLogger.LineWidth = 3;
            _sysLogger.ViewSlide(width: 200);
            _sysLogger.Label = "sys";
            _procPerfLogger = formsPlotSysCpu.Plot.AddDataLogger();
            _procPerfLogger.LineWidth = 3;
            _procPerfLogger.ViewSlide(width: 200);
            _procPerfLogger.Label = "cur";


            formsPlotProcCPU.Refresh();
            formsPlotProcMem.Refresh();
            formsPlotUpLink.Refresh();
            formsPlotSysCpu.Refresh();
        }

        async Task UpdateInfo()
        {
            if (!File.Exists(_csvPath))
            {
                return;
            }
            List<RunStatusItem> records;

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                AllowComments = true,
                Comment = '#',
                Delimiter = ",",
                HasHeaderRecord = true,
                HeaderValidated = null,
                MissingFieldFound = null,
            };

            using var fs = new FileStream(_csvPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fs);
            using var csv = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<RunStatusItemMap>();

            while (!IsDisposed)
            {
                records = csv.GetRecords<RunStatusItem>().ToList();
                int length = records.Count;

                for (int i = 0; i < length; i++)
                {
                    _procLogger.Add(_procLogger.Count, records[i].Cpu);
                    _memLogger.Add(_memLogger.Count, records[i].TotalMem);
                    _uplinkLogger.Add(_uplinkLogger.Count, records[i].UpLink);
                    _downlinkLogger.Add(_downlinkLogger.Count, records[i].DownLink);
                    _sysLogger.Add(_sysLogger.Count, records[i].SysCpu);
                    _procPerfLogger.Add(_procPerfLogger.Count, records[i].CpuPerf);
                }
                formsPlotSysCpu.Refresh();
                formsPlotProcCPU.Refresh();
                formsPlotProcMem.Refresh();
                formsPlotUpLink.Refresh();

                await Task.Delay(TimeSpan.FromMilliseconds(1000));
            }

            _procLogger?.Clear();
            _procPerfLogger?.Clear();
            _memLogger?.Clear();
            _uplinkLogger?.Clear();
            _downlinkLogger?.Clear();
            _sysLogger?.Clear();
            formsPlotSysCpu?.Dispose();
            formsPlotProcCPU?.Dispose();
            formsPlotProcMem?.Dispose();
            formsPlotUpLink?.Dispose();
        }

        private void BtnFull_Click(object sender, EventArgs e)
        {
            _procLogger?.ViewFull();
            _procPerfLogger?.ViewFull();
            _memLogger?.ViewFull();
            _uplinkLogger.ViewFull();
            _downlinkLogger?.ViewFull();
            _sysLogger.ViewFull();
        }

        private void BtnSlide_Click(object sender, EventArgs e)
        {
            _procLogger?.ViewSlide();
            _procPerfLogger?.ViewSlide();
            _memLogger?.ViewSlide();
            _uplinkLogger.ViewSlide();
            _downlinkLogger?.ViewSlide();
            _sysLogger.ViewSlide();
        }
    }
}
