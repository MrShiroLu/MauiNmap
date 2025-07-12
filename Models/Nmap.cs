using SQLite;
using System;

namespace NmapMaui.Models
{
    public class Nmap : BaseModel
    {
        public string NmapPort { get; set; }
        public DateTime Date { get; set; }

        public Nmap()
        {
            Date = DateTime.Now;
        }
    }
} 