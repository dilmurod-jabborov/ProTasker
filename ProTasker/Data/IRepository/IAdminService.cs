using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProTasker.Domain.Models;
using ProTasker.DTOModels.User;
using ProTasker.DTOModels.Worker;

namespace ProTasker.Data.IRepository
{
    public interface IAdminService
    {
        public void DeleteUser(User Id);
        public void UpdateUser(UserUpdateModel model);
        public void RegisterUser(UserRegisterModel model);
        //public void ChangeUserPassword(UserPasswordUpdate upasswordUpdate);
        public void LoginAdmin(string username, string password);
        public void DeleteWorker(Worker Id);
        public void UpdateWorker(WorkerUpdateModel updateModel);
       // public void ChangeWorkerPassword(WorkerPasswordUpdate wpasswordUpdate);
        public void RegisterWorker(WorkerRegisterModel registerModel);
        public void DeleteCategory(Category Id);
        public void GetAllUsers();
        public void GetAllWorkers();
        public void GetAllCategories();
    }
}
