namespace Ondfisk.B2C;

public record UpdateUserDto(string IssuerUserId, string DisplayName, string CivilRegistrationNumber);

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.IssuerUserId).NotEmpty();
        RuleFor(x => x.DisplayName).NotEmpty();
        RuleFor(x => x.CivilRegistrationNumber).NotEmpty().Matches(@"^\d{10}$");
    }
}
