namespace Dashboard.Web.Helpers
{
    public static class RankHelper
    {
        public static readonly Dictionary<string, string> RankNames =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "recruit", "1 Banana" },
                { "corporal", "2 Banana" },
                { "sergeant", "3 Banana" },
                { "general", "1 Star" },
                { "officer", "2 Star" },
                { "commander", "3 Star" },
                { "colonel", "1 Gem" },
                { "brigadier", "2 Gem" },
                { "admiral", "3 Gem" },
                { "xerician", "Raid coach" },
                { "illusionist", "Event staff" },
                { "administrator", "Bronze Key" },
                { "deputy_owner", "Silver Key" },
                { "owner", "Gold Key" }
            };

        public static string FormatRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return "1 Banana";

            return RankNames.TryGetValue(role, out var customRank)
                ? customRank
                : role;
        }
    }
}