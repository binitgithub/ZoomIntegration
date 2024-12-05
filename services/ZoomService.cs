using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using ZoomIntegration.Models;

public class ZoomService
{
    private readonly HttpClient _httpClient;
    private readonly ZoomOptions _options;

    public ZoomService(HttpClient httpClient, IOptions<ZoomOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    private string GenerateJwtToken()
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.ApiSecret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.ApiKey,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> CreateMeetingAsync(string userId, string topic, string startTime, int duration)
    {
        var token = GenerateJwtToken();
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var request = new
        {
            topic = topic,
            type = 2,
            start_time = startTime,
            duration = duration,
            settings = new
            {
                host_video = true,
                participant_video = true
            }
        };

        var response = await _httpClient.PostAsJsonAsync($"https://api.zoom.us/v2/users/{userId}/meetings", request);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
            return jsonResponse.GetProperty("join_url").GetString();
        }

        var errorResponse = await response.Content.ReadAsStringAsync();
        throw new Exception($"Failed to create meeting: {errorResponse}");
    }
}
