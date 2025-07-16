using ProTasker.Domain.Enum;

namespace ProTasker.DTOModels.Admin;

public class AdminViewModel
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber {  get; set; }
    public Role Role { get; set; }
    public int Age { get; set; }
}