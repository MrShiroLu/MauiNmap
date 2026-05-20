using System.Collections.Generic;
using System.Threading.Tasks;
using NmapMaui.Models;

namespace NmapMaui.Services
{
    public enum ExportFormat { Json, Csv, Pdf }

    public interface IExportService
    {
        Task<string> ExportAsync(IEnumerable<ScanResult> results, ExportFormat format, string baseFileName);
    }
}
