namespace Ondfisk.B2C;

public class UpdateUser
{
    private readonly UpdateUserDtoValidator _validator;
    private readonly ILogger _logger;

    public UpdateUser(UpdateUserDtoValidator validator, ILoggerFactory loggerFactory)
    {
        _validator = validator;
        _logger = loggerFactory.CreateLogger<UpdateUser>();
    }

    [Function(nameof(UpdateUser))]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var (validation, dto) = await ValidateRequest(req);

        if (!validation.IsValid)
        {
            _logger.LogWarning("Validation failed: {Errors}", validation.Errors);

            var response = req.CreateResponse(HttpStatusCode.BadRequest);

            await response.WriteAsJsonAsync(validation.ToDictionary());

            return response;
        }

        return req.CreateResponse(HttpStatusCode.NoContent);
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
