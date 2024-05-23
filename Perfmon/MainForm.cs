using CsvHelper;
using PerfMonitor.Library;
using PerfMonitor.Properties;
using System.Diagnostics;
using System.Globalization;
using System.Management;
using Windows.Win32;

namespace PerfMonitor
{
    public partial class MainForm : Form
    {
        private static int _phyMemTotal = 0;
        private static readonly List<RunStatusItem> _monitorResult = new();
        private readonly HistoryController _historyController;
        private ScottPlot.Plottable.DataStreamer  _cpuStreamer = default!;
        private ScottPlot.Plottable.HLine _cpu_max_line = default!;
        private ScottPlot.Plottable.HLine _cpu_cur_line = default!;
        private bool _close_when_exception = false;
        private static string _logPath = default!;

        internal class ProcessMonitorContext : IDisposable
        {
            public ProcessMonitor? Monitor;
            public int LiveVideIndex = 0;
            public CsvWriter? ResWriter;
            public string? ResPath;
            public Form? _visualForm;
            public HistoryContext? history;
            public bool IsDisposed = false;
            public int PID = 0;

            public void Dispose ()
            {
                IsDisposed = true;
                Stop();
                _visualForm?.Close();
            }

            public void Stop ()
            {
                Monitor?.Dispose();
                ResWriter?.Dispose();
                Monitor = null;
                ResWriter = null;

                if ( history != null && history.Running)
                {
                    history.Running = false;
                    history.End = DateTime.Now;
                }
            }

            public bool IsStop ()
            {
                return !history?.Running ?? true;
            }
        }

        private readonly Dictionary<int, ProcessMonitorContext> _monitorManager = new();

        private readonly string[] _colHeaders = new string[] { "测试内容", "PID", "进程名", "运行时间", "CPU", "虚拟内存", "物理内存", "下行", "上行", "下行流量", "上行流量", "状态", };
        private static readonly string[] _colDefaultValues = new string[] { "0", "0", "Attaching Process", "0 s", "0", "0", "0", "0", "0", "0", "0", "0" };
        private static readonly int[] _colSize = new int[] { 150, 80, 100, 80, 100, 100, 100, 100, 100, 100, 100, 100 };

        private static readonly Process _proc = Process.GetCurrentProcess();

        private double _sysCpu = 0;
        private string _taskList = string.Empty;

        private static string LogFolder
        {
            get
            {
                if( _logPath != null )
                {
                    return _logPath;
                }
                else
                {
                    string? oPath = null;
                    try
                    {
                        var s = _proc.MainModule?.FileName;
                        if ( s != null )
                        {
                            oPath = Path.GetDirectoryName(s);
                        }
                    }
                    catch ( Exception ) { oPath = null; }

                    var tPath = Path.Combine(Path.GetTempPath(), "PerfMonitor");
                    if ( !Directory.Exists(tPath) )
                    {
                        Directory.CreateDirectory(tPath);
                    }

                    oPath ??= tPath;

                    var output = Path.Combine(oPath, "output");

                    if ( !Directory.Exists($"{output}") )
                        Directory.CreateDirectory($"{output}");

                    _logPath = output;

                    return output;
                }
            }
        }

        private static string ConfigFolder
        {
            get
            {
                string? oPath = null;
                try
                {
                    var s = _proc.MainModule?.FileName;
                    if ( s != null )
                    {
                        oPath = Path.GetDirectoryName(s);
                    }
                }
                catch ( Exception ) { oPath = null; }

                var tPath = Environment.CurrentDirectory;
                oPath ??= tPath;

                var output = Path.Combine(oPath, "Config");

                if ( !Directory.Exists($"{output}") )
                    Directory.CreateDirectory($"{output}");

                return output;
            }
        }

        public bool Close_when_exception { get => _close_when_exception; set => _close_when_exception = value; }

