using SQLite;
using System;

namespace NmapMaui.Models
{
    public class Dns : BaseModel
    {
        public string Hostname { get; set; }
        public string IpAddress { get; set; }
        public DateTime Date { get; set; }

        public Dns()
        {
            Date = DateTime.Now;
        }
    }
} 