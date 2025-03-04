using FluentValidation;
using Acceloka.Application.Commands.BookedTickets;

namespace Acceloka.Validators.BookedTickets
{
    public class UpdateBookedTicketValidator : AbstractValidator<UpdateBookedTicketCommand>
    {
        public UpdateBookedTicketValidator()
        {
            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        }
    }
}
