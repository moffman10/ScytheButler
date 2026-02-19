using System;
using System.Collections.Generic;
using System.Text;

namespace ScytheButler.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public double Amount { get; set; }
        public string Type {  get; set; }
        public string Reason { get; set; }
        public string Bank { get; set; }
        public DateTime Date { get; set; }
    }
}
