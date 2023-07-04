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

        var dto = await ParseInput(req);

        if (dto == null)
        {
            _logger.LogWarning("Unable to parse input.");

            return new BadRequestObjectResult("Unable to parse input.");
        }

        var validation = await _validator.ValidateAsync(dto);

        if (!validation.IsValid)
        {
            _logger.LogWarning("Validation failed: {Errors}", validation.Errors);

            return new BadRequestObjectResult(validation.ToDictionary());
        }

        _logger.LogInformation("Validation passed: {dto}", new { dto.IssuerUserId, dto.DisplayName });

        var users = await _helper.GetUsersAsync(CIVIL_REGISTRATION_NUMBER_PROPERTY_NAME, Hash(dto.CivilRegistrationNumber));

        if (!users.Any())
        {
            _logger.LogWarning("No users found with given civil registration number");

            return new NotFoundObjectResult("No users found with given civil registration number");
        }

        if (users.Count() > 1)
        {
            _logger.LogWarning("Multiple users found with given civil registration number");

            return new ConflictObjectResult("No users found with given civil registration number");
        }

        var user = users.Single();
        var validated = _utcNow();
        user.AdditionalData[CIVIL_REGISTRATION_NUMBER_VALIDATED_PROPERTY_NAME] = validated;

        await _helper.PatchUserAsync(user);

        var updated = new ValidatedUserDto(user.Id!, user.DisplayName!, user.AccountEnabled!.Value, validated);

        _logger.LogInformation("Returning: {user}", updated);

        return new OkObjectResult(updated);
    }

    private async Task<ValidateUserDto?> ParseInput(HttpRequest req)
    {
        try
        {
            return await req.ReadFromJsonAsync<ValidateUserDto>();
        }
        catch
        {
            return null;
        }
    }

    private static string Hash(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);

        var hash = SHA256.HashData(bytes);

        return Convert.ToBase64String(hash);
    }
}
