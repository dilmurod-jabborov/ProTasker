using ProTasker.Domain.Models;
using ProTasker.DTOModels.Worker;

namespace ProTasker.Data.IRepository;

public interface IWorkerService
{
    void Register(WorkerRegisterModel model);
    WorkerViewModel Login(string phoneNumber,  string password);
    void Update(string phoneNumber, WorkerUpdateModel model);
    void Delete(string phoneNumber);
    WorkerViewModel GetWorker(int id);
    void ChangeCategory(string phoneNumber, List<int> id);
    void ChangeLocation(string phoneNumber, Location location);
    void ChangePassword(string phoneNumber, string oldPassword, string newPassword);
    List<WorkerSearchModel> SearchByCategory(int id);
    List<WorkerSearchModel> SearchByRegion(int regId);
}