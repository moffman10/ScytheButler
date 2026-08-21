using ScytheButler.data.DatabaseModels.WomModels;

namespace Dashboard.Web.Models.ClanRoster
{
    public class RosterResponseDto
    {
        public WomGroupDto ClanData { get; set; } = default!;
        public DateTime LastSyncedAt { get; set; }
    }
}
