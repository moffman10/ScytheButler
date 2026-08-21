using System;

namespace ScytheButler.data.DatabaseModels.WomModels
{
    public class WomMembership
    {
        public int PlayerId { get; set; }
        public WomPlayer Player { get; set; } = new();

        public int GroupId { get; set; }
        public WomGroup Group { get; set; } = default!;

        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}