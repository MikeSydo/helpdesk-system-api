using HelpDesk.Domain.Entities;

namespace HelpDesk.Application.Categories.Dtos;

public record CategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    IReadOnlyList<Ticket> Tickets);