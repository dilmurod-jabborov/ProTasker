using ProTasker.Domain.Enum;

namespace ProTasker.DTOModels.Admin;

public class AdminViewModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber {  get; set; }
    public Role Role { get; set; }
    public int Age { get; set; }
}