using Dashboard.ApiService.Services;
using Dashboard.Web.Models.PlayerDetails;
using Dashboard.Web.Models.ClanRoster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScytheButler.data.DatabaseModels.WomModels;
using ScytheButler.Data;
using Dashboard.ApiService.Helpers;
using System.Text.Json;
using System.Net.Http.Headers;
using ScytheButler.Models; 

namespace Dashboard.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClanController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly WomService _womService;
    private readonly DiscordRoleSyncService _discordRoleSyncService;
    private readonly AchievementService _achievementService;
    private readonly IConfiguration _configuration;

    public ClanController(AppDbContext db, WomService womService, DiscordRoleSyncService discordRoleSyncService, AchievementService achievementService, IConfiguration configuration)
    {
        _db = db;
        _womService = womService;
        _discordRoleSyncService = discordRoleSyncService;
        _achievementService = achievementService;
        _configuration = configuration;
    }

    [HttpGet("{groupId}")]
    public async Task<ActionResult<RosterResponseDto>> GetClanRoster(int groupId, [FromQuery] bool forceSync = false)
    {
        var existingGroup = await _db.Groups
            .Include(g => g.Memberships)
                .ThenInclude(m => m.Player)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        bool isStale = existingGroup == null ||
                       existingGroup.LastSyncedAt == null ||
                       (DateTime.UtcNow - existingGroup.LastSyncedAt.Value).TotalMinutes >= 15;

        if (forceSync || isStale)
        {
            var freshData = await _womService.GetGroupDetailsAsync(groupId);
            if (freshData != null)
            {
                await UpsertGroupDataAsync(freshData);

                await _discordRoleSyncService.SyncGroupRolesAsync(groupId);

                existingGroup = await _db.Groups
                    .Include(g => g.Memberships)
                        .ThenInclude(m => m.Player)
                    .FirstOrDefaultAsync(g => g.Id == groupId);
            }
        }

        if (existingGroup == null)
        {
            return NotFound($"Clan group with ID {groupId} could not be retrieved.");
        }

        return Ok(new RosterResponseDto
        {
            ClanData = existingGroup.MapToGroupDto(),
            LastSyncedAt = existingGroup.LastSyncedAt ?? DateTime.UtcNow
        });
    }

    [HttpGet("/api/player/{id}")]
    public async Task<ActionResult<object>> GetPlayerById(int id)
    {
        var player = await _db.Players.FindAsync(id);

        if (player == null)
        {
            return NotFound($"Player with ID {id} could not be found.");
        }

        var membership = await _db.Memberships.FirstOrDefaultAsync(m => m.PlayerId == id);

        // Fetch / Refresh Discord username live via Discord REST API when profile is loaded
        if (player.DiscordId.HasValue)
        {
            try
            {
                string? botToken = _configuration["DiscordToken"] ?? _configuration["Discord:Token"];

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "ScytheButler (Clan Dashboard, v1.0)");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", botToken);

                var response = await httpClient.GetAsync($"https://discord.com/api/v10/users/{player.DiscordId.Value}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadFromJsonAsync<JsonElement>();
                    string liveUsername = json.GetProperty("username").GetString() ?? player.DiscordUsername;

                    // If their username changed on Discord, update it in the database for all their linked accounts!
                    if (liveUsername != player.DiscordUsername)
                    {
                        var siblingAccounts = await _db.Players
                            .Where(p => p.DiscordId == player.DiscordId.Value)
                            .ToListAsync();

                        foreach (var acc in siblingAccounts)
                        {
                            acc.DiscordUsername = liveUsername;
                        }
                        await _db.SaveChangesAsync();
                    }
                }
            }
            catch
            {
                // Fallback gracefully to whatever is stored in SSMS if the REST call fails/rate-limits
            }
        }

        // Find sibling accounts sharing the same DiscordId
        var linkedAccounts = new List<WomPlayer>();
        if (player.DiscordId.HasValue)
        {
            linkedAccounts = await _db.Players
                .Where(p => p.DiscordId == player.DiscordId.Value && p.Id != id)
                .ToListAsync();
        }

        // Fetch saved achievements for this player from the database
        var achievements = await _db.PlayerAchievements
            .Where(pa => pa.PlayerId == id)
            .ToDictionaryAsync(pa => pa.AchievementKey, pa => pa.IsUnlocked);

        return Ok(new
        {
            Player = player,
            DiscordUsername = player.DiscordUsername, // Always returns the up-to-date username
            Role = membership?.Role ?? "recruit",
            LinkedAccounts = linkedAccounts,
            Achievements = achievements // Send achievement dictionary down to frontend
        });
    }

    [HttpPost("/api/player/{id}/notes")]
    public async Task<IActionResult> UpdatePlayerNotes(int id, [FromBody] UpdateNotesDto dto)
    {
        var player = await _db.Players.FindAsync(id);
        if (player == null)
        {
            return NotFound($"Player with ID {id} could not be found.");
        }

        player.Notes = dto.Notes;
        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("/api/player/{id}/events")]
    public async Task<IActionResult> UpdatePlayerEvents(int id, [FromBody] UpdateEventsDto dto)
    {
        var player = await _db.Players.FindAsync(id);
        if (player == null) return NotFound();

        player.Events = dto.Events;
        await _db.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// Toggles an achievement/title for a player, updates the unlocked date, and updates their Discord role.
    /// </summary>
    [HttpPost("/api/player/{id}/toggle-achievement")]
    public async Task<IActionResult> TogglePlayerAchievement(int id, [FromBody] ToggleAchievementDto dto)
    {
        var player = await _db.Players.FindAsync(id);
        if (player == null)
        {
            return NotFound($"Player with ID {id} could not be found.");
        }

        // 1. Find or create the achievement record for this player
        var record = await _db.PlayerAchievements
            .FirstOrDefaultAsync(pa => pa.PlayerId == id && pa.AchievementKey == dto.AchievementKey);

        if (record == null)
        {
            record = new PlayerAchievement
            {
                PlayerId = id,
                AchievementKey = dto.AchievementKey
            };
            _db.PlayerAchievements.Add(record);
        }

        record.IsUnlocked = dto.IsUnlocked;
        record.UnlockedAt = dto.IsUnlocked ? DateTime.UtcNow : null;
        await _db.SaveChangesAsync();

        string? botToken = _configuration["DiscordToken"] ?? _configuration["Discord:Token"];
        string? guildId = _configuration["Discord:GuildId"];
        string playerName = player.Username ?? player.DisplayName ?? "Unknown Member"; // Update property name based on your database model

        // 2. Sync Role with Discord if the player has a linked Discord account and a role mapping exists
        if (player.DiscordId.HasValue && player.LinkType == "primary")
        {
            string? roleId = _configuration[$"Discord:AchievementRoleMappings:{dto.AchievementKey}"];
            if (!string.IsNullOrEmpty(roleId) && !string.IsNullOrEmpty(botToken) && !string.IsNullOrEmpty(guildId))
            {
                try
                {
                    using var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "ScytheButler (Clan Dashboard, v1.0)");
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", botToken);

                    string memberUrl = $"https://discord.com/api/v10/guilds/{guildId}/members/{player.DiscordId.Value}";

                    if (dto.IsUnlocked)
                    {
                        await httpClient.PutAsync($"{memberUrl}/roles/{roleId}", null);
                    }
                    else
                    {
                        await httpClient.DeleteAsync($"{memberUrl}/roles/{roleId}");
                    }
                }
                catch
                {
                    // Handle or log discord api errors gracefully if needed
                }
            }
        }

        // 3. Sync the Discord Embed Message (adds/removes player name from the list)
        string? channelId = _configuration["Discord:AchievementChannelId"];
        if (!string.IsNullOrEmpty(channelId))
        {
            try
            {
                await _achievementService.SyncAchievementMessageContentAsync(channelId, dto.AchievementKey, playerName, dto.IsUnlocked);
            }
            catch
            {
                // Handle or log embed patch errors gracefully if needed
            }
        }

        return Ok(new { success = true, unlockedAt = record.UnlockedAt });
    }

    private async Task UpsertGroupDataAsync(WomGroup freshGroup)
    {
        var syncTime = DateTime.UtcNow;
        var existingGroup = await _db.Groups.FindAsync(freshGroup.Id);

        if (existingGroup == null)
        {
            existingGroup = new WomGroup
            {
                Id = freshGroup.Id,
                Name = freshGroup.Name,
                ClanChat = freshGroup.ClanChat,
                Description = freshGroup.Description,
                Homeworld = freshGroup.Homeworld,
                Verified = freshGroup.Verified,
                CreatedAt = freshGroup.CreatedAt,
                UpdatedAt = freshGroup.UpdatedAt,
                LastSyncedAt = syncTime
            };
            _db.Groups.Add(existingGroup);
        }
        else
        {
            existingGroup.Name = freshGroup.Name;
            existingGroup.ClanChat = freshGroup.ClanChat;
            existingGroup.Description = freshGroup.Description;
            existingGroup.Homeworld = freshGroup.Homeworld;
            existingGroup.Verified = freshGroup.Verified;
            existingGroup.UpdatedAt = freshGroup.UpdatedAt;
            existingGroup.LastSyncedAt = syncTime;
        }

        if (freshGroup.Memberships != null)
        {
            var womPlayerIds = new HashSet<int>();

            foreach (var membership in freshGroup.Memberships)
            {
                if (membership.Player == null || membership.Player.Id == 0)
                    continue;

                int playerId = membership.Player.Id;
                womPlayerIds.Add(playerId);

                var existingPlayer = await _db.Players.FindAsync(playerId);
                if (existingPlayer == null)
                {
                    var newPlayer = new WomPlayer
                    {
                        Id = playerId,
                        Username = membership.Player.Username,
                        DisplayName = membership.Player.DisplayName,
                        Type = membership.Player.Type,
                        Build = membership.Player.Build,
                        Country = membership.Player.Country,
                        Status = membership.Player.Status,
                        Patron = membership.Player.Patron,
                        Exp = membership.Player.Exp,
                        Ehp = membership.Player.Ehp,
                        Ehb = membership.Player.Ehb,
                        RegisteredAt = membership.Player.RegisteredAt,
                        UpdatedAt = membership.Player.UpdatedAt,
                        CreatedAt = membership.CreatedAt
                    };
                    _db.Players.Add(newPlayer);
                }
                else
                {
                    existingPlayer.Username = membership.Player.Username;
                    existingPlayer.DisplayName = membership.Player.DisplayName;
                    existingPlayer.Type = membership.Player.Type;
                    existingPlayer.Build = membership.Player.Build;
                    existingPlayer.Country = membership.Player.Country;
                    existingPlayer.Status = membership.Player.Status;
                    existingPlayer.Patron = membership.Player.Patron;
                    existingPlayer.Exp = membership.Player.Exp;
                    existingPlayer.Ehp = membership.Player.Ehp;
                    existingPlayer.Ehb = membership.Player.Ehb;
                    existingPlayer.UpdatedAt = membership.Player.UpdatedAt;
                    existingPlayer.CreatedAt = membership.CreatedAt;
                }

                var existingMembership = await _db.Memberships
                    .FirstOrDefaultAsync(m => m.PlayerId == playerId && m.GroupId == freshGroup.Id);

                if (existingMembership == null)
                {
                    _db.Memberships.Add(new WomMembership
                    {
                        PlayerId = playerId,
                        GroupId = freshGroup.Id,
                        Role = membership.Role,
                        CreatedAt = membership.CreatedAt,
                        UpdatedAt = membership.UpdatedAt
                    });
                }
                else
                {
                    existingMembership.Role = membership.Role;
                    existingMembership.UpdatedAt = membership.UpdatedAt;
                }
            }

            // Remove memberships that are no longer returned by WOM
            var existingMemberships = await _db.Memberships
                .Where(m => m.GroupId == freshGroup.Id)
                .ToListAsync();

            var membershipsToRemove = existingMemberships
                .Where(m => !womPlayerIds.Contains(m.PlayerId))
                .ToList();

            _db.Memberships.RemoveRange(membershipsToRemove);
        }

        await _db.SaveChangesAsync();
    }

}