using ProTasker.Constants;
using ProTasker.Domain.Enum;
using ProTasker.Helpers;

namespace ProTasker.Domain.Models;

public class User : Account
{
    public User()
    {
        Id = GeneratorHelper.GenerateId(PathHolder.UsersFilePath);
    }

    public int Id { get; set; }
    public int Age { get; set; }
    public Gender Gender { get; set; }
}