using FluentValidation;
using Ride_Hailing_API.DTOs.Ride;

namespace Ride_Hailing_API.Validators;

public class CreateRideRequestValidator : AbstractValidator<CreateRideRequest>
{
    public CreateRideRequestValidator()
    {
        RuleFor(x => x.PickupLocation)
            .NotEmpty().WithMessage("Pickup location is required.")
            .MinimumLength(3).WithMessage("Pickup location must be at least 3 characters.")
            .MaximumLength(200).WithMessage("Pickup location cannot exceed 200 characters.");

        RuleFor(x => x.Destination)
            .NotEmpty().WithMessage("Destination is required.")
            .MinimumLength(3).WithMessage("Destination must be at least 3 characters.")
            .MaximumLength(200).WithMessage("Destination cannot exceed 200 characters.")
            .NotEqual(x => x.PickupLocation).WithMessage("Destination must be different from pickup location.");
    }
}

public class UpdateRideStatusRequestValidator : AbstractValidator<UpdateRideStatusRequest>
{
    public UpdateRideStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid ride status.");
    }
}

public class CancelRideRequestValidator : AbstractValidator<CancelRideRequest>
{
    public CancelRideRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Cancellation reason is required.")
            .MaximumLength(500).WithMessage("Cancellation reason cannot exceed 500 characters.");
    }
}
