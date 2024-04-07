using Microsoft.Diagnostics.Tracing.Parsers.FrameworkEventSource;
using ScottPlot.Styles;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using static PerfMonitor.ProcsEnumForm;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace PerfMonitor
{
    public partial class ProcsEnumForm : Form
    {
        private readonly string[] _colHeaders = new string[] { "进程名", "PID", "窗口" };
        private static readonly string[] _colDefaultValues = new string[] { "0", "0", "-"};
        private static readonly int[] _colSize = new int[] { 150, 80, 150};
        private bool _exit = false;

        private int pid = DefaultPID();
        private string _filter = "";
        private List<Process> _ps_history = new();

        public int Pid { get => pid; set => pid = value; }
        public string Filter { set => _filter = value.ToLower(); }

        public static int DefaultPID () { return -1; }

        public ProcsEnumForm (Form parent)
        {
            InitializeComponent();
            ConstructLV();
            Owner = parent;

            Point loc = parent.Location;
            Size sz = parent.Size;
            Point location = new Point(loc.X + sz.Width /2 , loc.Y + sz.Height/2);
            location = new Point(loc.X + (sz.Width - Size.Width) / 2, loc.Y + (sz.Height - Size.Height) / 2);
            Location = location;
            _ = EnumProcs();
            _ = FilterProcs();

            this.FormClosing += thisFormClosing;
        }

        private void thisFormClosing (object? sender, FormClosingEventArgs e)
        {
            _exit = true;
        }

        private void ConstructLV ()
        {
            LVProcss.Columns.Clear();

            for ( int i = 0; i < _colSize.Length; i++ )
            {
                ColumnHeader ch = new()
                {
                    Width = _colSize[i],
                    TextAlign = HorizontalAlignment.Left,
                    Text = _colHeaders?[i],
                };
                LVProcss.Columns.Add(ch);
            }
        }
        private void LVProcss_MouseDoubleClick (object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo info = LVProcss.HitTest(e.X, e.Y);
            ListViewItem item = info.Item;
            if ( item != null )
            {
                var pro = item.Tag as Process;
                if ( pro != null )
                {
                    Pid = pro.Id;
                    _exit = true;
                    Close();
                }
            }
        }

        public class ProcessComparer<T> : IEqualityComparer<Process>
        {
            public bool Equals (Process? t1, Process? t2)
            {
                if ( t1 != null && t2 != null && t1.Id == t2.Id )
                {
                    return true;
                }

                return false;
            }
            public int GetHashCode (Process _obj)
            {
                return _obj.GetHashCode();
            }
        }
        async Task EnumProcs ()
        {
            while ( !_exit )
            {
                lock ( _ps_history )
                {
                    _ps_history = Process.GetProcesses().ToList();
                }
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
            }
        }

        async Task FilterProcs ()
        {
            List<Process> ps = new();
            IEqualityComparer<Process> ProcessCompare = new ProcessComparer<Process>();
            int seconds = 0;

            int tick = 200;

            while ( !_exit )
            {
                lock ( _ps_history )
                {
                    ps = _ps_history.ToList();
                }

                ps.Sort((Process a, Process b) =>
                {
                    int ret = a.ProcessName.CompareTo(b.ProcessName);
                    return ret == 0 ? a.Id - b.Id : ret;
                });

                labelProcess.Text = $"Total: {ps.Count}, {LVProcss.Items.Count}, {seconds++ / (1000/tick)}";

                if ( _filter.Length > 0 )
                {
                    ps = ps.FindAll(e => e.ProcessName.ToLower().Contains(_filter) || e.Id.ToString().Contains(_filter));
                }

                LVProcss.BeginUpdate();
                int psindex = 0;
                int lvindex = 0;
                for (; ; )
                {
                    if ( ps.Count <= 0 )
                    {
                        LVProcss.Items.Clear();
                        break;
                    }

                    if ( psindex > ps.Count - 1 || (lvindex > LVProcss.Items.Count - 1))
                    {
                        break;
                    }

                    var psproc = ps[psindex];
                    var lvproc = LVProcss.Items[lvindex].Tag as Process;
                    if ( lvproc != null )
                    {
                        int lvnamebigger = lvproc.ProcessName.CompareTo(psproc.ProcessName);
                        int lvpidbigger = lvproc.Id == psproc.Id ? 0 : lvproc.Id > psproc.Id ?  1 : -1;
                        if ( lvnamebigger == 0 && lvpidbigger == 0 )
                        {
                            psindex++;
                            lvindex++;
                        }
                        else if ( lvnamebigger == 0 && lvpidbigger < 0 )
                        {
                            var proc_alive = ps.Find(e => e.Id == lvproc.Id) != null ;
                            Debug.Assert(!proc_alive);
                            if ( !proc_alive )
                            {
                                LVProcss.Items.RemoveAt(lvindex);
                            }
                        }
                        else if ( lvnamebigger > 0 || (lvnamebigger == 0 && lvpidbigger > 0) )
                        {
                            string [] v = new string[]{ psproc.ProcessName, psproc.Id.ToString(), psproc.MainWindowTitle };
                            var lvi = new ListViewItem(v)
                            {
                                Tag = psproc,
                            };
                            LVProcss.Items.Insert(lvindex, lvi);
                            lvindex++;
                            psindex++;
                        }
                        else
                        {
                            var proc_alive = ps.Find(e => e.Id == lvproc.Id) != null ;
                            Debug.Assert(!proc_alive);
                            if ( !proc_alive )
                            {
                                LVProcss.Items.RemoveAt(lvindex);
                            }
                            else
                            {
                                lvindex++;
                            }
                        }
                    }
                }

                if (LVProcss.Items.Count > ps.Count)
                {
                    int count = LVProcss.Items.Count;
                    for(var idx = lvindex;lvindex < count; lvindex++ )
                    {
                        LVProcss.Items.RemoveAt(idx);
                    }
                }
                else if ( LVProcss.Items.Count < ps.Count )
                {
                    for ( var idx = psindex; idx < ps.Count; idx++ )
                    {
                        string [] v = new string[]{ ps[idx].ProcessName, ps[idx].Id.ToString(), ps[idx].MainWindowTitle };
                        var lvi = new ListViewItem(v)
                        {
                            Tag = ps[idx],
                        };
                        LVProcss.Items.Insert(lvindex, lvi);
                        lvindex++;
                    }
                }
                LVProcss.EndUpdate();
                await Task.Delay(TimeSpan.FromMilliseconds(tick));
            }
        }

        private void textBoxPidOrName_KeyUp (object sender, KeyEventArgs e)
        {
            Filter = textBoxPidOrName.Text;
            if(e.KeyCode == Keys.Enter)
            {
                if(LVProcss.Items.Count > 0)
                {
                    var item = LVProcss.Items[0];
                    if ( item != null )
                    {
                        var pro = item.Tag as Process;
                        if ( pro != null )
                        {
                            Pid = pro.Id;
                            _exit = true;
                            Close();
                        }
                    }
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                _exit = true;
                Close();
            }
        }
    }
}
