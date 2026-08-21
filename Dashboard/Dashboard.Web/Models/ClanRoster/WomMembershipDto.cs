namespace Dashboard.Web.Models.ClanRoster
{
    public class WomMembershipDto
    {
        public int PlayerId { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public WomPlayerDto Player { get; set; } = new();
    }
}