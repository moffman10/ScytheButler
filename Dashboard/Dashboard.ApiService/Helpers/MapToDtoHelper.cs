using Dashboard.Web.Models.ClanRoster;
using ScytheButler.data.DatabaseModels.WomModels;

namespace Dashboard.ApiService.Helpers
{
    public static class ClanMappingExtensions
    {
        public static WomGroupDto MapToGroupDto(this WomGroup group)
        {
            return new WomGroupDto
            {
                Id = group.Id,
                Name = group.Name,
                LastSyncedAt = group.LastSyncedAt,
                Memberships = group.Memberships?.Select(m => new WomMembershipDto
                {
                    PlayerId = m.PlayerId,
                    Role = m.Role,
                    Player = m.Player != null ? new WomPlayerDto
                    {
                        Id = m.Player.Id,
                        Username = m.Player.Username,
                        DisplayName = m.Player.DisplayName
                    } : null!
                }).ToList() ?? new List<WomMembershipDto>()
            };
        }
    }
}