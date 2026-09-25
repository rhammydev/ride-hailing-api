using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ride_Hailing_API.DTOs.Auth;
using Ride_Hailing_API.DTOs.Generic;
using Ride_Hailing_API.Extensions;
using Ride_Hailing_API.Services.Interfaces;

namespace Ride_Hailing_API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService auth) : ControllerBase
{
    private ObjectResult Respond(ApiResponse response) => StatusCode(response.HttpStatusCode, response);

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request) => Respond(await auth.RegisterAsync(request));

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request) => Respond(await auth.VerifyEmailAsync(request));

    [HttpPost("verify-phone")]
    public async Task<IActionResult> VerifyPhone(VerifyPhoneRequest request) => Respond(await auth.VerifyPhoneAsync(request));

    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp(ResendOtpRequest request) => Respond(await auth.ResendOtpAsync(request));

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request) => Respond(await auth.LoginAsync(request));

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request) => Respond(await auth.ForgotPasswordAsync(request));

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request) => Respond(await auth.ResetPasswordAsync(request));

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request) =>
        Respond(await auth.ChangePasswordAsync(User.GetUserId(), request));
}
