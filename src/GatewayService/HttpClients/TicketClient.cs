using System.Text;
using System.Text.Json;
using GatewayService.Models;

namespace GatewayService.HttpClients;

public class TicketClient : ITicketClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TicketClient> _logger;

    public TicketClient(HttpClient httpClient, ILogger<TicketClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<TicketResponse>> GetUserTicketsAsync(string username)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/tickets");
            request.Headers.Add("X-User-Name", username);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<TicketResponse>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<TicketResponse>();
            }
            
            _logger.LogWarning("Failed to get tickets for user: {Username}", username);
            return new List<TicketResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tickets for user: {Username}", username);
            return new List<TicketResponse>();
        }
    }

    public async Task<TicketResponse?> GetTicketAsync(string username, Guid ticketUid)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/tickets/{ticketUid}");
            request.Headers.Add("X-User-Name", username);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TicketResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            _logger.LogWarning("Ticket not found: {TicketUid} for user: {Username}", ticketUid, username);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ticket: {TicketUid} for user: {Username}", ticketUid, username);
            return null;
        }
    }

    public async Task<TicketPurchaseResponse?> PurchaseTicketAsync(string username, TicketPurchaseRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tickets")
            {
                Content = content
            };
            httpRequest.Headers.Add("X-User-Name", username);

            var response = await _httpClient.SendAsync(httpRequest);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TicketPurchaseResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            _logger.LogWarning("Failed to purchase ticket for user: {Username}. Status: {StatusCode}", 
                username, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purchasing ticket for user: {Username}", username);
            return null;
        }
    }

    public async Task<bool> CancelTicketAsync(string username, Guid ticketUid)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/tickets/{ticketUid}");
            request.Headers.Add("X-User-Name", username);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling ticket: {TicketUid} for user: {Username}", ticketUid, username);
            return false;
        }
    }
}