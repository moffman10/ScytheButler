using ScytheButler.data.DatabaseModels.WomModels;
using System.Numerics;

public class PlayerAchievement
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public WomPlayer Player { get; set; } = null!;

    public string AchievementKey { get; set; } = string.Empty; // e.g., "radiant_oathplate" or "xerician"
    public bool IsUnlocked { get; set; }
    public DateTime? UnlockedAt { get; set; }
}