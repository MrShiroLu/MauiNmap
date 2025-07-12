using SQLite;
using System;

namespace NmapMaui.Models
{
    public class Base64 : BaseModel
    {
        public string Input { get; set; }
        public string Output { get; set; }
        public bool IsEncode { get; set; }
        public DateTime Date { get; set; }

        public Base64()
        {
            Date = DateTime.Now;
        }
    }
} 