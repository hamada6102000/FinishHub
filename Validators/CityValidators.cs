using FluentValidation;
using test.DTOs;

namespace test.Validators;

public class CreateCityRequestValidator : AbstractValidator<CreateCityRequest>
{
    public CreateCityRequestValidator()
    {
        RuleFor(x => x.NameAr).NotEmpty().WithMessage("Arabic name is required.");
        RuleFor(x => x.NameEn).NotEmpty().WithMessage("English name is required.");
    }
}

public class UpdateCityRequestValidator : AbstractValidator<UpdateCityRequest>
{
    public UpdateCityRequestValidator()
    {
        RuleFor(x => x.NameAr).NotEmpty().WithMessage("Arabic name is required.");
        RuleFor(x => x.NameEn).NotEmpty().WithMessage("English name is required.");
    }
}
