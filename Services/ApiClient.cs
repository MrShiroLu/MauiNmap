using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace NmapMaui.Services
{
    /// <summary>
    /// NmapMaui.Api Minimal API ile iletişim kurar.
    /// JWT token Preferences'ta saklanır; her istek Authorization header'ı ile gönderilir.
    /// API URL'i ApiSettingsPage üzerinden değiştirilebilir.
    /// </summary>
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _http;

        public ApiClient(HttpClient http)
        {
            _http = http;

            // Android emülatöründe localhost → 10.0.2.2
            var defaultUrl = DeviceInfo.DeviceType == DeviceType.Virtual &&
                             DeviceInfo.Platform   == DevicePlatform.Android
                ? "http://10.0.2.2:5000"
                : "http://localhost:5000";

            var baseUrl = Preferences.Get("ApiBaseUrl", defaultUrl);
            _http.BaseAddress = new Uri(baseUrl);
            _http.Timeout     = TimeSpan.FromMinutes(5);

            // Daha önce kaydedilmiş JWT varsa ekle
            ApplySavedToken();
        }

        // ---- Auth ----

        public async Task<string?> LoginAsync(string username, string password)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("/auth/login", new { username, password });
                if (!resp.IsSuccessStatusCode) return null;

                var result = await resp.Content.ReadFromJsonAsync<ApiTokenResponse>();
                if (result?.Token is { } token)
                {
                    Preferences.Set("ApiJwtToken", token);
                    SetBearerToken(token);
                    return token;
                }
                return null;
            }
            catch { return null; }
        }

        public async Task<bool> RegisterAsync(string username, string password)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("/auth/register", new { username, password });
                return resp.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        // ---- Liveness ----

        public async Task<string> CheckHealthAsync()
        {
            try
            {
                var resp = await _http.GetAsync("/");
                return resp.IsSuccessStatusCode ? "OK" : $"HTTP {(int)resp.StatusCode}";
            }
            catch (Exception ex) { return $"unreachable: {ex.Message}"; }
        }

        // ---- Scan Endpoints ----

        public async Task<ApiScanLog?> NmapScanAsync(string host, int startPort, int endPort)
        {
            var resp = await _http.PostAsJsonAsync("/nmap/scan", new { host, startPort, endPort });
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<ApiScanLog>();
        }

        public async Task<ApiScanLog?> GobusterScanAsync(string url, string wordlist)
        {
            var resp = await _http.PostAsJsonAsync("/gobuster/scan", new { url, wordlist });
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<ApiScanLog>();
        }

        // ---- Helpers ----

        private void ApplySavedToken()
        {
            var token = Preferences.Get("ApiJwtToken", "");
            if (!string.IsNullOrEmpty(token))
                SetBearerToken(token);
        }

        private void SetBearerToken(string token) =>
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
    }
}
