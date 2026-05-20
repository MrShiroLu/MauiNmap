using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NmapMaui.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NmapMaui.Services
{
    public class ExportService : IExportService
    {
        public ExportService()
        {
            // QuestPDF community license — required by the library on first use.
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<string> ExportAsync(IEnumerable<ScanResult> results, ExportFormat format, string baseFileName)
        {
            var dir = FileSystem.AppDataDirectory;
            Directory.CreateDirectory(dir);
            var ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var list = results.ToList();

            switch (format)
            {
                case ExportFormat.Json:
                {
                    var path = Path.Combine(dir, $"{baseFileName}_{ts}.json");
                    var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(path, json);
                    return path;
                }
                case ExportFormat.Csv:
                {
                    var path = Path.Combine(dir, $"{baseFileName}_{ts}.csv");
                    var sb = new StringBuilder();
                    sb.AppendLine("Target,ScanType,ScanTime,IsSuccess,Result");
                    foreach (var r in list)
                        sb.AppendLine($"{Csv(r.Target)},{Csv(r.ScanType)},{r.ScanTime:o},{r.IsSuccess},{Csv(r.Result)}");
                    await File.WriteAllTextAsync(path, sb.ToString());
                    return path;
                }
                case ExportFormat.Pdf:
                {
                    var path = Path.Combine(dir, $"{baseFileName}_{ts}.pdf");
                    Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(30);
                            page.Header().Text($"NmapMaui Report — {DateTime.Now:yyyy-MM-dd HH:mm}").SemiBold().FontSize(16);
                            page.Content().Column(col =>
                            {
                                foreach (var r in list)
                                {
                                    col.Item().PaddingVertical(5).Column(c =>
                                    {
                                        c.Item().Text($"{r.ScanType} — {r.Target}").SemiBold();
                                        c.Item().Text($"Time: {r.ScanTime}  Success: {r.IsSuccess}").FontSize(10);
                                        c.Item().Text(r.Result ?? string.Empty).FontSize(9);
                                    });
                                }
                            });
                            page.Footer().AlignCenter().Text(x =>
                            {
                                x.Span("Page ");
                                x.CurrentPageNumber();
                                x.Span(" / ");
                                x.TotalPages();
                            });
                        });
                    }).GeneratePdf(path);
                    return path;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(format));
            }
        }

        private static string Csv(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "\"\"";
            return "\"" + s.Replace("\"", "\"\"").Replace("\r", " ").Replace("\n", " ") + "\"";
        }
    }
}
