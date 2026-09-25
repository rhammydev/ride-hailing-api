using System.Net.Http.Headers;
using Ride_Hailing_API.Domain.Settings;
using Ride_Hailing_API.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace Ride_Hailing_API.Services.Implementations;

public class TelecomAbodeSmsService(
    HttpClient httpClient,
    IOptions<TelecomAbode> options,
    ILogger<TelecomAbodeSmsService> logger) : ISmsService
{
    private readonly TelecomAbode _settings = options.Value;

    public async Task<bool> SendSmsAsync(string? recipientNumber, string message)
    {
        if (!IsConfigured())
        {
            logger.LogWarning("TelecomAbode is not configured. SMS delivery was skipped.");
            return false;
        }

        var recipients = ParseRecipients(recipientNumber);
        AddFallbackRecipient(recipients);
        if (recipients.Count == 0)
        {
            logger.LogWarning("SMS delivery was skipped because neither a recipient nor fallback recipient is configured.");
            return false;
        }

        var bulkPhones = string.Join(',', recipients);
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["subject"] = _settings.Subject,
            ["bulkPhones"] = bulkPhones,
            ["message"] = message
        });
        using var request = new HttpRequestMessage(HttpMethod.Post, _settings.BaseUrl) { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        try
        {
            logger.LogInformation("Sending SMS to {RecipientCount} recipient(s) through TelecomAbode.", recipients.Count);
            using var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("TelecomAbode accepted SMS delivery for {RecipientCount} recipient(s).", recipients.Count);
                return true;
            }
            else
            {
                logger.LogWarning("TelecomAbode rejected SMS delivery with status {StatusCode}. Response: {Response}",
                    (int)response.StatusCode, responseBody);
                return false;
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An error occurred while sending SMS through TelecomAbode.");
            return false;
        }
    }

    private bool IsConfigured() =>
        Uri.TryCreate(_settings.BaseUrl, UriKind.Absolute, out _) &&
        !string.IsNullOrWhiteSpace(_settings.ApiKey) &&
        !string.IsNullOrWhiteSpace(_settings.Subject);

    private static List<string> ParseRecipients(string? recipientNumbers) =>
        (recipientNumbers ?? string.Empty)
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(NormalizePhoneNumber)
        .Where(x => !string.IsNullOrWhiteSpace(x))
        .Distinct(StringComparer.Ordinal)
        .ToList();

    private void AddFallbackRecipient(List<string> recipients)
    {
        if (recipients.Count > 1 || string.IsNullOrWhiteSpace(_settings.FallbackRecipient)) return;
        var fallback = NormalizePhoneNumber(_settings.FallbackRecipient);
        if (!string.IsNullOrWhiteSpace(fallback) && !recipients.Contains(fallback, StringComparer.Ordinal))
            recipients.Add(fallback);
    }

    private static string NormalizePhoneNumber(string number)
    {
        var cleanPhone = new string(number.Trim().Where(x => char.IsDigit(x) || x == '+').ToArray());
        if (cleanPhone.StartsWith("+234", StringComparison.Ordinal)) return $"0{cleanPhone[4..]}";
        if (cleanPhone.StartsWith("234", StringComparison.Ordinal)) return $"0{cleanPhone[3..]}";
        return cleanPhone.StartsWith('+') ? cleanPhone[1..] : cleanPhone;
    }
}
