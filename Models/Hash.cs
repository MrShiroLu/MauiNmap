using SQLite;
using System;

namespace NmapMaui.Models
{
    public class Hash : BaseModel
    {
        public string Input { get; set; }
        public string Algorithm { get; set; }
        public string Output { get; set; }
        public DateTime Date { get; set; }

        public Hash()
        {
            Date = DateTime.Now;
        }
    }
} 