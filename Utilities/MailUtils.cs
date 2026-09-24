namespace Ride_Hailing_API.Utilities;

public static class MailUtils
{
    public static string Otp(string fullName, string otp) =>
        Template(
            "Verify your email",
            "Email verification",
            $"""
             <p style="margin:0 0 18px">Hello {fullName},</p>
             <p style="margin:0 0 22px">Use this one-time code to finish setting up your RideHail account.</p>
             <div style="background:#eef4ff;border:1px solid #b8d4fe;border-radius:16px;padding:24px;text-align:center;margin:0 0 22px">
               <div style="font-size:12px;letter-spacing:2px;text-transform:uppercase;color:#3b6cb5;margin-bottom:10px">Verification code</div>
               <div style="font-family:Consolas,monospace;font-size:34px;font-weight:800;letter-spacing:9px;color:#1a4a8a">{otp}</div>
             </div>
             <div style="background:#fff7e8;border-left:4px solid #f0a23b;border-radius:8px;padding:14px 16px;color:#76501d;font-size:14px">
               This code expires in <strong>5 minutes</strong>. Never share it with anyone.
             </div>
             <p style="margin:22px 0 0;font-size:13px;color:#667085">If you did not create this account, you can safely ignore this message.</p>
             """);

    public static string Welcome(string fullName, string email, string userId) => Template(
        "Welcome to RideHail",
        "Registration received",
        $"""
         <p style="margin:0 0 18px">Hello {fullName},</p>
         <p style="margin:0 0 22px">Welcome to RideHail. Your account has been created successfully.</p>
         <div style="background:#fff7e8;border:1px solid #fedf89;border-radius:14px;padding:18px;margin-bottom:22px">
           <div style="font-size:12px;text-transform:uppercase;letter-spacing:1px;color:#b54708;font-weight:700;margin-bottom:8px">Next step</div>
           <div style="font-size:16px;color:#93370d;font-weight:750">Verify your account with the OTP we sent you</div>
         </div>
         {Details(("User ID", userId), ("Email", email), ("Account status", "Awaiting verification"))}
         <p style="margin:22px 0 0">Your OTP expires after five minutes. Once verified, your account will become active.</p>
         """);

    public static string AccountVerified(string fullName, string email, string userId) => Template(
        "Account verified",
        "Account activated",
        $"""
         <p style="margin:0 0 18px">Hello {fullName},</p>
         <p style="margin:0 0 22px">Your contact details have been verified and your RideHail account is now active.</p>
         <div style="background:#ecfdf3;border:1px solid #abefc6;border-radius:14px;padding:18px;margin-bottom:22px">
           <div style="font-size:12px;text-transform:uppercase;letter-spacing:1px;color:#067647;font-weight:700;margin-bottom:8px">Account status</div>
           <div style="font-size:18px;color:#05603a;font-weight:750">Verified and active</div>
         </div>
         {Details(("User ID", userId), ("Email", email), ("Access", "Book and manage rides"))}
         """);

    public static string LoginAlert(string fullName, DateTime occurredAt) => Template(
        "New login detected",
        "Security notification",
        $"""
         <p style="margin:0 0 18px">Hello {fullName},</p>
         <p style="margin:0 0 22px">A successful login to your RideHail account was recorded.</p>
         {Details(("Date and time", FormatUtc(occurredAt)), ("Status", "Successful"))}
         <div style="background:#fef3f2;border:1px solid #fecdca;border-radius:12px;padding:16px;color:#912018;margin-top:22px">
           If this was not you, change your password immediately and contact support.
         </div>
         """);

    public static string PasswordChanged(string fullName, DateTime occurredAt) => Template(
        "Password changed",
        "Security confirmation",
        $"""
         <p style="margin:0 0 18px">Hello {fullName},</p>
         <p style="margin:0 0 22px">The password for your RideHail account was changed successfully.</p>
         {Details(("Changed at", FormatUtc(occurredAt)), ("Security status", "Password updated"))}
         <div style="background:#ecfdf3;border-left:4px solid #17b26a;border-radius:8px;padding:14px 16px;color:#05603a;margin-top:22px">
           No further action is needed if you made this change.
         </div>
         <p style="margin:20px 0 0;font-size:13px;color:#b42318">If you did not change your password, contact support immediately.</p>
         """);

    public static string PasswordResetOtp(string fullName, string otp) => Template(
        "Reset your password",
        "Password assistance",
        $"""
         <p style="margin:0 0 18px">Hello {fullName},</p>
         <p style="margin:0 0 22px">We received a request to reset your RideHail account password. Use the code below to continue.</p>
         <div style="background:#eef4ff;border:1px solid #b8d4fe;border-radius:16px;padding:24px;text-align:center;margin:0 0 22px">
           <div style="font-size:12px;letter-spacing:2px;text-transform:uppercase;color:#3b6cb5;margin-bottom:10px">Password reset code</div>
           <div style="font-family:Consolas,monospace;font-size:34px;font-weight:800;letter-spacing:9px;color:#1a4a8a">{otp}</div>
         </div>
         <div style="background:#fff7e8;border-left:4px solid #f0a23b;border-radius:8px;padding:14px 16px;color:#76501d;font-size:14px">
           This code expires in <strong>5 minutes</strong>. Never share it with anyone.
         </div>
         <p style="margin:22px 0 0;font-size:13px;color:#667085">If you did not request a password reset, you can safely ignore this message. Your password has not changed.</p>
         """);

