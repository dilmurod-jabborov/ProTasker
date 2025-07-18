using ProTasker.Domain.Enum;
using ProTasker.Domain.Models;

namespace ProTasker.DTOModels.Worker;

public class WorkerViewModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string Bio { get; set; }
    public int Age {  get; set; }
    public List<string> Category { get; set; }
    public Location Location { get; set; }
}
