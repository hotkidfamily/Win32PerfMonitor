using ScottPlot.Palettes;
using ScottPlot.Plottable.AxisManagers;
using System.Runtime.InteropServices;
using Windows.Win32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using PerfRoot = global::Windows.Win32.System.Performance;

namespace PerfMonitor
{
    enum PDHSTATUS : uint
    {
        PdhCstatusValidData = 0x00000000,// The returned data is valid.
        PdhCstatusNewData = 0x00000001,// The return data value is valid and different from the last sample.
        PdhCstatusNoMachine = 0x800007D0,// Unable to connect to the specified computer, or the computer is offline.
        PdhCstatusNoInstance = 0x800007D1,
        PdhMoreData = 0x800007D2,// The PdhGetFormattedCounterArray* function can return this if there's 'more data to be displayed'.
        PdhCstatusItemNotValidated = 0x800007D3,
        PdhRetry = 0x800007D4,
        PdhNoData = 0x800007D5,// The query does not currently contain any counters (for example, limited access)
        PdhCalcNegativeDenominator = 0x800007D6,
        PdhCalcNegativeTimebase = 0x800007D7,
        PdhCalcNegativeValue = 0x800007D8,
        PdhDialogCancelled = 0x800007D9,
        PdhEndOfLogFile = 0x800007DA,
        PdhAsyncQueryTimeout = 0x800007DB,
        PdhCannotSetDefaultRealtimeDatasource = 0x800007DC,
        PdhCstatusNoObject = 0xC0000BB8,
        PdhCstatusNoCounter = 0xC0000BB9,// The specified counter could not be found.
        PdhCstatusInvalidData = 0xC0000BBA,// The counter was successfully found, but the data returned is not valid.
        PdhMemoryAllocationFailure = 0xC0000BBB,
        PdhInvalidHandle = 0xC0000BBC,
        PdhInvalidArgument = 0xC0000BBD,// Required argument is missing or incorrect.
        PdhFunctionNotFound = 0xC0000BBE,
        PdhCstatusNoCountername = 0xC0000BBF,
        PdhCstatusBadCountername = 0xC0000BC0,// Unable to parse the counter path. Check the format and syntax of the specified path.
        PdhInvalidBuffer = 0xC0000BC1,
        PdhInsufficientBuffer = 0xC0000BC2,
        PdhCannotConnectMachine = 0xC0000BC3,
        PdhInvalidPath = 0xC0000BC4,
        PdhInvalidInstance = 0xC0000BC5,
        PdhInvalidData = 0xC0000BC6,// specified counter does not contain valid data or a successful status code.
        PdhNoDialogData = 0xC0000BC7,
        PdhCannotReadNameStrings = 0xC0000BC8,
        PdhLogFileCreateError = 0xC0000BC9,
        PdhLogFileOpenError = 0xC0000BCA,
        PdhLogTypeNotFound = 0xC0000BCB,
        PdhNoMoreData = 0xC0000BCC,
        PdhEntryNotInLogFile = 0xC0000BCD,
        PdhDataSourceIsLogFile = 0xC0000BCE,
        PdhDataSourceIsRealTime = 0xC0000BCF,
        PdhUnableReadLogHeader = 0xC0000BD0,
        PdhFileNotFound = 0xC0000BD1,
        PdhFileAlreadyExists = 0xC0000BD2,
        PdhNotImplemented = 0xC0000BD3,
        PdhStringNotFound = 0xC0000BD4,
        PdhUnableMapNameFiles = 0x80000BD5,
        PdhUnknownLogFormat = 0xC0000BD6,
        PdhUnknownLogsvcCommand = 0xC0000BD7,
        PdhLogsvcQueryNotFound = 0xC0000BD8,
        PdhLogsvcNotOpened = 0xC0000BD9,
        PdhWbemError = 0xC0000BDA,
        PdhAccessDenied = 0xC0000BDB,
        PdhLogFileTooSmall = 0xC0000BDC,
        PdhInvalidDatasource = 0xC0000BDD,
        PdhInvalidSqldb = 0xC0000BDE,
        PdhNoCounters = 0xC0000BDF,
        PdhSQLAllocFailed = 0xC0000BE0,
        PdhSQLAllocconFailed = 0xC0000BE1,
        PdhSQLExecDirectFailed = 0xC0000BE2,
        PdhSQLFetchFailed = 0xC0000BE3,
        PdhSQLRowcountFailed = 0xC0000BE4,
        PdhSQLMoreResultsFailed = 0xC0000BE5,
        PdhSQLConnectFailed = 0xC0000BE6,
        PdhSQLBindFailed = 0xC0000BE7,
        PdhCannotConnectWmiServer = 0xC0000BE8,
        PdhPlaCollectionAlreadyRunning = 0xC0000BE9,
        PdhPlaErrorScheduleOverlap = 0xC0000BEA,
        PdhPlaCollectionNotFound = 0xC0000BEB,
        PdhPlaErrorScheduleElapsed = 0xC0000BEC,
        PdhPlaErrorNostart = 0xC0000BED,
        PdhPlaErrorAlreadyExists = 0xC0000BEE,
        PdhPlaErrorTypeMismatch = 0xC0000BEF,
        PdhPlaErrorFilepath = 0xC0000BF0,
        PdhPlaServiceError = 0xC0000BF1,
        PdhPlaValidationError = 0xC0000BF2,
        PdhPlaValidationWarning = 0x80000BF3,
        PdhPlaErrorNameTooLong = 0xC0000BF4,
        PdhInvalidSQLLogFormat = 0xC0000BF5,
        PdhCounterAlreadyInQuery = 0xC0000BF6,
        PdhBinaryLogCorrupt = 0xC0000BF7,
        PdhLogSampleTooSmall = 0xC0000BF8,
        PdhOsLaterVersion = 0xC0000BF9,
        PdhOsEarlierVersion = 0xC0000BFA,
        PdhIncorrectAppendTime = 0xC0000BFB,
        PdhUnmatchedAppendCounter = 0xC0000BFC,
        PdhSQLAlterDetailFailed = 0xC0000BFD,
        PdhQueryPerfDataTimeout = 0xC0000BFE,
    };

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

