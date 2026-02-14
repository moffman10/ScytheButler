using Discord;
using Microsoft.Extensions.FileSystemGlobbing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace ScytheButler.Services
{
    public class CofferService
    {
        private readonly string _filePath = Path.Combine(AppContext.BaseDirectory,"..", "..", "..", "Balance.json");

        private Dictionary<string, long> _cofferBalances;
        public CofferService()
        {
            _filePath = Path.GetFullPath(_filePath);
            LoadBalances();
        }
    public long ParseAmount(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new FormatException("Amount cannot be empty.");

        input = input.Trim().ToUpper().Replace(",", "");

        long multiplier = 1;

        if (input.EndsWith("K"))
        {
            multiplier = 1_000;
            input = input.Substring(0, input.Length - 1);
        }
        else if (input.EndsWith("M"))
        {
            multiplier = 1_000_000;
            input = input.Substring(0, input.Length - 1);
        }
        else if (input.EndsWith("B"))
        {
            multiplier = 1_000_000_000;
            input = input.Substring(0, input.Length - 1);
        }

        if (!double.TryParse(input, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double number))
            throw new FormatException("Invalid number format.");

        return (long)(number * multiplier);
    }


    public void LoadBalances()
        {
            if (File.Exists(_filePath))
            {
                string json = File.ReadAllText(_filePath);

                var data = JsonSerializer.Deserialize<CofferData>(json);
                _cofferBalances = data?.Coffers ?? new Dictionary<string, long>();
            }
            else
            {
                _cofferBalances = new Dictionary<string, long>();
                SaveBalances();
            }
        }
        public void SaveBalances()
        {
            var data = new CofferData { Coffers = _cofferBalances };

            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true});
            File.WriteAllText(_filePath, json);
        }
        public long GetCofferBalance(string username)
        {
            return _cofferBalances.ContainsKey(username) ? _cofferBalances[username] : 0;
        }
        public long GetTotalBalance()
        {
            return _cofferBalances.Values.Sum();
        }
        public bool AddCoffer(string username)
        {
            if (_cofferBalances.ContainsKey(username)) return false;
            _cofferBalances[username] = 0;
            SaveBalances();
            return true;
        }
        public void AddToCoffer(string username, long amount)
        {
            if (_cofferBalances.ContainsKey(username)) _cofferBalances[username] += amount; 
            else _cofferBalances[username] = amount;

            SaveBalances();
        }
        public bool RemoveCoffer(string username)
        {
            if (!_cofferBalances.ContainsKey(username)) return false;
            _cofferBalances.Remove(username);
            SaveBalances();
            return true;
        }
        public bool RemoveFromCoffer(string username, long amount)
        {
            if (!_cofferBalances.ContainsKey(username)) return false;
            if (_cofferBalances[username] < amount) return false;

            _cofferBalances[username] -= amount;
            SaveBalances();
            return true;
        }
        public IEnumerable<string> GetAllCoffers() => _cofferBalances.Keys;
        private class CofferData
        {
            public Dictionary<string, long> Coffers { get; set; } = new Dictionary<string, long>();
        }
    }
}
