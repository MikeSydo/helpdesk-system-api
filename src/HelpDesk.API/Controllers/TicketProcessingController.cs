using HelpDesk.Application.Tickets;
using HelpDesk.Application.Tickets.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.API.Controllers;

[ApiController]
[Route("api/tickets")]
public sealed class TicketProcessingController(ITicketProcessingService processingService) : ControllerBase
{
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<TicketResponse>> ChangeStatus(
        Guid id,
        [FromBody] ChangeTicketStatusRequest request,
        CancellationToken cancellationToken)
    {
        // TODO: Require SupportOrAdmin authorization when authentication is implemented.
        TicketResponse? result = await processingService.ChangeStatusAsync(
            id,
            request,
            cancellationToken);
        return result == null ? NotFound(id) : Ok(result);
    }

    [HttpPatch("{id:guid}/assign")]
    public async Task<ActionResult<TicketResponse>> AssignSupport(
        Guid id,
        [FromBody] AssignTicketRequest request,
        CancellationToken cancellationToken)
    {
        // TODO: Require SupportOrAdmin authorization when authentication is implemented.
        TicketResponse? result = await processingService.AssignSupportAsync(
            id,
            request,
            cancellationToken);
        return result == null ? NotFound(id) : Ok(result);
    }

    [HttpDelete("{id:guid}/assign")]
    public async Task<ActionResult<TicketResponse>> UnassignSupport(
        Guid id,
        CancellationToken cancellationToken)
    {
        // TODO: Require SupportOrAdmin authorization when authentication is implemented.
        TicketResponse? result = await processingService.UnassignSupportAsync(
            id,
            cancellationToken);
        return result == null ? NotFound(id) : Ok(result);
    }
}