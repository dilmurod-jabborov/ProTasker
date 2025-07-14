using ProTasker.Data.Repository;
using ProTasker.Domain.Enum;
using ProTasker.Domain.Models;
using ProTasker.DTOModels.Worker;

namespace ProTasker;

internal class Program
{
    static void Main(string[] args)
    {
        WorkerService workerService = new WorkerService();

        var worker = new WorkerRegisterModel
        {
            FirstName = "Ali",
            LastName = "Valiyev",
            PhoneNumber = "99855",
            Password = "12345",
            Role = Role.User,
            Age = 25,
            Bio = "Santexnik",
            CategoryId = new List<int> { 1, 3, 5 },
            Gender = Gender.Male,
            Location = new Location
            {
                Region = Region.Tashkent,
                District = "Olmazor",
                Street = "Shifokor"
            }
        };

        workerService.Register(worker);

        //CategoryService categoryService = new CategoryService();
        //categoryService.Create("ALi");
        // categoryService.Create("Alo");
        // categoryService.Create("ALi");

        //categoryService.Update(2, "mohinur");

        // categoryService.Delete(3); 

        //categoryService.Get(1);

        //categoryService.GetAll();

        //var worker1 = workerService.GetWorker(1);

        //Console.WriteLine($"Fname - {worker.FirstName}" +
        //    $"Lname - {worker1.LastName}" +
        //    $"Phon - {worker1.PhoneNumber}" +
        //    $"Bio - {worker1.Bio}" +
        //    $"Age - {worker1.Age}" +
        //    $"Category - {worker1.CategoryId}" +
        //    $"Gender - {worker1.Gender}" +
        //    $"Loc - {worker1.Location}");
    }
}