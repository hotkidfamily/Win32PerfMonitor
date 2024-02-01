using CsvHelper;
using CsvHelper.Configuration;
using ScottPlot;
using ScottPlot.Plottable;
using System.Globalization;
using System.Windows.Forms;
using static ScottPlot.Plottable.PopulationPlot;

namespace PerfMonitor
{
    public partial class VisualForm : Form
    {
        private readonly string _csvPath;
        private readonly DateTime _begin;

        private static readonly string TAB_HEADER_MEMORY = "Memory";
        private static readonly string TAB_HEADER_VMEMORY = "VMemory";
        private static readonly string TAB_HEADER_LINK = "Link";
        private static readonly string TAB_HEADER_SYSTEM = "Performance Counter";
        private DataLogger _sysLogger = default!;
        private DataLogger _cpuLogger = default!;
        private DataLogger _cpuPerfLogger = default!;
        private DataLogger _memLogger = default!;
        private DataLogger _vmemLogger = default!;
        private DataLogger _uplinkLogger = default!;
        private DataLogger _downlinkLogger = default!;

        private int _refresh_counter = 0;

        class RunStatusItemMap : ClassMap<RunStatusItem>
        {
            public RunStatusItemMap()
            {
                Map(m => m.Pid).Name("Pid");
                Map(m => m.Cpu).Name("Cpu");
                Map(m => m.ProcName).Name("ProcName");
                Map(m => m.VMem).Name("VMem");
                Map(m => m.PhyMem).Name("PhyMem");
                Map(m => m.DownLink).Name("DownLink");
                Map(m => m.UpLink).Name("UpLink");
                Map(m => m.TotalUpLink).Name("TotalUpLink");
                Map(m => m.TotalDownLink).Name("TotalDownLink");
                Map(m => m.ExcuteSeconds).Name("ExcuteSeconds");
                Map(m => m.ExcuteStatus).Name("ExcuteStatus");
                Map(m => m.SysCpu).Name("SysCpu");
                Map(m => m.CpuPerf).Name("CpuPerf");
            }
        }

        public VisualForm(string path, string descriptor, DateTime begin)
        {
            _begin = begin;
            _csvPath = path;
            InitializeComponent();
            ConstructTabControl();
            _ = UpdateInfo();
            Text += descriptor;
        }

        private void ConstructTabControl()
        {
            PixelPadding padding = new(80, 4, 18, 2);
            int rotation_angle = 60;

            string datatimeLabels (double y)
            {
                DateTime day1 = _begin;
                return day1.AddSeconds(y).ToString("HH:mm:ss");
            }

            var plot = PlotVMem.Plot;
            PlotVMem.Name = TAB_HEADER_VMEMORY;
            PlotVMem.Configuration.RightClickDragZoom = false;
            PlotVMem.Configuration.MiddleClickDragZoom = false;
            PlotVMem.Configuration.LeftClickDragPan = false;
            PlotVMem.Configuration.ScrollWheelZoom = false;
            plot.ManualDataArea(new(80, 4, 18, 2));
            plot.Margins(x: .05, y: .05);
            plot.YLabel("VMemory");
            plot.YAxis.SetBoundary(-5);
            plot.YAxis.TickLabelStyle(rotation: rotation_angle);
            plot.XAxis.SetBoundary(0);
            plot.XAxis.MinimumTickSpacing(5);
            plot.XAxis.TickLabelFormat(datatimeLabels);
            _vmemLogger = plot.AddDataLogger();
            _vmemLogger.ViewSlide(200);


            plot = PlotMem.Plot;
            PlotMem.Name = TAB_HEADER_MEMORY;
            PlotMem.Configuration.RightClickDragZoom = false;
            PlotMem.Configuration.MiddleClickDragZoom = false;
            PlotMem.Configuration.LeftClickDragPan = false;
            PlotMem.Configuration.ScrollWheelZoom = false;
            plot.ManualDataArea(new(80, 4, 18, 2));
            plot.Margins(x: .05, y: .05);
            plot.YLabel("Memory (MB)");
            plot.YAxis.TickLabelStyle(rotation: rotation_angle);
            plot.YAxis.SetBoundary(-5);
            plot.XAxis.SetBoundary(0);
            plot.XAxis.MinimumTickSpacing(5);
            plot.XAxis.TickLabelFormat(datatimeLabels);
            _memLogger = plot.AddDataLogger();
            _memLogger.ViewSlide(200);


            plot = PlotLink.Plot;
            PlotLink.Name = TAB_HEADER_LINK;
            PlotLink.Configuration.RightClickDragZoom = false;
            PlotLink.Configuration.MiddleClickDragZoom = false;
            PlotLink.Configuration.LeftClickDragPan = false;
            PlotLink.Configuration.ScrollWheelZoom = false;
            plot.ManualDataArea(padding);
            plot.Margins(x: .05, y: .05);
            plot.YLabel("Traffic (Kb/s)");
            plot.YAxis.SetBoundary(-1000);
            plot.YAxis.TickLabelStyle(rotation: rotation_angle);
            plot.XAxis.SetBoundary(0);
            plot.XAxis.MinimumTickSpacing(5);
            plot.XAxis.TickLabelFormat(datatimeLabels);
            plot.Legend().Location = Alignment.UpperRight;
            _uplinkLogger = plot.AddDataLogger();
            _uplinkLogger.ViewSlide(200);
            _uplinkLogger.Label = "up";
            _downlinkLogger = plot.AddDataLogger();
            _downlinkLogger.ViewSlide(200);
            _downlinkLogger.Label = "down";


            plot = PlotPerf.Plot;
            PlotPerf.Name = TAB_HEADER_SYSTEM;
            PlotPerf.Configuration.RightClickDragZoom = false;
            PlotPerf.Configuration.MiddleClickDragZoom = false;
            PlotPerf.Configuration.LeftClickDragPan = false;
            PlotPerf.Configuration.ScrollWheelZoom = false;
            plot.ManualDataArea(padding);
            plot.Margins(x: .05, y: .05);
            plot.YLabel("CPU (%)(Perf Counter)");
            plot.YAxis.SetBoundary(-5);
            plot.XAxis.SetBoundary(0);
            plot.XAxis.MinimumTickSpacing(5);
            plot.XAxis.TickLabelFormat(datatimeLabels);
            plot.Legend().Location = Alignment.UpperRight;
            _sysLogger = plot.AddDataLogger();
            _sysLogger.ViewSlide(200);
            _sysLogger.Label = "sys";
            _cpuPerfLogger = plot.AddDataLogger();
            _cpuPerfLogger.ViewSlide(200);
            _cpuPerfLogger.Label = "cur";
            _cpuLogger = plot.AddDataLogger();
            _cpuLogger.ViewSlide(200);
            _cpuLogger.Label = "pro";


            PlotMem.Refresh();
            PlotVMem.Refresh();
            PlotLink.Refresh();
            PlotPerf.Refresh();
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

            while ( !IsDisposed )
            {
                records = csv.GetRecords<RunStatusItem>().ToList();
                int length = records.Count;

                for ( int i = 0; i < length; i++ )
                {
                    _cpuLogger.Add(_cpuLogger.Count, records[i].Cpu);
                    _memLogger.Add(_memLogger.Count, records[i].PhyMem);
                    _vmemLogger.Add(_vmemLogger.Count, records[i].VMem);
                    _uplinkLogger.Add(_uplinkLogger.Count, records[i].UpLink);
                    _downlinkLogger.Add(_downlinkLogger.Count, records[i].DownLink);
                    _sysLogger.Add(_sysLogger.Count, records[i].SysCpu);
                    _cpuPerfLogger.Add(_cpuPerfLogger.Count, records[i].CpuPerf);
                }

                if ( length > 0)
                {
                    _refresh_counter = 0;
                }
                else
                {
                    _refresh_counter++;
                }

                if ( _refresh_counter < 10 )
                {
                    PlotMem.Refresh();
                    PlotVMem.Refresh();
                    PlotLink.Refresh();
                    PlotPerf.Refresh();
                }

                await Task.Delay(TimeSpan.FromMilliseconds(1000));
            }

            _cpuLogger?.Clear();
            _cpuPerfLogger?.Clear();
            _memLogger?.Clear();
            _vmemLogger?.Clear();
            _uplinkLogger?.Clear();
            _downlinkLogger?.Clear();
            _sysLogger?.Clear();
        }

