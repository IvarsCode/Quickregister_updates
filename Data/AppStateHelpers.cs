using QuickRegister.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace QuickRegister.Data
{
    public class AppStateHelpers
    {
        private readonly AppDbContext _db;

        public AppStateHelpers(AppDbContext db)
        {
            _db = db;
        }

        public void EnsureUniekeTellingExists()
        {
            const string key = "UniekeTelling";

            var state = _db.AppState.FirstOrDefault(s => s.Key == key);

            if (state == null)
            {
                var value = new Random().Next(10, 100);

                _db.AppState.Add(new AppState
                {
                    Key = key,
                    Value = value
                });

                _db.SaveChanges();
            }
        }

        public int GetNextPrefixedId(string tableName)
        {
            EnsureUniekeTellingExists();

            var prefix = _db.AppState
                .Where(s => s.Key == "UniekeTelling")
                .Select(s => s.Value)
                .First();

            int nextIncrement = 1;

            int? maxId = tableName switch
            {
                "interventies" => _db.Interventies.Any() ? _db.Interventies.Max(i => i.Id) : null,
                "interventie_call" => _db.InterventieCalls.Any() ? _db.InterventieCalls.Max(c => c.Id) : null,
                _ => throw new ArgumentException($"Unknown table: {tableName}", nameof(tableName))
            };

            if (maxId.HasValue)
                nextIncrement = (maxId.Value % 100000) + 1;

            return (prefix * 100000) + nextIncrement;
        }

        private bool IsValidTableName(string tableName)
        {
            // Only allow alphanumeric and underscores
            return !string.IsNullOrWhiteSpace(tableName) &&
                   tableName.All(c => char.IsLetterOrDigit(c) || c == '_');
        }
    }
}
