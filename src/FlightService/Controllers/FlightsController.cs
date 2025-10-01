using FlightService.Interfaces;
using FlightService.Models;
using Microsoft.AspNetCore.Mvc;

namespace FlightService.Controllers;

[ApiController]
[Route("api/v1/flights")]
public class FlightsController : ControllerBase
{
    private readonly IFlightService _flightService;

    public FlightsController(IFlightService flightService)
    {
        _flightService = flightService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Flight>>> GetFlights([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        if (page < 1 || size < 1)
        {
            return BadRequest("Page and size must be positive integers");
        }

        var flights = await _flightService.GetAllFlightsAsync(page, size);
        var totalCount = await _flightService.GetTotalCountAsync(); // Нужно добавить этот метод в сервис
    
        var response = new PaginationResponse<Flight>
        {
            Page = page,
            PageSize = size,
            TotalCount = totalCount,
            Items = flights.ToList()
        };
    
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Flight>> GetFlight(int id)
    {
        var flight = await _flightService.GetFlightByIdAsync(id);
        if (flight == null)
        {
            return NotFound();
        }
        return Ok(flight);
    }

    [HttpGet("number/{flightNumber}")]
    public async Task<ActionResult<Flight>> GetFlightByNumber(string flightNumber)
    {
        var flight = await _flightService.GetFlightByNumberAsync(flightNumber);
        if (flight == null)
        {
            return NotFound();
        }
        return Ok(flight);
    }

    [HttpPost]
    public async Task<ActionResult<Flight>> CreateFlight(Flight flight)
    {
        try
        {
            var createdFlight = await _flightService.CreateFlightAsync(flight);
            return CreatedAtAction(nameof(GetFlight), new { id = createdFlight.Id }, createdFlight);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error creating flight: {ex.Message}");
        }
    }
}