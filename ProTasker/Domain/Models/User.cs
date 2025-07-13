using ProTasker.Domain.Enum;

namespace ProTasker.Domain.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public string Password { get; set; }
    public Role Role { get; set; }
    public int Age { get; set; }
    public Worker? Worker { get; set; }
}