using FluentValidation;
using test.DTOs;

namespace test.Validators;

public class BookDesignConversationRequestValidator : AbstractValidator<BookDesignConversationRequest>
{
    public BookDesignConversationRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessage("Full name is required.");
        RuleFor(x => x.WhatsAppNumber).NotEmpty().WithMessage("WhatsApp number is required.");
        RuleFor(x => x.CityId).GreaterThan(0).WithMessage("City is required.");
        RuleFor(x => x.Service).IsInEnum().WithMessage("Service is not valid.");
        RuleFor(x => x.PreferredSlotDate).NotEmpty().WithMessage("Preferred slot date is required.");
    }
}
