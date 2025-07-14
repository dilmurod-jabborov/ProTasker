using System.Reflection;
using ProTasker.Constants;
using ProTasker.Data.IRepository;
using ProTasker.Domain.Extension;
using ProTasker.Domain.Models;
using ProTasker.DTOModels.User;
using ProTasker.Helpers;

namespace ProTasker.Data.Repository;

public class UserService : IUserService
{

    public void DeleteUser(User Id)
    {
        Checker.CheckerMethod(Id.Id);

        var text = FileHelper.ReadFromFile(PathHolder.UsersFilePath);

        var users = text.ToUser();

        var existUser = users.Find(u => u.Id == Id.Id);

        if (existUser != null)
        {
            users.Remove(existUser);
            var updateLines = Id.UpdateByObj<User, User>(users, PathHolder.UsersFilePath, existUser.Id);
            FileHelper.WriteToFile(PathHolder.UsersFilePath, updateLines);
        }
        else
        {
            throw new Exception("This user is not found!");
        }
    }

    public void UpdateUser(UserUpdateModel model)
    {
        Checker.CheckerMethod(model.FirstName);
        Checker.CheckerMethod(model.LastName);

        var text = FileHelper.ReadFromFile(PathHolder.UsersFilePath);

        var users = text.ToUser();

        var existUser = users.Find(u => u.PhoneNumber == model.PhoneNumber);
        if (existUser == null)
            throw new Exception("This user is not found!");

        var updateLines = model.UpdateByObj<UserUpdateModel, User>(users, PathHolder.UsersFilePath, existUser.Id);

        FileHelper.WriteToFile(PathHolder.UsersFilePath, updateLines);
    }

    public void ChangeUserPassword(UserPasswordUpdate passwordUpdate)
    {
        Checker.CheckerMethod(passwordUpdate.UserName);
        Checker.CheckerMethod(passwordUpdate.OldPassword);
        Checker.CheckerMethod(passwordUpdate.NewPassword, passwordUpdate.ConfirmPassword);

        if (passwordUpdate.NewPassword != passwordUpdate.ConfirmPassword)
        {
            throw new Exception("New password and confirm password do not match.");
        }

        var text = FileHelper.ReadFromFile(PathHolder.UsersFilePath);

        var users = text.ToUser();

        var existUser = users.Find(u => u.FirstName == passwordUpdate.UserName && u.Password == passwordUpdate.OldPassword);

        if (existUser == null)
        {
            throw new Exception("User not found or old password is incorrect.");
        }

        existUser.Password = passwordUpdate.NewPassword;

        var updateLines = existUser.UpdateByObj<User, User>(users, PathHolder.UsersFilePath, existUser.Id);

        if (updateLines == null || updateLines.Count == 0)
        {
            throw new Exception("Failed to update user password.");
        }

        FileHelper.WriteToFile(PathHolder.UsersFilePath, updateLines);
    }

    public void Register(UserRegissterModel model)
    {
        Checker.CheckerMethod(model.FullName);
        Checker.CheckerMethodForNumber(model.PhoneNumber);
        Checker.CheckerMethod(model.Password);

        var text = FileHelper.ReadFromFile(PathHolder.UsersFilePath);
        var users = text.ToUser();
        var exists = users.Find(u => u.PhoneNumber == model.PhoneNumber);
        if (exists != null)
            throw new Exception("This phone number is already registered!");
        var newUser = model.ToNewObjDest<User>();
        users.Add(newUser);
        FileHelper.WriteToFile(PathHolder.UsersFilePath, users.ConvertToString<User>());
    }
    public void Login(string phoneNumber, string password)
    {
        Checker.CheckerMethod(phoneNumber);
        Checker.CheckerMethod(password);
        var text = FileHelper.ReadFromFile(PathHolder.UsersFilePath);
        var users = text.ToUser();
        var user = users.Find(u => u.PhoneNumber == phoneNumber && u.Password == password);
        if (user == null)
        {
            throw new Exception("Invalid phone number or password.");
        }
        else
        {
            Console.WriteLine($"Welcome {user.FirstName}-{user.LastName}!");
        }
        // Login davom etadi
    }
}

