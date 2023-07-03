namespace Ondfisk.B2C;

public static class Extensions
{
    public static string Hash(this string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);

        var hash = SHA256.HashData(bytes);

        return Convert.ToBase64String(hash);
    }

    public static async Task<HttpResponseData> CreateResponse(this HttpRequestData request, HttpStatusCode statusCode, object data)
    {
        var response = request.CreateResponse(statusCode);

        await response.WriteAsJsonAsync(data);

        return response;
    }
}