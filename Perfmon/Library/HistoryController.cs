using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace PerfMonitor.Library
{
    internal class HistoryContext
    {
        private uint pid;
        private string marker = string.Empty;
        private string name = string.Empty;
        private DateTime beginTime = default;
        private DateTime endTime = default;
        private string resPath = string.Empty;
        private Form ? _visualForm;

        [JsonPropertyName("Pid")]
        public uint Pid { get => pid; set => pid = value; }
        [JsonPropertyName("Marker")]
        public string Marker { get => marker; set => marker = value; }
        [JsonPropertyName("Begin")]
        public DateTime Begin { get => beginTime; set => beginTime = value; }
        [JsonPropertyName("ResPath")]
        public string ResPath { get => resPath; set => resPath = value; }
        [JsonIgnore]
        public Form? VisualForm { get => _visualForm; set => _visualForm = value; }

        [JsonPropertyName("End")]
        public DateTime End { get => endTime; set => endTime = value; }

        [JsonPropertyName("Name")]
        public string Name { get => name; set => name = value; }

        [JsonIgnore]
        public bool Running = false;

        public string[] Info ()
        {
            return new string[] {
                $"{Marker}", $"{Pid}", $"{Name}", $"{Begin}", $"{(End - Begin).ToString("hh\\:mm\\:ss")}", $"{ResPath}"
            };
        }
    }

    internal class HistoryController
    {
        private readonly string version = "1.0";
        private string machine = string.Empty;
        private List<HistoryContext> history = new();

        [JsonPropertyName("Version")]
        public string Version { get => version; }
        [JsonPropertyName("Machine")]
        public string Machine { get => machine; set => machine = value; }
        [JsonPropertyName("History")]
        public List<HistoryContext> History { get => history; set => history = value; }

        private string _path = string.Empty;

        public HistoryController () // for Deserialize
        {
        }
        public HistoryController (string path)
        {
            _path = path;
        }

        public HistoryContext AddItem (uint pid, string name, string respath, string marker)
        {
            var item = new HistoryContext()
            {
                Pid = pid,
                ResPath = respath,
                Name = name,
                Marker = marker,
                Begin = DateTime.Now,
                End = DateTime.Now,
                Running = true,
            };
            History.Add(item);
            Write();
            return item;
        }


        public void RemoveItem (HistoryContext item)
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