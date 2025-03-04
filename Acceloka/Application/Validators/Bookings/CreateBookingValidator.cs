using FluentValidation;
using Acceloka.Application.Commands.Bookings;

namespace Acceloka.Application.Validators.Bookings
{
    public class CreateBookingValidator : AbstractValidator<CreateBookingCommand>
    {
        public CreateBookingValidator()
        {
            RuleFor(x => x.Tickets)
                .NotEmpty().WithMessage("At least one ticket must be booked.");

            RuleForEach(x => x.Tickets).ChildRules(ticket =>
            {
                ticket.RuleFor(t => t.TicketCode)
                    .NotEmpty().WithMessage("Ticket code is required.")
                    .MaximumLength(20).WithMessage("Ticket code cannot exceed 20 characters.");

                ticket.RuleFor(t => t.Quantity)
                    .GreaterThan(0).WithMessage("Ticket quantity must be greater than 0.");
            });
        }
    }
}
