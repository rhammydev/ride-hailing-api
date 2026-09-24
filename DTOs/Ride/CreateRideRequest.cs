namespace Ride_Hailing_API.DTOs.Ride;

public class CreateRideRequest
{
    public string PickupLocation { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
}
