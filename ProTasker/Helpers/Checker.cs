using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ProTasker.Menu;

namespace ProTasker.Helpers;

public static class Checker
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

    public static void CheckerMethod(string Password, string ConfirmPassword)
    {
        if (string.IsNullOrEmpty(Password))
        {
            throw new ArgumentException("Password cannot be null or empty.", nameof(Password));
        }
        if (Password.Length < 6)
        {
            throw new ArgumentException("Password must be at least 6 characters long.", nameof(Password));
        }
        if (Password != ConfirmPassword)
        {
            throw new ArgumentException("Passwords do not match.", nameof(ConfirmPassword));
        }
    }
    public static void CheckerMethodForNumber(string PhoneNumber)
    {
        if (string.IsNullOrEmpty(PhoneNumber))
        {
            throw new ArgumentException("Phone number cannot be null or empty.", nameof(PhoneNumber));
        }
    }

    public static void CheckerPassword(this string Password)
    {
        if (string.IsNullOrEmpty(Password))
            throw new Exception("Password is null or empty!");

        if(Password.Length < 6) 
            throw new Exception($"There must be at least 6 characters!");

        if (!Password.Any(char.IsUpper))
            throw new Exception("The password must contain at least one capital letter!");

        if (!Password.Any(char.IsLower))
            throw new Exception("The password must contain at least one lowercase letter!");

        if (!Password.Any(char.IsDigit))
            throw new Exception("The password must contain at least one number!");

        if (!Password.Any(ch => "!@#$%^&*()_+-=<>?/[]{}".Contains(ch)))
            throw new Exception("The password must contain at least one special character!");
    }
}
