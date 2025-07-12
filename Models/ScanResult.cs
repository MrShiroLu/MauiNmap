using System;

namespace NmapMaui.Models
{
    public class ScanResult
    {
        public int Id { get; set; }
        public required string Target { get; set; }
        public required string Result { get; set; }
        public DateTime ScanTime { get; set; }
        public required string ScanType { get; set; }
        public bool IsSuccess { get; set; }
    }
} 