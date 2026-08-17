using FluentValidation;
using test.DTOs;

namespace test.Validators;

public class CreateUserTypeRequestValidator : AbstractValidator<CreateUserTypeRequest>
{
    public CreateUserTypeRequestValidator()
    {
        RuleFor(x => x.NameAr).NotEmpty().WithMessage("Arabic name is required.")
            .MaximumLength(100).WithMessage("Arabic name cannot exceed 100 characters.");
        RuleFor(x => x.NameEn).NotEmpty().WithMessage("English name is required.")
            .MaximumLength(100).WithMessage("English name cannot exceed 100 characters.");
    }
}

public class UpdateUserTypeRequestValidator : AbstractValidator<UpdateUserTypeRequest>
{
    public UpdateUserTypeRequestValidator()
    {
        RuleFor(x => x.NameAr).NotEmpty().WithMessage("Arabic name is required.")
            .MaximumLength(100).WithMessage("Arabic name cannot exceed 100 characters.");
        RuleFor(x => x.NameEn).NotEmpty().WithMessage("English name is required.")
            .MaximumLength(100).WithMessage("English name cannot exceed 100 characters.");
    }
}

public class SetUserTypeRequestValidator : AbstractValidator<SetUserTypeRequest>
{
    public SetUserTypeRequestValidator()
    {
        RuleFor(x => x.UserTypeId).GreaterThan(0).WithMessage("User type is required.");
    }
}
