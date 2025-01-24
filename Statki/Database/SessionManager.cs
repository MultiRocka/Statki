using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statki.Database
{
    public static class SessionManager
    {
        private static string _loggedInUser;

        private const string SessionFilePath = "user_session.txt";
        private static string _currentSessionToken;

        public static string CurrentSessionToken => _currentSessionToken;

        public static void SetLoggedInUser(string username)
        {
            _loggedInUser = username;
        }

        public static string GetLoggedInUser()
        {
            return _loggedInUser;
        }

        public static void SetSessionToken(string token)
        {
            File.WriteAllText(SessionFilePath, token);
        }

        public static string GetSessionToken()
        {
            if (File.Exists(SessionFilePath))
            {
                Console.WriteLine($"Plik sesji znaleziony: {SessionFilePath}");

                string token = File.ReadAllText(SessionFilePath).Trim();

                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("Plik sesji jest pusty.");
                }
                else
                {
                    Console.WriteLine($"Odczytany token sesji: {token}");
                    _currentSessionToken = token;
                }

                return token;
            }
            else
            {
                Console.WriteLine("Plik sesji nie istnieje.");
                return string.Empty;
            }

            
        }


        public static void ClearSession()
        {
            _currentSessionToken = null;

            // Usunięcie tokenu z pliku
            if (File.Exists(SessionFilePath))
            {
                File.Delete(SessionFilePath);
            }
        }

        public static bool IsSessionValid(DatabaseManager databaseManager)
        {
            if (string.IsNullOrEmpty(_currentSessionToken))
            {
                return false;
            }

            var user = databaseManager.GetUserBySessionToken(_currentSessionToken);
            return user != null;
        }

    }

}
