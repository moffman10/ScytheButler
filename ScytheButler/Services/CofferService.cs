using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace ScytheButler.Services
{
    public class CofferService
    {
        private readonly string _filePath;

        private Dictionary<string, long> _coffers;

        public CofferService(string filePath = "Balance.json")
        {
            _filePath = filePath;
            LoadCoffers();
        }

        private void LoadCoffers()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _coffers = JsonSerializer.Deserialize<Dictionary<string, long>>(json)
                           ?? new Dictionary<string, long>();
            }
            else
            {
                _coffers = new Dictionary<string, long>();
                SaveCoffers();
            }
        }

        private void SaveCoffers()
        {
            var json = JsonSerializer.Serialize(_coffers, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
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
        public bool AddCoffer(string username)
        {
            if (_coffers.ContainsKey(username)) return false;
            _coffers[username] = 0;
            SaveCoffers();
            return true;
        }

        public bool RemoveCoffer(string username)
        {
            if (!_coffers.ContainsKey(username)) return false;
            _coffers.Remove(username);
            SaveCoffers();
            return true;
        }

        public long GetCofferBalance(string username)
        {
            return _coffers.TryGetValue(username, out var balance) ? balance : 0;
        }

        public List<string> GetAllCoffers()
        {
            return new List<string>(_coffers.Keys);
        }

        public long GetTotalBalance()
        {
            long total = 0;
            foreach (var balance in _coffers.Values)
                total += balance;
            return total;
        }
        public Dictionary<string, long> GetAllBalances()
        {
            return new Dictionary<string, long>(_coffers);
        }
        public void AddToCoffer(string username, long amount)
        {
            if (!_coffers.ContainsKey(username)) AddCoffer(username);
            _coffers[username] += amount;
            SaveCoffers();
        }

        public bool RemoveFromCoffer(string username, long amount)
        {
            if (!_coffers.TryGetValue(username, out var currentBalance)) return false;
            if (currentBalance < amount) return false;

            _coffers[username] -= amount;
            SaveCoffers();
            return true;
        }
    }
}
