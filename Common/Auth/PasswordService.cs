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

            // Legacy accounts của project cũ dùng MD5 32 ký tự.
            // Mọi hash khác được xác minh bằng Crypto.VerifyHashedPassword.
            // Không phụ thuộc prefix vì format hash có thể khác giữa các phiên bản.
            if (!IsLegacyHash(storedHash))
            {
                try
                {
                    return Crypto.VerifyHashedPassword(
                        storedHash,
                        plainPassword
                    );
                }
                catch
                {
                    return false;
                }
            }

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
