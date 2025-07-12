using ProTasker.Domain.Enum;

namespace ProTasker.Domain.Models;

public class Location
{
    public int Id { get; set; }
    public Region Region { get; set; }
    public string Neighborhood {  get; set; }
    public string Street { get; set; }
}
