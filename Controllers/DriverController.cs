using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ride_Hailing_API.DTOs.Driver;
using Ride_Hailing_API.DTOs.Generic;
using Ride_Hailing_API.Extensions;
using Ride_Hailing_API.Services.Interfaces;

namespace Ride_Hailing_API.Controllers;

[ApiController]
[Route("api/driver")]
[Authorize(Roles = "Driver")]
public class DriverController(IDriverService drivers, IRideService rides) : ControllerBase
{
    private ObjectResult Respond(ApiResponse response) => StatusCode(response.HttpStatusCode, response);

    [HttpPost("onboarding")]
    public async Task<IActionResult> SubmitOnboarding(DriverOnboardingRequest request) =>
        Respond(await drivers.SubmitOnboardingAsync(User.GetUserId(), request));

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile() => Respond(await drivers.GetProfileAsync(User.GetUserId()));

    [HttpPut("availability")]
    public async Task<IActionResult> SetAvailability(DriverAvailabilityRequest request) =>
        Respond(await drivers.SetAvailabilityAsync(User.GetUserId(), request));

    [HttpGet("rides/available")]
    public async Task<IActionResult> GetAvailableRides() => Respond(await drivers.GetAvailableRidesAsync(User.GetUserId()));

    [HttpGet("rides")]
    public async Task<IActionResult> GetMyRides() => Respond(await rides.GetDriverRidesAsync(User.GetUserId()));
}