    public static string ProfileUpdated(string fullName, DateTime occurredAt) => Template(
        "Profile updated",
        "Account confirmation",
        $"""
         <p style="margin:0 0 18px">Hello {fullName},</p>
         <p style="margin:0 0 22px">Your RideHail profile information was updated successfully.</p>
         {Details(("Updated at", FormatUtc(occurredAt)), ("Status", "Changes saved"))}
         <p style="margin:20px 0 0;font-size:13px;color:#667085">If you did not request this change, please contact support.</p>
         """);

    public static string AccountDeactivated(string fullName, DateTime occurredAt) => Template(
        "Account deactivated",
        "Account notification",
        $"""
         <p style="margin:0 0 18px">Hello {fullName},</p>
         <p style="margin:0 0 22px">Your RideHail account has been deactivated and can no longer be used to sign in.</p>
         {Details(("Deactivated at", FormatUtc(occurredAt)), ("Status", "Inactive"))}
         <div style="background:#fef3f2;border:1px solid #fecdca;border-radius:12px;padding:16px;color:#912018;margin-top:22px">Contact support if you believe this was a mistake.</div>
         """);

    public static string RideStatus(string fullName, string rideReference, string pickupLocation,
        string destination, string status, string? cancellationReason) => Template(
        $"Ride {status}",
        "Ride notification",
        $"""
         <p style="margin:0 0 18px">Hello {fullName},</p>
         <p style="margin:0 0 22px">Your ride has been <strong>{status}</strong>.</p>
         {Details(
             ("Reference", rideReference),
             ("Pickup", pickupLocation),
             ("Destination", destination),
             ("Status", status),
             ("Reason", string.IsNullOrWhiteSpace(cancellationReason) ? "N/A" : cancellationReason))}
         <p style="margin:20px 0 0;font-size:13px;color:#667085">Open the RideHail app for live tracking and details.</p>
         """);

    public static string DriverApproved(string fullName) => Template(
        "Driver application approved",
        "Driver onboarding",
        $"""
         <p style="margin:0 0 18px">Hello {fullName},</p>
         <p style="margin:0 0 22px">Congratulations! Your driver application has been reviewed and approved.</p>
         <div style="background:#ecfdf3;border:1px solid #abefc6;border-radius:14px;padding:18px;margin-bottom:22px">
           <div style="font-size:12px;text-transform:uppercase;letter-spacing:1px;color:#067647;font-weight:700;margin-bottom:8px">Application status</div>
           <div style="font-size:18px;color:#05603a;font-weight:750">Approved</div>
         </div>
         <p style="margin:0 0 0">You can now log in and start accepting ride requests. Drive safely!</p>
         """);

    public static string DriverRejected(string fullName, string? reason) => Template(
        "Driver application update",
        "Driver onboarding",
        $"""
         <p style="margin:0 0 18px">Hello {fullName},</p>
         <p style="margin:0 0 22px">We regret to inform you that your driver application was not approved at this time.</p>
         <div style="background:#fef3f2;border:1px solid #fecdca;border-radius:14px;padding:18px;margin-bottom:22px">
           <div style="font-size:12px;text-transform:uppercase;letter-spacing:1px;color:#912018;font-weight:700;margin-bottom:8px">Application status</div>
           <div style="font-size:18px;color:#7a271a;font-weight:750">Not approved</div>
         </div>
         {(string.IsNullOrWhiteSpace(reason)
             ? ""
             : $"""
                {Details(("Reason", reason))}
                """)}
         <p style="margin:20px 0 0">Please contact support for further details or to reapply.</p>
         """);

    private static string Template(string title, string eyebrow, string content) =>
        $"""
         <!doctype html>
         <html lang="en">
         <head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>{title}</title></head>
         <body style="margin:0;background:#eef2f7;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Arial,sans-serif;color:#263238">
           <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#eef2f7;padding:36px 12px">
             <tr><td align="center">
               <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:590px;background:#fff;border-radius:22px;overflow:hidden;box-shadow:0 12px 36px rgba(20,50,90,.12)">
                 <tr><td style="background:#1a4a8a;padding:32px 38px;color:#fff">
                   <table role="presentation" width="100%"><tr>
                     <td style="width:50px"><div style="width:44px;height:44px;line-height:44px;text-align:center;border-radius:13px;background:#fff;color:#1a4a8a;font-size:22px;font-weight:800">RH</div></td>
                     <td><div style="font-size:12px;letter-spacing:2px;text-transform:uppercase;color:#a7c4e8">{eyebrow}</div><div style="font-size:24px;font-weight:750;margin-top:5px">{title}</div></td>
                   </tr></table>
                 </td></tr>
                 <tr><td style="padding:34px 38px;font-size:15px;line-height:1.65">{content}</td></tr>
                 <tr><td style="padding:24px 38px;background:#f5f8fb;border-top:1px solid #dce4ed;color:#667085;font-size:12px;line-height:1.6">
                   <strong style="color:#1a4a8a">RideHail</strong><br>
                   Your ride, your way.<br>
                   This is an automated message; please do not reply.
                 </td></tr>
               </table>
             </td></tr>
           </table>
         </body>
         </html>
         """;

    private static string Details(params (string Label, string Value)[] rows)
    {
        var body = string.Join(string.Empty, rows.Select(row =>
            $"<tr><td style='padding:12px 14px;color:#667085;border-bottom:1px solid #dce4ed;width:38%'>{row.Label}</td>" +
            $"<td style='padding:12px 14px;font-weight:650;border-bottom:1px solid #dce4ed'>{row.Value}</td></tr>"));
        return $"<table role='presentation' width='100%' cellspacing='0' cellpadding='0' style='border:1px solid #dce4ed;border-radius:12px;border-collapse:separate;overflow:hidden'>{body}</table>";
    }

    private static string FormatUtc(DateTime value) =>
        value.ToUniversalTime().ToString("dddd, dd MMMM yyyy 'at' HH:mm 'UTC'");
}