        private void BtnFull_Click(object sender, EventArgs e)
        {
            _cpuLogger?.ViewFull();
            _memLogger?.ViewFull();
            _vmemLogger?.ViewFull();
            _uplinkLogger.ViewFull();
            _downlinkLogger?.ViewFull();
            _sysLogger?.ViewFull();
            _cpuPerfLogger?.ViewFull();

            UpdateYAxisLimits();

            PlotMem.Refresh();
            PlotVMem.Refresh();
            PlotLink.Refresh();
            PlotPerf.Refresh();
        }

        private void BtnSlide_Click(object sender, EventArgs e)
        {
            _cpuLogger?.ViewSlide(200);
            _memLogger?.ViewSlide(200);
            _vmemLogger?.ViewSlide(200);
            _uplinkLogger.ViewSlide(200);
            _downlinkLogger?.ViewSlide(200);
            _sysLogger?.ViewSlide(200);
            _cpuPerfLogger?.ViewSlide(200);

            UpdateYAxisLimits();

            PlotMem.Refresh();
            PlotVMem.Refresh();
            PlotLink.Refresh();
            PlotPerf.Refresh();
        }

        private void UpdateYAxisLimits ()
        {
            {
                double yMin = Math.Min(Math.Min(_sysLogger.GetAxisLimits().YMin, _cpuPerfLogger.GetAxisLimits().YMin), _cpuLogger.GetAxisLimits().YMin);
                double yMax = Math.Max(Math.Max(_sysLogger.GetAxisLimits().YMax , _cpuPerfLogger.GetAxisLimits().YMax), _cpuLogger.GetAxisLimits().YMax);
                var diff = (yMax - yMin) / 20;
                yMin -= diff;
                yMax += diff;
                PlotPerf.Plot.YAxis.Dims.SetAxis(yMin, yMax);
            }
            {
                double yMin = Math.Min(_uplinkLogger.GetAxisLimits().YMin , _downlinkLogger.GetAxisLimits().YMin);
                double yMax = Math.Max(_uplinkLogger.GetAxisLimits().YMax , _downlinkLogger.GetAxisLimits().YMax);
                var diff = (yMax - yMin) / 20;
                yMin -= diff;
                yMax += diff;

                PlotLink.Plot.YAxis.Dims.SetAxis(yMin, yMax);
            }
        }
    }
}
