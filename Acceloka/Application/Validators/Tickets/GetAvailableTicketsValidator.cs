using FluentValidation;
using Acceloka.Application.Queries.Tickets;

namespace Acceloka.Application.Validators.Tickets
{
    public class GetAvailableTicketsValidator : AbstractValidator<GetAvailableTicketsQuery>
    {
        private static readonly string[] ValidOrderByFields = { "TicketName", "Category", "TicketPrice", "EventDate", "TicketCode" };
        private static readonly string[] ValidOrderStates = { "ascending", "descending" };

        public GetAvailableTicketsValidator()
        {
            RuleFor(q => q.TicketPrice)
                .GreaterThan(0).When(q => q.TicketPrice.HasValue)
                .WithMessage("Ticket price must be greater than 0.");

            RuleFor(q => q.MinEventDate)
                .LessThanOrEqualTo(q => q.MaxEventDate)
                .When(q => q.MinEventDate.HasValue && q.MaxEventDate.HasValue)
                .WithMessage("MinEventDate cannot be later than MaxEventDate.");

            RuleFor(q => q.Category)
                .MaximumLength(50).When(q => !string.IsNullOrEmpty(q.Category))
                .WithMessage("Category cannot exceed 50 characters.");

            RuleFor(q => q.TicketCode)
                .MaximumLength(20).When(q => !string.IsNullOrEmpty(q.TicketCode))
                .WithMessage("TicketCode cannot exceed 20 characters.");

            RuleFor(q => q.OrderBy)
                .Must(value => string.IsNullOrEmpty(value) || ValidOrderByFields.Contains(value))
                .WithMessage("Invalid OrderBy field. Allowed values: TicketName, Category, TicketPrice, EventDate, TicketCode.");

            RuleFor(q => q.OrderState)
                .Must(value => string.IsNullOrEmpty(value) || ValidOrderStates.Contains(value.ToLower()))
                .WithMessage("OrderState must be either 'ascending' or 'descending'.");
        }
    }
}
