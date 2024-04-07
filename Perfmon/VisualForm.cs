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
        private readonly int SLIDE_VIEW_WIDTH = 200;

        private static readonly string TAB_HEADER_MEMORY = "Memory";
        private static readonly string TAB_HEADER_VMEMORY = "VMemory";
        private static readonly string TAB_HEADER_LINK = "UpLink";
        private static readonly string TAB_HEADER_DownLINK = "DownLink";
        private static readonly string TAB_HEADER_SYSTEM = "Performance Counter";
        private DataLogger _sysLogger = default!;
        private DataLogger _cpuLogger = default!;
        private DataLogger _cpuPerfLogger = default!;
        private DataLogger _memLogger = default!;
        private DataLogger _vmemLogger = default!;
        private DataLogger _uplinkLogger = default!;
        private DataLogger _downlinkLogger = default!;

        class RunStatusItemMap : ClassMap<RunStatusItem>
        {
            public RunStatusItemMap ()
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

        public VisualForm (string path, string descriptor, DateTime begin)
        {
            _begin = begin;
            _csvPath = path;
            InitializeComponent();
            ConstructTabControl();
            _ = UpdateInfo();
            Text += descriptor;
        }

        private void ConstructTabControl ()
        {
            PixelPadding padding = new(80, 4, 18, 2);
            int rotation_angle = 60;

            string datatimeLabels (double y)
            {
                DateTime day1 = _begin;
                return day1.AddSeconds(y).ToString("dd.HH:mm:ss");
            }

            var plot = PlotVMem.Plot;
            PlotVMem.Name = TAB_HEADER_VMEMORY;
            PlotVMem.Configuration.RightClickDragZoom = false;
            PlotVMem.Configuration.MiddleClickDragZoom = false;
            PlotVMem.Configuration.LeftClickDragPan = false;
            PlotVMem.Configuration.ScrollWheelZoom = false;
            PlotVMem.Configuration.Quality = ScottPlot.Control.QualityMode.Low;
            plot.ManualDataArea(new(80, 4, 18, 2));
            plot.Margins(x: .05, y: .05);
            plot.YLabel("VMemory");
            plot.YAxis.SetBoundary(-5);
            plot.YAxis.TickLabelStyle(rotation: rotation_angle);
            plot.XAxis.SetBoundary(0);
            plot.XAxis.MinimumTickSpacing(5);
            plot.XAxis.TickLabelFormat(datatimeLabels);
            plot.Legend().Location = Alignment.UpperRight;
            _vmemLogger = plot.AddDataLogger(label: "VMem");


            plot = PlotMem.Plot;
            PlotMem.Name = TAB_HEADER_MEMORY;
            PlotMem.Configuration.RightClickDragZoom = false;
            PlotMem.Configuration.MiddleClickDragZoom = false;
            PlotMem.Configuration.LeftClickDragPan = false;
            PlotMem.Configuration.ScrollWheelZoom = false;
            PlotMem.Configuration.Quality = ScottPlot.Control.QualityMode.Low;
            plot.ManualDataArea(new(80, 4, 18, 2));
            plot.Margins(x: .05, y: .05);
            plot.YLabel("Memory (MB)");
            plot.YAxis.TickLabelStyle(rotation: rotation_angle);
            plot.YAxis.SetBoundary(-5);
            plot.XAxis.SetBoundary(0);
            plot.XAxis.MinimumTickSpacing(5);
            plot.XAxis.TickLabelFormat(datatimeLabels);
            plot.Legend().Location = Alignment.UpperRight;
            _memLogger = plot.AddDataLogger(label:"PhyMem");

            plot = PlotUpLink.Plot;
            PlotUpLink.Name = TAB_HEADER_LINK;
            PlotUpLink.Configuration.RightClickDragZoom = false;
            PlotUpLink.Configuration.MiddleClickDragZoom = false;
            PlotUpLink.Configuration.LeftClickDragPan = false;
            PlotUpLink.Configuration.ScrollWheelZoom = false;
            PlotUpLink.Configuration.Quality = ScottPlot.Control.QualityMode.Low;
            plot.ManualDataArea(padding);
            plot.Margins(x: .05, y: .05);
            plot.YLabel("Upload (Kb/s)");
            plot.YAxis.SetBoundary(-1000);
            plot.YAxis.TickLabelStyle(rotation: rotation_angle);
            plot.XAxis.SetBoundary(0);
            plot.XAxis.MinimumTickSpacing(5);
            plot.XAxis.TickLabelFormat(datatimeLabels);
            plot.Legend().Location = Alignment.UpperRight;
            _uplinkLogger = plot.AddDataLogger(label: "upload");


            plot = PlotDownlink.Plot;
            PlotDownlink.Name = TAB_HEADER_DownLINK;
            PlotDownlink.Configuration.RightClickDragZoom = false;
            PlotDownlink.Configuration.MiddleClickDragZoom = false;
            PlotDownlink.Configuration.LeftClickDragPan = false;
            PlotDownlink.Configuration.ScrollWheelZoom = false;
            PlotDownlink.Configuration.Quality = ScottPlot.Control.QualityMode.Low;
            plot.ManualDataArea(padding);
            plot.Margins(x: .05, y: .05);
            plot.YLabel("Download (Kb/s)");
            plot.YAxis.SetBoundary(-1000);
            plot.YAxis.TickLabelStyle(rotation: rotation_angle);
            plot.XAxis.SetBoundary(0);
            plot.XAxis.MinimumTickSpacing(5);
            plot.XAxis.TickLabelFormat(datatimeLabels);
            plot.Legend().Location = Alignment.UpperRight;
            _downlinkLogger = plot.AddDataLogger(label: "download");


            plot = PlotPerf.Plot;
            PlotPerf.Name = TAB_HEADER_SYSTEM;
            PlotPerf.Configuration.RightClickDragZoom = false;
            PlotPerf.Configuration.MiddleClickDragZoom = false;
            PlotPerf.Configuration.LeftClickDragPan = false;
            PlotPerf.Configuration.ScrollWheelZoom = false;
            PlotPerf.Configuration.Quality = ScottPlot.Control.QualityMode.Low;
            plot.ManualDataArea(padding);
            plot.Margins(x: .05, y: .05);
            plot.YLabel("CPU Usage");
            plot.YAxis.SetBoundary(-5);
            plot.XAxis.SetBoundary(0);
            plot.XAxis.MinimumTickSpacing(5);
            plot.XAxis.TickLabelFormat(datatimeLabels);
            plot.Legend().Location = Alignment.UpperRight;
            _sysLogger = plot.AddDataLogger(label: "total");
            _cpuPerfLogger = plot.AddDataLogger(label: "cur (counter)");
            _cpuLogger = plot.AddDataLogger(label: "pro (timer)");

            _cpuLogger?.ViewSlide(SLIDE_VIEW_WIDTH);
            _memLogger?.ViewSlide(SLIDE_VIEW_WIDTH);
            _vmemLogger?.ViewSlide(SLIDE_VIEW_WIDTH);
            _uplinkLogger.ViewSlide(SLIDE_VIEW_WIDTH);
            _downlinkLogger?.ViewSlide(SLIDE_VIEW_WIDTH);
            _sysLogger?.ViewSlide(SLIDE_VIEW_WIDTH);
            _cpuPerfLogger?.ViewSlide(SLIDE_VIEW_WIDTH);
        }

        async Task UpdateInfo ()
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1000));

            if ( !File.Exists(_csvPath) )
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
            int _refresh_counter = 0;

            while ( !IsDisposed )
            {
                records = csv.GetRecords<RunStatusItem>().ToList();
                int length = records.Count;

                foreach (var rd in records)
                {
                    _cpuLogger.Add(_cpuLogger.Count, rd.Cpu);
                    _memLogger.Add(_memLogger.Count, rd.PhyMem);
                    _vmemLogger.Add(_vmemLogger.Count, rd.VMem);
                    _uplinkLogger.Add(_uplinkLogger.Count, rd.UpLink);
                    _downlinkLogger.Add(_downlinkLogger.Count, rd.DownLink);
                    _sysLogger.Add(_sysLogger.Count, rd.SysCpu);
                    _cpuPerfLogger.Add(_cpuPerfLogger.Count, rd.CpuPerf);
                }

                if ( length > 0 )
                {
                    _refresh_counter = 0;
                }
                else
                {
                    _refresh_counter++;
                }

                if ( _refresh_counter < 10 )
                {
                    _Refresh_View();
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

        private void _Refresh_View ()
        {
            _Update_YAxisLimits();
            _Update_Quality();

            PlotMem.Refresh();
            PlotVMem.Refresh();
            PlotUpLink.Refresh();
            PlotDownlink.Refresh();
            PlotPerf.Refresh();
        }

        private void _Update_YAxisLimits ()
        {
            {
                double yMin = Math.Min(Math.Min(_sysLogger.GetAxisLimits().YMin, _cpuPerfLogger.GetAxisLimits().YMin), _cpuLogger.GetAxisLimits().YMin);
                double yMax = Math.Max(Math.Max(_sysLogger.GetAxisLimits().YMax , _cpuPerfLogger.GetAxisLimits().YMax), _cpuLogger.GetAxisLimits().YMax);
                var diff = (yMax - yMin) / 20;
                yMin -= diff;
                yMax += diff;
                var v1 = PlotPerf.Plot.YAxis.Dims.Span;
                if ( v1 < yMax - yMin )
                    PlotPerf.Plot.YAxis.Dims.SetAxis(yMin, yMax);
            }
            {
                double yMin = Math.Min(_uplinkLogger.GetAxisLimits().YMin , _downlinkLogger.GetAxisLimits().YMin);
                double yMax = Math.Max(_uplinkLogger.GetAxisLimits().YMax , _downlinkLogger.GetAxisLimits().YMax);
                var diff = (yMax - yMin) / 20;
                yMin -= diff;
                yMax += diff;

                var v1 = PlotUpLink.Plot.YAxis.Dims.Span;
                if ( v1 < yMax - yMin )
                    PlotUpLink.Plot.YAxis.Dims.SetAxis(yMin, yMax);
            }
        }

        private void _Update_Quality ()
        {
            var quality = _sysLogger.Plot.GetAxisLimits().XSpan < 5000 ? ScottPlot.Control.QualityMode.High : ScottPlot.Control.QualityMode.Low;
            PlotMem.Configuration.Quality = quality;
            PlotUpLink.Configuration.Quality = quality;
            PlotDownlink.Configuration.Quality = quality;
            PlotPerf.Configuration.Quality = quality;
            PlotVMem.Configuration.Quality = quality;
        }

        private void VisualForm_KeyDown (object sender, KeyEventArgs e)
        {
            if ( e.KeyCode == Keys.Escape )
            {
                Close();
            }
        }

        private void RadioFull_CheckedChanged (object sender, EventArgs e)
        {
            _cpuLogger?.ViewFull();
            _memLogger?.ViewFull();
            _vmemLogger?.ViewFull();
            _uplinkLogger.ViewFull();
            _downlinkLogger?.ViewFull();
            _sysLogger?.ViewFull();
            _cpuPerfLogger?.ViewFull();

            _Refresh_View();
        }

        private void RadioSlide_CheckedChanged (object sender, EventArgs e)
        {
            _cpuLogger?.ViewSlide(SLIDE_VIEW_WIDTH);
            _memLogger?.ViewSlide(SLIDE_VIEW_WIDTH);
            _vmemLogger?.ViewSlide(SLIDE_VIEW_WIDTH);
            _uplinkLogger.ViewSlide(SLIDE_VIEW_WIDTH);
            _downlinkLogger?.ViewSlide(SLIDE_VIEW_WIDTH);
            _sysLogger?.ViewSlide(SLIDE_VIEW_WIDTH);
            _cpuPerfLogger?.ViewSlide(SLIDE_VIEW_WIDTH);

            _Refresh_View();
        }
    }
}
