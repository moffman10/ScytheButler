using System;

namespace ScytheButler.data.DatabaseModels.Lfr
{
    public class LfrPartyMember
    {
        public int Id { get; set; }

        // Party this member belongs to
        public int LfrPartyId { get; set; }

        public LfrParty LfrParty { get; set; } = null!;

        // Discord user
        // SQL Server BIGINT maps to C# long.
        public long UserId { get; set; }

        // Discord username at the time they joined
        public string Username { get; set; } = string.Empty;

        // Used for ToB/HMT.
        //
        // Possible values:
        // Mage
        // NFRZ
        // RDPS
        // MDPS
        // MDPS2
        //
        // Null for ToA, CoX and CM CoX.
        public string? Role { get; set; }

        public DateTime JoinedAt { get; set; }
    }
}