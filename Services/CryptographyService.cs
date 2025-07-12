using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NmapMaui.Services
{
    public class CryptographyService : ICryptographyService
    {
        public async Task<string> ComputeHashAsync(string input, string algorithm)
        {
            return await Task.Run(() =>
            {
                using HashAlgorithm hashAlgorithm = algorithm.ToUpper() switch
                {
                    "MD5" => MD5.Create(),
                    "SHA1" => SHA1.Create(),
                    "SHA256" => SHA256.Create(),
                    "SHA384" => SHA384.Create(),
                    "SHA512" => SHA512.Create(),
                    _ => throw new ArgumentException("Unsupported hash algorithm")
                };

                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = hashAlgorithm.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            });
        }

        public async Task<string> EncodeBase64Async(string input)
        {
            return await Task.Run(() =>
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                return Convert.ToBase64String(inputBytes);
            });
        }

        public async Task<string> DecodeBase64Async(string input)
        {
            return await Task.Run(() =>
            {
                var inputBytes = Convert.FromBase64String(input);
                return Encoding.UTF8.GetString(inputBytes);
            });
        }

        public async Task<string> EncryptAsync(string input, string key)
        {
            return await Task.Run(() =>
            {
                using var aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
                aes.GenerateIV(); // Generate a random IV
                var iv = aes.IV;

                using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                var inputBytes = Encoding.UTF8.GetBytes(input);
                
                using var ms = new System.IO.MemoryStream();
                ms.Write(iv, 0, iv.Length); // Prepend the IV
                using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
                cs.Write(inputBytes, 0, inputBytes.Length);
                cs.FlushFinalBlock();

                var encryptedBytes = ms.ToArray();
                return Convert.ToBase64String(encryptedBytes);
            });
        }

        public async Task<string> DecryptAsync(string input, string key)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var fullCipher = Convert.FromBase64String(input);

                    using var aes = Aes.Create();
                    aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));

                    var iv = new byte[16];
                    if (fullCipher.Length < 16)
                    {
                        // Not enough bytes for an IV. This is not our encrypted data.
                        // It could be the old format, or something else.
                        // Returning original input for now.
                        return input;
                    }
                    Array.Copy(fullCipher, 0, iv, 0, iv.Length); // Extract the IV
                    aes.IV = iv;

                    var ciphertext = new byte[fullCipher.Length - 16];
                    Array.Copy(fullCipher, 16, ciphertext, 0, ciphertext.Length);

                    using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using var ms = new System.IO.MemoryStream(ciphertext);
                    using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                    using var sr = new System.IO.StreamReader(cs);

                    return sr.ReadToEnd();
                }
                catch (FormatException)
                {
                    // The input is not a valid base-64 string.
                    // This can happen if we are trying to decrypt unencrypted data.
                    return input; // Return original string if not base64
                }
                catch (CryptographicException)
                {
                    // Decryption failed. This could be due to wrong key, corrupted data, or old format.
                    return input; // Return original string if decryption fails
                }
            });
        }
    }
} 