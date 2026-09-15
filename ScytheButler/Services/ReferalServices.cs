using Dashboard.Web.Models.ClanRoster;
using ScytheButler.Data;
using System.Net.Http.Json;

namespace ScytheButler.Services
{
    public class ReferralService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _db;

        public ReferralService(
            HttpClient httpClient,
            AppDbContext db)
        {
            _httpClient = httpClient;
            _db = db;
        }

        public async Task<List<ReferralMatch>> FindReferralsAsync(
            ulong discordId,
            int groupId,
            IEnumerable<string> names)
        {
            // Force the API to sync the clan first
            var roster = await _httpClient.GetFromJsonAsync<RosterResponseDto>(
                $"api/Clan/{groupId}?forceSync=true");

            if (roster?.ClanData?.Memberships == null)
            {
                return new List<ReferralMatch>();
            }

            var players = roster.ClanData.Memberships
                .Where(m => m.Player != null)
                .Select(m => m.Player!)
                .Where(p => !string.IsNullOrWhiteSpace(p.Username))
                .ToList();

            var results = new List<ReferralMatch>();

            foreach (var input in names)
            {
                var name = input.Trim();

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                // Exact match first
                var exactMatch = players.FirstOrDefault(p =>
                    string.Equals(
                        p.Username,
                        name,
                        StringComparison.OrdinalIgnoreCase));

                if (exactMatch != null)
                {
                    results.Add(new ReferralMatch
                    {
                        InputName = name,
                        MatchedUsername = exactMatch.Username,
                        IsExactMatch = true,
                        Similarity = 1.0,
                        Successful = true
                    });

                    continue;
                }

                // Otherwise find the closest username
                var closest = players
                    .Select(p => new
                    {
                        Player = p,
                        Similarity = CalculateSimilarity(
                            name,
                            p.Username ?? string.Empty)
                    })
                    .OrderByDescending(x => x.Similarity)
                    .FirstOrDefault();

                if (closest != null)
                {
                    // 60% or higher = successful referral
                    bool successful = closest.Similarity >= 0.60;

                    results.Add(new ReferralMatch
                    {
                        InputName = name,
                        MatchedUsername = closest.Player.Username,
                        IsExactMatch = false,
                        Similarity = closest.Similarity,
                        Successful = successful
                    });
                }
                else
                {
                    results.Add(new ReferralMatch
                    {
                        InputName = name,
                        MatchedUsername = null,
                        IsExactMatch = false,
                        Similarity = 0,
                        Successful = false
                    });
                }
            }

            // Give the person who ran the command +1 referral
            // for every successful referral.
            int successfulCount = results.Count(r => r.Successful);

            if (successfulCount > 0)
            {
                await AddReferralsAsync(discordId, successfulCount);
            }

            return results;
        }

        private async Task AddReferralsAsync(
    ulong discordId,
    int amount)
        {
            var players = _db.Players
                .Where(p => p.DiscordId == (long)discordId)
                .ToList();

            if (players.Count == 0)
                return;

            foreach (var player in players)
            {
                player.Referals += amount;
            }

            await _db.SaveChangesAsync();
        }

        private static double CalculateSimilarity(
            string a,
            string b)
        {
            a = a.ToLowerInvariant();
            b = b.ToLowerInvariant();

            if (a == b)
                return 1.0;

            int distance = LevenshteinDistance(a, b);
            int maxLength = Math.Max(a.Length, b.Length);

            if (maxLength == 0)
                return 1.0;

            return 1.0 - (double)distance / maxLength;
        }

        private static int LevenshteinDistance(
            string a,
            string b)
        {
            var matrix = new int[a.Length + 1, b.Length + 1];

            for (int i = 0; i <= a.Length; i++)
                matrix[i, 0] = i;

            for (int j = 0; j <= b.Length; j++)
                matrix[0, j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1]
                        ? 0
                        : 1;

                    matrix[i, j] = Math.Min(
                        Math.Min(
                            matrix[i - 1, j] + 1,
                            matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[a.Length, b.Length];
        }
    }

    public class ReferralMatch
    {
        public string InputName { get; set; } = string.Empty;

        public string? MatchedUsername { get; set; }

        public bool IsExactMatch { get; set; }

        /// <summary>
        /// Similarity score between 0.0 and 1.0.
        /// Example: 0.75 = 75%.
        /// </summary>
        public double Similarity { get; set; }

        /// <summary>
        /// True when the similarity score is 60% or higher.
        /// </summary>
        public bool Successful { get; set; }
    }
}