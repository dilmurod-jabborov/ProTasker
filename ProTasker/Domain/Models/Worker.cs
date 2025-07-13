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
    public string Name { get; set; }
    public string Bio { get; set; }
    public List<int> CategoryId { get; set; }
    public Gender Gender { get; set; }
    public double Rating { get; set; }
    public Location Location { get; set; }
}
