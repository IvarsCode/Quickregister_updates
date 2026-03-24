using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using QuickRegister.Data;
using QuickRegister.Models;

class MedewerkersLaden
{
    public static void LoadMedewerkerCsvToDb(AppDbContext db)
    {
        try
        {
            var csvPath = Path.Combine(AppContext.BaseDirectory, "Data", "medewerkerLijst.csv");

            if (!File.Exists(csvPath))
            {
                Console.WriteLine($"CSV file not found at: {csvPath}");
                return;
            }

            var lines = File.ReadAllLines(csvPath);
            var medewerkers = new List<Medewerker>();

            int startIndex = 0;
            if (lines.Length > 0 && lines[0].Trim().Equals("Naam", StringComparison.OrdinalIgnoreCase))
                startIndex = 1;

            for (int i = startIndex; i < lines.Length; i++)
            {
                try
                {
                    var parts = SplitCsvLine(lines[i]);

                    if (parts.Length < 1 || string.IsNullOrWhiteSpace(parts[0]))
                    {
                        Console.WriteLine($"Skipping line {i + 1}: empty or insufficient columns");
                        continue;
                    }

                    var naam = parts[0];

                    if (db.Medewerkers.Any(m => m.Naam == naam))
                    {
                        Console.WriteLine($"Skipping duplicate Naam (already in DB): {naam}");
                        continue;
                    }

                    if (medewerkers.Any(m => m.Naam == naam))
                    {
                        Console.WriteLine($"Skipping duplicate Naam (duplicate in CSV): {naam}");
                        continue;
                    }

                    medewerkers.Add(new Medewerker { Naam = naam });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing line {i + 1}: {ex.Message}");
                }
            }

            if (medewerkers.Count > 0)
            {
                db.Medewerkers.AddRange(medewerkers);
                db.SaveChanges();
                Console.WriteLine($"{medewerkers.Count} medewerkers added to the database.");
            }
            else
            {
                Console.WriteLine("No medewerkers to add from CSV.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading medewerkers: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    static string[] SplitCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var current = "";

        foreach (char c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (c == ',' && !inQuotes)
            {
                result.Add(current.Trim());
                current = "";
            }
            else current += c;
        }

        result.Add(current.Trim());
        return result.ToArray();
    }
}
