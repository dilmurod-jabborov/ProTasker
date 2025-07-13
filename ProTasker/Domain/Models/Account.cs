using ProTasker.Domain.Enum;

namespace ProTasker.Domain.Models;

public class Account //Base class 
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Password { get; set; }
    public string PhoneNumber { get; set; }
    public Role Role { get; set; } //Admin, User va Worker
}
