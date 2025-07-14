using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ProTasker.Helpers
{
    internal static class Checker
    {
        public static void CheckerMethod(string Username)
        {
            if (string.IsNullOrEmpty(Username))
            {
                throw new ArgumentException("Username cannot be null or empty.", nameof(Username));
            }
            if (Username.Length < 3)
            {
                throw new ArgumentException("Username must be at least 3 characters long.", nameof(Username));
            }
            if (Username.Length > 30)
            {
                throw new ArgumentException("Username cannot exceed 50 characters.", nameof(Username));
            }
            if (!Username.All(char.IsLetterOrDigit))
            {
                throw new ArgumentException("Username can only contain letters and digits.", nameof(Username));
            }
            if (Username.Any(char.IsWhiteSpace))
            {
                throw new ArgumentException("Username cannot contain whitespace.", nameof(Username));
            }
        }
        public static void CheckerMethod(int Id)
        {
            if (Id <= 0)
            {
                throw new ArgumentException("Id must be a positive integer.", nameof(Id));
            }
        }
    }
}
