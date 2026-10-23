using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tickets.Dtos;

public sealed record TicketQuery(
    TicketStatus? Status = null,
    TicketPriority? Priority = null,
    Guid? CategoryId = null,
    int Page = 1,
    int PageSize = 20)
{
    public const int MaxPageSize = 100;

    public void Validate()
    {
        if (Page < 1)
        {
            throw new ArgumentException("Page must be greater than zero.", nameof(Page));
        }

        if (PageSize < 1 || PageSize > MaxPageSize)
        {
            throw new ArgumentException(
                $"PageSize must be between 1 and {MaxPageSize}.",
                nameof(PageSize));
        }
    }
}
