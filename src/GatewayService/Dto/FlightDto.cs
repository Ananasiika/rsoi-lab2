namespace GatewayService.Dto;

public class FlightDto
{
    public int Id { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public int FromAirportId { get; set; }
    public int ToAirportId { get; set; }
    public int Price { get; set; }
    public AirportDto FromAirport { get; set; } = null!;
    public AirportDto ToAirport { get; set; } = null!;
}