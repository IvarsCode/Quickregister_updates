using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using QuickRegister.Data;
using QuickRegister.Models;

class MachineLaden
{
    public static void LoadMachineCsvToDb(AppDbContext db)
    {
        try
        {
            var csvPath = Path.Combine(AppContext.BaseDirectory, "Data", "MachineLijst.csv");

            if (!File.Exists(csvPath))
            {
                Console.WriteLine($"CSV file not found at: {csvPath}");
                return;
            }

            var lines = File.ReadAllLines(csvPath);
            var machines = new List<Machine>();

            int startIndex = 0;
            if (lines.Length > 0 && lines[0].Trim().Equals("MachineNaam", StringComparison.OrdinalIgnoreCase))
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

                    var machineNaam = parts[0];

                    if (machineNaam.StartsWith("XXX", StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (machineNaam.StartsWith("TEST ", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (db.Machines.Any(m => m.MachineNaam == machineNaam))
                    {
                        Console.WriteLine($"Skipping duplicate MachineNaam (already in DB): {machineNaam}");
                        continue;
                    }

                    if (machines.Any(m => m.MachineNaam == machineNaam))
                    {
                        Console.WriteLine($"Skipping duplicate MachineNaam (duplicate in CSV): {machineNaam}");
                        continue;
                    }

                    machines.Add(new Machine { MachineNaam = machineNaam });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing line {i + 1}: {ex.Message}");
                }
            }

            if (machines.Count > 0)
            {
                db.Machines.AddRange(machines);
                db.SaveChanges();
                Console.WriteLine($"{machines.Count} machines added to the database.");
            }
            else
            {
                Console.WriteLine("No machines to add from CSV.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading machines: {ex.Message}");
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
