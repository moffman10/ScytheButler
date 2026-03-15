using System;
using System.Collections.Generic;
using System.Text;

namespace ScytheButler.Models
{
    public class CompetitionResult
    {
        public bool Success { get; set; }
        public string Title { get; set; }
        public string Metric { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string[] Participants { get; set; }
        public Team[] Teams { get; set; }
        public string ApiResponse { get; set; }
        public string Error { get; set; }
    }
}
