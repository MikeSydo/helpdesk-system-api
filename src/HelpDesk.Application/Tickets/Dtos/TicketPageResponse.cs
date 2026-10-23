namespace HelpDesk.Application.Tickets.Dtos;

public sealed record TicketPageResponse(
    IReadOnlyList<TicketResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
