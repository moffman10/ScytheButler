namespace Dashboard.Web.Models.PlayerDetails
{
    public class PlayerDetailResponseDto
    {
        public WomPlayerDto Player { get; set; } = default!;
        public string Role { get; set; } = string.Empty;
    }
}
