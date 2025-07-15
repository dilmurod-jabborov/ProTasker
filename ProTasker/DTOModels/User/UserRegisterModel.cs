using ProTasker.Domain.Enum;
using ProTasker.Domain.Models;

namespace ProTasker.DTOModels.User;

public class UserRegisterModel
{
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public string Password { get; set; }
    public string Bio { get; set; }
    public Gender Gender { get; set; }
    public string Location { get; set; }
}
