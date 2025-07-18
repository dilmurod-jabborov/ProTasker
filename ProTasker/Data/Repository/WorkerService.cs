using System.Diagnostics;
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
    private ICategoryService categoryService;
    public WorkerService()
    {
        categoryService = new CategoryService();
    }
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
            ?? throw new Exception("Invalid phone number or password.");

        var categories = categoryService.GetAll();

        var catList = new List<string>();

        foreach (var category in categories)
        {
            var workerCatIds = worker.CategoryId;

            foreach (var categoryId in workerCatIds)
            {
                if (category.Id == categoryId)
                {
                    catList.Add(category.Name);
                }
            }
        }

        var workerView = new WorkerViewModel()
        {
            FirstName = worker.FirstName,
            LastName = worker.LastName,
            PhoneNumber = worker.PhoneNumber,
            Bio = worker.Bio,
            Age = worker.Age,
            Category = catList,
            Location = new Location()
            {
                Region = worker.Location.Region,
                District = worker.Location.District,
                Street = worker.Location.Street,
            }
        };

        return workerView;

    }

    public void Update(int id, WorkerUpdateModel model)
    {
        var text = FileHelper.ReadFromFile(PathHolder.WorkersFilePath);

        var workers = text.ToWorkers();

        var existWorker = workers.Find(u => u.Id == id);
        if (existWorker == null)
            throw new Exception("This worker is not found!");

        var updateLines = model.UpdateByObj<WorkerUpdateModel, Worker>(workers, PathHolder.WorkersFilePath, existWorker.PhoneNumber);

        FileHelper.WriteToFile(PathHolder.WorkersFilePath, updateLines.ConvertToString<Worker>());
    }

    public void Delete(int id)
    {
        var text = FileHelper.ReadFromFile(PathHolder.WorkersFilePath);

        var workers = text.ToWorkers();

        var existWorker = workers.Find(u => u.Id == id)
          ?? throw new Exception("This user is not found!");


        workers.Remove(existWorker);

        FileHelper.WriteToFile(PathHolder.WorkersFilePath, workers.ConvertToString<Worker>());
    }

    public WorkerViewModel GetWorker(int id)
    {
        var text = FileHelper.ReadFromFile(PathHolder.WorkersFilePath);

        var workers = text.ToWorkers();

        var worker = workers.Find(w => w.Id == id)
            ?? throw new Exception("This worker is not found!");

        var categories = categoryService.GetAll();

        var catList = new List<string>();

        foreach (var category in categories)
        {
            var workerCatIds = worker.CategoryId;

            foreach (var categoryId in workerCatIds)
            {
                if (category.Id == categoryId)
                {
                    catList.Add(category.Name);
                }
            }
        }

        return new WorkerViewModel()
        {
            FirstName = worker.FirstName,
            LastName = worker.LastName,
            PhoneNumber = worker.PhoneNumber,
            Bio = worker.Bio,
            Age = worker.Age,
            Category = catList,
            Location = new Location()
            {
                Region = worker.Location.Region,
                District = worker.Location.District,
                Street = worker.Location.Street,
            }
        };
    }

    public void ChangePassword(int id, string oldPassword, string newPassword)
    {
        var text = FileHelper.ReadFromFile(PathHolder.WorkersFilePath);

        var workers = text.ToWorkers();

        var existWorker = workers.Find(u => u.Id == id && u.Password == oldPassword)
            ?? throw new Exception("This worker is not found or old password is incorrect!");

        newPassword.CheckerPassword();
        if (existWorker.Password == oldPassword)
            throw new Exception("The old and new password should not be the same!");

        existWorker.Password = newPassword;


        FileHelper.WriteToFile(PathHolder.WorkersFilePath, workers.ConvertToString<Worker>());
    }

    public List<WorkerSearchModel> SearchByCategory(int id)
    {
        var text = FileHelper.ReadFromFile(PathHolder.WorkersFilePath);

        var workers = text.ToWorkers();

        var categories = categoryService.GetAll();

        var workersList = new List<WorkerSearchModel>();

        var catName = "";

        foreach (var category in categories)
        {
            if (category.Id == id)
                catName = category.Name;
        }

        foreach (var worker in workers)
        {
            var workerCatIds = worker.CategoryId;

            foreach (var workerCatId in workerCatIds)
            {
                if (workerCatId == id)
                {
                    var workerView = new WorkerSearchModel()
                    {
                        FirstName = worker.FirstName,
                        LastName = worker.LastName,
                        PhoneNumber = worker.PhoneNumber,
                        Bio = worker.Bio,
                        Age = worker.Age,
                        Category = catName
                    };

                    workersList.Add(workerView);
                }
            }
        }

        return workersList;
    }

    public List<WorkerSearchModel> SearchByRegion(int regId)
    {
        var text = FileHelper.ReadFromFile(PathHolder.WorkersFilePath);

        var workers = text.ToWorkers();

        var workerList = new List<WorkerSearchModel>();

        foreach (var worker in workers)
        {
            int workerRegionId = (int)worker.Location.Region;

            if (workerRegionId == regId)
            {
                workerList.Add(new WorkerSearchModel()
                {
                    FirstName = worker.FirstName,
                    LastName = worker.LastName,
                    PhoneNumber = worker.PhoneNumber,
                    Bio = worker.Bio,
                    Age = worker.Age
                });
            }
        }

        return workerList;
    }
}
