using Microsoft.Extensions.Configuration;
using ScytheButler.Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ScytheButler.Services
{
    public class WiseOldManService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        public WiseOldManService(IConfiguration config )
        {
            _httpClient = new HttpClient();
            _apiKey = config["WiseOldManApiKey"];
        }

        public async Task<string> CreateCompetitionAsync(string title, Metric metric, DateTime startAt, DateTime endAt, string[]? participants, int? groupId, string? groupVerificationCode, Team[]? teams)
        {
            var requestBody = new CreateCompetitionRequest
            {
                Title = title,
                Metric = metric,
                StartAt = startAt,
                EndAt = endAt,
                Participants = participants,
                groupId = groupId,
                groupVerificationCode = groupVerificationCode,
                teams = teams          
            };

            var jsonOptions = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(requestBody, jsonOptions);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.wiseoldman.net/v2/competitions")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
            

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"WiseOldMan Error: {content}");

            return content;
        }
    }
}