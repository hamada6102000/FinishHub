using FluentValidation;
using test.DTOs;

namespace test.Validators;

public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleForEach(x => x.Materials).SetValidator(new ProjectMaterialRequestValidator());
    }
}

public class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
{
    public UpdateProjectRequestValidator()
    {
        RuleForEach(x => x.Materials).SetValidator(new ProjectMaterialRequestValidator());
    }
}

public class ProjectMaterialRequestValidator : AbstractValidator<ProjectMaterialRequest>
{
    public ProjectMaterialRequestValidator()
    {
        RuleFor(x => x.MaterialName).NotEmpty().WithMessage("Material name is required.");
    }
}
