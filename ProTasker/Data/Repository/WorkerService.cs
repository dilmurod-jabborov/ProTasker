using ProTasker.Constants;
using ProTasker.Data.IRepository;
using ProTasker.Domain.Enum;
using ProTasker.Domain.Extension;
using ProTasker.Domain.Models;
using ProTasker.DTOModels.User;
using ProTasker.DTOModels.Worker;
using ProTasker.Helpers;

namespace ProTasker.Data.Repository;

public class WorkerService : IWorkerService
{
    public void Register(WorkerRegisterModel model)
    {
        var text = FileHelper.ReadFromFile(PathHolder.WorkersFilePath);

        var workers = text.ToWorkers();

        var exists = workers.Find(w => w.PhoneNumber == model.PhoneNumber);
        if (exists != null)
            throw new Exception("Through this number, another person is registered!");

        if (model.Age < 18)
            throw new Exception("The age should be older than 18!");

        if (model.CategoryId == null || model.CategoryId.Count == 0)
            throw new Exception("Choose at least one category!");

        var newWorker = model.ToNewObjDest<Worker>();

        workers.Add(newWorker);

        FileHelper.WriteToFile(PathHolder.WorkersFilePath, workers.ConvertToString<Worker>());
    }

    public WorkerViewModel Login(string phoneNumber, string password)
    {
        var text = FileHelper.ReadFromFile(PathHolder.WorkersFilePath);

        var workers = text.ToWorkers();

        var worker = workers.Find(w => w.PhoneNumber == phoneNumber && w.Password == password)
            ??   throw new Exception("Invalid phone number or password.");
        
        var workerView = new WorkerViewModel()
        {
            FirstName = worker.FirstName,
            LastName = worker.LastName,
            PhoneNumber = worker.PhoneNumber,
            Bio = worker.Bio,
            Age = worker.Age,
            CategoryId = worker.CategoryId,
            Gender = worker.Gender,
            Location = new Location()
            {
                Region = worker.Location.Region,
                District = worker.Location.District,
                Street = worker.Location.Street,
            }
        };

        return workerView;

    }

    public void Update(WorkerUpdateModel model)
    {
        var text = FileHelper.ReadFromFile(PathHolder.WorkersFilePath);

        var workers = text.ToWorkers();

        var existWorker = workers.Find(u => u.PhoneNumber == model.PhoneNumber);
        if (existWorker == null)
            throw new Exception("This worker is not found!");

        var updateLines = model.UpdateByObj<WorkerUpdateModel, Worker>(workers, PathHolder.WorkersFilePath, existWorker.Id);

        FileHelper.WriteToFile(PathHolder.UsersFilePath, updateLines.ConvertToString<Worker>());
    }

    public void Delete(int id)
    {
        var text = FileHelper.ReadFromFile(PathHolder.WorkersFilePath);

        var workers = text.ToWorkers();

        var existWorker = workers.Find(u => u.Id == id)
          ??  throw new Exception("This user is not found!");
        

        workers.Remove(existWorker);

        FileHelper.WriteToFile(PathHolder.WorkersFilePath, workers.ConvertToString<Worker>());
    }

    public WorkerViewModel GetWorker(int id)
    {
        var text = FileHelper.ReadFromFile(PathHolder.WorkersFilePath);

        var workers = text.ToWorkers();

        var worker = workers.Find(w => w.Id == id)
            ?? throw new Exception("This worker is not found!");

        return new WorkerViewModel()
        {
            FirstName = worker.FirstName,
            LastName = worker.LastName,
            PhoneNumber = worker.PhoneNumber,
            Bio = worker.Bio,
            Age = worker.Age,
            CategoryId = worker.CategoryId,
            Gender = worker.Gender,
            Location = new Location()
            {
                Region = worker.Location.Region,
                District = worker.Location.District,
                Street = worker.Location.Street,
            }
        };
    }

    public List<WorkerViewModel> GetAllWorkers()
    {
        var text = FileHelper.ReadFromFile(PathHolder.WorkersFilePath);

        var workers = text.ToWorkers();

        var workersList = new List<WorkerViewModel>();

        foreach (var worker in workers)
        {
            var workerViewModel = new WorkerViewModel()
            {
                FirstName = worker.FirstName,
                LastName = worker.LastName,
                PhoneNumber = worker.PhoneNumber,
                Bio = worker.Bio,
                Age = worker.Age,
                CategoryId = worker.CategoryId,
                Gender = worker.Gender,
                Location = new Location()
                {
                    Region = worker.Location.Region,
                    District = worker.Location.District,
                    Street = worker.Location.Street,
                }
            };

            workersList.Add(workerViewModel);
        }

        return workersList;
    }
}
