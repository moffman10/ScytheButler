namespace Dashboard.Web.Helpers
{
    public static class ExclusivegroupHelper
    {
        private static readonly string[] CombatAchievementGroup =
{
    "elite_ca",
    "master_ca",
    "grandmaster_ca"
};

        private static readonly string[] CloggerAchievementGroup =
        {
    "black_clogger",
    "mithril_clogger",
    "adamant_clogger",
    "rune_clogger",
    "dragon_clogger",
    "gilded_clogger"
};

        public static IEnumerable<string> GetExclusiveGroup(string achievementKey)
        {
            if (CombatAchievementGroup.Contains(achievementKey, StringComparer.OrdinalIgnoreCase))
                return CombatAchievementGroup;

            if (CloggerAchievementGroup.Contains(achievementKey, StringComparer.OrdinalIgnoreCase))
                return CloggerAchievementGroup;

            return Enumerable.Empty<string>();
        }
    }
}
