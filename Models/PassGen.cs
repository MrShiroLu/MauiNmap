using SQLite;
using System;

namespace NmapMaui.Models
{
    public class PassGen : BaseModel
    {
        public string Input { get; set; }
        public string Output { get; set; }
        public DateTime Date { get; set; }

        public PassGen()
        {
            Date = DateTime.Now;
        }
    }
} 