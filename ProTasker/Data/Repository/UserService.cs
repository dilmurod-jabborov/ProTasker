using ProTasker.Data.IRepository;
using ProTasker.Data.UserList;
using ProTasker.Domain.Models;
using ProTasker.Helpers;

namespace ProTasker.Data.Repository;

public class UserService : IUserService
{
    UserList.UserList lists = new UserList.UserList();

    public List<string> GetAllWorkers()
    {

    }

    public List<string> GetAllWorkersByLocation(Location location)
    {
        throw new NotImplementedException();
    }

    void IUserService.DeleteUser(string Username)
    {
        Checker.CheckerMethod(Username);

        foreach (var lines in lists.Users)
        {
            lines.Split('\n');
            if (lines.Contains(Username))
            {
                lists.Users.Remove(lines);
                Console.WriteLine($"User {Username} has been deleted successfully.");
                return;
            }
        }
    }

    string IUserService.GetUserByUsername(string Username)
    {
        Checker.CheckerMethod(Username);
        foreach (var lines in lists.Users)
        {
            var line = lines.Split('\n');
            foreach (var item in line)
            {
                var userDetails = item.Split(',');
                if (userDetails[1] == Username)
                {
                    return item.ToString();
                }
            }
        }
        return string.Empty;
    }

    void IUserService.UpdateUser(string Username, string Phonenumber, int age)
    {
        Checker.CheckerMethod(Username);
        lists.Users = lists.Users.Select(user =>
        {
            var userDetails = user.Split(',');
            if (userDetails[1] == Username)
            {
                userDetails[2] = Phonenumber;
                userDetails[5] = age.ToString();
                return string.Join(",", userDetails);
            }
            return user;
        }).ToList();
        Console.WriteLine($"User {Username} has been updated successfully.");
    }

    void IUserService.UpdateUserPassword(string Username, string OldPassword, string NewPassword, string ConfirmPassword)
    {
        Checker.CheckerMethod(Username);
        if (NewPassword != ConfirmPassword)
        {
            throw new ArgumentException("New password and confirm password do not match.");
        }
        foreach (var lines in lists.Users)
        {
            var userDetails = lines.Split(',');
            if (userDetails[1] == Username && userDetails[3] == OldPassword)
            {
                userDetails[3] = NewPassword;
                Console.WriteLine($"Password for {Username} has been updated successfully.");
                return;
            }
        }
    }

}

