using SQLite;
using System;

namespace NmapMaui.Models
{
    public class PassStr : BaseModel
    {
        public string Input { get; set; }
        public string Output { get; set; }
        public string Result { get; set; }
        public DateTime Date { get; set; }

        public PassStr()
        {
            Date = DateTime.Now;
        }
    }
} 