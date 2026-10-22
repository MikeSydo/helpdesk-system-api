using HelpDesk.Application.Tickets;
using HelpDesk.Application.Tickets.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HelpDesk.API.Security;

namespace HelpDesk.API.Controllers;

[ApiController]
[Route("api/tickets")]
public sealed class TicketProcessingController(ITicketProcessingService processingService) : ControllerBase
{
    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = AuthorizationPolicies.SupportOrAdmin)]
    public async Task<ActionResult<TicketResponse>> ChangeStatus(
        Guid id,
        [FromBody] ChangeTicketStatusRequest request,
        CancellationToken cancellationToken)
    {
        TicketResponse? result = await processingService.ChangeStatusAsync(
            id,
            request,
            cancellationToken);
        return result == null ? NotFound(id) : Ok(result);
    }

    [HttpPatch("{id:guid}/assign")]
    [Authorize(Policy = AuthorizationPolicies.SupportOrAdmin)]
    public async Task<ActionResult<TicketResponse>> AssignSupport(
        Guid id,
        [FromBody] AssignTicketRequest request,
        CancellationToken cancellationToken)
    {
        TicketResponse? result = await processingService.AssignSupportAsync(
            id,
            request,
            cancellationToken);
        return result == null ? NotFound(id) : Ok(result);
    }

    [HttpDelete("{id:guid}/assign")]
    [Authorize(Policy = AuthorizationPolicies.SupportOrAdmin)]
    public async Task<ActionResult<TicketResponse>> UnassignSupport(
        Guid id,
        CancellationToken cancellationToken)
    {
        TicketResponse? result = await processingService.UnassignSupportAsync(
            id,
            cancellationToken);
        return result == null ? NotFound(id) : Ok(result);
    }
}