namespace Ondfisk.B2C.Models;

public interface IGraphHelper
{
    Task<User?> GetUserAsync(string id);
    Task<IEnumerable<User>> GetUsersAsync(string propertyName, string propertyValue);
    Task PatchUserAsync(User user);
}