        public MainForm ()
        {
            InitializeComponent();
            ConstructListView();

            _phyMemTotal = GetPhisicalMemory();
            Task.Run(QuerySystemInfo);
            _ = RefreshListView();
            labelCpuAndMem.Text = "loading...";
            _taskList = Path.Combine(ConfigFolder + "\\tasks.json");
            _historyController = new(_taskList);
            _historyController.Read();

            this.Text += $" {Resources.AppVersion}";

            PlotSysCpuUsage.Name = "CPU";
            PlotSysCpuUsage.Plot.YLabel("CPU (%)");
            PlotSysCpuUsage.Plot.YAxis.SetBoundary(-5, 200);
            PlotSysCpuUsage.Plot.XAxis.SetBoundary(0);
            PlotSysCpuUsage.Plot.XAxis.Ticks(false);
            PlotSysCpuUsage.Configuration.RightClickDragZoom = false;
            PlotSysCpuUsage.Configuration.MiddleClickDragZoom = false;
            PlotSysCpuUsage.Configuration.LeftClickDragPan = false;
            PlotSysCpuUsage.Configuration.ScrollWheelZoom = false;
            ScottPlot.PixelPadding padding = new(50, 4, 4, 2);
            PlotSysCpuUsage.Plot.ManualDataArea(padding);
            _cpuStreamer = PlotSysCpuUsage.Plot.AddDataStreamer(200);
            _cpu_max_line = PlotSysCpuUsage.Plot.AddHorizontalLine(0.0, color:Color.Red, width:1, ScottPlot.LineStyle.Dot);
            _cpu_max_line.PositionLabel = true;
            _cpu_max_line.PositionLabelBackground = Color.Red;
            _cpu_max_line.PositionFormatter = position => ((int)position).ToString();
            _cpu_cur_line = PlotSysCpuUsage.Plot.AddHorizontalLine(0.0, color: Color.Green, width: 1, ScottPlot.LineStyle.Dot);
            _cpu_cur_line.PositionLabel = true;
            _cpu_cur_line.PositionLabelBackground = Color.Green;
            _cpu_cur_line.PositionFormatter = position => ((int) position).ToString();
            _cpuStreamer.LineWidth = 1;
            _cpuStreamer.ViewScrollLeft();
            PlotSysCpuUsage.Refresh();
            CenterToScreen();
        }

        private void BtnShotProcess_MouseDown (object sender, MouseEventArgs e)
        {
            this.Opacity = 0;
        }

        private void BtnShotProcess_MouseUp (object sender, MouseEventArgs e)
        {
            PInvoke.GetCursorPos(out Point v);
            var Handle = PInvoke.WindowFromPoint(v);
            uint pid = 0;
            unsafe
            {
                _ = PInvoke.GetWindowThreadProcessId(Handle, &pid);
            }

            CreateNewMonitor((int)pid);

            this.Opacity = 1;
        }

        private void OnUpdateMonitorStatus (ref RunStatusItem status)
        {
            lock ( _monitorResult )
            {
                _monitorResult.Add(status);
            }
            if ( _monitorManager.ContainsKey(status.Pid) )
            {
                var it = _monitorManager[status.Pid];
                status.SysCpu = _sysCpu;
                try
                {
                    it.ResWriter?.WriteRecord(status);
                    it.ResWriter?.NextRecord();
                    it.ResWriter?.Flush();
                }
                catch ( Exception )
                {
                    // ignore file
                }
            }
        }

        private void ConstructListView ()
        {
            LVMonitorDetail.Columns.Clear();

            for ( int i = 0; i < _colSize.Length; i++ )
            {
                ColumnHeader ch = new()
                {
                    Width = _colSize[i],
                    TextAlign = HorizontalAlignment.Left,
                    Text = _colHeaders?[i],
                };
                LVMonitorDetail.Columns.Add(ch);
            }
        }

