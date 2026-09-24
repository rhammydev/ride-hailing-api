namespace Ride_Hailing_API.Utilities;

public static class SmsUtils
{
    public static string Otp(string otp) =>
        $"RideHail verification code: {otp}. It expires in 5 minutes. Do not share this code with anyone.";

    public static string PasswordChanged() =>
        "RideHail security alert: your account password was changed. If this was not you, contact support immediately.";

    public static string PasswordResetOtp(string otp) =>
        $"RideHail password reset code: {otp}. It expires in 5 minutes. Do not share this code with anyone.";

    public static string AccountDeactivated() =>
        "RideHail: your account has been deactivated. Contact support if you believe this was a mistake.";

    public static string RideStatus(string rideReference, string status) =>
        $"RideHail: your ride {rideReference} has been {status}. Open the app for details.";

    public static string DriverApproved() =>
        "RideHail: congratulations! Your driver application has been approved. You can now start accepting ride requests.";

    public static string DriverRejected(string? reason) =>
        $"RideHail: your driver application was not approved.{(string.IsNullOrWhiteSpace(reason) ? "" : $" Reason: {reason}.")} Contact support for more information.";
}
