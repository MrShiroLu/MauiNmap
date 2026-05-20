namespace NmapMaui.Api.Models;

public class ScanLog
{
    public int Id { get; set; }
    public string Tool { get; set; } = "";
    public string Target { get; set; } = "";
    public string Args { get; set; } = "";
    public string Output { get; set; } = "";
    public bool Success { get; set; }
    public DateTime Timestamp { get; set; }
}
