using ProTasker.Domain.Enum;

namespace ProTasker.DTOModels.Admin;

public class AdminRegisterModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string Password { get; set; }
    public Role Role { get; set; }
    public int Age { get; set; }
}
