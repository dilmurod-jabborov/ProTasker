using ProTasker.Data.IRepository;
using ProTasker.Data.ListsOfUsersAndWorkers;
using ProTasker.Data.UserList;
using ProTasker.Domain.Models;
using ProTasker.Helpers;

namespace ProTasker.Data.Repository;

public class UserService : IUserService
{
    UserList.UserList lists = new UserList.UserList();
    WorkerList workerList = new WorkerList();

    public void GetAllWorkers()
    {
        foreach (var lines in workerList.Workers)
        {
            var line = lines.Split('\n');
            foreach (var item in line)
            {
                var workerDetails = item.Split(',');
                Console.WriteLine($"Id: {workerDetails[0]}," +
                    $" Username: {workerDetails[1]}," +
                    $" Category: {workerDetails[2]}," +
                    $" Gender: {workerDetails[3]}," +
                    $" Rating: {workerDetails[4]}," +
                    $" Location: {workerDetails[5]}");
            }
        }
    }

    public string GetAllWorkersByLocation(Location location)
    {
        Checker.CheckerMethod(location.ToString());
        var workersByLocation = new List<string>();
        foreach (var lines in workerList.Workers)
        {
            var line = lines.Split('\n');
            foreach (var item in line)
            {
                var workerDetails = item.Split(',');
                if (workerDetails[5] == location.ToString())
                {
                    workersByLocation.Add(item);
                }
            }
        }
        return string.Join("\n", workersByLocation);
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

