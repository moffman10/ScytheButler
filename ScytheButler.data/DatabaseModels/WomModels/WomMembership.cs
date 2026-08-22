using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScytheButler.data.DatabaseModels.WomModels
{
    public class WomMembership
    {
        public int PlayerId { get; set; }

        [ForeignKey(nameof(PlayerId))]
        public WomPlayer? Player { get; set; }

        public int GroupId { get; set; }

        [ForeignKey(nameof(GroupId))]
        public WomGroup? Group { get; set; }

        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}