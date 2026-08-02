using System.IO;
using System.Security.Cryptography;

namespace Prog_POE.Services
{
    public interface IEncryptionService
    {
        void EncryptStream(Stream inputStream, Stream outputStream);
        void DecryptStream(Stream inputStream, Stream outputStream);
        string GenerateSecureFileName(string originalFileName);
    }
}