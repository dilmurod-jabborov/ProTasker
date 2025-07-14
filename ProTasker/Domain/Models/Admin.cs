using ProTasker.Constants;
using ProTasker.Domain.Enum;
using ProTasker.Helpers;

namespace ProTasker.Domain.Models;

public class Admin
{
    //public Admin()
    //{
    //    Id = GeneratorHelper.GenerateId(PathHolder.AdminsFilePath);
    //}

    //public int Id { get; set; }
    //public string FirstName { get; set; }
    //public string LastName { get; set; }
    //public string PhoneNumber { get; set; }
    //public string Password { get; set; }
    //public Role Role { get; set; }
    //public int Age { get; set; }

    //public override string ToString()
    //{
    //    return $"{Id},{FirstName},{LastName},{PhoneNumber},{Password},{Role},{Age}";
    //}
    public string Username1 { get;} = "Dilmurod";
    public string Password1 { get;} = "Dilmurod";

    public string Username2 { get; } = "Ali";
    public string Password2 { get; } = "Ali";

}