using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ride_Hailing_API.DTOs.Admin;
using Ride_Hailing_API.DTOs.Generic;
using Ride_Hailing_API.Extensions;
using Ride_Hailing_API.Services.Interfaces;

namespace Ride_Hailing_API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(IAdminService admin) : ControllerBase
{
    private ObjectResult Respond(ApiResponse response) => StatusCode(response.HttpStatusCode, response);

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers() => Respond(await admin.GetAllUsersAsync());

    [HttpPut("users/{id:int}/status")]
    public async Task<IActionResult> UpdateUserStatus(int id, UpdateUserStatusRequest request) =>
        Respond(await admin.UpdateUserStatusAsync(User.GetUserId(), id, request));

    [HttpGet("drivers")]
    public async Task<IActionResult> GetAllDrivers() => Respond(await admin.GetAllDriversAsync());

    [HttpGet("drivers/pending")]
    public async Task<IActionResult> GetPendingDrivers() => Respond(await admin.GetPendingDriversAsync());

    [HttpPut("drivers/{id:int}/approve")]
    public async Task<IActionResult> ApproveDriver(int id) => Respond(await admin.ApproveDriverAsync(User.GetUserId(), id));

    [HttpPut("drivers/{id:int}/reject")]
    public async Task<IActionResult> RejectDriver(int id, RejectDriverRequest request) =>
        Respond(await admin.RejectDriverAsync(User.GetUserId(), id, request));

    [HttpGet("rides")]
    public async Task<IActionResult> GetAllRides() => Respond(await admin.GetAllRidesAsync());

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs() => Respond(await admin.GetAuditLogsAsync());

    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications() => Respond(await admin.GetNotificationsAsync());
}
