using System.Net.Http.Headers;
using System.Text.Json;
using Dashboard.ApiService.Models;

namespace Dashboard.ApiService.Services;

public class WomService
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;

    public WomService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.wiseoldman.net/v2/");

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ScytheButlerDashboard/1.0");

        _apiKey = config["WiseOldManApiKey"];
        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }
    }

    public async Task<WomGroup?> GetGroupDetailsAsync(int groupId)
    {
        var response = await _httpClient.GetAsync($"groups/{groupId}");

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"WOM API Error ({response.StatusCode}): {error}");
        }

        var content = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<WomGroup>(content, options);
    }
}