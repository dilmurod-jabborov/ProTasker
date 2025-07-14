using ProTasker.Constants;
using ProTasker.Domain.Enum;
using ProTasker.Helpers;

namespace ProTasker.Domain.Models;

public class User
{
    public User()
    {
        Id = GeneratorHelper.GenerateId(PathHolder.UsersFilePath);
    }

    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string Password { get; set; }
    public Role Role { get; set; } //Admin, User va Worker
    public int Age { get; set; }
    public Gender Gender { get; set; }

    public override string ToString()
    {
        return $"{Id},{FirstName},{LastName},{PhoneNumber},{Password},{Role},{Age},{Gender}";
    }
}