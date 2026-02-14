using Npgsql;
using System;
using System.Globalization;

namespace ScytheButler.Services
{
    public class CofferService
    {
        private readonly string _connectionString;

        public CofferService()
        {
            _connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
                                ?? throw new Exception("DATABASE_URL not found.");
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS coffer (
                    username TEXT PRIMARY KEY,
                    balance BIGINT NOT NULL
                );", conn);

            cmd.ExecuteNonQuery();
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
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(
                "INSERT INTO coffer (username, balance) VALUES (@username, 0) ON CONFLICT DO NOTHING;", conn);
            cmd.Parameters.AddWithValue("username", username);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool RemoveCoffer(string username)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand("DELETE FROM coffer WHERE username = @username;", conn);
            cmd.Parameters.AddWithValue("username", username);

            return cmd.ExecuteNonQuery() > 0;
        }

        public long GetCofferBalance(string username)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand("SELECT balance FROM coffer WHERE username = @username;", conn);
            cmd.Parameters.AddWithValue("username", username);

            var result = cmd.ExecuteScalar();
            return result == null ? 0 : (long)result;
        }

        public long GetTotalBalance()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand("SELECT SUM(balance) FROM coffer;", conn);
            var result = cmd.ExecuteScalar();
            return result == DBNull.Value ? 0 : (long)result;
        }

        public void AddToCoffer(string username, long amount)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(
                "UPDATE coffer SET balance = balance + @amount WHERE username = @username;", conn);
            cmd.Parameters.AddWithValue("amount", amount);
            cmd.Parameters.AddWithValue("username", username);

            cmd.ExecuteNonQuery();
        }

        public bool RemoveFromCoffer(string username, long amount)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var checkCmd = new NpgsqlCommand(
                "SELECT balance FROM coffer WHERE username = @username;", conn);
            checkCmd.Parameters.AddWithValue("username", username);

            var currentBalance = (long?)(checkCmd.ExecuteScalar() ?? 0);

            if (currentBalance < amount) return false;

            var cmd = new NpgsqlCommand(
                "UPDATE coffer SET balance = balance - @amount WHERE username = @username;", conn);
            cmd.Parameters.AddWithValue("amount", amount);
            cmd.Parameters.AddWithValue("username", username);

            cmd.ExecuteNonQuery();
            return true;
        }
    }
}
