using Ride_Hailing_API.Domain.Enums;

namespace Ride_Hailing_API.DTOs.Ride;

public class UpdateRideStatusRequest
{
    public RideStatus Status { get; set; }
}
