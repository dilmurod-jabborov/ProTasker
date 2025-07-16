using ProTasker.Constants;
using ProTasker.Data.IRepository;
using ProTasker.DTOModels.Admin;
using ProTasker.Helpers;

namespace ProTasker.Data.Repository;

public class AdminService : IAdminService
{
    public void Register(AdminRegisterModel model)
    {
        var text = FileHelper.ReadFromFile(PathHolder.AdminsFilePath);

        var admins = text.ToAdmins();
    }

    public void Logout()
    {
        throw new NotImplementedException();
    }

    public void Update(AdminUpdateModel model)
    {
        throw new NotImplementedException();
    }

    public void ChangePassword(string phoneNumber, string oldPassword, string newPassword)
    {
        throw new NotImplementedException();
    }

    public AdminViewModel Login(string phoneNumber, string password)
    {
        throw new NotImplementedException();
    }

}
