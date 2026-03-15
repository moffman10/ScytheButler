using ScytheButler.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
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
            int size = 200;
            int spacing = 20;

            int width = rolls.Count * (size + spacing);
            int height = size;

            var image = new Image<Rgba32>(width, height);

            var font = SystemFonts.CreateFont("Arial", 80, FontStyle.Bold);

            image.Mutate(ctx =>
            {
                ctx.Fill(Color.DarkGreen);

                for (int i = 0; i < rolls.Count; i++)
                {
                    int x = i * (size + spacing);

                    ctx.Fill(Color.White, new RectangleF(x, 0, size, size));
                    ctx.Draw(Color.Black, 5, new RectangleF(x, 0, size, size));

                    string text = rolls[i].ToString();

                    var textSize = TextMeasurer.Measure(text, new RendererOptions(font));

                    float textX = x + (size - textSize.Width) / 2;
                    float textY = (size - textSize.Height) / 2;

                    ctx.DrawText(text, font, Color.Black, new PointF(textX, textY));
                }
            });

            return image;
        }
    }
}