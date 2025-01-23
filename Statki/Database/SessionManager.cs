using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statki.Database
{
    public static class SessionManager
    {
        private static string _loggedInUser;

        public static void SetLoggedInUser(string username)
        {
            _loggedInUser = username;
        }

        public static string GetLoggedInUser()
        {
            return _loggedInUser;
        }
    }

}
