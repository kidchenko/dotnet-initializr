#if (IncludeValidation)
using FluentValidation;
using Company.ProjectName.Domain.Entities;

namespace Company.ProjectName.Application;

public class SampleEntityValidator : AbstractValidator<SampleEntity>
{
    public SampleEntityValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
#endif
