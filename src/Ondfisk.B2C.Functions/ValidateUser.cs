namespace Ondfisk.B2C;

public class ValidateUser
{
    private const string CIVIL_REGISTRATION_NUMBER_PROPERTY_NAME = "extension_1be97e586e4944eea17a42fd1fc944cf_civilRegistrationNumber";
    private const string CIVIL_REGISTRATION_NUMBER_VALIDATED_PROPERTY_NAME = "extension_1be97e586e4944eea17a42fd1fc944cf_civilRegistrationNumberValidated";

    private readonly ValidateUserDtoValidator _validator;
    private readonly IGraphHelper _helper;
    private readonly Func<DateTime> _utcNow;
    private readonly ILogger _logger;

    public ValidateUser(ValidateUserDtoValidator validator, IGraphHelper helper, Func<DateTime> utcNow, ILoggerFactory loggerFactory)
    {
        _validator = validator;
        _utcNow = utcNow;
        _helper = helper;
        _logger = loggerFactory.CreateLogger<ValidateUser>();
    }

    [Function(nameof(ValidateUser))]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var (validation, dto) = await ValidateRequest(req);

        if (!validation.IsValid)
        {
            _logger.LogWarning("Validation failed: {Errors}", validation.Errors);

            return new BadRequestObjectResult(validation.ToDictionary());
        }

        _logger.LogInformation("Validation passed: {dto}", new { dto.IssuerUserId, dto.DisplayName });

        var users = await _helper.GetUsersAsync(CIVIL_REGISTRATION_NUMBER_PROPERTY_NAME, Hash(dto.CivilRegistrationNumber));

        if (!users.Any())
        {
            _logger.LogWarning("No users found with civil registration number: {CivilRegistrationNumber}", dto.CivilRegistrationNumber);

            return new NotFoundObjectResult(new { Message = $"No users found with civil registration number: {dto.CivilRegistrationNumber}" });
        }

        if (users.Count() > 1)
        {
            _logger.LogWarning("Multiple users found with civil registration number: {CivilRegistrationNumber}", dto.CivilRegistrationNumber);

            return new ConflictObjectResult(new { Message = $"Multiple users found with civil registration number: {dto.CivilRegistrationNumber}" });
        }

        var user = users.Single();
        var validated = _utcNow();
        user.AdditionalData[CIVIL_REGISTRATION_NUMBER_VALIDATED_PROPERTY_NAME] = validated;

        await _helper.PatchUserAsync(user);

        var updated = new ValidatedUserDto(user.Id ?? string.Empty, user.DisplayName ?? string.Empty, validated);

        _logger.LogInformation("Returning: {user}", updated);

        return new OkObjectResult(updated);
    }

    private async Task<(ValidationResult, ValidateUserDto)> ValidateRequest(HttpRequest req)
    {
        try
        {
            var dto = await req.ReadFromJsonAsync<ValidateUserDto>();

            if (dto != null)
            {
                var validation = await _validator.ValidateAsync(dto);

                return (validation, dto);
            }
        }
        catch
        {
        }

        var validationFailures = new[]
        {
            new ValidationFailure(string.Empty, "Unable to parse input.")
        };

        var validationResult = new ValidationResult(validationFailures);

        return (validationResult, new ValidateUserDto(string.Empty, string.Empty, string.Empty));
    }

    public static string Hash(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);

        var hash = SHA256.HashData(bytes);

        return Convert.ToBase64String(hash);
    }
}
