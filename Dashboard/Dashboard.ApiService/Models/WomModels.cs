namespace Dashboard.ApiService.Models;

public class WomGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<WomMembership> Memberships { get; set; } = new();
}

public class WomMembership
{
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } // Matches Wise Old Man JSON directly
    public WomPlayer Player { get; set; } = new();
}

public class WomPlayer
{
    public string DisplayName { get; set; } = string.Empty;
}