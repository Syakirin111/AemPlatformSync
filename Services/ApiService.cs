
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace AemPlatformSync.Services;

public sealed class ApiService : IDisposable
{
    private const string BaseUrl =
        "http://test-demo.aemenersol.com/api/";

    private readonly HttpClient _httpClient;

    public ApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(60)
        };
    }
    public void Dispose()
    {
        _httpClient.Dispose();
    }

    public async Task<string> LoginAsync(
        string username,
        string password)
    {
        var loginRequest = new
        {
            username,
            password
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "Account/Login",
            loginRequest);

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Login failed: HTTP {(int)response.StatusCode}. " +
                $"Response: {responseBody}");
        }

        // The Swagger example shows a JSON string response.
        var token = JsonSerializer.Deserialize<string>(responseBody);

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Login succeeded, but the API returned an empty token.");
        }

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        Console.WriteLine("API login successful. Bearer token received.");

        return token;
    }

    public async Task<string> GetPlatformWellActualAsync()
    {
        using var response = await _httpClient.GetAsync(
            "PlatformWell/GetPlatformWellActual");

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"GetPlatformWellActual failed: " +
                $"HTTP {(int)response.StatusCode}. Response: {responseBody}");
        }

        Console.WriteLine("Actual platform/well API request successful.");

        return responseBody;
    }

    public async Task<string> GetPlatformWellDummyAsync()
    {
        using var response = await _httpClient.GetAsync(
            "PlatformWell/GetPlatformWellDummy");

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"GetPlatformWellDummy failed: " +
                $"HTTP {(int)response.StatusCode}. Response: {responseBody}");
        }

        Console.WriteLine("Dummy platform/well API request successful.");

        return responseBody;
    }
}
