using ProTasker.Constants;
using ProTasker.Helpers;

namespace ProTasker.Domain.Models;

public class Admin : Account
{
    public Admin()
    {
        Id = GeneratorHelper.GenerateId(PathHolder.AdminsFilePath);
    }
    public int Id { get; set; }
}