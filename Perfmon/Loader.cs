using Microsoft.VisualBasic.Logging;
using PerfMonitor.Library;

namespace PerfMonitor
{
    public class Loader
    {
        private static MainForm? _mainForm = null;
        [STAThread]
        static void Main(string[] args)
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            _mainForm = new();
            _mainForm.ShowDialog();
        }

        private static void Application_ThreadException (object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            var dr = handle();
            if ( dr == DialogResult.OK )
            {
                _mainForm?.Close();
                Application.Restart();
            }
        }

        private static void CurrentDomain_UnhandledException (object sender, UnhandledExceptionEventArgs e)
        {
            var dr = handle();
            if ( dr == DialogResult.OK )
            {
                _mainForm?.Close();
                Application.Restart();
            }
        }

        private static DialogResult handle ()
        {
            if ( _mainForm == null )
            {
                return DialogResult.Cancel;
            }

            Point loc = _mainForm.Location;
            Size sz = _mainForm.Size;
            Point location = new Point(loc.X + sz.Width /2 , loc.Y + sz.Height/2);

            CustomMessageBox mf = new(_mainForm, "发生异常，要重启嘛？","确认", location, MessageBoxButtons.OKCancel);
            location = new Point(loc.X + (sz.Width - mf.Size.Width) / 2, loc.Y + (sz.Height - mf.Size.Height) / 2);
            mf.Location = location;
            mf.ShowDialog();
            DialogResult dr = mf.ShowResult();
            return dr;
        }
    }

}