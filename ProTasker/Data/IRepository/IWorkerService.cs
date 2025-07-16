using ProTasker.DTOModels.Worker;

namespace ProTasker.Data.IRepository;

public interface IWorkerService
{
    void Register(WorkerRegisterModel model);
    WorkerViewModel Login(string phoneNumber,  string password);
    void Update(int id, WorkerUpdateModel model);
    void Delete(int id);
    List<WorkerViewModel> GetAllWorkers();
    WorkerViewModel GetWorker(int id);
    void ChangePassword(int id, string oldPassword, string newPassword);
}