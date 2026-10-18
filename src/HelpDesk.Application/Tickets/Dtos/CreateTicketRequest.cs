using System.ComponentModel.DataAnnotations;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tickets.Dtos;

public sealed record CreateTicketRequest(
    [Required(AllowEmptyStrings = false,  ErrorMessage = "Title is required.")]
    [MaxLength(200)]
    string Title,
    
    [Required(AllowEmptyStrings = false,  ErrorMessage = "Description is required.")]
    [MaxLength(4000)]
    string Description,
    
    [Required]
    Guid CategoryId,
    TicketPriority Priority);