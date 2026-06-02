using System;

namespace NmapMaui.Models
{
    public class BaseModel
    {
        public int Id { get; set; }
        
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public BaseModel()
        {
        }
    }
} 