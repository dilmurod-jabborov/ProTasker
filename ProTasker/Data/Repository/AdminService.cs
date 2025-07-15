using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProTasker.Constants;
using ProTasker.Data.IRepository;
using ProTasker.Domain.Extension;
using ProTasker.Domain.Models;
using ProTasker.DTOModels.User;
using ProTasker.DTOModels.Worker;
using ProTasker.Helpers;

namespace ProTasker.Data.Repository
{
    public class AdminService : IAdminService
    {
        UserService userService = new UserService();
        WorkerService workerService = new WorkerService();
        Admin admin = new Admin();
        //void IAdminService.ChangeUserPassword(UserPasswordUpdate upasswordUpdate)
        //{
        //    userService.ChangeUserPassword(upasswordUpdate);
        //}

        //void IAdminService.ChangeWorkerPassword(WorkerPasswordUpdate wpasswordUpdate)
        //{
        //   // workerService.ChangeWorkerPassword(wpasswordUpdate);  
        //}

        void IAdminService.DeleteCategory(Category Id)
        {
            throw new NotImplementedException();
        }

        void IAdminService.DeleteUser(User Id)
        {
           // userService.DeleteUser(Id);
        }

        void IAdminService.DeleteWorker(Worker Id)
        {
            workerService.Delete(Id.Id);
        }

        void IAdminService.GetAllCategories()
        {
            throw new NotImplementedException();
        }

        void IAdminService.GetAllUsers()
        {
            var text = FileHelper.ReadFromFile(PathHolder.UsersFilePath);

            var users = text.ToUser();

            if (users.Count == 0)
            {
                Console.WriteLine("No users found.");
                return;
            }

            foreach (var user in users)
            {
                Console.WriteLine($"ID: {user.Id}, Name: {user.FirstName} {user.LastName}, Phone: {user.PhoneNumber}");
            }
        }

        void IAdminService.GetAllWorkers()
        {
            var text = FileHelper.ReadFromFile(PathHolder.WorkersFilePath);

            var workers = text.ToWorkers();

            if (workers.Count == 0)
            {
                Console.WriteLine("No workers found.");
                return;
            }

            foreach (var worker in workers)
            {
                Console.WriteLine($"ID: {worker.Id}, Name: {worker.FirstName} {worker.LastName}, Phone: {worker.PhoneNumber}");
            }
        }

        void IAdminService.LoginAdmin(string username, string password)
        {
            if(username != admin.Username1 && password != admin.Password1 || username != admin.Username2 && password != admin.Password2)
            {
                throw new Exception("Invalid username or password.");
            }
            Console.WriteLine("Admin logged in successfully.");
        }

        void IAdminService.RegisterUser(UserRegisterModel model)
        {
            userService.Register(model);
        }

        void IAdminService.RegisterWorker(WorkerRegisterModel registerModel)
        {
            workerService.Register(registerModel);
        }

        void IAdminService.UpdateUser(UserUpdateModel model)
        {
            userService.UpdateUser(model);
        }

        void IAdminService.UpdateWorker(WorkerUpdateModel updateModel)
        {
            workerService.Update(updateModel);
        }
    }
}
