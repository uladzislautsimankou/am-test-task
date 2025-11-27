using AM.TestTask.Business.Models;
using FluentValidation;

namespace AM.TestTask.Business.Validators;

public sealed class MeteoriteStatisticFilterValidator : AbstractValidator<MeteoriteStatisticFilter>
{
    public MeteoriteStatisticFilterValidator()
    {
        RuleFor(x => x.YearFrom)
            .GreaterThan((short)0).WithMessage("Start year must be a positive number.")
            .LessThanOrEqualTo((short)DateTime.UtcNow.Year).WithMessage("Start year cannot be in the future.")
            .When(x => x.YearFrom.HasValue);

        RuleFor(x => x.YearTo)
            .GreaterThan((short)0).WithMessage("End year must be a positive number.")
            .When(x => x.YearTo.HasValue);

        RuleFor(x => x)
            .Must(x => x.YearTo >= x.YearFrom)
            .WithMessage("End year must be greater than or equal to start year.")
            .When(x => x.YearFrom.HasValue && x.YearTo.HasValue);

        RuleFor(x => x.RecClassId)
            .GreaterThan(0).WithMessage("Class Id must be greater than 0.")
            .When(x => x.RecClassId.HasValue);

        RuleFor(x => x.NamePart)
            .MaximumLength(100).WithMessage("Name part is too long (max 100 characters).")
            .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Name part cannot be empty.")
            .When(x => x.NamePart != null);
    }
}

