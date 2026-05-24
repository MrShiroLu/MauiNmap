using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NmapMaui.Api.Data;
using NmapMaui.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// ── Veritabanı (MSSQL) ─────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'Default' is missing or empty in configuration.");
}
builder.Services.AddDbContext<ApiDbContext>(opt =>
    opt.UseSqlServer(connectionString));

// ── JWT Authentication ──────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("Jwt:Key is missing or empty in configuration.");
}
var jwtIssuer  = builder.Configuration["Jwt:Issuer"]   ?? "NmapMaui.Api";
var jwtAudience= builder.Configuration["Jwt:Audience"] ?? "NmapMaui.Client";
var jwtExpHrs  = int.TryParse(builder.Configuration["Jwt:ExpiryHours"], out var h) ? h : 8;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtIssuer,
            ValidAudience            = jwtAudience,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew                = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// ── DB Migration / EnsureCreated ───────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    db.Database.EnsureCreated();
}

app.UseAuthentication();
app.UseAuthorization();

// ════════════════════════════════════════════════════════════════════════════
//  Auth Endpoints (korumasız — token almak için)
// ════════════════════════════════════════════════════════════════════════════

// POST /auth/register — Yeni kullanıcı kaydı
app.MapPost("/auth/register", async (RegisterRequest req, ApiDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest(new { error = "Username and password are required." });

    if (req.Username.Length < 3)
        return Results.BadRequest(new { error = "Username must be at least 3 characters." });

    if (req.Password.Length < 8 || !req.Password.Any(char.IsUpper) || !req.Password.Any(char.IsDigit))
        return Results.BadRequest(new { error = "Password must be at least 8 characters and contain at least 1 uppercase letter and 1 digit." });

    if (await db.Users.AnyAsync(u => u.Username == req.Username))
        return Results.Conflict(new { error = "Username already exists." });

    db.Users.Add(new ApiUser
    {
        Username     = req.Username,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password, workFactor: 12),
        CreatedAt    = DateTime.UtcNow,
        IsActive     = true
    });
    await db.SaveChangesAsync();
    return Results.Ok(new { message = "Registration successful." });
}).AllowAnonymous();

// POST /auth/login — JWT token döner
app.MapPost("/auth/login", async (LoginRequest req, ApiDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest(new { error = "Username and password are required." });

    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == req.Username && u.IsActive);
    if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
        return Results.Unauthorized();

    user.LastLoginAt = DateTime.UtcNow;
    await db.SaveChangesAsync();

    var token  = GenerateJwt(user.Username, jwtKey, jwtIssuer, jwtAudience, jwtExpHrs);
    var expiry = DateTime.UtcNow.AddHours(jwtExpHrs);
    return Results.Ok(new { Token = token, Username = user.Username, Expiry = expiry });
}).AllowAnonymous();

// ════════════════════════════════════════════════════════════════════════════
//  Liveness (korumasız)
// ════════════════════════════════════════════════════════════════════════════
app.MapGet("/", () => Results.Ok(new { service = "NmapMaui.Api", status = "ok" }))
   .AllowAnonymous();

// ════════════════════════════════════════════════════════════════════════════
//  Korumalı Endpoints — RequireAuthorization()
// ════════════════════════════════════════════════════════════════════════════

// POST /nmap/scan — Nmap port range taraması
app.MapPost("/nmap/scan", async (NmapScanRequest req, ApiDbContext db, HttpContext ctx) =>
{
    var hostType = Uri.CheckHostName(req.Host);
    if (hostType == UriHostNameType.Unknown)
        return Results.BadRequest(new { error = "Invalid host name or IP address format" });

    if (req.StartPort < 1 || req.EndPort > 65535 || req.StartPort > req.EndPort)
        return Results.BadRequest(new { error = "Invalid port range" });

    var (output, success) = await RunProcessAsync("nmap", $"-sC -T4 -p{req.StartPort}-{req.EndPort} {req.Host}");
    var entry = new ScanLog
    {
        Tool      = "nmap",
        Target    = req.Host,
        Args      = $"{req.StartPort}-{req.EndPort}",
        Output    = output,
        Success   = success,
        Timestamp = DateTime.UtcNow
    };
    db.ScanLogs.Add(entry);
    await db.SaveChangesAsync();
    return Results.Ok(entry);
}).RequireAuthorization();

// POST /gobuster/scan — Gobuster dizin taraması
app.MapPost("/gobuster/scan", async (GobusterRequest req, ApiDbContext db) =>
{
    if (!Uri.TryCreate(req.Url, UriKind.Absolute, out var uriResult) ||
        (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
    {
        return Results.BadRequest(new { error = "Invalid target URL format. Only HTTP/HTTPS is allowed." });
    }

    // Wordlist path injection koruması
    if (string.IsNullOrWhiteSpace(req.Wordlist) ||
        req.Wordlist.Any(c => c is '"' or '\'' or ';' or '`' or '|' or '$' or '&' or '<' or '>'))
    {
        return Results.BadRequest(new { error = "Invalid characters detected in wordlist argument" });
    }

    var (output, success) = await RunProcessAsync("gobuster", $"dir -u {req.Url} -w \"{req.Wordlist}\" -q");
    var entry = new ScanLog
    {
        Tool      = "gobuster",
        Target    = req.Url,
        Args      = req.Wordlist,
        Output    = output,
        Success   = success,
        Timestamp = DateTime.UtcNow
    };
    db.ScanLogs.Add(entry);
    await db.SaveChangesAsync();
    return Results.Ok(entry);
}).RequireAuthorization();

// GET /logs — Son 200 tarama logu
app.MapGet("/logs", async (ApiDbContext db) =>
    Results.Ok(await db.ScanLogs
        .OrderByDescending(x => x.Timestamp)
        .Take(200)
        .ToListAsync()))
.RequireAuthorization();

app.Run();

// ════════════════════════════════════════════════════════════════════════════
//  Yardımcı fonksiyonlar
// ════════════════════════════════════════════════════════════════════════════

static string GenerateJwt(string username, string key, string issuer, string audience, int expiryHours)
{
    var secKey   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var creds    = new SigningCredentials(secKey, SecurityAlgorithms.HmacSha256);
    var claims   = new[]
    {
        new Claim(ClaimTypes.Name,               username),
        new Claim(JwtRegisteredClaimNames.Sub,   username),
        new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Iat,   DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                  ClaimValueTypes.Integer64)
    };

    var token = new JwtSecurityToken(
        issuer:             issuer,
        audience:           audience,
        claims:             claims,
        notBefore:          DateTime.UtcNow,
        expires:            DateTime.UtcNow.AddHours(expiryHours),
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
}

static async Task<(string output, bool success)> RunProcessAsync(string file, string args)
{
    var psi = new ProcessStartInfo
    {
        FileName               = file,
        Arguments              = args,
        RedirectStandardOutput = true,
        RedirectStandardError  = true,
        UseShellExecute        = false,
        CreateNoWindow         = true
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

// ── Request Records ─────────────────────────────────────────────────────────
public record NmapScanRequest(string Host, int StartPort, int EndPort);
public record GobusterRequest(string Url, string Wordlist);
public record LoginRequest(string Username, string Password);
public record RegisterRequest(string Username, string Password);
