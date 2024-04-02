using PerfMonitor.Library;
using System.Diagnostics;
using static PerfMonitor.MainForm;

namespace PerfMonitor
{
    public partial class HistoryForm : Form
    {
        private readonly string[] _columns = new string[] { "测试内容", "PID", "进程名", "测试开始","测试时长", "结果" };
        private readonly int[] _columnsWidth = new int[] { 100, 100, 100, 150, 150, 500 };
        private readonly HistoryController _history;

        public HistoryForm (object history)
        {
            InitializeComponent();
            LVHistory.Columns.Clear();

            for ( int i = 0; i < _columns?.Length; i++ )
            {
                ColumnHeader ch = new()
                {
                    Width = _columnsWidth[i],
                    TextAlign = HorizontalAlignment.Left,
                    Text = _columns?[i],
                };
                LVHistory.Columns.Add(ch);
            }

            _history = (HistoryController) history;
            LVHistory.BeginUpdate();
            foreach ( var res in _history.History )
            {
                var lvi = new ListViewItem(res.Info())
                {
                    Tag = res
                };
                LVHistory.Items.Add(lvi);
            }
            LVHistory.EndUpdate();

            this.Text = $"History Viewer {Properties.Resources.AppVersion}";
        }

        private void LVHistory_MouseClick (object sender, MouseEventArgs e)
        {
            if ( e.Button == MouseButtons.Right )
            {
                var item = LVHistory.FocusedItem;
                if ( item != null && item.Bounds.Contains(e.Location) )
                {
                    HistoryMenuStrip.Show(Cursor.Position);
                    var ctx = (HistoryContext)item.Tag;
                    if ( ctx != null && ctx.Running )
                    {
                        DeleteToolStripMenuItem.Enabled = false;
                    }
                    else
                    {
                        DeleteToolStripMenuItem.Enabled = true;
                    }
                }
            }
        }

        private void OpenToolStripMenuItem_Click (object sender, EventArgs e)
        {
            var item = LVHistory.FocusedItem;
            if ( item != null )
            {
                HistoryContext v = (HistoryContext)item.Tag;

                ProcessStartInfo psi = new()
                {
                    FileName = v.ResPath,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
        }

        private void DeleteToolStripMenuItem_Click (object sender, EventArgs e)
        {
            var items = LVHistory.SelectedItems;
            foreach ( ListViewItem item in items )
            {
                HistoryContext v = (HistoryContext)item.Tag;
                if ( !v.Running )
                {
                    _history.RemoveItem(v);
                    LVHistory.Items.Remove(item);
                }
            }
        }

        private void ModifyMarkerToolStripMenuItem_Click (object sender, EventArgs e)
        {
            var item = LVHistory.FocusedItem;
            item?.BeginEdit();
        }

        private void LVHistory_AfterLabelEdit (object sender, LabelEditEventArgs e)
        {
            if ( e.Label != null )
            {
                string editedText = e.Label;

                var item = LVHistory.FocusedItem;
                var ctx = (HistoryContext)item.Tag;
                if ( ctx != null )
                {
                    ctx.Marker = editedText;
                    _history.Write();
                }
            }
        }

        private void LVHistory_KeyDown (object sender, KeyEventArgs e)
        {
            var item = LVHistory.FocusedItem;
            if ( e.KeyCode == Keys.F2 && item != null )
            {
                item.BeginEdit();
            }
            else if ( e.KeyCode == Keys.F5 )
            {
                LVHistory.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

                LVHistory.BeginUpdate();
                for ( int i = 0; i <= LVHistory.Columns.Count - 1; i++ )
                {
                    LVHistory.Columns[i].Width += 20;
                }
                LVHistory.EndUpdate();
            }
            else if(e.KeyCode == Keys.Escape )
            {
                HistoryForm_KeyDown(this, e);
            }
        }

        private void FreshToolStripMenuItem_Click (object sender, EventArgs e)
        {
            LVHistory.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

            LVHistory.BeginUpdate();
            for ( int i = 0; i <= LVHistory.Columns.Count - 1; i++ )
            {
                LVHistory.Columns[i].Width += 20;
            }
            LVHistory.EndUpdate();
        }

        private void LVHistory_MouseDoubleClick (object sender, MouseEventArgs e)
        {
            var item = LVHistory.FocusedItem;
            if ( item != null )
            {
                var ctx = (HistoryContext)item.Tag;
                if ( ctx != null && !ctx.Running )
                {
                    if ( ctx.VisualForm == null )
                    {
                        string desc = $" - {ctx.Name}( {ctx.Pid} )( {ctx.Begin} )";
                        var visual = new VisualForm(ctx.ResPath, desc, ctx.Begin);
                        visual.Show();
                        visual.FormClosed += Visual_FormClosed;
                        visual.Location = Location + new Size(50, 50);
                        ctx.VisualForm = visual;
                        ctx.VisualForm.Tag = ctx;
                    }
                    else
                    {
                        ctx.VisualForm.Location = Location + new Size(50, 50);
                        ctx.VisualForm.Focus();
                    }
                }
            }
        }

        private void Visual_FormClosed (object? sender, FormClosedEventArgs e)
        {
            var form = sender as VisualForm;
            HistoryContext? b = form?.Tag as HistoryContext?? null;
            if ( b != null )
            {
                b.VisualForm = null;
            }
        }

        private void HistoryForm_KeyDown (object sender, KeyEventArgs e)
        {
            if ( e.KeyCode == Keys.Escape )
            {
                var item = LVHistory.FocusedItem;
                if ( item != null )
                {
                    var ctx = (HistoryContext)item.Tag;
                    if ( ctx != null && !ctx.Running )
                    {
                        ctx.VisualForm?.Close();

                    }
                }
                Close();
            }
        }
    }
}