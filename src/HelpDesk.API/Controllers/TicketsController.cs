using HelpDesk.Application.Tickets;
using HelpDesk.Application.Tickets.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController(ITicketService ticketsService) : ControllerBase
{
    private readonly ITicketService _ticketsService = ticketsService;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TicketResponse>>> GetTickets(CancellationToken cancellationToken)
    {
        var tickets = await _ticketsService.GetAllAsync(cancellationToken);
        return Ok(tickets);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TicketResponse>> GetTicket(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await _ticketsService.GetByIdAsync(id, cancellationToken);
        if (ticket == null) return NotFound(id);
        return Ok(ticket);
    }

    [HttpPost]
    public async Task<ActionResult<TicketResponse>> CreateTicket([FromBody] CreateTicketRequest request, 
        [FromQuery] Guid currentUserId, CancellationToken cancellationToken)
    {
        if (currentUserId == Guid.Empty)
            return BadRequest("currentUserId is required.");

        TicketResponse ticket = await _ticketsService.CreateAsync(request, currentUserId, cancellationToken);

        return CreatedAtAction(
            nameof(GetTicket),
            new { id = ticket.Id },
            ticket);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TicketResponse>> UpdateTicket(Guid id, [FromBody] UpdateTicketRequest request,
        [FromQuery] Guid currentUserId, CancellationToken cancellationToken)
    {
        if (currentUserId == Guid.Empty)
            return BadRequest("currentUserId is required.");
        var ticket = await _ticketsService.UpdateAsync(id, request, currentUserId, cancellationToken);
        if (ticket == null) return NotFound();
        return Ok(ticket);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<TicketResponse>> DeleteTicket(Guid id, CancellationToken cancellationToken)
    {
        bool status = await _ticketsService.DeleteAsync(id, cancellationToken);
        return status ? Ok(id) : NoContent();
    }
}