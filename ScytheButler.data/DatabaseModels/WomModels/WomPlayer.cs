using System;

namespace ScytheButler.data.DatabaseModels.WomModels
{
    public class WomPlayer
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? Build { get; set; }
        public string? Status { get; set; }
        public string? Country { get; set; }
        public bool Patron { get; set; }
        public double Exp { get; set; }
        public double Ehp { get; set; }
        public double Ehb { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}