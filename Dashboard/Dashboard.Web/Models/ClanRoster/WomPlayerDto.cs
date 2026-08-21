namespace Dashboard.Web.Models.ClanRoster
{
    public class WomPlayerDto
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Country { get; set; }
        public double Exp { get; set; }
        public double Ehp { get; set; }
        public double Ehb { get; set; }
    }
}