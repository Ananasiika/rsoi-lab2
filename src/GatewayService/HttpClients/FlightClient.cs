using System.Text.Json;
using System.Text.Json.Serialization;
using GatewayService.Dto;
using GatewayService.Models;

namespace GatewayService.HttpClients;

public class FlightClient : IFlightClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FlightClient> _logger;

    public FlightClient(HttpClient httpClient, ILogger<FlightClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PaginationResponse<FlightDto>> GetFlightsAsync(int page, int size)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/flights?page={page}&size={size}");
        
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<PaginationResponse<FlightDto>>(content, options) ?? new PaginationResponse<FlightDto>();
            }
        
            _logger.LogWarning("Failed to get flights. Status: {StatusCode}", response.StatusCode);
            return new PaginationResponse<FlightDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting flights");
            return new PaginationResponse<FlightDto>();
        }
    }

    public async Task<FlightDto?> GetFlightByNumberAsync(string flightNumber)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/flights/number/{flightNumber}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
            
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
            
                return JsonSerializer.Deserialize<FlightDto>(content, options);
            }
            
            _logger.LogWarning("Flight not found: {FlightNumber}", flightNumber);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting flight by number: {FlightNumber}", flightNumber);
            return null;
        }
    }
}