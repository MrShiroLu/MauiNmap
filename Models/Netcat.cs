using System;

namespace NmapMaui.Models
{
    public class Netcat : BaseModel
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Result { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
