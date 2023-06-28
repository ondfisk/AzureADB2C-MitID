namespace Ondfisk.B2C;

public class UpdateUser
{
    private readonly UpdateUserDtoValidator _validator;
    private readonly GraphServiceClient _client;
    private readonly ILogger _logger;

    public UpdateUser(UpdateUserDtoValidator validator, GraphServiceClient client, ILoggerFactory loggerFactory)
    {
        _validator = validator;
        _logger = loggerFactory.CreateLogger<UpdateUser>();
        _client = client;
    }

    [Function(nameof(UpdateUser))]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        HttpResponseData response;

        var (validation, dto) = await ValidateRequest(req);

        if (!validation.IsValid)
        {
            _logger.LogWarning("Validation failed: {Errors}", validation.Errors);

            response = req.CreateResponse(HttpStatusCode.BadRequest);

            await response.WriteAsJsonAsync(validation.ToDictionary());

            return response;
        }

        _logger.LogInformation("Validation passed: {dto}", new { IssuerUserId = dto.IssuerUserId, DisplayName = dto.DisplayName });

        var user = new
        {
            ObjectId = Guid.NewGuid().ToString()
        };

        _logger.LogInformation("Returning: {user}", user);

        response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(user);

        return response;
    }

    // private async Task<string> GetUserAsync(string cprNumber)
    // {
    //     var hash = HashStringWithSha256(cprNumber);

    //     // login to graph with managed id





    //     // call graph api with hash
    //     // return user id

    // }

    // private async Task<User> GetUserByExtensionAttributeAsync(string extensionName, string extensionValue)
    // {
    //     GraphServiceClient client = GetGraphClient();

    //     var users = await client.Users.Request()
    //         .Filter($"extension_{extensionName} eq '{extensionValue}'")
    //         .GetAsync();

    //     return users.FirstOrDefault();
    // }

    private GraphServiceClient GetGraphClient()
    {
        var authProvider = new DefaultAzureCredential();

        return new GraphServiceClient(authProvider);
    }

    private string HashStringWithSha256(string input)
    {
        using var sha256 = SHA256.Create();

        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);

        return Convert.ToBase64String(hash);
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
