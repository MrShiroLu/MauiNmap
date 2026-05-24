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

#if WINDOWS
            path = await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync<string>(async () =>
            {
                try
                {
                    var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                    savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;

                    string extension = format switch
                    {
                        ExportFormat.Json => ".json",
                        ExportFormat.Csv => ".csv",
                        ExportFormat.Pdf => ".pdf",
                        _ => throw new ArgumentOutOfRangeException(nameof(format))
                    };

                    string choiceLabel = format switch
                    {
                        ExportFormat.Json => "JSON Document",
                        ExportFormat.Csv => "CSV Document",
                        ExportFormat.Pdf => "PDF Document",
                        _ => "Document"
                    };

                    savePicker.FileTypeChoices.Add(choiceLabel, new List<string> { extension });
                    savePicker.SuggestedFileName = $"{baseFileName}_{ts}";

                    var mauiWindow = Microsoft.Maui.Controls.Application.Current?.Windows?.FirstOrDefault();
                    if (mauiWindow == null)
                        return string.Empty;

                    var nativeWindow = mauiWindow.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
                    if (nativeWindow == null)
                        return string.Empty;

                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
                    WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

                    var file = await savePicker.PickSaveFileAsync();
                    return file?.Path ?? string.Empty;
                }
                catch
                {
                    return string.Empty;
                }
            });
#else
            var dir = Microsoft.Maui.Storage.FileSystem.AppDataDirectory;
            Directory.CreateDirectory(dir);
            string extension = format switch
            {
                ExportFormat.Json => ".json",
                ExportFormat.Csv => ".csv",
                ExportFormat.Pdf => ".pdf",
                _ => throw new ArgumentOutOfRangeException(nameof(format))
            };
            path = Path.Combine(dir, $"{baseFileName}_{ts}{extension}");
#endif

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
