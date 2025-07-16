using ProTasker.Constants;
using ProTasker.Data.IRepository;
using ProTasker.Domain.Extension;
using ProTasker.Domain.Models;
using ProTasker.DTOModels.Admin;
using ProTasker.Helpers;

namespace ProTasker.Data.Repository;

public class AdminService : IAdminService
{
    public void Register(AdminRegisterModel model)
    {
        var text = FileHelper.ReadFromFile(PathHolder.AdminsFilePath);

        var admins = text.ToAdmins();

        var existsAdmin = admins.Find(a => a.PhoneNumber == model.PhoneNumber);
        if (existsAdmin != null)
            throw new Exception("Through this number, another person is registered!");

        model.Password.CheckerPassword();

        var newAdmin = model.ToNewObjDest<Admin>();

        admins.Add(newAdmin);

        FileHelper.WriteToFile(PathHolder.AdminsFilePath, admins.ConvertToString<Admin>());
    }

    public AdminViewModel Login(string phoneNumber, string password)
    {
        var text = FileHelper.ReadFromFile(PathHolder.AdminsFilePath);

        var admins = text.ToAdmins();

        var existsAdmin = admins.Find(admins => admins.PhoneNumber == phoneNumber && admins.Password == password)
            ?? throw new Exception("Phone number or password incorrect!");

        return new AdminViewModel
        {
            FirstName = existsAdmin.FirstName,
            LastName = existsAdmin.LastName,
            PhoneNumber = phoneNumber,
            Role = existsAdmin.Role,
            Age = existsAdmin.Age,
        };
    }
    
    public void Update(string phoneNumber, AdminUpdateModel model)
    {
        var text = FileHelper.ReadFromFile(PathHolder.AdminsFilePath);

        var admins = text.ToAdmins();

        var existsAdmin = admins.Find(a => a.PhoneNumber == phoneNumber)
                ?? throw new Exception("This admin is not found!");

        var updAdmins = model.UpdateByObj<AdminUpdateModel, Admin>(admins, PathHolder.AdminsFilePath, existsAdmin.PhoneNumber);

        FileHelper.WriteToFile(PathHolder.AdminsFilePath, updAdmins.ConvertToString<Admin>());
    }

    public void Logout(int id)
    {
        var text = FileHelper.ReadFromFile(PathHolder.AdminsFilePath);

        var admins = text.ToAdmins();

        var existsAdmin = admins.Find(admin => admin.Id == id)
            ?? throw new Exception("This admin is not found!");

        admins.Remove(existsAdmin);

        FileHelper.WriteToFile(PathHolder.AdminsFilePath, admins.ConvertToString<Admin>());
    }

    public void ChangePassword(string phoneNumber, string oldPassword, string newPassword)
    {
        var text = FileHelper.ReadFromFile(PathHolder.AdminsFilePath);

        var admins = text.ToAdmins();

        var existsAdmin = admins.Find(a => a.PhoneNumber == phoneNumber)
            ?? throw new Exception("No one has signed up by this phone number!");

        newPassword.CheckerPassword();

        if (oldPassword == newPassword)
            throw new Exception("The new password should be different from the old one!");

        existsAdmin.Password = newPassword;

        FileHelper.WriteToFile(PathHolder.AdminsFilePath, admins.ConvertToString<Admin>());
    }
}
