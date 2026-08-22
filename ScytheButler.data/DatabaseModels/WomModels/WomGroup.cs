using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ScytheButler.data.DatabaseModels.WomModels
{
    public class WomGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ClanChat { get; set; }
        public string? Description { get; set; }
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
        public int? Homeworld { get; set; }
        public bool Verified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public DateTime? LastSyncedAt { get; set; }

        public List<WomMembership> Memberships { get; set; } = new();
    }
}