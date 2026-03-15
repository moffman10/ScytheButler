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

        public async Task<CompetitionResult> CreateCompetitionFromInputAsync(
    string title,
    string metric,
    string startDate,
    string endDate,
    string? participants,
    string? teams,
    int? groupId,
    string? groupVerificationCode)
        {
            try
            {
                if (!Enum.TryParse<Metric>(metric, true, out var metricEnum))
                    throw new Exception($"Invalid metric: {metric}");

                if (!DateTime.TryParse(startDate, out var start))
                    throw new Exception("Invalid start date. Use YYYY-MM-DD");

                if (!DateTime.TryParse(endDate, out var end))
                    throw new Exception("Invalid end date. Use YYYY-MM-DD");

                if (end <= start)
                    throw new Exception("End date must be after start date");

                string[]? participantList = null;

                if (!string.IsNullOrWhiteSpace(participants))
                {
                    participantList = participants
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToArray();
                }

                Team[]? teamList = null;

                if (!string.IsNullOrWhiteSpace(teams))
                {
                    var parsed = new List<Team>();

                    foreach (var teamInput in teams.Split(';', StringSplitOptions.RemoveEmptyEntries))
                    {
                        var parts = teamInput
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim())
                            .ToArray();

                        parsed.Add(new Team
                        {
                            name = parts[0],
                            participants = parts.Skip(1).ToArray()
                        });
                    }

                    teamList = parsed.ToArray();
                }

                var apiResult = await CreateCompetitionAsync(
                    title,
                    metricEnum,
                    start,
                    end,
                    participantList,
                    groupId,
                    groupVerificationCode,
                    teamList);

                return new CompetitionResult
                {
                    Success = true,
                    Title = title,
                    Metric = metricEnum.ToString(),
                    Start = start,
                    End = end,
                    Participants = participantList,
                    Teams = teamList,
                    ApiResponse = apiResult
                };
            }
            catch (Exception ex)
            {
                return new CompetitionResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<string> CreateCompetitionAsync(string title, Metric metric, DateTime startAt, DateTime endAt, string[]? participants, int? groupId, string? groupVerificationCode, Team[]? teams)
        {
            var requestBody = new CreateCompetitionRequest
            {
                Title = title,
                Metric = metric,
                StartsAt = startAt,
                EndsAt = endAt,
                Participants = participants,
                groupId = groupId,
                groupVerificationCode = groupVerificationCode,
                teams = teams          
            };

            var jsonOptions = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
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