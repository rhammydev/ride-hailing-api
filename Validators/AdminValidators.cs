using FluentValidation;
using Ride_Hailing_API.DTOs.Admin;

namespace Ride_Hailing_API.Validators;

public class RejectDriverRequestValidator : AbstractValidator<RejectDriverRequest>
{
    public RejectDriverRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Rejection reason is required.")
            .MaximumLength(500).WithMessage("Rejection reason cannot exceed 500 characters.");
    }
}

public class UpdateUserStatusRequestValidator : AbstractValidator<UpdateUserStatusRequest>
{
    public UpdateUserStatusRequestValidator()
    {
        RuleFor(x => x.IsActive)
            .NotNull().WithMessage("User active status is required.");
    }
}
