using ProTasker.Constants;
using ProTasker.Domain.Enum;
using ProTasker.Helpers;

namespace ProTasker.Domain.Models;

public class Worker 
{
    public Worker()
    {
        Id = GeneratorHelper.GenerateId(PathHolder.WorkersFilePath);
    }

    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string Password { get; set; }
    public Role Role { get; set; }
    public int Age { get; set; }
    public string Bio { get; set; }
    public List<int> CategoryId { get; set; }
    public Location Location { get; set; }

    public override string ToString()
    {
        var categories = string.Join(";", CategoryId);
        var location = Location is not null
            ? $"{Location.Region}|{Location.District}|{Location.Street}"
            : "";

        return $"{Id},{FirstName},{LastName},{PhoneNumber},{Password},{Role},{Age},{Bio},{categories},{location}";
    }
}
