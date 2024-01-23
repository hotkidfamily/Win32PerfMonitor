namespace PerfMonitor
{
    public class Loader
    {
        static void Main(string[] args)
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Thread t = new (() =>
            {
                MainForm mainForm = new();
                mainForm.ShowDialog();
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }
    }

}