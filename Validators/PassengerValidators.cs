using FluentValidation;
using Ride_Hailing_API.DTOs.Passenger;

namespace Ride_Hailing_API.Validators;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FullName)
            .ValidName();

        RuleFor(x => x.PhoneNumber)
            .ValidPhone();
    }
}
