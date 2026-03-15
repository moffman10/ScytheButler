using ScytheButler.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Text.RegularExpressions;

namespace ScytheButler.Services
{
    public class DiceService
    {
        private readonly Random _random = new();

        // Light method: just validate and roll dice, no heavy ImageSharp work
        public DiceRollData RollDiceData(string diceInput)
        {
            var match = Regex.Match(diceInput, @"(\d+)d(\d+)");

            if (!match.Success)
                throw new Exception("Invalid dice format.");

            int count = int.Parse(match.Groups[1].Value);
            int sides = int.Parse(match.Groups[2].Value);

            if (count > 20)
                throw new Exception("Maximum 20 dice allowed.");

            List<int> rolls = new();
            for (int i = 0; i < count; i++)
            {
                rolls.Add(_random.Next(1, sides + 1));
            }

            int total = rolls.Sum();

            return new DiceRollData
            {
                Rolls = rolls,
                Total = total
            };
        }

        // Heavy method: generate image in background thread
        public Image<Rgba32> GenerateDiceImage(List<int> rolls)
        {
            int size = 48;       // smaller dice for faster generation
            int spacing = 0;

            int width = rolls.Count * (size + spacing) - spacing;
            int height = size;

            var image = new Image<Rgba32>(width, height);

            image.Mutate(ctx =>
            {
                for (int i = 0; i < rolls.Count; i++)
                {
                    int xOffset = i * (size + spacing);

                    ctx.Fill(SixLabors.ImageSharp.Color.White, new RectangleF(xOffset, 0, size, size));
                    ctx.Draw(SixLabors.ImageSharp.Color.Black, 2, new RectangleF(xOffset, 0, size, size));
                    DrawDicePips(ctx, rolls[i], xOffset, 0, size);
                }
            });

            return image;
        }

        private void DrawDicePips(IImageProcessingContext ctx, int number, float x, float y, float size)
        {
            float r = size * 0.1f;
            float mid = x + size / 2;
            float top = y + size * 0.25f;
            float bottom = y + size * 0.75f;
            float left = x + size * 0.25f;
            float right = x + size * 0.75f;

            void DrawPip(float cx, float cy) => ctx.Fill(SixLabors.ImageSharp.Color.Black, new EllipsePolygon(cx, cy, r));

            switch (number)
            {
                case 1: DrawPip(mid, (top + bottom) / 2); break;
                case 2: DrawPip(left, top); DrawPip(right, bottom); break;
                case 3: DrawPip(left, top); DrawPip(mid, (top + bottom) / 2); DrawPip(right, bottom); break;
                case 4: DrawPip(left, top); DrawPip(right, top); DrawPip(left, bottom); DrawPip(right, bottom); break;
                case 5: DrawPip(left, top); DrawPip(right, top); DrawPip(mid, (top + bottom) / 2); DrawPip(left, bottom); DrawPip(right, bottom); break;
                case 6: DrawPip(left, top); DrawPip(left, (top + bottom) / 2); DrawPip(left, bottom); DrawPip(right, top); DrawPip(right, (top + bottom) / 2); DrawPip(right, bottom); break;
            }
        }
    }

    // Lightweight data class
    public class DiceRollData
    {
        public List<int> Rolls { get; set; }
        public int Total { get; set; }
    }
}