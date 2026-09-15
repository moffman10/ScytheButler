using System;
using System.Collections.Generic;

namespace ScytheButler.data.DatabaseModels.Lfr
{
    public class LfrParty
    {
        public int Id { get; set; }

        // Discord information
        // SQL Server BIGINT maps to C# long.
        public long GuildId { get; set; }
        public long ChannelId { get; set; }
        public long MessageId { get; set; }

        // User who created the LFR
        public long CreatorId { get; set; }

        // Raid information
        public string RaidType { get; set; } = string.Empty;

        // Only used for ToB/HMT.
        // 0 for raids that don't have a team size.
        public int TeamSize { get; set; }

        // Whether the party has been closed.
        public bool IsClosed { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        // Party members
        public ICollection<LfrPartyMember> Members { get; set; }
            = new List<LfrPartyMember>();
    }
}