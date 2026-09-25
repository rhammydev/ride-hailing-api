using System.ComponentModel.DataAnnotations;
using Ride_Hailing_API.Domain.Enums;

namespace Ride_Hailing_API.Domain.Entities;

public class Ride
{
    public int Id { get; set; }
    public int? DriverId { get; set; }
    public int PassengerId { get; set; }
    public string? Reference { get; set; }
    public string? PickupLocation { get; set; }
    public string? Destination {get; set;}
    public string? CancellationReason {get; set;}   
    public RideStatus Status { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    public User? Passenger { get; set; }
    public User? Driver { get; set; }
}