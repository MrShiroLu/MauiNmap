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
            var ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var list = results.ToList();
            string path = string.Empty;

            string projectDir = AppDomain.CurrentDomain.BaseDirectory;
            while (!string.IsNullOrEmpty(projectDir) && !File.Exists(Path.Combine(projectDir, "NmapMaui.csproj")))
            {
                var parent = Directory.GetParent(projectDir);
                projectDir = parent?.FullName;
            }
            if (string.IsNullOrEmpty(projectDir))
            {
                projectDir = AppDomain.CurrentDomain.BaseDirectory;
            }

            string documentsDir = Path.Combine(projectDir, "Documents");
            Directory.CreateDirectory(documentsDir);

            string extension = format switch
            {
                ExportFormat.Json => ".json",
                ExportFormat.Csv => ".csv",
                ExportFormat.Pdf => ".pdf",
                _ => throw new ArgumentOutOfRangeException(nameof(format))
            };

            path = Path.Combine(documentsDir, $"{baseFileName}_{ts}{extension}");

            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            switch (format)
            {
                case ExportFormat.Json:
                {
                    var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(path, json);
                    return path;
                }
                case ExportFormat.Csv:
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Target,ScanType,ScanTime,IsSuccess,Result");
                    foreach (var r in list)
                        sb.AppendLine($"{Csv(r.Target)},{Csv(r.ScanType)},{r.ScanTime:o},{r.IsSuccess},{Csv(r.Result)}");
                    await File.WriteAllTextAsync(path, sb.ToString());
                    return path;
                }
                case ExportFormat.Pdf:
                {
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
