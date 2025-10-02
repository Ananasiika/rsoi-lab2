namespace GatewayService.Dto;

public class FlightDto
{
    public string FlightNumber { get; set; } = string.Empty;
    public string FromAirport { get; set; } = string.Empty;  // Просто строка, не объект
    public string ToAirport { get; set; } = string.Empty;    // Просто строка, не объект
    public DateTime Date { get; set; }
    public int Price { get; set; }
}