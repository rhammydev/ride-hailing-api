using Ride_Hailing_API.DTOs.Auth;
using Ride_Hailing_API.DTOs.Generic;

namespace Ride_Hailing_API.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse> RegisterAsync(RegisterRequest request);
    Task<ApiResponse> LoginAsync(LoginRequest request);
    Task<ApiResponse> VerifyEmailAsync(VerifyEmailRequest request);
    Task<ApiResponse> VerifyPhoneAsync(VerifyPhoneRequest request);
    Task<ApiResponse> ResendOtpAsync(ResendOtpRequest request);
    Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<ApiResponse> ResetPasswordAsync(ResetPasswordRequest request);
    Task<ApiResponse> ChangePasswordAsync(int userId, ChangePasswordRequest request);
}
