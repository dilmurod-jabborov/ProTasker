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

        //var worker = new WorkerRegisterModel
        //{
        //    FirstName = "Ali",
        //    LastName = "Valiyev",
        //    PhoneNumber = "99855",
        //    Password = "12345",
        //    Role = Role.User,
        //    Age = 25,
        //    Bio = "Santexnik",
        //    CategoryId = new List<int> { 1, 3, 5 },
        //    Gender = Gender.Male,
        //    Location = new Location
        //    {
        //        Region = Region.Tashkent,
        //        District = "Olmazor",
        //        Street = "Shifokor"
        //    }
        //};

        //workerService.Register(worker);

        var worker = workerService.GetWorker(2);

        Console.WriteLine($"Fname - {worker.FirstName}\n" +
            $"Lname - {worker.LastName}\n" +
            $"Phon - {worker.PhoneNumber}\n" +
            $"Bio - {worker.Bio}\n" +
            $"Age - {worker.Age}\n" +
            $"Region - {worker.Location.Region}\n" +
            $"Destrict - {worker.Location.District}\n" +
            $"Street - {worker.Location.Street}\n" +
            $"Works - {worker.CategoryId[0]},{worker.CategoryId[1]}");
    }
}