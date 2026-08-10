using FluentValidation;
using test.DTOs;
using test.Helpers;

namespace test.Validators;

public class CreateWhatsAppNumberRequestValidator : AbstractValidator<CreateWhatsAppNumberRequest>
{
    public CreateWhatsAppNumberRequestValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("WhatsApp number is required.")
            // Length is checked against the normalised value, which is what gets stored
            // in the nvarchar(30) column.
            .Must(PhoneNumberHelper.IsValid).WithMessage(PhoneNumberHelper.Message)
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}

public class UpdateWhatsAppNumberRequestValidator : AbstractValidator<UpdateWhatsAppNumberRequest>
{
    public UpdateWhatsAppNumberRequestValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("WhatsApp number is required.")
            .Must(PhoneNumberHelper.IsValid).WithMessage(PhoneNumberHelper.Message)
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}
