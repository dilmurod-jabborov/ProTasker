using ProTasker.DTOModels.Worker;

namespace ProTasker.Data.IRepository;

public interface IWorkerService
{
    void Register(WorkerRegisterModel model);
    int Login(string phoneNumber,  string password);
    void Update(WorkerUpdateModel model);
    
}