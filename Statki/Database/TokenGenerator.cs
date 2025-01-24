using System;
using System.Text;
using System.Security.Cryptography;


namespace Statki.Database
{
    public static class TokenGenerator
    {
        public static string GenerateSessionToken(int userId)
        {
            // Użycie SHA256 do generowania unikalnego tokenu
            string rawToken = $"{userId}-{Guid.NewGuid()}-{DateTime.UtcNow.Ticks}";

            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
    }

}
