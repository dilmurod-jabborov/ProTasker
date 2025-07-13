using ProTasker.Data.IRepository;
using ProTasker.DTOModels.Worker;

namespace ProTasker.Data.Repository;

public class WorkerService : IWorkerService
{
    public void Register(WorkerRegisterModel model)
    {

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

    public WorkerGetModel GetWorker(int id)
    {
        throw new NotImplementedException();
    }
}
