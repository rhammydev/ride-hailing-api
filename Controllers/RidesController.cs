using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ride_Hailing_API.DTOs.Generic;
using Ride_Hailing_API.DTOs.Ride;
using Ride_Hailing_API.Extensions;
using Ride_Hailing_API.Services.Interfaces;

namespace Ride_Hailing_API.Controllers;

[ApiController]
[Route("api/rides")]
[Authorize]
public class RidesController(IRideService rides) : ControllerBase
{
    private ObjectResult Respond(ApiResponse response) => StatusCode(response.HttpStatusCode, response);

    [Authorize(Roles = "Passenger")]
    [HttpPost]
    public async Task<IActionResult> CreateRide(CreateRideRequest request) =>
        Respond(await rides.CreateRideAsync(User.GetUserId(), request));

    [Authorize(Roles = "Passenger")]
    [HttpGet("my-rides")]
    public async Task<IActionResult> GetMyRides() => Respond(await rides.GetPassengerRidesAsync(User.GetUserId()));

    [Authorize(Roles = "Passenger")]
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentRide() => Respond(await rides.GetCurrentRideAsync(User.GetUserId()));

    [Authorize(Roles = "Passenger,Driver,Admin")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetRideById(int id) =>
        Respond(await rides.GetRideByIdAsync(id, User.GetUserId(), User.GetRole()));

    [Authorize(Roles = "Passenger,Driver,Admin")]
    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> CancelRide(int id, CancelRideRequest request) =>
        Respond(await rides.CancelRideAsync(User.GetUserId(), User.GetRole(), id, request));

    [Authorize(Roles = "Driver")]
    [HttpPut("{id:int}/accept")]
    public async Task<IActionResult> AcceptRide(int id) => Respond(await rides.AcceptRideAsync(User.GetUserId(), id));

    [Authorize(Roles = "Driver")]
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateRideStatus(int id, UpdateRideStatusRequest request) =>
        Respond(await rides.UpdateRideStatusAsync(User.GetUserId(), id, request));
}
