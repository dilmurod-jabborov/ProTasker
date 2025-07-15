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
    public void DeleteUser(int id)
    {
        var text = FileHelper.ReadFromFile(PathHolder.UsersFilePath);

        var users = text.ToUser();

        var existUser = users.Find(u => u.Id == id);
        if (existUser == null)
        {
             throw new Exception("This user is not found!");
        }
        
        users.Remove(existUser);

        FileHelper.WriteToFile(PathHolder.UsersFilePath, users.ConvertToString<User>());
    }

    public void UpdateUser(UserUpdateModel model)
    {
        Checker.CheckerMethod(model.FirstName); //bu method umuman keremas
        Checker.CheckerMethod(model.LastName); ///////

        var text = FileHelper.ReadFromFile(PathHolder.UsersFilePath);

        var users = text.ToUser();

        var existUser = users.Find(u => u.PhoneNumber == model.PhoneNumber);
        if (existUser == null)
            throw new Exception("This user is not found!");

        var updateLines = model.UpdateByObj<UserUpdateModel, User>(users, PathHolder.UsersFilePath, existUser.Id);

        FileHelper.WriteToFile(PathHolder.UsersFilePath, updateLines.ConvertToString<User>());
    }

    public void ChangeUserPassword(string phoneNumber, string oldPassword, string newPassword)
    {
        var text = FileHelper.ReadFromFile(PathHolder.UsersFilePath);

        var users = text.ToUser();

        var existUser = users.Find(u => u.PhoneNumber == phoneNumber);

        if (existUser == null)
        {
            throw new Exception("Phone number not found or old password is incorrect.");
        }

        if (existUser.Password == newPassword)
            throw new Exception("The old and new password should not be the same!");

        newPassword.CheckerPassword();

        existUser.Password = newPassword;

        FileHelper.WriteToFile(PathHolder.UsersFilePath, users.ConvertToString<User>());
    }

    public void Register(UserRegisterModel model)
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

