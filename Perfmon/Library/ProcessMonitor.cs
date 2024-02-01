using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using System.Diagnostics;

namespace PerfMonitor
{
    delegate void UpdateMonitorStatusDelegate (ref RunStatusItem status);

    internal class RunStatusItem
    {
        private uint pid = 0;
        private string procName = "undefined";
        private double cpu = 0;
        private double vMem = 0;
        private double phyMem = 0;
        private double downLink = 0;
        private double upLink = 0;
        private double totalUpLink = 0;
        private double totalDownLink = 0;
        private long excuteSeconds = 0;
        private string excuteStatus = "no exist";
        private double sysCpu = 0;
        private double cpuPerf = 0;

        public uint Pid { get => pid; set => pid = value; }
        public string ProcName { get => procName; set => procName = value; }
        public double Cpu { get => cpu; set => cpu = value; }
        public double VMem { get => vMem; set => vMem = value; }
        public double PhyMem { get => phyMem; set => phyMem = value; }
        public double DownLink { get => downLink; set => downLink = value; }
        public double UpLink { get => upLink; set => upLink = value; }
        public double TotalUpLink { get => totalUpLink; set => totalUpLink = value; }
        public double TotalDownLink { get => totalDownLink; set => totalDownLink = value; }
        public long ExcuteSeconds { get => excuteSeconds; set => excuteSeconds = value; }
        public string ExcuteStatus { get => excuteStatus; set => excuteStatus = value; }
        public double SysCpu { get => sysCpu; set => sysCpu = value; }
        public double CpuPerf { get => cpuPerf; set => cpuPerf = value; }

        public string[] Info()
        {
            string uposfix = " Kbps";
            string dposfix = " Kbps";
            double totalu = TotalUpLink / 1024.0f;
            double totald = TotalDownLink / 1024.0f;
            double up = UpLink;
            double down = DownLink;

            if(up > 1 << 10)
            {
                up /= 1024.0f;
                uposfix = " Mbps";
            }

            if (down > 1 << 10)
            {
                down /= 1024.0f;
                dposfix = " Mbps";
            }

            return new string[] {
                $"{Pid}",
                ProcName,
                $"{TimeSpan.FromSeconds(ExcuteSeconds)} s",
                $"{Cpu :F2}%",
                $"{VMem/1024 :F2} GB",
                $"{PhyMem :F2} MB",
                $"{down :F2}{dposfix}",
                $"{up :F2}{uposfix}",
                $"{totald :F2} MB",
                $"{totalu :F2} MB",
                $"{ExcuteStatus}",
                $"{sysCpu :F2}%",
                $"{cpuPerf :F2}%"
            };
        }
    }

    internal class NetspeedTrace {
        public long send = 0;
        public long received = 0;
    };

    internal class ProcessMonitor : IDisposable
    {
        private readonly uint _pid = 0;

        private readonly int _interval = 1000;
        private bool _endTask = true;
        private readonly Task? _task;

        private RunStatusItem _onceRes = new();

        private readonly System.Diagnostics.Process? _process;

        private readonly UpdateMonitorStatusDelegate? _updateMonitorStatus;

        private TraceEventSession ?_traceSession = null;
        private readonly NetspeedTrace _netspeedDetail = new();
        private readonly NetspeedTrace _netspeedDetailOld = new();
        private readonly string _desc = "invalid process desc";

        void ProcessExitEventHandler(object? sender, EventArgs e)
        {
            _endTask = true;
            _task?.Wait();
            _onceRes.ExcuteStatus = "exit";

            _updateMonitorStatus?.Invoke(ref _onceRes);
        }

        public string Descriptor()
        {
            return _desc;
        }

