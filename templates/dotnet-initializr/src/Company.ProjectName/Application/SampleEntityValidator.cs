#if (IncludeValidation)
using FluentValidation;

namespace Company.ProjectName.Application;

/// <summary>
/// Sample FluentValidation validator. Replace with your own entity and rules.
/// </summary>
public class SampleRequest
{
    public string Name { get; set; } = string.Empty;
}

public class SampleRequestValidator : AbstractValidator<SampleRequest>
{
    public SampleRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
#endif
