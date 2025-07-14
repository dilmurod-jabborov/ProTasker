using ProTasker.Constants;
using ProTasker.Data.IRepository;
using ProTasker.Domain.Extension;
using ProTasker.Domain.Models;
using ProTasker.DTOModels.Worker;
using ProTasker.Helpers;

namespace ProTasker.Data.Repository;

public class WorkerService : IWorkerService
{
    // ChangeWorkerPassword method yozish kere
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

    public int Login(string phoneNumber, string password)
    {
        throw new NotImplementedException();
    }

    public void Update(WorkerUpdateModel model)
    {
        throw new NotImplementedException();
    }
    public void Delete(int id)
    {
        throw new NotImplementedException();
    }

    public WorkerViewModel GetWorker(int id)
    {
        var text = FileHelper.ReadFromFile(PathHolder.WorkersFilePath);

        var workers = text.TextToObjectList<Worker>();

        var worker = workers.Find(w => w.Id == id)
            ?? throw new Exception("This worker is not found!");

        return new WorkerViewModel()
        {
            FirstName = worker.FirstName,
            LastName = worker.LastName,
            PhoneNumber = worker.PhoneNumber,
            Bio = worker.Bio,
            Age = worker.Age,
            CategoryId=worker.CategoryId,
            Gender= worker.Gender,
            Location = worker.Location
        };
    }
}
