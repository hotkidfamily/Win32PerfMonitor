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
            Application.Run(_mainForm);
        }

        private static void Application_ThreadException (object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            if ( _mainForm != null )
            {
                _mainForm.Close_when_exception = true;
            }
            var dr = handle();
            if ( dr == DialogResult.OK )
            {
                Application.Restart();
            }
        }

        private static void CurrentDomain_UnhandledException (object sender, UnhandledExceptionEventArgs e)
        {
            if ( _mainForm != null )
            {
                _mainForm.Close_when_exception = true;
            }
            var dr = handle();
            if ( dr == DialogResult.OK )
            {
                Application.Restart();
            }
        }

        private static DialogResult handle ()
        {
            if ( _mainForm == null )
            {
                return DialogResult.Cancel;
            }

            CustomMessageBox mf = new(_mainForm, "发生异常，要重启嘛？","确认", MessageBoxButtons.OKCancel);
            mf.ShowDialog();
            DialogResult dr = mf.ShowResult();
            return dr;
        }
    }

}