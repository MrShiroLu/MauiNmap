using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace NmapMaui.Services
{
    // HttpClient adapter that talks to the NmapMaui.Api Minimal API project.
    // Default base address points at localhost:5000 — override via the
    // `ApiBaseUrl` Preference (Preferences.Set("ApiBaseUrl", "http://...")) when
    // running on a device or against a remote host.
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _http;

        public ApiClient(HttpClient http)
        {
            _http = http;
            
            var defaultUrl = "http://localhost:5000";
            if (DeviceInfo.DeviceType == DeviceType.Virtual && DeviceInfo.Platform == DevicePlatform.Android)
            {
                defaultUrl = "http://10.0.2.2:5000";
            }

            var baseUrl = Preferences.Get("ApiBaseUrl", defaultUrl);
            _http.BaseAddress = new Uri(baseUrl);
            _http.Timeout = TimeSpan.FromMinutes(5);
        }

        public async Task<string> CheckHealthAsync()
        {
            try
            {
                var resp = await _http.GetAsync("/");
                return resp.IsSuccessStatusCode ? "OK" : $"HTTP {(int)resp.StatusCode}";
            }
            catch (Exception ex) { return $"unreachable: {ex.Message}"; }
        }

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
    }
}
