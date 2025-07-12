namespace ProTasker.Domain.Models;

public class Booking
{
    public int Id { get; set; }
    public int WorkerId { get; set; }
    public DateTime BookedAt { get; set; }
    public string Status { get; set; } // Busy, Available, Offline
}