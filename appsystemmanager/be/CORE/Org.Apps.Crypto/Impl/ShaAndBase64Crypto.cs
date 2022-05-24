using Org.Apps.Crypto.Intf;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Org.Apps.Crypto.Impl
{
    public class ShaAndBase64Crypto : IGenericCrypto
    {
        private static SHA256 sha256 = SHA256Managed.Create();

        public string EncryptUserPassword(string userId, string passWord)
        {
            return Convert.ToBase64String(sha256.ComputeHash(Encoding.ASCII.GetBytes(passWord + userId)));
        }
    }
}