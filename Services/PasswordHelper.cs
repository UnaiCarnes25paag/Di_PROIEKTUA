using System;
using System.Security.Cryptography;
using System.Text;

namespace Erronka.Services
{
    public static class PasswordHelper
    {
        public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public static (byte[] hash, byte[] salt) HashPassword(string password)
        {
            CreatePasswordHash(password, out byte[] hash, out byte[] salt);
            return (hash, salt);
        }

        public static bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return AreHashesEqual(storedHash, computedHash);
            }
        }

        private static bool AreHashesEqual(byte[] hash1, byte[] hash2)
        {
            if (hash1.Length != hash2.Length)
                return false;

            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1[i] != hash2[i])
                    return false;
            }
            return true;
        }
    }
}