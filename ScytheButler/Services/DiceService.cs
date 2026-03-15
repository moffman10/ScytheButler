using ScytheButler.Models;
using SixLabors.Fonts;
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

        public DiceRollResult RollDice(string diceInput)
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

            var image = GenerateDiceImage(rolls);

            return new DiceRollResult
            {
                Rolls = rolls,
                Total = total,
                Image = image
            };
        }

        public Image<Rgba32> GenerateDiceImage(List<int> rolls)
        {
            int size = 64;       // smaller dice
            int spacing = 0;     // no space between dice

            int width = rolls.Count * (size + spacing) - spacing;
            int height = size;

            var image = new Image<Rgba32>(width, height);

            image.Mutate(ctx =>
            {
                // No background fill, keeps transparency

                for (int i = 0; i < rolls.Count; i++)
                {
                    int xOffset = i * (size + spacing);

                    // Draw dice background as white with black border
                    ctx.Fill(Color.White, new RectangleF(xOffset, 0, size, size));
                    ctx.Draw(Color.Black, 3, new RectangleF(xOffset, 0, size, size));

                    // Draw pips
                    DrawDicePips(ctx, rolls[i], xOffset, 0, size);
                }
            });

            return image;
        }

        private void DrawDicePips(IImageProcessingContext ctx, int number, float x, float y, float size)
        {
            float r = size * 0.1f; // pip radius
            float mid = x + size / 2;
            float top = y + size * 0.25f;
            float bottom = y + size * 0.75f;
            float left = x + size * 0.25f;
            float right = x + size * 0.75f;

            void DrawPip(float cx, float cy) => ctx.Fill(Color.Black, new EllipsePolygon(cx, cy, r));

            switch (number)
            {
                case 1:
                    DrawPip(mid, (top + bottom) / 2);
                    break;
                case 2:
                    DrawPip(left, top);
                    DrawPip(right, bottom);
                    break;
                case 3:
                    DrawPip(left, top);
                    DrawPip(mid, (top + bottom) / 2);
                    DrawPip(right, bottom);
                    break;
                case 4:
                    DrawPip(left, top);
                    DrawPip(right, top);
                    DrawPip(left, bottom);
                    DrawPip(right, bottom);
                    break;
                case 5:
                    DrawPip(left, top);
                    DrawPip(right, top);
                    DrawPip(mid, (top + bottom) / 2);
                    DrawPip(left, bottom);
                    DrawPip(right, bottom);
                    break;
                case 6:
                    DrawPip(left, top);
                    DrawPip(left, (top + bottom) / 2);
                    DrawPip(left, bottom);
                    DrawPip(right, top);
                    DrawPip(right, (top + bottom) / 2);
                    DrawPip(right, bottom);
                    break;
            }
        }
    }
}