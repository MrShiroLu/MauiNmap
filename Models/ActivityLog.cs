using System;

namespace NmapMaui.Models
{
    public class ActivityLog : BaseModel
    {
        public string Action { get; set; } = string.Empty;      // e.g. "Login", "PortScan", "Export"
        public string Category { get; set; } = string.Empty;    // e.g. "Auth", "Network", "Crypto"
        public string Details { get; set; } = string.Empty;     // free-form text (params, target, etc.)
        public string Level { get; set; } = string.Empty;       // "Info", "Warning", "Error"
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
