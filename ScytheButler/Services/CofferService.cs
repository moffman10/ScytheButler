using Discord;
using Microsoft.Extensions.FileSystemGlobbing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace ScytheButler.Services
{
    public class CofferService
    {
        private readonly string _filePath = Path.Combine(AppContext.BaseDirectory,"..", "..", "..", "Balance.json");

        private Dictionary<string, int> _cofferBalances;
        public CofferService()
        {
            _filePath = Path.GetFullPath(_filePath);
            LoadBalances();
        }

        public int ParseAmount(string input)
        {
            input = input.Trim().ToUpper();

            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("Amount cannot be empty.");

            double number;

            if (input.EndsWith("K"))
                number = double.Parse(input[..^1]) * 1_000;
            if (input.EndsWith("M"))
                number = double.Parse(input[..^1]) * 1_000_000;
            if (input.EndsWith("B"))
                number = double.Parse(input[..^1]) * 1_000_000;
            else
                number = double.Parse(input);

            return (int)number;
        }

        public void LoadBalances()
        {
            if (File.Exists(_filePath))
            {
                string json = File.ReadAllText(_filePath);

                var data = JsonSerializer.Deserialize<CofferData>(json);
                _cofferBalances = data?.Coffers ?? new Dictionary<string, int>();
            }
            else
            {
                _cofferBalances = new Dictionary<string, int>();
                SaveBalances();
            }
        }
        public void SaveBalances()
        {
            var data = new CofferData { Coffers = _cofferBalances };

            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true});
            File.WriteAllText(_filePath, json);
        }
        public int GetCofferBalance(string username)
        {
            return _cofferBalances.ContainsKey(username) ? _cofferBalances[username] : 0;
        }
        public int GetTotalBalance()
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
        public void AddToCoffer(string username, int amount)
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
        public bool RemoveFromCoffer(string username, int amount)
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
            public Dictionary<string, int> Coffers { get; set; } = new Dictionary<string, int>();
        }
    }
}
