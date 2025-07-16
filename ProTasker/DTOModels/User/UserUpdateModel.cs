using ProTasker.Domain.Enum;
using ProTasker.Domain.Models;

namespace ProTasker.DTOModels.User;

public class UserUpdateModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public Gender Gender { get; set; }
}
