using System.Diagnostics;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NmapMaui.Api.Data;
using NmapMaui.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApiDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")
                  ?? "Server=127.0.0.1;Database=NmapMauiDb;User Id=sa;Password=YourStrongPassword!;TrustServerCertificate=True;"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    db.Database.EnsureCreated();
}

// Liveness
app.MapGet("/", () => Results.Ok(new { service = "NmapMaui.Api", status = "ok" }));

// Run an nmap port range scan (blocking — client awaits the full result).
app.MapPost("/nmap/scan", async (NmapScanRequest req, ApiDbContext db) =>
{
    var hostType = Uri.CheckHostName(req.Host);
    if (hostType == UriHostNameType.Unknown)
    {
        return Results.BadRequest(new { error = "Invalid host name or IP address format" });
    }

    if (req.StartPort < 1 || req.EndPort > 65535 || req.StartPort > req.EndPort)
    {
        return Results.BadRequest(new { error = "Invalid port range" });
    }

    var (output, success) = await RunProcessAsync("nmap", $"-sC -T4 -p{req.StartPort}-{req.EndPort} {req.Host}");
    var entry = new ScanLog
    {
        Tool = "nmap",
        Target = req.Host,
        Args = $"{req.StartPort}-{req.EndPort}",
        Output = output,
        Success = success,
        Timestamp = DateTime.UtcNow
    };
    db.ScanLogs.Add(entry);
    await db.SaveChangesAsync();
    return Results.Ok(entry);
});

app.MapPost("/gobuster/scan", async (GobusterRequest req, ApiDbContext db) =>
{
    if (!Uri.TryCreate(req.Url, UriKind.Absolute, out var uriResult) || 
        (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
    {
        return Results.BadRequest(new { error = "Invalid target URL format. Only HTTP/HTTPS is allowed." });
    }

    if (string.IsNullOrWhiteSpace(req.Wordlist) || req.Wordlist.Any(c => c == '"' || c == '\'' || c == ';' || c == '`' || c == '|' || c == '$' || c == '&' || c == '<' || c == '>'))
    {
        return Results.BadRequest(new { error = "Invalid characters detected in wordlist argument" });
    }

    var (output, success) = await RunProcessAsync("gobuster", $"dir -u {req.Url} -w \"{req.Wordlist}\" -q");
    var entry = new ScanLog
    {
        Tool = "gobuster",
        Target = req.Url,
        Args = req.Wordlist,
        Output = output,
        Success = success,
        Timestamp = DateTime.UtcNow
    };
    db.ScanLogs.Add(entry);
    await db.SaveChangesAsync();
    return Results.Ok(entry);
});

app.MapGet("/logs", async (ApiDbContext db) =>
    Results.Ok(await db.ScanLogs.OrderByDescending(x => x.Timestamp).Take(200).ToListAsync()));

app.Run();

static async Task<(string output, bool success)> RunProcessAsync(string file, string args)
{
    var psi = new ProcessStartInfo
    {
        FileName = file,
        Arguments = args,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };
    var sb = new StringBuilder();
    try
    {
        using var p = Process.Start(psi)!;
        string? line;
        while ((line = await p.StandardOutput.ReadLineAsync()) != null)
            sb.AppendLine(line);
        var err = await p.StandardError.ReadToEndAsync();
        await p.WaitForExitAsync();
        if (p.ExitCode != 0) sb.AppendLine("[stderr] " + err);
        return (sb.ToString(), p.ExitCode == 0);
    }
    catch (Exception ex)
    {
        return ($"Failed to run {file}: {ex.Message}", false);
    }
}

public record NmapScanRequest(string Host, int StartPort, int EndPort);
public record GobusterRequest(string Url, string Wordlist);
