using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Ride_Hailing_API.Domain.Entities;
using Ride_Hailing_API.Domain.Enums;
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
    IConfiguration configuration,
    ILogger<AuthService> logger) : IAuthService
{
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

            var otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString("D6");
            var emailOtp = new Otp
            {
                UserId = user.Id,
                Code = otpCode,
                Purpose = OtpPurpose.EmailVerification,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            await otpRepository.AddAsync(emailOtp);
            await otpRepository.SaveChangesAsync();

            await auditLogRepository.AddAsync(new AuditLog
            {
                UserId = user.Id,
                Action = "Registration",
                Status = "Success",
                CreatedAt = DateTime.UtcNow
            });
            await auditLogRepository.SaveChangesAsync();

            await notificationService.SendOtpEmailAsync(
                user.Email ?? email,
                $"{user.FirstName} {user.LastName}".Trim(),
                otpCode
            );

            logger.LogInformation("Registered {Role} with ID {UserId} ({Email})", user.Role, user.Id, user.Email);

            return ApiResponse.Success(
                "Registration successful. A verification OTP has been sent to your email.",
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

    public async Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        try
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await userRepository.GetByEmailAsync(email);

            if (user != null)
            {
                await otpRepository.InvalidateUnusedOtpsAsync(user.Id, OtpPurpose.PasswordReset);

                var otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString("D6");
                var resetOtp = new Otp
                {
                    UserId = user.Id,
                    Code = otpCode,
                    Purpose = OtpPurpose.PasswordReset,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                };

                await otpRepository.AddAsync(resetOtp);
                await otpRepository.SaveChangesAsync();

                await auditLogRepository.AddAsync(new AuditLog
                {
                    UserId = user.Id,
                    Action = "ForgotPassword",
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow
                });
                await auditLogRepository.SaveChangesAsync();

                await notificationService.SendPasswordResetOtpAsync(
                    user.Email ?? string.Empty,
                    $"{user.FirstName} {user.LastName}".Trim(),
                    user.PhoneNumber,
                    otpCode
                );
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

            await notificationService.SendPasswordChangedAsync(
                user.Email ?? string.Empty,
                $"{user.FirstName} {user.LastName}".Trim(),
                user.PhoneNumber
            );

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
            await notificationService.SendPasswordChangedAsync(
                user.Email ?? string.Empty,
                $"{user.FirstName} {user.LastName}".Trim(),
                user.PhoneNumber
            );

            return ApiResponse.Success("Password changed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Change password failed for User {UserId}", userId);
            return ApiResponse.Fail("An unexpected error occurred while changing password.", 500, ResponseCodes.ServerError);
        }
    }

    private LoginResponse GenerateJwt(User user)
    {
        var expiryMinutes = int.TryParse(configuration["Jwt:ExpirationInMinutes"], out var minutes) ? minutes : 120;
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var fullName = $"{user.FirstName} {user.LastName}".Trim();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, fullName),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("phoneNumber", user.PhoneNumber ?? string.Empty)
        };

        var secret = configuration["Jwt:Key"] ?? "RideHailingSuperSecretKey2026!MustBeAtLeast32BytesLong";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"] ?? "RideHailingAPI",
            audience: configuration["Jwt:Audience"] ?? "RideHailingUsers",
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
