using Ride_Hailing_API.Domain.Enums;

namespace Ride_Hailing_API.Domain.Entities;

public class RideStatusHistory
{
    public int Id { get; set; }
    public int RideId { get; set; }
    public Ride? Ride { get; set; }
    public RideStatus PreviousStatus { get; set; }
    public RideStatus NewStatus { get; set; }
    public string? ChangedBy { get; set; }
    public string? Reason {get; set;}
    public DateTime CreatedAt { get; set; }
}