namespace Ondfisk.B2C.Models;

public record ValidateUserDto(string IssuerUserId, string DisplayName, string CivilRegistrationNumber);

public class ValidateUserDtoValidator : AbstractValidator<ValidateUserDto>
{
    public ValidateUserDtoValidator()
    {
        RuleFor(x => x.IssuerUserId).NotEmpty();
        RuleFor(x => x.DisplayName).NotEmpty();
        RuleFor(x => x.CivilRegistrationNumber).NotEmpty().Matches(@"^\d{10}$");
    }
}
