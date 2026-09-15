using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScytheButler.data.DatabaseModels.WomModels
{
    public class WomPlayer
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? Build { get; set; }
        public long? DiscordId { get; set; }
        [NotMapped]
        public string? DiscordUsername { get; set; }
        public string? CustomType { get; set; }
        public string? LinkType { get; set; }
        public string? Country { get; set; }
        public string? Status { get; set; }
        public bool Patron { get; set; }
        public long Exp { get; set; }
        public double Ehp { get; set; }
        public double Ehb { get; set; }
        public int Events { get; set; }
        public int Referals { get; set; }
        public DateTime? RegisteredAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Notes { get; set; }
    }
}