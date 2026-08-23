namespace Dashboard.Web.Helpers
{
    public static class GetBadgeClassHelper
    {
        public static string GetRankBadgeClass(string role)
        {
            if (string.IsNullOrWhiteSpace(role)) return "badge-secondary";

            var r = role.ToLowerInvariant().Trim();

            // Gems
            if (r == "admiral") return "badge-3-gem";
            if (r == "brigadier") return "badge-2-gem";
            if (r == "colonel") return "badge-1-gem";

            // Stars
            if (r == "commander") return "badge-3-star";
            if (r == "officer") return "badge-2-star";
            if (r == "general") return "badge-1-star";

            // Bananas
            if (r == "sergeant") return "badge-3-banana";
            if (r == "corporal") return "badge-2-banana";
            if (r == "recruit") return "badge-1-banana";

            // Keys
            if (r == "owner") return "badge-gold-key";
            if (r == "deputy_owner") return "badge-silver-key";
            if (r == "administrator") return "badge-bronze-key";

            // Special Roles
            if (r == "illusionist") return "badge-event-manager";
            if (r == "xerician") return "badge-raid-coach";

            return "badge-secondary";
        }
    }
}