using ProTasker.Domain.Models;

namespace ProTasker.DTOModels.User;

public class UserUpdateModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string Bio { get; set; }
    public string Location { get; set; }
}