        private async Task RefreshListView ()
        {
            while ( !IsDisposed )
            {
                List<RunStatusItem> ress;
                lock ( _monitorResult )
                {
                    ress = new List<RunStatusItem>(_monitorResult.ToArray());
                    _monitorResult.Clear();
                }
                if ( ress.Count > 0 )
                {
                    LVMonitorDetail.BeginUpdate();
                    foreach ( RunStatusItem res in ress )
                    {
                        if ( _monitorManager.ContainsKey(res.Pid) )
                        {
                            var ctx = _monitorManager[res.Pid];
                            var index = ctx.LiveVideIndex;

                            var values = res.Info();
                            var item = LVMonitorDetail.Items[index];
                            for ( int i = 1; i < _colHeaders.Length; i++ )
                            {
                                item.SubItems[i].Text = values[i - 1];
                            }

                            if ( ctx.history != null )
                                item.SubItems[0].Text = ctx.history.Marker;

                            if ( res.ExcuteStatus == "exit" )
                            {
                                item.BackColor = Color.Red;
                                var v = _monitorManager[res.Pid];
                                v.Stop();
                            }
                        }
                    }
                    LVMonitorDetail.EndUpdate();
                }
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
            }
        }

        private async Task QuerySystemInfo ()
        {
            var core = Environment.ProcessorCount;
            var mnam = Environment.MachineName;
            var os = Environment.OSVersion.Version.ToString();
            using PerformanceCounter ramAva = new("Memory", "Available Bytes");
            using PerformanceCounter ramUsed = new("Memory", "Committed Bytes");

            string strQuery;
            if ( Environment.OSVersion.Version.Major >= 10 )
            {
                strQuery = "\\Processor Information(_Total)\\% Processor Utility";
            }
            else
            {
                strQuery = "\\Processor Information(_Total)\\% Processor Time";
            }

            using PerfQuery cpuTotal = new(strQuery);
            var sw = Stopwatch.StartNew();
            while ( !IsDisposed )
            {
                _proc.Refresh();
                int rama = (int)((long)Math.Round(ramAva?.NextValue() ?? 0) / Units.MB);
                int ram = (int)((long)(ramUsed?.NextValue() ?? 0) / Units.MB) + rama;
                double pVRam = _proc.VirtualMemorySize64 * 1.0 / Units.GB;
                int pPhyRam = (int)(_proc.WorkingSet64 / Units.MB);
                _sysCpu = cpuTotal.NextValue();

                var sb = $"{_sysCpu:F2}%, {ram}MB, {rama}MB | {core} C, {mnam}, {os}, {_phyMemTotal}GB | {pVRam:F2}GB, {pPhyRam}MB";

                labelCpuAndMem.Invoke(new Action(() =>
                {
                    labelCpuAndMem.Text = sb;
                })
                );

                double amax = _cpuStreamer.DataMax;
                double vmax = _cpuStreamer.Data.Max();
                if ( vmax <= 100 && amax > 100 )
                {
                    PlotSysCpuUsage.Plot.YAxis.SetBoundary(min:0, max: vmax*1.1);
                    var b = _cpuStreamer.Data;
                    _cpuStreamer.Clear();
                    _cpuStreamer.AddRange(b);
                }
                _cpu_max_line.Y = vmax;
                _cpu_max_line.Label = $"{vmax}";
                _cpu_cur_line.Y = _sysCpu;
                _cpu_cur_line.Label = $"{_sysCpu}";
                _cpuStreamer.Add(_sysCpu);
                
                PlotSysCpuUsage.Invoke(() =>
                {
                    PlotSysCpuUsage.Refresh();
                });

                var q = sw.ElapsedMilliseconds;
                var d = 1000 - (q % 1000);
                await Task.Delay(TimeSpan.FromMilliseconds(d));
            }

            cpuTotal?.Dispose();
            _cpuStreamer.Clear();
        }

        private void TextBoxPID_KeyPress (object sender, KeyPressEventArgs e)
        {
            if ( e.KeyChar == '\r' )
            {
                if ( int.TryParse(textBoxPID.Text, out int pi) )
                {
                    CreateNewMonitor(pi);
                }
                else
                {
                    MessageBox.Show("Bad PID, PID wrong or has been exit", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Question);
                }
            }
        }

