
using System;
using System.Security.Cryptography;

namespace KioskManagement.Helpers
{
    public class PasswordHelper
    {
        private const int Iterations = 120000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        // Ma hoa mat khau
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[SaltSize];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(
                password, salt, Iterations))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);

                return "PBKDF2$" + Iterations + "$"
                    + Convert.ToBase64String(salt) + "$"
                    + Convert.ToBase64String(hash);
            }
        }

        // Kiem tra mat khau da duoc ma hoa hay chua
        public static bool IsHashed(string storedPassword)
        {
            return !string.IsNullOrEmpty(storedPassword)
                && storedPassword.StartsWith("PBKDF2$");
        }

        // Kiem tra mat khau dang nhap
        public static bool VerifyPassword(
            string password, string storedPassword)
        {
            if (string.IsNullOrEmpty(password)
                || string.IsNullOrEmpty(storedPassword))
            {
                return false;
            }

            // Tuong thich voi mat khau cu dang van ban
            // Se nang cap sang PBKDF2 sau khi dang nhap thanh cong
            if (!IsHashed(storedPassword))
            {
                return password == storedPassword;
            }

            try
            {
                string[] parts = storedPassword.Split('$');

                if (parts.Length != 4 || parts[0] != "PBKDF2")
                    return false;

                int iterations = int.Parse(parts[1]);
                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] expectedHash = Convert.FromBase64String(parts[3]);

                using (var pbkdf2 = new Rfc2898DeriveBytes(
                    password, salt, iterations))
                {
                    byte[] actualHash = pbkdf2.GetBytes(expectedHash.Length);

                    if (actualHash.Length != expectedHash.Length)
                        return false;

                    int difference = 0;

                    for (int i = 0; i < actualHash.Length; i++)
                    {
                        difference |= actualHash[i] ^ expectedHash[i];
                    }

                    return difference == 0;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
