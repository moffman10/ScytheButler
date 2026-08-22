namespace Dashboard.Web.Helpers
{
    public static class CalculateTimeInClanHelper
    {
        public static string CalculateTimeInClan(DateTime joinedAt)
        {
            if (joinedAt == DateTime.MinValue)
                return "N/A";

            var span = DateTime.UtcNow - joinedAt;

            int years = span.Days / 365;
            int months = (span.Days % 365) / 30;
            int days = (span.Days % 365) % 30;

            if (years > 0)
                return $"{years}y {months}m {days}d";

            if (months > 0)
                return $"{months}m {days}d";

            return $"{days} days";
        }
    }
}
