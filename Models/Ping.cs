using SQLite;
using System;

namespace NmapMaui.Models
{
    public class Ping : BaseModel
    {
        public string Input { get; set; }
        public DateTime Date { get; set; }

        public Ping()
        {
            Date = DateTime.Now;
        }
    }
} 