using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScytheButler.Models
{
    public class DiceRollResult
    {
        public List<int> Rolls { get; set; } = new();
        public int Total { get; set; }
        public Image<Rgba32> Image { get; set; }
    }
}
