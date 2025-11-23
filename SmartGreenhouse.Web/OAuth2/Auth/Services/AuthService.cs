using System.Net.Http.Json;

public class AuthService
{
    private readonly HttpClient _http;

    public AuthService(HttpClient http)
    {
        _http = http;
    }

    public async Task<GoogleUserInfo?> GetUserInfoAsync(string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            "https://openidconnect.googleapis.com/v1/userinfo");

        request.Headers.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _http.SendAsync(request);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<GoogleUserInfo>();
    }
}

public class GoogleUserInfo
{
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Picture { get; set; }
    public string? Sub { get; set; }
}
