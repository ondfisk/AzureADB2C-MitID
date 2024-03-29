﻿namespace Ondfisk.B2C.Tests;

public class ValidateUserTests
{
    private readonly ValidateUserDtoValidator _validator;
    private readonly IGraphHelper _helper;
    private readonly ValidateUser _function;

    public ValidateUserTests()
    {
        _validator = new ValidateUserDtoValidator();
        _helper = Substitute.For<IGraphHelper>();
        static DateTime utcNow() => DateTime.Parse("2020-02-02 20:20:20");
        var loggerFactory = new LoggerFactory();

        _function = new ValidateUser(_validator, _helper, utcNow, loggerFactory);
    }

    private static HttpRequest CreateHttpRequest(string method, object? body)
    {
        var context = new DefaultHttpContext();

        var request = context.Request;

        request.Method = method;
        request.Headers.ContentType = "application/json";
        request.Headers.ContentEncoding = "utf-8";
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(body)));

        return request;
    }

    [Fact]
    public async Task Run_WithInvalidRequest_ReturnsBadRequestObjectResult()
    {
        // Arrange
        var dto = new ValidateUserDto(string.Empty, string.Empty, string.Empty);
        var req = CreateHttpRequest("POST", dto);

        // Act
        var result = await _function.Run(req);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Run_WithEmptyRequest_ReturnsBadRequestObjectResult()
    {
        // Arrange
        var req = CreateHttpRequest("POST", null);

        // Act
        var result = await _function.Run(req);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Run_WithInvalidJsonRequest_ReturnsBadRequestObjectResult()
    {
        // Arrange
        var req = CreateHttpRequest("POST", "{{");

        // Act
        var result = await _function.Run(req);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Run_WithValidRequestAndMatchingUser_patches_CivilRegistationNumberValidated_and_ReturnsOkObjectResultWithUser()
    {
        // Arrange
        var dto = new ValidateUserDto("ce64b510-1877-49d5-9ce4-2b2e0dd0b9af", "Test User", "1234567890");
        var req = CreateHttpRequest("POST", dto);

        _helper.GetUsersAsync("extension_1be97e586e4944eea17a42fd1fc944cf_civilRegistrationNumber", "x3Xnt1ft5jDNCqERO9ECZhqziCnKUqZCKreChi8mhkY=").Returns([
            new User
            {
                Id = "817e5374-583c-40c0-a84b-e67ab51f05dc",
                DisplayName = "Test User",
                AccountEnabled = true,
                AdditionalData = new Dictionary<string, object>
                {
                    ["extension_1be97e586e4944eea17a42fd1fc944cf_civilRegistrationNumber"] = "x3Xnt1ft5jDNCqERO9ECZhqziCnKUqZCKreChi8mhkY="
                }
            }
        ]);

        // Act
        var result = await _function.Run(req) as OkObjectResult;

        // Assert
        await _helper.Received().PatchUserAsync(Arg.Is<User>(u => (DateTime)u.AdditionalData["extension_1be97e586e4944eea17a42fd1fc944cf_civilRegistrationNumberValidated"] == DateTime.Parse("2020-02-02 20:20:20")));

        Assert.NotNull(result);
        var user = result.Value as ValidatedUserDto;
        Assert.NotNull(user);
        user.Id.Should().Be("817e5374-583c-40c0-a84b-e67ab51f05dc");
        user.DisplayName.Should().Be("Test User");
        user.AccountEnabled.Should().BeTrue();
        user.CivilRegistrationNumberValidated.Should().Be(DateTime.Parse("2020-02-02 20:20:20"));
    }

    [Fact]
    public async Task Run_WithValidRequestAndNoMatchingUser_ReturnsNotFoundObjectResult()
    {
        // Arrange
        var dto = new ValidateUserDto("ce64b510-1877-49d5-9ce4-2b2e0dd0b9af", "Test User", "1234567890");
        var req = CreateHttpRequest("POST", dto);

        // Act
        var result = await _function.Run(req);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Run_WithValidRequestAndMultipleMatchingUsers_ReturnsConflictObjectResult()
    {
        // Arrange
        var dto = new ValidateUserDto("ce64b510-1877-49d5-9ce4-2b2e0dd0b9af", "Test User", "1234567890");
        var req = CreateHttpRequest("POST", dto);

        _helper.GetUsersAsync("extension_1be97e586e4944eea17a42fd1fc944cf_civilRegistrationNumber", "x3Xnt1ft5jDNCqERO9ECZhqziCnKUqZCKreChi8mhkY=").Returns([
            new User
            {
                Id = "817e5374-583c-40c0-a84b-e67ab51f05dc",
                DisplayName = "Test User",
                AccountEnabled = true,
                AdditionalData = new Dictionary<string, object>
                {
                    ["extension_1be97e586e4944eea17a42fd1fc944cf_civilRegistrationNumber"] = "x3Xnt1ft5jDNCqERO9ECZhqziCnKUqZCKreChi8mhkY="
                }
            },
            new User
            {
                Id = "2f3b49ba-3532-4c64-8608-8079a3571e5f",
                DisplayName = "Test User 2",
                AccountEnabled = false,
                AdditionalData = new Dictionary<string, object>
                {
                    ["extension_1be97e586e4944eea17a42fd1fc944cf_civilRegistrationNumber"] = "x3Xnt1ft5jDNCqERO9ECZhqziCnKUqZCKreChi8mhkY="
                }
            }
        ]);

        // Act
        var result = await _function.Run(req);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }
}