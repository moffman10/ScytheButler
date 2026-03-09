using System;
using System.Collections.Generic;
using System.Linq;

namespace ScytheButler.Commands
{
    public static class HopperService
    {
        public static HopperResult PickWeighted(List<HopperEntry> entries)
        {
            var weightedList = entries.SelectMany(e => Enumerable.Repeat(e.UserId, e.Weight)).ToList();
            var random = new Random();
            ulong picked = weightedList[random.Next(weightedList.Count)];

            return new HopperResult { PickedUserId = picked };
        }
    }
}