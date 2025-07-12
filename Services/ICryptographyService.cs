using System.Threading.Tasks;

namespace NmapMaui.Services
{
    public interface ICryptographyService
    {
        Task<string> ComputeHashAsync(string input, string algorithm);
        Task<string> EncodeBase64Async(string input);
        Task<string> DecodeBase64Async(string input);
        Task<string> EncryptAsync(string input, string key);
        Task<string> DecryptAsync(string input, string key);
    }
} 