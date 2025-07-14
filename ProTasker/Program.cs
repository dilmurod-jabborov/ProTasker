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
            PhoneNumber = "998901234567",
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

        Console.WriteLine("✅ Worker muvaffaqiyatli ro'yxatdan o'tdi!");

        var worker = workerService.GetWorker(1);

        Console.WriteLine($"Fname - {worker.FirstName}" +
            $"Lname - {worker.LastName}" +
            $"Phon - {worker.PhoneNumber}" +
            $"Bio - {worker.Bio}" +
            $"Age - {worker.Age}" +
            $"Category - {worker.CategoryId}" +
            $"Gender - {worker.Gender}" +
            $"Loc - {worker.Location}");
    }
}