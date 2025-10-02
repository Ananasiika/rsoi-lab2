namespace FlightService.Models;

public class FlightDto
{
    public string FlightNumber { get; set; } = string.Empty;
    public string FromAirport { get; set; } = string.Empty;
    public string ToAirport { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int Price { get; set; }
}