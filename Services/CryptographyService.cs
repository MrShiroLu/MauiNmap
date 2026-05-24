using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NmapMaui.Services
{
    public class CryptographyService : ICryptographyService
    {
        // ---------------------------------------------------------------
        // Yeni şifreleme formatı  : [Magic(4)] + [Salt(16)] + [IV(16)] + [Ciphertext]
        // Eski format (uyumluluk) : [IV(16)] + [Ciphertext]
        //
        // "NMAP" magic header (0x4E 0x4D 0x41 0x50) yeni formatı işaret eder.
        // Bu sayede şifresi çözülemeyen eski veri kaybolmaz; eski format
        // algılandığında eski yöntemle deşifre edilir.
        // ---------------------------------------------------------------

        private static readonly byte[] MagicHeader = { 0x4E, 0x4D, 0x41, 0x50 }; // "NMAP"
        private const int SaltSize = 16;
        private const int IvSize   = 16;
        private const int Pbkdf2Iterations = 100_000; // OWASP min. önerisi

        // ---- Hash ----

        public async Task<string> ComputeHashAsync(string input, string algorithm)
        {
            return await Task.Run(() =>
            {
                using HashAlgorithm ha = algorithm.ToUpper() switch
                {
                    "MD5"    => MD5.Create(),
                    "SHA1"   => SHA1.Create(),
                    "SHA256" => SHA256.Create(),
                    "SHA384" => SHA384.Create(),
                    "SHA512" => SHA512.Create(),
                    _        => throw new ArgumentException("Unsupported hash algorithm")
                };
                var bytes = ha.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            });
        }

        // ---- Base64 ----

        public async Task<string> EncodeBase64Async(string input) =>
            await Task.Run(() => Convert.ToBase64String(Encoding.UTF8.GetBytes(input)));

        public async Task<string> DecodeBase64Async(string input) =>
            await Task.Run(() => Encoding.UTF8.GetString(Convert.FromBase64String(input)));

        // ---- AES-256 Şifreleme (PBKDF2 key türetimi) ----

        public async Task<string> EncryptAsync(string input, string key)
        {
            return await Task.Run(() =>
            {
                // Her şifreleme için rastgele salt üret
                var salt = RandomNumberGenerator.GetBytes(SaltSize);
                var derivedKey = DeriveKey(key, salt);

                using var aes = Aes.Create();
                aes.Key = derivedKey;
                aes.GenerateIV();
                var iv = aes.IV;

                using var ms = new MemoryStream();
                // Başlık: Magic + Salt + IV
                ms.Write(MagicHeader, 0, MagicHeader.Length);
                ms.Write(salt,        0, salt.Length);
                ms.Write(iv,          0, iv.Length);

                using var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
                var inputBytes = Encoding.UTF8.GetBytes(input);
                cs.Write(inputBytes, 0, inputBytes.Length);
                cs.FlushFinalBlock();

                return Convert.ToBase64String(ms.ToArray());
            });
        }

        public async Task<string> DecryptAsync(string input, string key)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var fullCipher = Convert.FromBase64String(input);

                    return IsNewFormat(fullCipher)
                        ? DecryptNewFormat(fullCipher, key)
                        : DecryptLegacyFormat(fullCipher, key);
                }
                catch (FormatException)       { return input; } // Geçersiz base64
                catch (CryptographicException){ return input; } // Yanlış anahtar / bozuk veri
            });
        }

        // ---- Yardımcı Metotlar ----

        private static bool IsNewFormat(byte[] data)
        {
            if (data.Length < MagicHeader.Length + SaltSize + IvSize + 1)
                return false;
            for (int i = 0; i < MagicHeader.Length; i++)
                if (data[i] != MagicHeader[i]) return false;
            return true;
        }

        private static string DecryptNewFormat(byte[] fullCipher, string key)
        {
            int offset = MagicHeader.Length;
            var salt       = fullCipher[offset..(offset + SaltSize)]; offset += SaltSize;
            var iv         = fullCipher[offset..(offset + IvSize)];   offset += IvSize;
            var ciphertext = fullCipher[offset..];

            var derivedKey = DeriveKey(key, salt);

            using var aes = Aes.Create();
            aes.Key = derivedKey;
            aes.IV  = iv;

            using var ms = new MemoryStream(ciphertext);
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }

        // Eski format: IV(16) + Ciphertext — key boşlukla dolduruluyordu (backward compat)
        private static string DecryptLegacyFormat(byte[] fullCipher, string key)
        {
            if (fullCipher.Length < 16) return string.Empty;
            var iv         = fullCipher[..16];
            var ciphertext = fullCipher[16..];

            using var aes = Aes.Create();
            // Eski yöntem: key'i 32 bayta pad'le (güvensiz ama eski veriyi açmak için gerekli)
            aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32)[..32]);
            aes.IV  = iv;

            using var ms = new MemoryStream(ciphertext);
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }

        /// <summary>
        /// PBKDF2-SHA256 ile güvenli key türetimi.
        /// 100.000 iterasyon → OWASP 2024 önerisiyle uyumlu.
        /// </summary>
        private static byte[] DeriveKey(string password, byte[] salt) =>
            Rfc2898DeriveBytes.Pbkdf2(
                password:        Encoding.UTF8.GetBytes(password),
                salt:            salt,
                iterations:      Pbkdf2Iterations,
                hashAlgorithm:   HashAlgorithmName.SHA256,
                outputLength:    32);
    }
}