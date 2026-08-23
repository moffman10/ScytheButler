using Dashboard.ApiService.Services;
using Dashboard.Web.Models.PlayerDetails;
using Dashboard.Web.Models.ClanRoster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScytheButler.data.DatabaseModels.WomModels;
using ScytheButler.Data;
using Dashboard.ApiService.Helpers;

namespace Dashboard.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClanController : ControllerBase
{
    private readonly ScytheButler.Data.AppDbContext _db;
    private readonly WomService _womService;

    public ClanController(AppDbContext db, WomService womService)
    {
        _db = db;
        _womService = womService;
    }

    [HttpGet("{groupId}")]
    public async Task<ActionResult<RosterResponseDto>> GetClanRoster(int groupId, [FromQuery] bool forceSync = false)
    {
        // 1. Fetch group with memberships and players from internal database
        var existingGroup = await _db.Groups
            .Include(g => g.Memberships)
                .ThenInclude(m => m.Player)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        // 2. Determine if cache is stale (>15 minutes) or missing
        bool isStale = existingGroup == null ||
                       existingGroup.LastSyncedAt == null ||
                       (DateTime.UtcNow - existingGroup.LastSyncedAt.Value).TotalMinutes >= 15;

        // 3. Upsert if forced or stale
        if (forceSync || isStale)
        {
            var freshData = await _womService.GetGroupDetailsAsync(groupId);
            if (freshData != null)
            {
                await UpsertGroupDataAsync(freshData);

                // Reload from DB to verify saved state
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

        return Ok(new
        {
            Player = player,
            Role = membership?.Role ?? "recruit"
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
        // Find player and update events in your database, e.g.:
        var player = await _db.Players.FindAsync(id);
        if (player == null) return NotFound();

        player.Events = dto.Events;
        await _db.SaveChangesAsync();

        return Ok();
    }

   

    private async Task UpsertGroupDataAsync(WomGroup freshGroup)
    {
        var syncTime = DateTime.UtcNow;

        // 1. Upsert Group Entity
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

        // 2. Upsert Players & Memberships
        if (freshGroup.Memberships != null)
        {
            foreach (var membership in freshGroup.Memberships)
            {
                if (membership.Player == null || membership.Player.Id == 0)
                    continue;

                int playerId = membership.Player.Id;

                // Handle Player Entity
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

                // Handle Membership Entity
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
        }

        await _db.SaveChangesAsync();
    }
}