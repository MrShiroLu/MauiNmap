using SQLite;
using System;

namespace NmapMaui.Models
{
    public class BaseModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
//        public int CreatedByUserId { get; set; }
  //      public string UpdatedBy { get; set; }
    //    public int? UpdatedByUserId { get; set; }

        public BaseModel()
        {
        }
    }
} 