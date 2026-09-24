using Ride_Hailing_API.Domain.Enums;

namespace Ride_Hailing_API.DTOs.Ride;

public class RideResponse
{
    public int Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public int PassengerId { get; set; }
    public string PassengerName { get; set; } = string.Empty;
    public int? DriverId { get; set; }
    public string? DriverName { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public RideStatus Status { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}
