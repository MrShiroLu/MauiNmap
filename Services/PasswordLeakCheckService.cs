using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NmapMaui.Services
{
    public class PasswordLeakCheckService
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://api.pwnedpasswords.com/range/";

        public PasswordLeakCheckService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "NmapMaui");
        }

        public async Task<int> CheckPasswordLeakAsync(string password)
        {
            using (var sha1 = SHA1.Create())
            {
                var hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
                var hashString = BitConverter.ToString(hashBytes).Replace("-", "");
                var prefix = hashString.Substring(0, 5);
                var suffix = hashString.Substring(5);

                var response = await _httpClient.GetStringAsync($"{ApiUrl}{prefix}");
                var lines = response.Split('\n');

                foreach (var line in lines)
                {
                    var parts = line.Split(':');
                    if (parts[0].Equals(suffix, StringComparison.OrdinalIgnoreCase))
                    {
                        return int.Parse(parts[1]);
                    }
                }

                return 0;
            }
        }
    }
} 