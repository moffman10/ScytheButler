using ScytheButler.Data;
using ScytheButler.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace ScytheButler.Services
{
    public class CofferService
    {
        private readonly AppDbContext _db;
        public CofferService(AppDbContext db)
        {
            _db = db;
        }
        public long ParseAmount(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new FormatException("Amount cannot be empty.");

            input = input.Trim().ToUpper().Replace(",", "");
            long multiplier = 1;

            if (input.EndsWith("K")) { multiplier = 1_000; input = input[..^1]; }
            else if (input.EndsWith("M")) { multiplier = 1_000_000; input = input[..^1]; }
            else if (input.EndsWith("B")) { multiplier = 1_000_000_000; input = input[..^1]; }

            if (!double.TryParse(input, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double number))
                throw new FormatException("Invalid number format.");

            return (long)(number * multiplier);
        }
        public bool AddCoffer(string bank)
        {
            if (_db.Balances.Any(b => b.Bank == bank)) return false;
            _db.Balances.Add(new Balance { Bank = bank, Value = 0 });
            _db.SaveChanges();
            return true;
        }

        public bool RemoveCoffer(string bank)
        {
            var entry = _db.Balances.SingleOrDefault(b => b.Bank == bank);
            if (entry == null) return false;
            _db.Balances.Remove(entry);
            _db.SaveChanges();
            return true;
        }

        public double GetCofferBalance(string bank)
        {
            var entry = _db.Balances.SingleOrDefault(b => b.Bank == bank);
            return entry?.Value ?? 0;

        }

        public List<string> GetAllCoffers()
        {
            return _db.Balances.Select(b => b.Bank).ToList();
        }

        public double GetTotalBalance()
        {
            return _db.Balances.Sum(b => b.Value);
        }
        public Dictionary<string, double> GetAllBalances()
        {
            return _db.Balances.ToDictionary(b => b.Bank, b => b.Value);
        }
        public bool AddToCoffer(string bank, double amount)
        {
            var entry = _db.Balances.SingleOrDefault(b => b.Bank == bank);
            if (entry == null) return false;

            entry.Value += amount;
            _db.SaveChanges();
            return true;
        }
        public async Task AddTransactionAsync(string username, double amount, string type, string reason, string bank)
        {
            var transaction = new Transaction
            {
                Username = username,
                Amount = amount,
                Type = type,
                Reason = reason,
                Bank = bank,
                Date = DateTime.Now
            };

            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync();
        }
        public bool RemoveFromCoffer(string bank, double amount)
        {
            var entry = _db.Balances.SingleOrDefault(b => b.Bank == bank);
            if (entry == null || entry.Value < amount) return false;

            entry.Value -= amount;
            _db.SaveChanges();
            return true;
        }
        public List<string> GetAllReasons()
        {
            var path = Path.Combine("Data", "reasons.txt");

            if (!File.Exists(path))
                return new List<string>();

            return File.ReadAllLines(path)
                       .Where(x => !string.IsNullOrWhiteSpace(x))
                       .Select(x => x.Trim())
                       .ToList();
        }
    }
}
