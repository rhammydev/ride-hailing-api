using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Ride_Hailing_API.Domain.Entities;
using Ride_Hailing_API.Domain.Enums;
using Ride_Hailing_API.Domain.Settings;
using Ride_Hailing_API.DTOs.Auth;
using Ride_Hailing_API.DTOs.Generic;
using Ride_Hailing_API.Repositories.Interfaces;
using Ride_Hailing_API.Services.Interfaces;

namespace Ride_Hailing_API.Services.Implementations;

public class AuthService(
    IUserRepository userRepository,
    IOtpRepository otpRepository,
    IAuditLogRepository auditLogRepository,
    INotificationService notificationService,
    IOptions<JwtSettings> jwtOptions,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public async Task<ApiResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var email = request.Email.Trim().ToLowerInvariant();
            if (await userRepository.EmailExistsAsync(email))
            {
                return ApiResponse.Fail("A user with this email address already exists.", 409, ResponseCodes.BadRequest);
            }

            var phone = request.PhoneNumber.Trim();
            if (await userRepository.PhoneExistsAsync(phone))
            {
                return ApiResponse.Fail("A user with this phone number already exists.", 409, ResponseCodes.BadRequest);
            }

            var nameParts = request.FullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var firstName = nameParts.Length > 0 ? nameParts[0] : request.FullName.Trim();
            var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PhoneNumber = phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role,
                IsEmailVerified = false,
                IsPhoneVerified = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await userRepository.AddAsync(user);
            await userRepository.SaveChangesAsync();

            var emailOtpCode = await IssueOtpAsync(user.Id, OtpPurpose.EmailVerification);
            var phoneOtpCode = await IssueOtpAsync(user.Id, OtpPurpose.PhoneVerification);

            await auditLogRepository.AddAsync(new AuditLog
            {
                UserId = user.Id,
                Action = "Registration",
                Status = "Success",
                CreatedAt = DateTime.UtcNow
            });
            await auditLogRepository.SaveChangesAsync();

            await notificationService.SendVerificationOtpsAsync(user, emailOtpCode, phoneOtpCode);

            logger.LogInformation("Registered {Role} with ID {UserId} ({Email})", user.Role, user.Id, user.Email);

            return ApiResponse.Success(
                "Registration successful. Verification OTPs have been sent to your email and phone number.",
                new { user.Id, user.Email, user.Role },
                201
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Registration failed for {Email}", request.Email);
            return ApiResponse.Fail("An unexpected error occurred during registration.", 500, ResponseCodes.ServerError);
        }
    }

    public async Task<ApiResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await userRepository.GetByEmailAsync(email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                await auditLogRepository.AddAsync(new AuditLog
                {
                    UserId = user?.Id,
                    Action = "Login",
                    Status = "Failed",
                    CreatedAt = DateTime.UtcNow
                });
                await auditLogRepository.SaveChangesAsync();

                return ApiResponse.Fail("Invalid email or password.", 401, ResponseCodes.Unauthorized);
            }

            if (!user.IsActive)
            {
                await auditLogRepository.AddAsync(new AuditLog
                {
                    UserId = user.Id,
                    Action = "Login",
                    Status = "Failed - Account Deactivated",
                    CreatedAt = DateTime.UtcNow
                });
                await auditLogRepository.SaveChangesAsync();

                return ApiResponse.Fail("Your account has been deactivated. Please contact support.", 403, ResponseCodes.Forbidden);
            }

            if (!user.IsEmailVerified)
            {
                await auditLogRepository.AddAsync(new AuditLog
                {
                    UserId = user.Id,
                    Action = "Login",
                    Status = "Failed - Email Not Verified",
                    CreatedAt = DateTime.UtcNow
                });
                await auditLogRepository.SaveChangesAsync();

                return ApiResponse.Fail("Please verify your email address before logging in.", 403, ResponseCodes.Forbidden);
            }

            await auditLogRepository.AddAsync(new AuditLog
            {
                UserId = user.Id,
                Action = "Login",
                Status = "Success",
                CreatedAt = DateTime.UtcNow
            });
            await auditLogRepository.SaveChangesAsync();

            var tokenResponse = GenerateJwt(user);
            logger.LogInformation("User {UserId} ({Email}) logged in successfully.", user.Id, user.Email);

            return ApiResponse.Success("Login successful.", tokenResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Login failed for {Email}", request.Email);
            return ApiResponse.Fail("An unexpected error occurred during login.", 500, ResponseCodes.ServerError);
        }
    }

    public async Task<ApiResponse> VerifyEmailAsync(VerifyEmailRequest request)
    {
        try
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return ApiResponse.Fail("User not found.", 404, ResponseCodes.NotFound);
            }

            if (user.IsEmailVerified)
            {
                return ApiResponse.Success("Email is already verified.");
            }

            var otp = await otpRepository.GetLatestUnusedOtpAsync(user.Id, OtpPurpose.EmailVerification);
            if (otp == null || otp.Code != request.OtpCode)
            {
                return ApiResponse.Fail("Invalid or expired verification code.", 400, ResponseCodes.BadRequest);
            }

            if (otp.ExpiresAt < DateTime.UtcNow)
            {
                return ApiResponse.Fail("Verification code has expired. Please request a new one.", 400, ResponseCodes.BadRequest);
            }

            otp.IsUsed = true;
            otp.VerifiedAt = DateTime.UtcNow;
            user.IsEmailVerified = true;

            userRepository.Update(user);
            await userRepository.SaveChangesAsync();
            await otpRepository.SaveChangesAsync();

            await auditLogRepository.AddAsync(new AuditLog
            {
                UserId = user.Id,
                Action = "EmailVerification",
                Status = "Success",
                CreatedAt = DateTime.UtcNow
            });
            await auditLogRepository.SaveChangesAsync();

            return ApiResponse.Success("Email address verified successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Email verification failed for {Email}", request.Email);
            return ApiResponse.Fail("An unexpected error occurred during email verification.", 500, ResponseCodes.ServerError);
        }
    }

    public async Task<ApiResponse> VerifyPhoneAsync(VerifyPhoneRequest request)
    {
        try
        {
            var phone = request.PhoneNumber.Trim();
            var user = await userRepository.GetByPhoneNumberAsync(phone);
            if (user == null)
            {
                return ApiResponse.Fail("User with this phone number not found.", 404, ResponseCodes.NotFound);
            }

            if (user.IsPhoneVerified)
            {
                return ApiResponse.Success("Phone number is already verified.");
            }

            var otp = await otpRepository.GetLatestUnusedOtpAsync(user.Id, OtpPurpose.PhoneVerification);
            if (otp == null || otp.Code != request.OtpCode)
            {
                return ApiResponse.Fail("Invalid or expired verification code.", 400, ResponseCodes.BadRequest);
            }

            if (otp.ExpiresAt < DateTime.UtcNow)
            {
                return ApiResponse.Fail("Verification code has expired. Please request a new one.", 400, ResponseCodes.BadRequest);
            }

            otp.IsUsed = true;
            otp.VerifiedAt = DateTime.UtcNow;
            user.IsPhoneVerified = true;

            userRepository.Update(user);
            await userRepository.SaveChangesAsync();
            await otpRepository.SaveChangesAsync();

            await auditLogRepository.AddAsync(new AuditLog
            {
                UserId = user.Id,
                Action = "PhoneVerification",
                Status = "Success",
                CreatedAt = DateTime.UtcNow
            });
            await auditLogRepository.SaveChangesAsync();

            return ApiResponse.Success("Phone number verified successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Phone verification failed for {Phone}", request.PhoneNumber);
            return ApiResponse.Fail("An unexpected error occurred during phone verification.", 500, ResponseCodes.ServerError);
        }
    }

    public async Task<ApiResponse> ResendOtpAsync(ResendOtpRequest request)
    {
        try
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return ApiResponse.Fail("User not found.", 404, ResponseCodes.NotFound);
            }

            var isEmail = request.Purpose == OtpPurpose.EmailVerification;
            if (isEmail ? user.IsEmailVerified : user.IsPhoneVerified)
            {
                return ApiResponse.Success(isEmail ? "Email is already verified." : "Phone number is already verified.");
            }

            // Issuing a new OTP invalidates any previously issued, unused OTP for the same purpose.
            var otpCode = await IssueOtpAsync(user.Id, request.Purpose);

            await auditLogRepository.AddAsync(new AuditLog
            {
                UserId = user.Id,
                Action = isEmail ? "ResendEmailOtp" : "ResendPhoneOtp",
                Status = "Success",
                CreatedAt = DateTime.UtcNow
            });
            await auditLogRepository.SaveChangesAsync();

            if (isEmail)
            {
                await notificationService.SendEmailOtpAsync(user, otpCode);
                return ApiResponse.Success("A new verification OTP has been sent to your email.");
            }

            await notificationService.SendPhoneOtpAsync(user, otpCode);
            return ApiResponse.Success("A new verification OTP has been sent to your phone number.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Resend OTP failed for {Email}", request.Email);
            return ApiResponse.Fail("An unexpected error occurred while resending the OTP.", 500, ResponseCodes.ServerError);
        }
    }

    public async Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        try
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await userRepository.GetByEmailAsync(email);

            if (user != null)
            {
                var otpCode = await IssueOtpAsync(user.Id, OtpPurpose.PasswordReset);

                await auditLogRepository.AddAsync(new AuditLog
                {
                    UserId = user.Id,
                    Action = "ForgotPassword",
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow
                });
                await auditLogRepository.SaveChangesAsync();

                await notificationService.SendPasswordResetOtpAsync(user, otpCode);
            }

            // User Story 35: Password-reset responses must not expose whether an email address exists.
            return ApiResponse.Success("If your email address is registered, a password reset code has been sent.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Forgot password failed for {Email}", request.Email);
            return ApiResponse.Fail("An unexpected error occurred while processing your request.", 500, ResponseCodes.ServerError);
        }
    }

    public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        try
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return ApiResponse.Fail("Invalid password reset request.", 400, ResponseCodes.BadRequest);
            }

            var otp = await otpRepository.GetLatestUnusedOtpAsync(user.Id, OtpPurpose.PasswordReset);
            if (otp == null || otp.Code != request.OtpCode)
            {
                return ApiResponse.Fail("Invalid or expired password reset code.", 400, ResponseCodes.BadRequest);
            }

            if (otp.ExpiresAt < DateTime.UtcNow)
            {
                return ApiResponse.Fail("Password reset code has expired. Please request a new one.", 400, ResponseCodes.BadRequest);
            }

            otp.IsUsed = true;
            otp.VerifiedAt = DateTime.UtcNow;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            userRepository.Update(user);
            await userRepository.SaveChangesAsync();
            await otpRepository.SaveChangesAsync();

            await auditLogRepository.AddAsync(new AuditLog
            {
                UserId = user.Id,
                Action = "ResetPassword",
                Status = "Success",
                CreatedAt = DateTime.UtcNow
            });
            await auditLogRepository.SaveChangesAsync();

            await notificationService.SendPasswordChangedAsync(user);

            return ApiResponse.Success("Password has been reset successfully. You can now log in.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Password reset failed for {Email}", request.Email);
            return ApiResponse.Fail("An unexpected error occurred during password reset.", 500, ResponseCodes.ServerError);
        }
    }

    public async Task<ApiResponse> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse.Fail("User not found.", 404, ResponseCodes.NotFound);
            }

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return ApiResponse.Fail("Current password is incorrect.", 400, ResponseCodes.BadRequest);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            userRepository.Update(user);
            await userRepository.SaveChangesAsync();

            await auditLogRepository.AddAsync(new AuditLog
            {
                UserId = user.Id,
                Action = "ChangePassword",
                Status = "Success",
                CreatedAt = DateTime.UtcNow
            });
            await auditLogRepository.SaveChangesAsync();

            // Story 76: Send security notification after a successful password change.
            await notificationService.SendPasswordChangedAsync(user);

            return ApiResponse.Success("Password changed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Change password failed for User {UserId}", userId);
            return ApiResponse.Fail("An unexpected error occurred while changing password.", 500, ResponseCodes.ServerError);
        }
    }

    private async Task<string> IssueOtpAsync(int userId, OtpPurpose purpose)
    {
        await otpRepository.InvalidateUnusedOtpsAsync(userId, purpose);

        var otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString("D6");
        await otpRepository.AddAsync(new Otp
        {
            UserId = userId,
            Code = otpCode,
            Purpose = purpose,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        });
        await otpRepository.SaveChangesAsync();

        return otpCode;
    }

    private LoginResponse GenerateJwt(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);

        var fullName = $"{user.FirstName} {user.LastName}".Trim();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, fullName),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("phoneNumber", user.PhoneNumber ?? string.Empty)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt
        };
    }
}
