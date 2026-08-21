namespace Dashboard.Web.Models.ClanRoster
{
    public class WomGroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime? LastSyncedAt { get; set; }
        public List<WomMembershipDto> Memberships { get; set; } = new();
    }
}