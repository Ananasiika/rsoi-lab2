using GatewayService.Dto;
using GatewayService.HttpClients;
using GatewayService.Models;

namespace GatewayService.Services;

public class GatewayService : IGatewayService
{
    private readonly IFlightClient _flightClient;
    private readonly IBonusClient _bonusClient;
    private readonly ITicketClient _ticketClient;
    private readonly ILogger<GatewayService> _logger;

    public GatewayService(
        IFlightClient flightClient,
        IBonusClient bonusClient,
        ITicketClient ticketClient,
        ILogger<GatewayService> logger)
    {
        _flightClient = flightClient;
        _bonusClient = bonusClient;
        _ticketClient = ticketClient;
        _logger = logger;
    }

    public Task<PaginationResponse<FlightDto>> GetFlightsAsync(int page, int size)
    {
        return _flightClient.GetFlightsAsync(page, size);
    }

    public async Task<UserInfoResponse> GetUserInfoAsync(string username)
    {
        var ticketsTask = _ticketClient.GetUserTicketsAsync(username);
        var privilegeTask = _bonusClient.GetPrivilegeShortInfoAsync(username);

        await Task.WhenAll(ticketsTask, privilegeTask);

        return new UserInfoResponse
        {
            Tickets = await ticketsTask,
            Privilege = await privilegeTask ?? new PrivilegeShortInfo()
        };
    }

    public async Task<List<TicketResponse>> GetUserTicketsAsync(string username)
    {
        // 1. Получаем билеты из TicketService
        var tickets = await _ticketClient.GetUserTicketsAsync(username);
    
        var result = new List<TicketResponse>();
    
        foreach (var ticket in tickets)
        {
            // 2. Для каждого билета получаем информацию о рейсе из FlightService
            var flight = await _flightClient.GetFlightByNumberAsync(ticket.FlightNumber);
        
            if (flight != null)
            {
                var ticketResponse = new TicketResponse
                {
                    TicketUid = ticket.TicketUid,
                    FlightNumber = ticket.FlightNumber,
                    FromAirport = $"{flight.FromAirport.City} {flight.FromAirport.Name}",
                    ToAirport = $"{flight.ToAirport.City} {flight.ToAirport.Name}",
                    Date = flight.DateTime,
                    Price = ticket.Price,
                    Status = ticket.Status
                };
                result.Add(ticketResponse);
            }
            else
            {
                // Если информация о рейсе не найдена, возвращаем базовую информацию
                var ticketResponse = new TicketResponse
                {
                    TicketUid = ticket.TicketUid,
                    FlightNumber = ticket.FlightNumber,
                    FromAirport = "Unknown",
                    ToAirport = "Unknown",
                    Date = DateTime.MinValue,
                    Price = ticket.Price,
                    Status = ticket.Status
                };
                result.Add(ticketResponse);
            }
        }
    
        return result;
    }

    public async Task<TicketResponse?> GetTicketAsync(string username, Guid ticketUid)
    {
        // 1. Получаем билет из TicketService
        var ticket = await _ticketClient.GetTicketAsync(username, ticketUid);
        if (ticket == null) return null;

        // 2. Получаем информацию о рейсе из FlightService
        var flight = await _flightClient.GetFlightByNumberAsync(ticket.FlightNumber);
    
        if (flight == null) return null;

        return new TicketResponse
        {
            TicketUid = ticket.TicketUid,
            FlightNumber = ticket.FlightNumber,
            FromAirport = $"{flight.FromAirport.City} {flight.FromAirport.Name}",
            ToAirport = $"{flight.ToAirport.City} {flight.ToAirport.Name}",
            Date = flight.DateTime,
            Price = ticket.Price,
            Status = ticket.Status
        };
    }

    public async Task<TicketPurchaseResponse?> PurchaseTicketAsync(string username, TicketPurchaseRequest request)
    {
        // 1. Проверяем существование рейса
        var flight = await _flightClient.GetFlightByNumberAsync(request.FlightNumber);
        if (flight == null)
        {
            _logger.LogWarning("Flight not found: {FlightNumber}", request.FlightNumber);
            return null;
        }

        // 2. Вычисляем оплату бонусами и деньгами
        var privilegeInfo = await _bonusClient.GetPrivilegeInfoAsync(username);
        int paidByBonuses = 0;
        int paidByMoney = request.Price;
        int bonusToAdd = 0;

        if (request.PaidFromBalance && privilegeInfo != null)
        {
            paidByBonuses = Math.Min(privilegeInfo.Balance, request.Price);
            paidByMoney = request.Price - paidByBonuses;
        }
        else
        {
            bonusToAdd = (int)(request.Price * 0.1);
        }

        // 3. Покупаем билет
        var purchaseResponse = await _ticketClient.PurchaseTicketAsync(username, request);
        if (purchaseResponse == null)
        {
            return null;
        }

        // 4. Обновляем бонусный счет
        await _bonusClient.UpdatePrivilegeAfterPurchase(username, request, purchaseResponse.TicketUid, paidByBonuses, paidByMoney, bonusToAdd);

        // 5. Получаем актуальную информацию о привилегиях
        var updatedPrivilege = await _bonusClient.GetPrivilegeShortInfoAsync(username);

        // 6. Формируем полный ответ
        return new TicketPurchaseResponse
        {
            TicketUid = purchaseResponse.TicketUid,
            FlightNumber = flight.FlightNumber,
            FromAirport = flight.FromAirport.Name,
            ToAirport = flight.ToAirport.Name,
            Date = flight.DateTime,
            Price = request.Price,
            PaidByMoney = paidByMoney,
            PaidByBonuses = paidByBonuses,
            Status = "PAID",
            Privilege = updatedPrivilege ?? new PrivilegeShortInfo()
        };
    }

    public async Task<bool> CancelTicketAsync(string username, Guid ticketUid)
    {
        // 1. Отменяем билет
        var success = await _ticketClient.CancelTicketAsync(username, ticketUid);
        if (!success)
        {
            return false;
        }

        // 2. Обновляем бонусный счет
        await _bonusClient.UpdatePrivilegeAfterCancel(username, ticketUid);

        return true;
    }

    public Task<PrivilegeInfoResponse?> GetPrivilegeInfoAsync(string username)
    {
        return _bonusClient.GetPrivilegeInfoAsync(username);
    }
}