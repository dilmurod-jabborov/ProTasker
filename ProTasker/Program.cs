using ProTasker.Data.Repository;
using ProTasker.Domain.Enum;
using ProTasker.Domain.Models;
using ProTasker.DTOModels.Admin;
using ProTasker.DTOModels.User;
using ProTasker.DTOModels.Worker;

namespace ProTasker;

internal class Program
{
    static void Main(string[] args)
    {
        WorkerService workerService = new WorkerService();
        UserService userService = new UserService();
        CategoryService categoryService = new CategoryService();
        AdminService adminService = new AdminService();

        userService.ChangeUserPassword("+999999999999", "Dilmurod_03", "Dilmurod/03");

        Console.WriteLine("Muvaffaqqiyatli yangilandi");
    }
        
}