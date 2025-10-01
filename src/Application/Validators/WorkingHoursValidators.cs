using FluentValidation;
using Getir.Application.DTO;

namespace Getir.Application.Validators;

public class CreateWorkingHoursRequestValidator : AbstractValidator<CreateWorkingHoursRequest>
{
    public CreateWorkingHoursRequestValidator()
    {
        RuleFor(x => x.MerchantId)
            .NotEmpty().WithMessage("Merchant ID is required");

        RuleFor(x => x.DayOfWeek)
            .IsInEnum().WithMessage("Invalid day of week");

        RuleFor(x => x.OpenTime)
            .NotEmpty().When(x => !x.IsClosed)
            .WithMessage("Open time is required when not closed");

        RuleFor(x => x.CloseTime)
            .NotEmpty().When(x => !x.IsClosed)
            .WithMessage("Close time is required when not closed")
            .GreaterThan(x => x.OpenTime).When(x => !x.IsClosed && x.OpenTime.HasValue)
            .WithMessage("Close time must be after open time");
    }
}

public class UpdateWorkingHoursRequestValidator : AbstractValidator<UpdateWorkingHoursRequest>
{
    public UpdateWorkingHoursRequestValidator()
    {
        RuleFor(x => x.OpenTime)
            .NotEmpty().When(x => !x.IsClosed)
            .WithMessage("Open time is required when not closed");

        RuleFor(x => x.CloseTime)
            .NotEmpty().When(x => !x.IsClosed)
            .WithMessage("Close time is required when not closed")
            .GreaterThan(x => x.OpenTime).When(x => !x.IsClosed && x.OpenTime.HasValue)
            .WithMessage("Close time must be after open time");
    }
}

public class WorkingHoursDayRequestValidator : AbstractValidator<WorkingHoursDayRequest>
{
    public WorkingHoursDayRequestValidator()
    {
        RuleFor(x => x.DayOfWeek)
            .IsInEnum().WithMessage("Invalid day of week");

        RuleFor(x => x.OpenTime)
            .NotEmpty().When(x => !x.IsClosed)
            .WithMessage("Open time is required when not closed");

        RuleFor(x => x.CloseTime)
            .NotEmpty().When(x => !x.IsClosed)
            .WithMessage("Close time is required when not closed")
            .GreaterThan(x => x.OpenTime).When(x => !x.IsClosed && x.OpenTime.HasValue)
            .WithMessage("Close time must be after open time");
    }
}

public class BulkUpdateWorkingHoursRequestValidator : AbstractValidator<BulkUpdateWorkingHoursRequest>
{
    public BulkUpdateWorkingHoursRequestValidator()
    {
        RuleFor(x => x.WorkingHours)
            .NotEmpty().WithMessage("Working hours list cannot be empty");

        RuleForEach(x => x.WorkingHours)
            .SetValidator(new WorkingHoursDayRequestValidator());
    }
}
