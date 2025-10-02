using System.Text.Json.Serialization;

namespace Sales.Domain.DTOs;

public class RequestCreateOrderValidationResponse
{
    public Guid IdOrder { get; init; }
    [JsonPropertyName("Available")] public bool IsStockAvailable { get; init; }
    [JsonPropertyName("TotalAmount")] public decimal ValueAmount { get; init; }

    public DateTime CreatedAt { get; init; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
        TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"));

    public RequestCreateOrderValidationResponse()
    {
        IdOrder = Guid.Empty;
        IsStockAvailable = false;
        ValueAmount = 0;
    }
}