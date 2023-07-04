namespace Ondfisk.B2C.Models;

public record ValidatedUserDto(string Id, string DisplayName, bool AccountEnabled, DateTimeOffset CivilRegistrationNumberValidated);