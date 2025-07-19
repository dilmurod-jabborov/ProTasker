using ProTasker.Domain.Enum;
using ProTasker.Domain.Models;

namespace ProTasker.DTOModels.Worker;

public class WorkerUpdateModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age {  get; set; }
    public string Bio { get; set; }
    public Location Location { get; set; }
}
