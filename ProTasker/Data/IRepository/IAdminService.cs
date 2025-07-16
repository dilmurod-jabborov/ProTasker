using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProTasker.Domain.Models;
using ProTasker.DTOModels.Admin;
using ProTasker.DTOModels.User;
using ProTasker.DTOModels.Worker;

namespace ProTasker.Data.IRepository
{
    public interface IAdminService
    {
        void Register(AdminRegisterModel model);
        AdminViewModel Login(string phoneNumber, string password);
        void Update(int id, AdminUpdateModel model);
        void Logout(int id);
        void ChangePassword(string phoneNumber, string oldPassword, string newPassword);
    }
}