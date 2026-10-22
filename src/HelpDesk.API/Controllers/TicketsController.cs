using HelpDesk.Application.Tickets;
using HelpDesk.Application.Tickets.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HelpDesk.API.Security;
using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Common.Interfaces;

namespace HelpDesk.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController(
    ITicketService ticketsService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TicketResponse>>> GetTickets(CancellationToken cancellationToken)
    {
        var tickets = await ticketsService.GetAllAsync(cancellationToken);
        return Ok(tickets);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TicketResponse>> GetTicket(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await ticketsService.GetByIdAsync(id, cancellationToken);
        if (ticket == null) return NotFound(id);
        return Ok(ticket);
    }

    [HttpPost]
    public async Task<ActionResult<TicketResponse>> CreateTicket(
        [FromBody] CreateTicketRequest request,
        CancellationToken cancellationToken)
    {
        Guid currentUserId = RequireUserId();

        TicketResponse ticket = await ticketsService.CreateAsync(request, currentUserId, cancellationToken);

        return CreatedAtAction(
            nameof(GetTicket),
            new { id = ticket.Id },
            ticket);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TicketResponse>> UpdateTicket(
        Guid id,
        [FromBody] UpdateTicketRequest request,
        CancellationToken cancellationToken)
    {
        Guid currentUserId = RequireUserId();
        var ticket = await ticketsService.UpdateAsync(id, request, currentUserId, cancellationToken);
        if (ticket == null) return NotFound(id);
        return Ok(ticket);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<TicketResponse>> DeleteTicket(Guid id, CancellationToken cancellationToken)
    {
        RequireUserId();
        bool status = await ticketsService.DeleteAsync(id, cancellationToken);
        return status ? NoContent() : NotFound(id);
    }

    private Guid RequireUserId()
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            throw new UnauthorizedException("Authentication is required.");
        }

        return currentUser.UserId.Value;
    }
    
}