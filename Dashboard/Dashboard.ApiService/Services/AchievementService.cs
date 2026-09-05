using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Dashboard.ApiService.Services
{
    public class DeploymentItem
    {
        public bool IsHeader { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;
        public int Color { get; set; } = 3447003;
    }

    public class AchievementService
    {
        private readonly IConfiguration _configuration;

        public AchievementService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SyncAchievementMessageContentAsync(
            string channelId,
            string achievementKey,
            string playerName,
            bool isUnlocked)
        {
            var messageId = _configuration[$"Discord:AchievementMessages:{achievementKey}"];
            var botToken = _configuration["DiscordToken"] ?? _configuration["Discord:Token"];

            if (string.IsNullOrEmpty(messageId) || string.IsNullOrEmpty(botToken))
            {
                return;
            }

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ScytheButler (Clan Dashboard, v1.0)");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", botToken);

            string url = $"https://discord.com/api/v10/channels/{channelId}/messages/{messageId}";

            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return;

            var messageNode = await response.Content.ReadFromJsonAsync<JsonObject>();
            var embedsArray = messageNode?["embeds"]?.AsArray();

            if (embedsArray == null || embedsArray.Count == 0) return;

            var embedNode = embedsArray[0]?.AsObject()?.DeepClone()?.AsObject();
            if (embedNode == null) return;

            // Update description text
            string currentDesc = embedNode["description"]?.GetValue<string>() ?? string.Empty;
            string updatedDesc = UpdateAchieversList(currentDesc, playerName, isUnlocked);

            if (updatedDesc == currentDesc) return;
            embedNode["description"] = updatedDesc;

            var existingAttachments = messageNode?["attachments"]?.AsArray();
            bool imageConvertedToCdn = false;

            // Handle Image URL Protection & CDN Conversion
            if (embedNode["image"] is JsonObject imageObj)
            {
                string? imageUrl = imageObj["url"]?.GetValue<string>();

                if (!string.IsNullOrEmpty(imageUrl))
                {
                    if (imageUrl.StartsWith("attachment://", StringComparison.OrdinalIgnoreCase))
                    {
                        string targetFileName = imageUrl["attachment://".Length..];
                        string? cdnUrl = null;

                        if (existingAttachments != null)
                        {
                            foreach (var att in existingAttachments)
                            {
                                if (att is JsonObject attObj &&
                                    string.Equals(attObj["filename"]?.GetValue<string>(), targetFileName, StringComparison.OrdinalIgnoreCase))
                                {
                                    cdnUrl = attObj["url"]?.GetValue<string>();
                                    break;
                                }
                            }
                        }

                        // If found in attachments, upgrade it to a permanent CDN URL
                        if (!string.IsNullOrEmpty(cdnUrl))
                        {
                            imageObj["url"] = cdnUrl;
                            imageConvertedToCdn = true;
                        }
                    }
                    else if (imageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        // It's already a CDN URL from a previous successful update
                        imageConvertedToCdn = true;
                    }
                }
            }

            // Build payload dynamically based on whether the image is safely on the CDN
            var payload = new JsonObject
            {
                ["embeds"] = new JsonArray { embedNode }
            };

            if (imageConvertedToCdn)
            {
                // Image is safe on CDN, safe to clear raw attachments to prevent duplication
                payload["attachments"] = new JsonArray();
            }
            else if (existingAttachments != null)
            {
                // Image is still using local attachment reference; preserve attachments so it doesn't break
                var attachmentsPayload = new JsonArray();
                foreach (var att in existingAttachments)
                {
                    if (att is JsonObject attObj && attObj["id"] != null)
                    {
                        var cleanAtt = new JsonObject { ["id"] = attObj["id"]?.DeepClone() };
                        if (attObj["filename"] != null) cleanAtt["filename"] = attObj["filename"]?.DeepClone();
                        attachmentsPayload.Add(cleanAtt);
                    }
                }
                payload["attachments"] = attachmentsPayload;
            }

            var patchResponse = await httpClient.PatchAsJsonAsync(url, payload);
            if (!patchResponse.IsSuccessStatusCode)
            {
                string error = await patchResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to update Discord achievement '{achievementKey}': {error}");
            }
        }

        public string UpdateAchieversList(string content, string playerName, bool isUnlocked)
        {
            var lines = content.Split('\n').ToList();
            lines.RemoveAll(l => l.Contains("Think you belong here?", StringComparison.OrdinalIgnoreCase));

            int achieversIndex = lines.FindIndex(l => l.Contains("Achievers:", StringComparison.OrdinalIgnoreCase));
            if (achieversIndex == -1) return content;

            int endIdx = achieversIndex + 1;
            while (endIdx < lines.Count && lines[endIdx].TrimStart().StartsWith("- "))
            {
                endIdx++;
            }

            var playerLines = lines.GetRange(achieversIndex + 1, endIdx - (achieversIndex + 1));
            bool exists = playerLines.Any(l => l.Contains(playerName, StringComparison.OrdinalIgnoreCase));

            if (isUnlocked && !exists)
            {
                lines.Insert(achieversIndex + 1, $"- {playerName}");
            }
            else if (!isUnlocked && exists)
            {
                lines.RemoveAll(l => l.Trim().Equals($"- {playerName}", StringComparison.OrdinalIgnoreCase));
            }

            return string.Join("\n", lines);
        }
    }
}