        public ProcessMonitor(uint pid, int interval, UpdateMonitorStatusDelegate UpdateHandle) 
        {
            _pid = pid;
            _interval = interval;
            _onceRes.ExcuteStatus = "running";
            
            try
            {
                _process = System.Diagnostics.Process.GetProcessById((int)pid);
            }
            catch (ArgumentException)
            {
                _onceRes.ExcuteStatus = "no exist";
            }

            if (_process != null)
            {
                _desc = $" {_process.ProcessName}:{_pid} ";

                _endTask = false;
                _onceRes.Pid = _pid;
                _onceRes.ProcName = _process.ProcessName;

                _updateMonitorStatus = UpdateHandle;
                _process.EnableRaisingEvents = true;
                _process.Exited += new EventHandler(ProcessExitEventHandler);

                _task = new Task(() =>
                {
                    long lastMonitorTicks = 0;
                    double lastProcessorTime = 0;
                    double cores = 100.0f / Environment.ProcessorCount;
                    NetspeedTrace netspeedTracer = new();

                    string strQuery = $"\\Process({_process.ProcessName})\\% Processor Time";
                    if ((Environment.OSVersion.Version.Major == 10 && Environment.OSVersion.Version.Build >= 22000) 
                        || Environment.OSVersion.Version.Major > 10)
                    {
                        strQuery = $"\\Process V2({_process.ProcessName}:{_pid})\\% Processor Time";
                    }
                    else
                    {
                        bool bMultiInstance = false;
                        uint instanceCount = 0;

                        while (true)
                        {
                            string pidQuery;
                            if (!bMultiInstance)
                            {
                                pidQuery = $"\\Process({_process.ProcessName})\\ID Process";
                            }
                            else
                            {
                                pidQuery = $"\\Process({_process.ProcessName}#{instanceCount})\\ID Process";
                            }

                            uint v = (uint)QueryOnce.Query(pidQuery);
                            if (v == _pid)
                            {
                                if(instanceCount != 0)
                                    strQuery = $"\\Process({_process.ProcessName}#{instanceCount})\\% Processor Time";
                                else
                                    strQuery = $"\\Process({_process.ProcessName})\\% Processor Time";
                                break;
                            }
                            else
                            {
                                bMultiInstance = true;
                                instanceCount++;
                            }

                            if(instanceCount > 100)
                            {
                                break;
                            }
                        }
                    }

                    using PerfQuery cpuUsage = new(strQuery);

                    Stopwatch sw = Stopwatch.StartNew();
                    long firstMonitorTicks = sw.ElapsedMilliseconds;
                    while ( !_endTask )
                    {
                        try
                        {
                            _process.Refresh();
                        }
                        catch ( Exception e ) { var _ = e; }
                        
                        _onceRes.VMem = _process.VirtualMemorySize64 / 1048576.0f;
                        _onceRes.PhyMem = _process.WorkingSet64 / 1048576.0f;

                        double nowProcessorTime = _process.TotalProcessorTime.TotalMilliseconds;
                        long nowTicks = sw.ElapsedMilliseconds;
                        if ( lastMonitorTicks != 0 && lastProcessorTime != 0 )
                        {
                            _onceRes.CpuPerf = cpuUsage?.NextValue() ?? 0;
                            _onceRes.Cpu = Math.Round((nowProcessorTime - lastProcessorTime) * cores / (nowTicks - lastMonitorTicks), 2);
                            lastMonitorTicks = nowTicks;
                            lastProcessorTime = nowProcessorTime;

                            _onceRes.ExcuteSeconds = (nowTicks - firstMonitorTicks) / 1000;

                            netspeedTracer.send = _netspeedDetail.send;
                            netspeedTracer.received = _netspeedDetail.received;

                            _onceRes.UpLink = (netspeedTracer.send - _netspeedDetailOld.send) * 8 / 1024.0f;
                            _onceRes.DownLink = (netspeedTracer.received - _netspeedDetailOld.received) * 8 / 1024.0f;
                            _onceRes.TotalUpLink = netspeedTracer.send / 1024.0f;
                            _onceRes.TotalDownLink = _netspeedDetailOld.received / 1024.0f;

                            _updateMonitorStatus?.Invoke(ref _onceRes);
                        }

                        var q = sw.ElapsedMilliseconds;
                        var d = _interval - (q % _interval);
                        Thread.Sleep(TimeSpan.FromMilliseconds(d));

                        lastMonitorTicks = nowTicks;
                        lastProcessorTime = nowProcessorTime;
                        _netspeedDetailOld.send = netspeedTracer.send;
                        _netspeedDetailOld.received = netspeedTracer.received;
                    }
                });

                Task.Run(() => { StartEtwSession(); });
                _task.Start();
            }
        }

        private void StartEtwSession()
        {
            var processId = _pid;
            _netspeedDetail.send = 0;
            _netspeedDetail.received = 0;
            
            var sessionName = $"Perfmon_KernelAndClrEventsSession-{Guid.NewGuid()}-{_pid}";
            using ( _traceSession = new(sessionName, TraceEventSessionOptions.Create) )
            {
                _traceSession.EnableKernelProvider(KernelTraceEventParser.Keywords.NetworkTCPIP);

                _traceSession.Source.Kernel.TcpIpRecv += data =>
                {
                    if ( data.ProcessID == processId )
                    {
                        _netspeedDetail.received += data.size;
                    }
                };

                _traceSession.Source.Kernel.TcpIpSend += data =>
                {
                    if ( data.ProcessID == processId )
                    {
                        _netspeedDetail.send += data.size;
                    }
                };

                _traceSession.Source.Kernel.UdpIpRecv += data =>
                {
                    if ( data.ProcessID == processId )
                    {
                        _netspeedDetail.received += data.size;
                    }
                };

                _traceSession.Source.Kernel.UdpIpSend += data =>
                {
                    if ( data.ProcessID == processId )
                    {
                        _netspeedDetail.send += data.size;
                    }
                };

                _traceSession.Source.Process();
            }
        }

        public void Dispose()
        {
            _endTask = true;
            _traceSession?.Stop(true);
            _traceSession?.Dispose();
            _traceSession = null;
            _task?.Wait();
        }
    }
}
