using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NmapMaui.Services
{
    public class PasswordLeakCheckService
    {
        private readonly IHttpClientFactory _factory;
        private const string ApiUrl = "https://api.pwnedpasswords.com/range/";

        // IHttpClientFactory → socket exhaustion riski yok (HttpClient yeniden kullanılır)
        public PasswordLeakCheckService(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        public async Task<int> CheckPasswordLeakAsync(string password)
        {
            using var sha1 = SHA1.Create();
            var hashBytes  = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
            var hashString = BitConverter.ToString(hashBytes).Replace("-", "");
            var prefix     = hashString[..5];
            var suffix     = hashString[5..];

            // Named client — Preferences'te kayıtlı header'larla oluşturulur
            var client   = _factory.CreateClient("hibp");
            var response = await client.GetStringAsync($"{ApiUrl}{prefix}");

            foreach (var line in response.Split('\n'))
            {
                var parts = line.Split(':');
                if (parts.Length == 2 &&
                    parts[0].Equals(suffix, StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(parts[1].Trim(), out var count))
                {
                    return count;
                }
            }

            return 0;
        }
    }
}