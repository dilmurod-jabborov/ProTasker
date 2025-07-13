using ProTasker.Domain.Enum;

namespace ProTasker.Domain.Models;

public class Worker
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Bio { get; set; }
    public List<Category> Categories { get; set; }
    public Gender Gender { get; set; }
    public double Rating { get; set; }
    public Location Location { get; set; }
}
