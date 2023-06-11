namespace Ondfisk.B2C;

public record UpdateUserDto(string IssuerUserId, string DisplayName, string CprNumber);

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.IssuerUserId).NotEmpty();
        RuleFor(x => x.DisplayName).NotEmpty();
        RuleFor(x => x.CprNumber).NotEmpty();
    }
}
