using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ride_Hailing_API.DTOs.Generic;
using Ride_Hailing_API.DTOs.Passenger;
using Ride_Hailing_API.Extensions;
using Ride_Hailing_API.Services.Interfaces;

namespace Ride_Hailing_API.Controllers;

[ApiController]
[Route("api/passenger")]
[Authorize(Roles = "Passenger")]
public class PassengerController(IPassengerService passengers) : ControllerBase
{
    private ObjectResult Respond(ApiResponse response) => StatusCode(response.HttpStatusCode, response);

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile() => Respond(await passengers.GetProfileAsync(User.GetUserId()));

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request) =>
        Respond(await passengers.UpdateProfileAsync(User.GetUserId(), request));
}
