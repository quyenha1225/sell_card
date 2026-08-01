using System;
using System.Web.Helpers;

namespace BBEcom.Common.Auth
{
    public static class PasswordService
    {
        public static string Hash(string password)
        {
            if (String.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Mật khẩu không được để trống.");

            return Crypto.HashPassword(password);
        }

        public static bool Verify(string storedHash, string plainPassword)
        {
            if (String.IsNullOrWhiteSpace(storedHash)
                || String.IsNullOrWhiteSpace(plainPassword))
                return false;

            try
            {
                if (storedHash.StartsWith("AQAAAA")
                    || storedHash.StartsWith("AAEAA"))
                    return Crypto.VerifyHashedPassword(storedHash, plainPassword);
            }
            catch { }

            return String.Equals(
                storedHash,
                Encryptor.MD5Hash(plainPassword),
                StringComparison.OrdinalIgnoreCase
            );
        }

        public static bool IsLegacyHash(string hash)
        {
            return !String.IsNullOrWhiteSpace(hash)
                && hash.Length == 32
                && !hash.Contains(".");
        }
    }
}