        public long NextValue()
        {
            long cpu = 0;
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
                PerfRoot.PDH_FMT fmt = PerfRoot.PDH_FMT.PDH_FMT_LARGE;
                PInvoke.PdhCalculateCounterFromRawValue(_hCounter, fmt, rawData, _lastData, out fmtValue);
                cpu = fmtValue.Anonymous.largeValue;
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


    internal class PerfQueryWildcard : IDisposable
    {
        private nint _hQuery;
        private nint _hCounter;
        private bool _first = true;

        public PerfQueryWildcard(string query)
        {
            PInvoke.PdhOpenQuery(null, 0, out _hQuery);
            nuint user = 0;
            PInvoke.PdhAddCounter(_hQuery, query, user, out _hCounter);
        }

        public long NextValue()
        {
            long cpu = 0;

            PInvoke.PdhCollectQueryData(_hQuery);
            if (!_first)
            {
                unsafe
                {
                    uint buffSize = 0;
                    uint itemCnt = 0;
                    PerfRoot.PDH_FMT_COUNTERVALUE_ITEM_W* items = null;
                    var status = PInvoke.PdhGetFormattedCounterArray(_hCounter, PerfRoot.PDH_FMT.PDH_FMT_LONG,
                        ref buffSize, out itemCnt, items);
                    if ((uint)PDHSTATUS.PdhMoreData == status)
                    {
                        items = (PerfRoot.PDH_FMT_COUNTERVALUE_ITEM_W*)Marshal.AllocHGlobal((int)buffSize);
                        if (items != null)
                        {
                            status = PInvoke.PdhGetFormattedCounterArray(_hCounter, PerfRoot.PDH_FMT.PDH_FMT_LONG, ref buffSize, out itemCnt, items);
                            if (0 == status)
                            {
                                // Loop through the array and print the instance name and counter value.
                                for (int i = 0; i < itemCnt; i++)
                                {
                                    PerfRoot.PDH_FMT_COUNTERVALUE_ITEM_W item = items[i];
                                    cpu += item.FmtValue.Anonymous.longValue;
                                }
                            }

                            Marshal.FreeHGlobal((nint)items);
                        }
                    }
                }
            }
            else { 
                _first = false;
            }
            
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
