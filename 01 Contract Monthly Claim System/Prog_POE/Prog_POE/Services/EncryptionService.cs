using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace Prog_POE.Services
{
    public class EncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public EncryptionService(IConfiguration configuration, IWebHostEnvironment environment)
        {
            // Get or create encryption keys
            (_key, _iv) = GetEncryptionKeys(configuration, environment);
        }

        private (byte[] Key, byte[] IV) GetEncryptionKeys(IConfiguration configuration, IWebHostEnvironment environment)
        {
            var keySection = configuration["Encryption:Key"];
            var ivSection = configuration["Encryption:IV"];

            // If keys are configured, use them
            if (!string.IsNullOrEmpty(keySection) && !string.IsNullOrEmpty(ivSection))
            {
                try
                {
                    var key = Encoding.UTF8.GetBytes(keySection.PadRight(32).Substring(0, 32));
                    var iv = Encoding.UTF8.GetBytes(ivSection.PadRight(16).Substring(0, 16));
                    return (key, iv);
                }
                catch
                {
                    // Fall back to generated keys if configured ones are invalid
                }
            }

            // Generate and log new keys (in development)
            var newKey = GenerateSecureKey(32);
            var newIV = GenerateSecureKey(16);

            if (environment.IsDevelopment())
            {
                Console.WriteLine($"Generated Encryption Key: {Convert.ToBase64String(newKey)}");
                Console.WriteLine($"Generated Encryption IV: {Convert.ToBase64String(newIV)}");
            }

            return (newKey, newIV);
        }

        private byte[] GenerateSecureKey(int length)
        {
            using var rng = RandomNumberGenerator.Create();
            var key = new byte[length];
            rng.GetBytes(key);
            return key;
        }

        public void EncryptStream(Stream inputStream, Stream outputStream)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var cryptoStream = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write);

            inputStream.CopyTo(cryptoStream);
            cryptoStream.FlushFinalBlock();
        }

        public void DecryptStream(Stream inputStream, Stream outputStream)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var cryptoStream = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read);

            cryptoStream.CopyTo(outputStream);
        }

        public string GenerateSecureFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var encryptedFileName = $"{Guid.NewGuid()}{extension}.encrypted";
            return encryptedFileName;
        }
    }
}