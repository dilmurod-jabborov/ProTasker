using ProTasker.Domain.Enum;

namespace ProTasker.Domain.Models;

public class Booking
{
    public int Id { get; set; }
    public int WorkerId { get; set; }
    public DateTime BookedAt { get; set; }
    public Status Status { get; set; } // Busy, Available, Offline
}