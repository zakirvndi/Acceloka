using FluentValidation;
using Acceloka.Application.Commands.BookedTickets;

namespace Acceloka.Validators.BookedTicket
{
    public class DeleteBookedTicketValidator : AbstractValidator<DeleteBookedTicketCommand>
    {
        public DeleteBookedTicketValidator()
        {
            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        }
    }
}
