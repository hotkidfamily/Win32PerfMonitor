using CsvHelper;
using PerfMonitor.Library;
using PerfMonitor.Properties;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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

        internal enum MonitorStatus : uint
        {
            MonitorStatusMonitoring = 0,
            MonitorStatusStopped = 1,
            MonitorStatusRemoved = 2,
        };

        internal class ProcessMonitorContext : IDisposable
        {
            public ProcessMonitor? Monitor;
            public int LiveVideIndex = 0;
            public CsvWriter? ResWriter;
            public string? ResPath;
            public Thread? VisualThread;
            public MonitorStatus MntStatus;
            public HistoryItem? history;
            public bool IsDisposed = false;

            public void Dispose ()
            {
                IsDisposed = true;
                Monitor?.Dispose();
                ResWriter?.Dispose();
                Monitor = null;
                ResWriter = null;
                MntStatus = MonitorStatus.MonitorStatusRemoved;
            }

            public void Stop ()
            {
                Monitor?.Dispose();
                ResWriter?.Dispose();
                Monitor = null;
                ResWriter = null;
                MntStatus = MonitorStatus.MonitorStatusStopped;
            }

            public bool IsStop ()
            {
                return MntStatus == MonitorStatus.MonitorStatusStopped;
            }
        }

        private readonly Dictionary<uint, ProcessMonitorContext> _monitorManager = new();

        private readonly string[] _colHeaders = new string[] { "测试内容", "PID", "进程名", "CPU", "虚拟内存", "物理内存", "总内存", "上行", "下行", "流量", "运行时间", "状态", };
        private static readonly string[] _colDefaultValues = new string[] { "0", "0", "Attaching Process", "0", "0", "0", "0", "0", "0", "0", "0 s", "0" };
        private static readonly int[] _colSize = new int[] { 150, 80, 100, 80, 100, 100, 100, 100, 100, 100, 120, 100 };

        private static readonly Process _proc = Process.GetCurrentProcess();

        private double _sysCpu = 0;
        private string _taskList = string.Empty;

        private static string LogFolder
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

                var tPath = Path.Combine(Path.GetTempPath(), "PerfMonitor");
                if ( !Directory.Exists(tPath) )
                {
                    Directory.CreateDirectory(tPath);
                }

                oPath ??= tPath;

                var output = Path.Combine(oPath, "output");

                if ( !Directory.Exists($"{output}") )
                    Directory.CreateDirectory($"{output}");

                return output;
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
            ScottPlot.PixelPadding padding = new(50, 4, 4, 2);
            PlotSysCpuUsage.Plot.ManualDataArea(padding);
            _cpuStreamer = PlotSysCpuUsage.Plot.AddDataStreamer(200);
            _cpuStreamer.LineWidth = 1;
            _cpuStreamer.ViewScrollLeft();
            PlotSysCpuUsage.Refresh();

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

            CreateNewMonitor(pid);

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
                if ( uint.TryParse(textBoxPID.Text, out uint pi) )
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

        private void CreateNewMonitor (uint pid)
        {
            if ( !_monitorManager.ContainsKey(pid) )
            {
                Process? p = null;
                try
                {
                    p = Process.GetProcessById((int) pid);
                }
                catch { return; }

                string name = p.ProcessName;
                ProcessMonitor monitor = new(pid, 1000, OnUpdateMonitorStatus);

                string resPath = $"{LogFolder}{Path.DirectorySeparatorChar}{name}({pid}).{DateTime.Now:yyyy.MMdd.HHmm.ss}.csv";
                var writer = new StreamWriter(resPath);
                var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                ProcessMonitorContext ctx = new()
                {
                    Monitor = monitor,
                    ResWriter = csv,
                    ResPath = resPath,
                    MntStatus = MonitorStatus.MonitorStatusMonitoring,
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
                var his = _historyController.AddItem(pid, resPath, "无说明...");
                ctx.history = his;
            }
        }

        private void MainForm_FormClosing (object sender, FormClosingEventArgs e)
        {
            foreach ( var it in _monitorManager )
            {
                it.Value.Dispose();
            }
            _monitorManager.Clear();

            _proc.Dispose();
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
            if ( uint.TryParse(item.SubItems[1].Text, out uint pid) )
            {
                if ( _monitorManager.ContainsKey(pid) )
                {
                    var it = _monitorManager[pid];
                    string path = it.ResPath ?? "";
                    if (
                        path != null
                        && (it.VisualThread == null) // fix: already running form
                        && (it.LiveVideIndex == info.Item.Index) // fix: a monitor recapture after removed, then item be double cliked
                        )
                    {
                        var helpThread = new Thread(new ThreadStart(() =>
                        {
                            string desc = it.Monitor?.Descriptor() ?? "invalid";
                            var visual = new VisualForm(path, desc);
                            visual.FormClosed += (s, e) =>
                            {
                                it.VisualThread = null;
                            };
                            visual.ShowDialog();
                        }));
                        helpThread.SetApartmentState(ApartmentState.STA);
                        helpThread.Start();
                        it.VisualThread = helpThread;
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
                    ItemContextMenuStrip.Show(Cursor.Position);
                }
            }
        }

        private void OpenToolStripMenuItem_Click (object sender, EventArgs e)
        {
            var item = LVMonitorDetail.FocusedItem;
            if ( item != null && uint.TryParse(item.SubItems[1].Text, out uint pid) && _monitorManager.ContainsKey(pid) )
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
            if ( item != null && uint.TryParse(item.SubItems[1].Text, out uint pid) && _monitorManager.ContainsKey(pid) )
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
            if ( item != null && uint.TryParse(item.SubItems[1].Text, out uint pid) && _monitorManager.ContainsKey(pid) )
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
            if ( item != null && uint.TryParse(item.SubItems[1].Text, out uint pid) && _monitorManager.ContainsKey(pid) )
            {
                var v = _monitorManager[pid];
                if ( !v.IsStop() )
                {
                    Point loc = this.Location;
                    Size sz = this.Size;
                    Point location = new Point(loc.X + sz.Width /2 , loc.Y + sz.Height/2);
                    CustomMessageBox mf = new(this, "确定要删除？","确认", location, MessageBoxButtons.OKCancel);
                    location = new Point(loc.X + (sz.Width - mf.Size.Width) / 2, loc.Y + (sz.Height - mf.Size.Height) / 2);
                    mf.Location = location;
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

                    if ( v1 != null && v1.history != null )
                        v1.history.Running = false;

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
                    && uint.TryParse(item.SubItems[1].Text, out uint pid) && _monitorManager.ContainsKey(pid) )
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
            if ( item != null && uint.TryParse(item.SubItems[1].Text, out uint pid) && _monitorManager.ContainsKey(pid) )
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
    }
}