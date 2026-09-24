using System.Text.Json.Serialization;

namespace Ride_Hailing_API.DTOs.Generic;

public class ApiResponse
{
    public string ResponseCode { get; init; } = string.Empty;
    public string ResponseMessage { get; init; } = string.Empty;
    public object? Data { get; init; }
    [JsonIgnore] public int HttpStatusCode { get; init; }

    public static ApiResponse Success(string message, object? data = null, int statusCode = 200) =>
        new() { ResponseCode = "00", ResponseMessage = message, Data = data, HttpStatusCode = statusCode };

    public static ApiResponse Fail(string message, int statusCode = 400, string code = "99") =>
        new() { ResponseCode = code, ResponseMessage = message, HttpStatusCode = statusCode };
}