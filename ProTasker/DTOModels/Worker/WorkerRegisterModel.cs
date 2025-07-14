using ProTasker.Domain.Enum;
using ProTasker.Domain.Models;

namespace ProTasker.DTOModels.Worker;

public class WorkerRegisterModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string Password { get; set; }
    public Role Role { get; set; }
    public int Age {  get; set; }
    public string Bio { get; set; }
    public List<int> CategoryId { get; set; }
    public Gender Gender { get; set; }
    public Location Location { get; set; }
}
