using Microsoft.AspNetCore.Mvc;
using TicketService.Dto;
using TicketService.Interfaces;

namespace TicketService.Controllers;

[ApiController]
[Route("api/v1/tickets")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserTickets([FromHeader(Name = "X-User-Name")] string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest("Username is required");
        }

        var tickets = await _ticketService.GetUserTicketsAsync(username);
        // Здесь должен быть вызов к FlightService для получения деталей рейса (FromAirport, ToAirport, Date)
        // Пока возвращаем только данные из билетов
        var response = tickets.Select(t => new
        {
            t.TicketUid,
            t.FlightNumber,
            t.Price,
            Status = t.Status.ToString()
        });
        return Ok(response);
    }

    [HttpGet("{ticketUid}")]
    public async Task<IActionResult> GetTicket(Guid ticketUid, [FromHeader(Name = "X-User-Name")] string username)
    {
        var ticket = await _ticketService.GetTicketByUidAsync(ticketUid);
        if (ticket == null || ticket.Username != username)
        {
            return NotFound();
        }
        // Аналогично, нужно добавить информацию о рейсе
        var response = new
        {
            ticket.TicketUid,
            ticket.FlightNumber,
            ticket.Price,
            Status = ticket.Status.ToString()
        };
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> PurchaseTicket([FromBody] TicketPurchaseRequestDto request, [FromHeader(Name = "X-User-Name")] string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest("Username is required");
        }

        try
        {
            var ticket = await _ticketService.CreateTicketAsync(request, username);
            // Аналогично, нужно добавить информацию о рейсе
            var response = new
            {
                ticket.TicketUid,
                request.FlightNumber,
                request.Price,
                Status = ticket.Status.ToString()
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("{ticketUid}")]
    public async Task<IActionResult> DeleteTicket(Guid ticketUid, [FromHeader(Name = "X-User-Name")] string username)
    {
        var ticket = await _ticketService.GetTicketByUidAsync(ticketUid);
        if (ticket == null || ticket.Username != username)
        {
            return NotFound();
        }

        var success = await _ticketService.DeleteTicketAsync(ticketUid);
        if (!success)
        {
            return StatusCode(500, "Failed to cancel ticket");
        }

        return NoContent(); // 204 No Content
    }
}