        private static int GetPhisicalMemory ()
        {
            ManagementObjectSearcher searcher = new()
            {
                Query = new SelectQuery("Win32_PhysicalMemory ", "", new[] { "Capacity" })
            };
            ManagementObjectCollection collection = searcher.Get();
            ManagementObjectCollection.ManagementObjectEnumerator em = collection.GetEnumerator();

            long capacity = 0;
            while ( em.MoveNext() )
            {
                ManagementBaseObject baseObj = em.Current;
                if ( baseObj.Properties["Capacity"].Value != null )
                {
                    try
                    {
                        _ = long.TryParse(baseObj.Properties["Capacity"].Value.ToString(), out long cap);
                        capacity += cap;
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            return (int) (capacity / Units.GiB);
        }

        private void CreateNewMonitor (int pid)
        {
            if ( !_monitorManager.ContainsKey(pid) )
            {
                Process? p = null;
                ProcessMonitor? monitor = null;
                try
                {
                    p = Process.GetProcessById(pid);
                    monitor = new(pid, 1000, OnUpdateMonitorStatus);
                }
                catch { return; }

                string name = p.ProcessName;
                string resPath = $"{LogFolder}{Path.DirectorySeparatorChar}{name}({pid}).{DateTime.Now:yyyy.MMdd.HHmm.ss}.csv";
                var writer = new StreamWriter(resPath);
                var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                ProcessMonitorContext ctx = new()
                {
                    PID = pid,
                    Monitor = monitor,
                    ResWriter = csv,
                    ResPath = resPath,
                };
                csv.WriteHeader<RunStatusItem>();
                csv.NextRecord();

                LVMonitorDetail.BeginUpdate();
                var lvi = new ListViewItem(_colDefaultValues)
                {
                    Tag = ctx
                };
                var it = LVMonitorDetail.Items.Add(lvi);
                LVMonitorDetail.Items[it.Index].Selected = true;
                ctx.LiveVideIndex = it.Index;
                LVMonitorDetail.EndUpdate();

                _monitorManager.Add(pid, ctx);
                var his = _historyController.AddItem(pid, name, resPath, "请添加说明...");
                ctx.history = his;
            }
        }

        private void MainForm_FormClosing (object sender, FormClosingEventArgs e)
        {
            if ( _monitorManager.Count != 0 )
            {
                var dr = DialogResult.OK;

                if ( !Close_when_exception )
                {
                    CustomMessageBox mf = new(this, "确认关闭？","确认", MessageBoxButtons.OKCancel);
                    mf.ShowDialog();
                    dr = mf.ShowResult();
                }

                if ( dr == DialogResult.Cancel )
                {
                    e.Cancel = true;
                }
                else
                {
                    foreach ( var it in _monitorManager )
                    {
                        it.Value.Dispose();
                    }
                    _monitorManager.Clear();

                    _proc.Dispose();
                    _historyController.Write();
                }
            }
        }

        private void BtnOpenFloder_Click (object sender, EventArgs e)
        {
            if ( LogFolder != null )
            {
                ProcessStartInfo psi = new()
                {
                    FileName = LogFolder,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
        }

        private void MonitorDetailLV_MouseDoubleClick (object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo info = LVMonitorDetail.HitTest(e.X, e.Y);
            ListViewItem item = info.Item;
            if ( int.TryParse(item.SubItems[1].Text, out int pid) )
            {
                if ( _monitorManager.ContainsKey(pid) )
                {
                    var it = _monitorManager[pid];
                    string path = it.ResPath ?? "";
                    if (
                        path != null
                        && (it.LiveVideIndex == info.Item.Index) // fix: a monitor recapture after removed, then item be double cliked
                        )
                    {
                        if ( it._visualForm == null )
                        {
                            string desc = it.Monitor?.Descriptor() ?? "invalid";
                            var bein = it.history!.Begin;
                            var visual = new VisualForm(path, desc, bein);
                            visual.FormClosed += Visual_FormClosed;
                            visual.Tag = it;
                            visual.Show();
                            visual.Location = Location + new Size(50, 50);
                            it._visualForm = visual;
                        }
                        else
                        {
                            it._visualForm.Location = Location + new Size(50, 50);
                            it._visualForm.Focus();
                        }
                    }
                }
            }
        }

        private void Visual_FormClosed (object? sender, FormClosedEventArgs e)
        {
            var form = sender as VisualForm;
            ProcessMonitorContext? b = form?.Tag as ProcessMonitorContext?? null;
            lock ( _monitorManager )
            {
                if ( b != null )
                {
                    int pid = b.PID;
                    if ( _monitorManager.ContainsKey(pid) )
                    {
                        b._visualForm = null;
                    }
                }
            }
        }

        private void BtnSetting_Click (object sender, EventArgs e)
        {
            //using var setting = new SettingForm();
            //setting.ShowDialog();
        }

        private void MonitorDetailLV_MouseClick (object sender, MouseEventArgs e)
        {
            if ( e.Button == MouseButtons.Right )
            {
                var item = LVMonitorDetail.FocusedItem;
                if ( item != null && item.Bounds.Contains(e.Location) )
                {
                    var ctx = item.Tag as ProcessMonitorContext;
                    if(ctx != null)
                    {
                        restartCaptureToolStripMenuItem.Enabled = ctx.IsStop();
                        stopToolStripMenuItem.Enabled = !ctx.IsStop();
                    }
                    ItemContextMenuStrip.Show(Cursor.Position);
                }
            }
        }

        private void OpenToolStripMenuItem_Click (object sender, EventArgs e)
        {
            var item = LVMonitorDetail.FocusedItem;
            if ( item != null && int.TryParse(item.SubItems[1].Text, out int pid) && _monitorManager.ContainsKey(pid) )
            {
                var path = _monitorManager[pid].ResPath;
                if ( path != null )
                {
                    ProcessStartInfo psi = new()
                    {
                        FileName = path,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
            }
        }

        private void StopToolStripMenuItem_Click (object sender, EventArgs e)
        {
            var item = LVMonitorDetail.FocusedItem;
            if ( item != null && int.TryParse(item.SubItems[1].Text, out int pid) && _monitorManager.ContainsKey(pid) )
            {
                var v = _monitorManager[pid];
                v.Stop();

                item.BackColor = Color.Black;
                item.ForeColor = Color.White;
            }
        }

        private void RestartCaptureToolStripMenuItem_Click (object sender, EventArgs e)
        {
            var item = LVMonitorDetail.FocusedItem;
            if ( item != null && int.TryParse(item.SubItems[1].Text, out int pid) && _monitorManager.ContainsKey(pid) )
            {
                ProcessMonitorContext v = (ProcessMonitorContext)item.Tag;
                if ( v != null && v.IsStop() )
                {
                    v.Dispose();

                    _monitorManager.Remove(pid);
                    item.Tag = null;
                    LVMonitorDetail.Items.RemoveAt(item.Index);
                    for ( int i = 0; i < LVMonitorDetail.Items.Count; i++ )
                    {
                        ProcessMonitorContext v2 = (ProcessMonitorContext)LVMonitorDetail.Items[i].Tag;
                        v2.LiveVideIndex = i;
                    }
                    CreateNewMonitor(pid);
                }
            }
        }

        private void DeleteCaptureToolStripMenuItem_Click (object sender, EventArgs e)
        {
            var item = LVMonitorDetail.FocusedItem;
            if ( item != null && int.TryParse(item.SubItems[1].Text, out int pid) && _monitorManager.ContainsKey(pid) )
            {
                var v = _monitorManager[pid];
                if ( !v.IsStop() )
                {
                    CustomMessageBox mf = new(this, "确定要删除？","确认", MessageBoxButtons.OKCancel);
                    mf.ShowDialog();
                    DialogResult dr = mf.ShowResult();
                    if ( dr == DialogResult.OK )
                    {
                        v.Stop();
                    }
                    else if ( dr == DialogResult.Cancel )
                    {
                        return;
                    }
                }
                {
                    v.Dispose();
                    _monitorManager.Remove(pid);
                    var v1 = (ProcessMonitorContext)item.Tag;
                    LVMonitorDetail.Items.RemoveAt(item.Index);
                    for ( int i = 0; i < LVMonitorDetail.Items.Count; i++ )
                    {
                        ProcessMonitorContext v2 = (ProcessMonitorContext)LVMonitorDetail.Items[i].Tag;
                        v2.LiveVideIndex = i;
                    }
                }
            }
        }

        private void FreshToolStripMenuItem_Click (object sender, EventArgs e)
        {
            LVMonitorDetail.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

            LVMonitorDetail.BeginUpdate();
            for ( int i = 0; i <= LVMonitorDetail.Columns.Count - 1; i++ )
            {
                LVMonitorDetail.Columns[i].Width += 20;
            }
            LVMonitorDetail.EndUpdate();
        }

        private void MonitorDetailLV_MouseDown (object sender, MouseEventArgs e)
        {
            if ( e.Button == MouseButtons.Middle )
            {
                var item = LVMonitorDetail.FocusedItem;
                if ( item != null && item.Bounds.Contains(e.Location)
                    && int.TryParse(item.SubItems[1].Text, out int pid) && _monitorManager.ContainsKey(pid) )
                {
                    var monitor = _monitorManager[pid];
                    if ( monitor.ResPath != null )
                    {
                        ProcessStartInfo psi = new()
                        {
                            FileName = monitor.ResPath,
                            UseShellExecute = true
                        };
                        Process.Start(psi);
                    }
                }
            }
        }

        private void MarkerToolStripMenuItem_Click (object sender, EventArgs e)
        {
            var item = LVMonitorDetail.FocusedItem;
            if ( item != null && int.TryParse(item.SubItems[1].Text, out int pid) && _monitorManager.ContainsKey(pid) )
            {
                item.BeginEdit();
            }
        }

        private void MonitorDetailLV_AfterLabelEdit (object sender, LabelEditEventArgs e)
        {
            if ( e.Label != null )
            {
                string editedText = e.Label;

                var item = LVMonitorDetail.FocusedItem;
                var ctx = (ProcessMonitorContext)item.Tag;
                if ( ctx != null )
                {
                    ctx.history!.Marker = editedText;
                    _historyController.Write();
                }
            }
        }

        private void BtnHistory_Click (object sender, EventArgs e)
        {
            using var history = new HistoryForm(_historyController);
            if ( history.ShowDialog() == DialogResult.OK )
            {
            }
        }

        private void MonitorDetailLV_KeyDown (object sender, KeyEventArgs e)
        {
            var item = LVMonitorDetail.FocusedItem;
            if ( e.KeyCode == Keys.F2 && item != null )
            {
                item.BeginEdit();
            }
            else if ( e.KeyCode == Keys.F5 )
            {
                LVMonitorDetail.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

                LVMonitorDetail.BeginUpdate();
                for ( int i = 0; i <= LVMonitorDetail.Columns.Count - 1; i++ )
                {
                    LVMonitorDetail.Columns[i].Width += 20;
                }
                LVMonitorDetail.EndUpdate();
            }
        }
        private void Proc_FormClosed (object? sender, FormClosedEventArgs e)
        {
            var form = sender as ProcsEnumForm;
            if( form != null )
            {
                var pid = form.Pid;
                if(pid != ProcsEnumForm.DefaultPID())
                    CreateNewMonitor(pid);
            }
        }
        private void textBoxPID_Focus (object sender, EventArgs e)
        {
            using ( var procs = new ProcsEnumForm(this) )
            {
                procs.FormClosed += Proc_FormClosed;
                procs.ShowDialog();
            }
        }
    }
}