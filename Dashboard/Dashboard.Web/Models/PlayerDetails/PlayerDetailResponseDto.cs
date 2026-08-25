public class PlayerDetailResponseDto
{
    public WomPlayerDto Player { get; set; } = null!;
    public string? Role { get; set; }
    public List<WomPlayerDto>? LinkedAccounts { get; set; }

    // Add this dictionary to send achievement states to the frontend
    public Dictionary<string, bool> Achievements { get; set; } = new();
}