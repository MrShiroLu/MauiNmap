using System;

namespace NmapMaui.Models
{
    public class Gobuster : BaseModel
    {
        public string Target { get; set; } = string.Empty;
        public string Wordlist { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
