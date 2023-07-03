namespace Ondfisk.B2C;

public class UpdateUser
{
    private const string CIVIL_REGISTRATION_NUMBER_PROPERTY_NAME = "extension_1be97e586e4944eea17a42fd1fc944cf_civilRegistrationNumber";
    private const string CIVIL_REGISTRATION_NUMBER_VALIDATED_PROPERTY_NAME = "extension_1be97e586e4944eea17a42fd1fc944cf_civilRegistrationNumberValidated";

    private readonly UpdateUserDtoValidator _validator;
    private readonly GraphHelper _helper;
    private readonly ILogger _logger;

    public UpdateUser(UpdateUserDtoValidator validator, GraphHelper helper, ILoggerFactory loggerFactory)
    {
        _validator = validator;
        _logger = loggerFactory.CreateLogger<UpdateUser>();
        _helper = helper;
    }

    [Function(nameof(UpdateUser))]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var (validation, dto) = await ValidateRequest(req);

        if (!validation.IsValid)
        {
            _logger.LogWarning("Validation failed: {Errors}", validation.Errors);

            return await req.CreateResponse(HttpStatusCode.BadRequest, validation.ToDictionary());
        }

        _logger.LogInformation("Validation passed: {dto}", new { dto.IssuerUserId, dto.DisplayName });

        var users = await _helper.GetUsersAsync(CIVIL_REGISTRATION_NUMBER_PROPERTY_NAME, dto.CivilRegistrationNumber.Hash());

        if (!users.Any())
        {
            _logger.LogWarning("No users found with civil registration number: {CivilRegistrationNumber}", dto.CivilRegistrationNumber);

            return await req.CreateResponse(HttpStatusCode.NotFound, new { Message = $"No users found with civil registration number: {dto.CivilRegistrationNumber}" });
        }

        if (users.Count() > 1)
        {
            _logger.LogWarning("Multiple users found with civil registration number: {CivilRegistrationNumber}", dto.CivilRegistrationNumber);

            return await req.CreateResponse(HttpStatusCode.Conflict, new { Message = $"Multiple users found with civil registration number: {dto.CivilRegistrationNumber}" });
        }

        var user = users.Single();
        var validated = DateTime.UtcNow;
        user.AdditionalData[CIVIL_REGISTRATION_NUMBER_VALIDATED_PROPERTY_NAME] = validated;

        await _helper.PatchUserAsync(user);

        var updated = new UpdatedUserDto(user.Id!, user.DisplayName!, validated);

        _logger.LogInformation("Returning: {user}", updated);

        return await req.CreateResponse(HttpStatusCode.OK, updated);
    }

    private async Task<(ValidationResult, UpdateUserDto)> ValidateRequest(HttpRequestData req)
    {
        UpdateUserDto dto;

        try
        {
            dto = await req.ReadFromJsonAsync<UpdateUserDto>() ?? new(string.Empty, string.Empty, string.Empty);
        }
        catch
        {
            dto = new(string.Empty, string.Empty, string.Empty);
        }

        var validation = await _validator.ValidateAsync(dto);

        return (validation, dto);
    }
}
