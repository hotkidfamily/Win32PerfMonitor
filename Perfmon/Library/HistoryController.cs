using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace PerfMonitor.Library
{
    internal class HistoryItem
    {
        private uint pid;
        private string marker = string.Empty;
        private string date = string.Empty;
        private string resPath = string.Empty;

        [JsonPropertyName("Pid")]
        public uint Pid { get => pid; set => pid = value; }
        [JsonPropertyName("Marker")]
        public string Marker { get => marker; set => marker = value; }
        [JsonPropertyName("Date")]
        public string Date { get => date; set => date = value; }
        [JsonPropertyName("ResPath")]
        public string ResPath { get => resPath; set => resPath = value; }

        public bool Running = false;

        public string[] Info ()
        {
            return new string[] {
                $"{Marker}", $"{Pid}", $"{Date}", $"{ResPath}"
            };
        }
    }

    internal class HistoryController
    {
        private readonly string version = "1.0";
        private string machine = string.Empty;
        private List<HistoryItem> history = new();

        [JsonPropertyName("Version")]
        public string Version { get => version; }
        [JsonPropertyName("Machine")]
        public string Machine { get => machine; set => machine = value; }
        [JsonPropertyName("History")]
        public List<HistoryItem> History { get => history; set => history = value; }

        private string _path = string.Empty;

        public HistoryController () // for Deserialize
        {
        }
        public HistoryController (string path)
        {
            _path = path;
        }

        public HistoryItem AddItem (uint pid, string respath, string marker)
        {
            var item = new HistoryItem()
            {
                Pid = pid,
                ResPath = respath,
                Marker = marker,
                Date = $"{DateTime.Now:yyyy.MMdd.HHmm.ss}",
                Running = true,
            };
            History.Add(item);
            Write();
            return item;
        }

        public void RemoveItem (HistoryItem item)
        {
            History.Remove(item);
            Write();
        }

        public void Write()
        {
            string json = JsonSerializer.Serialize(this);
            using var sw = new StreamWriter(_path, false);
            sw.Write(json);
        }

        public void Read ()
        {
            History?.Clear();
            if ( File.Exists(_path) )
            {
                using var sr = new StreamReader(new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                var json = sr.ReadToEnd();

                var v = JsonSerializer.Deserialize<HistoryController>(json);
                if ( v == null || v.version != version ) return;
                History = v.History;
            }
        }
    }
}