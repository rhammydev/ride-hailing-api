using FluentValidation;
using Ride_Hailing_API.DTOs.Driver;

namespace Ride_Hailing_API.Validators;

public class DriverOnboardingRequestValidator : AbstractValidator<DriverOnboardingRequest>
{
    public DriverOnboardingRequestValidator()
    {
        RuleFor(x => x.DriverLicence)
            .NotEmpty().WithMessage("Driver license number is required.")
            .MaximumLength(50).WithMessage("Driver license cannot exceed 50 characters.");

        RuleFor(x => x.Nin)
            .NotEmpty().WithMessage("National Identification Number (NIN) is required.")
            .Length(6, 30).WithMessage("NIN must be between 6 and 30 characters.");

        RuleFor(x => x.VehicleMake)
            .NotEmpty().WithMessage("Vehicle make is required.")
            .MaximumLength(50).WithMessage("Vehicle make cannot exceed 50 characters.");

        RuleFor(x => x.VehicleModel)
            .NotEmpty().WithMessage("Vehicle model is required.")
            .MaximumLength(50).WithMessage("Vehicle model cannot exceed 50 characters.");

        RuleFor(x => x.VehicleYear)
            .NotEmpty().WithMessage("Vehicle year is required.")
            .Matches(@"^(19|20)\d{2}$").WithMessage("Vehicle year must be a valid 4-digit year (e.g. 2022).");

        RuleFor(x => x.VehicleColor)
            .NotEmpty().WithMessage("Vehicle color is required.")
            .MaximumLength(30).WithMessage("Vehicle color cannot exceed 30 characters.");

        RuleFor(x => x.PlateNumber)
            .NotEmpty().WithMessage("Vehicle license plate number is required.")
            .MaximumLength(20).WithMessage("Plate number cannot exceed 20 characters.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Vehicle passenger capacity must be at least 1.")
            .LessThanOrEqualTo(15).WithMessage("Vehicle passenger capacity cannot exceed 15.")
            .When(x => x.Capacity.HasValue);
    }
}

public class DriverAvailabilityRequestValidator : AbstractValidator<DriverAvailabilityRequest>
{
    public DriverAvailabilityRequestValidator()
    {
        RuleFor(x => x.IsAvailable)
            .NotNull().WithMessage("Availability status is required.");
    }
}
