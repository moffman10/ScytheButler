using System;
using System.Collections.Generic;

namespace ScytheButler.data.DatabaseModels.WomModels
{
    public class WomGroup
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ClanChat { get; set; }
        public string? Description { get; set; }
        public string? Homeworld { get; set; }
        public bool Verified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public DateTime? LastSyncedAt { get; set; }

        public List<WomMembership> Memberships { get; set; } = new();
    }
}