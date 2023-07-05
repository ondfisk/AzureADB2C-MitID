namespace Ondfisk.B2C.Models;

public class GraphHelper : IGraphHelper
{
    private readonly GraphServiceClient _client;

    public GraphHelper(GraphServiceClient client)
    {
        _client = client;
    }

    public async Task<User?> GetUserAsync(string id) => await _client.Users[id].GetAsync();

    public async Task<IEnumerable<User>> GetUsersAsync(string propertyName, string propertyValue)
    {
        var users = await _client.Users.GetAsync(configuration =>
        {
            configuration.QueryParameters.Select = new[] { "id", "displayName", "accountEnabled", propertyName };
            configuration.QueryParameters.Filter = $"{propertyName} eq '{propertyValue}'";
            configuration.QueryParameters.Count = true;
            configuration.Headers.Add("ConsistencyLevel", "eventual");
        });

        return users?.Value ?? new List<User>();
    }

    public async Task PatchUserAsync(User user)
    {
        await _client.Users[user.Id].PatchAsync(user);
    }
}
