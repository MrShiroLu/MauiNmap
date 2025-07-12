using SQLite;
using System;

namespace NmapMaui.Models
{
    public class Encryption : BaseModel
    {
        public string Input { get; set; }
        public string Output { get; set; }
        public bool IsEncrypt { get; set; }
        public DateTime Date { get; set; }

        public Encryption()
        {
            Date = DateTime.Now;
        }
    }
} 