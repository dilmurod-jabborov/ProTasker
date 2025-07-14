using ProTasker.Domain.Enum;

namespace ProTasker.Domain.Models;

public class Location
{
    public Region Region { get; set; }
    public string District { get; set; }
    public string Street { get; set; }
}
