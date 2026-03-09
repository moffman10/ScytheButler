using System;
using System.Collections.Generic;
using System.Linq;

namespace ScytheButler.Commands
{
    public static class HopperHelper
    {
        public static List<HopperEntry> ParseEntries(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new List<HopperEntry>();

            var entries = input.Split(',')
                               .Select(e => e.Trim())
                               .Select(e =>
                               {
                                   var parts = e.Split(' ');
                                   if (parts.Length != 2) return null;

                                   string userString = parts[0];
                                   if (!int.TryParse(parts[1], out int weight) || weight <= 0) return null;

                                   ulong userId = 0;
                                   if (userString.StartsWith("<@") && userString.EndsWith(">"))
                                   {
                                       var idString = userString.Replace("<@", "").Replace("!", "").Replace(">", "");
                                       if (!ulong.TryParse(idString, out userId))
                                           return null;
                                   }
                                   else return null;

                                   return new HopperEntry { UserId = userId, Weight = weight };
                               })
                               .Where(x => x != null)
                               .ToList();

            return entries;
        }
    }
}