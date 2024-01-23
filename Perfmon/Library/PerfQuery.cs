using Windows.Win32;
using PerfRoot = global::Windows.Win32.System.Performance;

namespace PerfMonitor
{
    class QueryOnce
    {
        public static double Query(string query)
        {
            nint _hQuery;
            nint _hCounter;
            _ = PInvoke.PdhOpenQuery(null, 0, out _hQuery);
            nuint user = 0;
            _ = PInvoke.PdhAddCounter(_hQuery, query, user, out _hCounter);

            PerfRoot.PDH_RAW_COUNTER rawData;

            _ = PInvoke.PdhCollectQueryData(_hQuery);

            uint type = 0;
            unsafe
            {
                _ = PInvoke.PdhGetRawCounterValue(_hCounter, &type, out rawData);
            }

            _ = PInvoke.PdhRemoveCounter(_hCounter);
            _ = PInvoke.PdhCloseQuery(_hQuery);

            return rawData.FirstValue;
        }
    }

    internal class PerfQuery : IDisposable
    {
        private nint _hQuery;
        private nint _hCounter;
        private PerfRoot.PDH_RAW_COUNTER _lastData;
        private bool _first = true;

        public PerfQuery(string query)
        {
            PInvoke.PdhOpenQuery(null, 0, out _hQuery);
            nuint user = 0;
            PInvoke.PdhAddCounter(_hQuery, query, user, out _hCounter);
        }

        public double NextValue()
        {
            double cpu = 0.0f;
            PerfRoot.PDH_RAW_COUNTER rawData;

            PInvoke.PdhCollectQueryData(_hQuery);

            uint type = 0;
            unsafe
            {
                PInvoke.PdhGetRawCounterValue(_hCounter, &type, out rawData);
            }

            if (!_first)
            {
                PerfRoot.PDH_FMT_COUNTERVALUE fmtValue;
                PerfRoot.PDH_FMT fmt = PerfRoot.PDH_FMT.PDH_FMT_DOUBLE;
                PInvoke.PdhCalculateCounterFromRawValue(_hCounter, fmt, rawData, _lastData, out fmtValue);
                cpu = fmtValue.Anonymous.doubleValue;
            }
            else { _first = false; }

            _lastData = rawData;

            return cpu;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                PInvoke.PdhRemoveCounter(_hCounter);
                PInvoke.PdhCloseQuery(_hQuery);
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
