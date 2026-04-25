using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuickRegister.Models;

namespace QuickRegister.Data;

public class CsvDiff
{
    public List<Machine> MachinesToAdd { get; } = new();
    public List<Machine> MachinesToRemove { get; } = new();
    public List<Medewerker> MedewerkersToAdd { get; } = new();
    public List<Medewerker> MedewerkersToRemove { get; } = new();
    public List<Bedrijf> BedrijvenToAdd { get; } = new();
    public List<Bedrijf> BedrijvenToRemove { get; } = new();
    public List<Bedrijf> BedrijvenToUpdate { get; } = new();

    public bool HasMachineChanges => MachinesToAdd.Count > 0 || MachinesToRemove.Count > 0;
    public bool HasMedewerkerChanges => MedewerkersToAdd.Count > 0 || MedewerkersToRemove.Count > 0;
    public bool HasBedrijfChanges => BedrijvenToAdd.Count > 0 || BedrijvenToRemove.Count > 0 || BedrijvenToUpdate.Count > 0;

    public bool HasChanges => HasMachineChanges || HasMedewerkerChanges || HasBedrijfChanges;

    public int TotalChanges =>
        MachinesToAdd.Count + MachinesToRemove.Count +
        MedewerkersToAdd.Count + MedewerkersToRemove.Count +
        BedrijvenToAdd.Count + BedrijvenToRemove.Count + BedrijvenToUpdate.Count;
}

public static class CsvDiffChecker
{
    public static CsvDiff CheckDiff(AppDbContext db)
    {
        var diff = new CsvDiff();
        CheckMachines(db, diff);
        CheckMedewerkers(db, diff);
        CheckBedrijven(db, diff);
        return diff;
    }

    public static void ApplyDiff(AppDbContext db, CsvDiff diff,
        bool applyMachines = true, bool applyMedewerkers = true, bool applyBedrijven = true)
    {
        if (applyMachines)
        {
            if (diff.MachinesToAdd.Count > 0)
                db.Machines.AddRange(diff.MachinesToAdd);
            if (diff.MachinesToRemove.Count > 0)
                db.Machines.RemoveRange(diff.MachinesToRemove);
        }

        if (applyMedewerkers)
        {
            if (diff.MedewerkersToAdd.Count > 0)
                db.Medewerkers.AddRange(diff.MedewerkersToAdd);
            if (diff.MedewerkersToRemove.Count > 0)
                db.Medewerkers.RemoveRange(diff.MedewerkersToRemove);
        }

        if (applyBedrijven)
        {
            if (diff.BedrijvenToAdd.Count > 0)
                db.Bedrijven.AddRange(diff.BedrijvenToAdd);
            if (diff.BedrijvenToRemove.Count > 0)
                db.Bedrijven.RemoveRange(diff.BedrijvenToRemove);

            foreach (var updated in diff.BedrijvenToUpdate)
            {
                var existing = db.Bedrijven.Local.FirstOrDefault(b => b.klantId == updated.klantId)
                               ?? db.Bedrijven.FirstOrDefault(b => b.klantId == updated.klantId);
                if (existing == null) continue;
                existing.BedrijfNaam = updated.BedrijfNaam;
                existing.StraatNaam = updated.StraatNaam;
                existing.AdresNummer = updated.AdresNummer;
                existing.Postcode = updated.Postcode;
                existing.Stad = updated.Stad;
                existing.Land = updated.Land;
            }
        }

        db.SaveChanges();
    }

    private static void CheckMachines(AppDbContext db, CsvDiff diff)
    {
        var csvPath = GetCsvPath("MachineLijst.csv");
        if (!File.Exists(csvPath)) return;

        var csvNames = ReadMachinesFromCsv(csvPath);
        var dbMachines = db.Machines.ToList();
        var dbNames = dbMachines.Select(m => m.MachineNaam).ToHashSet();

        foreach (var name in csvNames.Where(n => !dbNames.Contains(n)))
            diff.MachinesToAdd.Add(new Machine { MachineNaam = name });

        foreach (var m in dbMachines.Where(m => !csvNames.Contains(m.MachineNaam)))
            diff.MachinesToRemove.Add(m);
    }

    private static void CheckMedewerkers(AppDbContext db, CsvDiff diff)
    {
        var csvPath = GetCsvPath("MedewerkerLijst.csv");
        if (!File.Exists(csvPath)) return;

        var csvNames = ReadMedewerkersFromCsv(csvPath);
        var dbMedewerkers = db.Medewerkers.ToList();
        var dbNames = dbMedewerkers.Select(m => m.Naam).ToHashSet();

        foreach (var name in csvNames.Where(n => !dbNames.Contains(n)))
            diff.MedewerkersToAdd.Add(new Medewerker { Naam = name });

        // Skip medewerkers who are referenced by an InterventieCall (FK constraint)
        var referencedIds = db.InterventieCalls.Select(c => c.MedewerkerId).Distinct().ToHashSet();
        foreach (var m in dbMedewerkers.Where(m => !csvNames.Contains(m.Naam) && !referencedIds.Contains(m.Id)))
            diff.MedewerkersToRemove.Add(m);
    }

    private static void CheckBedrijven(AppDbContext db, CsvDiff diff)
    {
        var csvPath = GetCsvPath("BedrijvenLijst.csv");
        if (!File.Exists(csvPath)) return;

        var csvBedrijven = ReadBedrijvenFromCsv(csvPath);
        var dbBedrijven = db.Bedrijven.ToList();
        var dbById = dbBedrijven.ToDictionary(b => b.klantId);
        var csvIds = csvBedrijven.Select(b => b.klantId).ToHashSet();

        foreach (var b in csvBedrijven.Where(b => !dbById.ContainsKey(b.klantId)))
            diff.BedrijvenToAdd.Add(b);

        foreach (var b in dbBedrijven.Where(b => !csvIds.Contains(b.klantId)))
            diff.BedrijvenToRemove.Add(b);

        foreach (var csvB in csvBedrijven.Where(b => dbById.ContainsKey(b.klantId)))
        {
            var dbB = dbById[csvB.klantId];
            if (dbB.BedrijfNaam != csvB.BedrijfNaam ||
                dbB.StraatNaam != csvB.StraatNaam ||
                dbB.AdresNummer != csvB.AdresNummer ||
                dbB.Postcode != csvB.Postcode ||
                dbB.Stad != csvB.Stad ||
                dbB.Land != csvB.Land)
            {
                diff.BedrijvenToUpdate.Add(csvB);
            }
        }
    }

    private static string GetCsvPath(string fileName)
        => Path.Combine(AppContext.BaseDirectory, "Data", fileName);

    private static HashSet<string> ReadMachinesFromCsv(string path)
    {
        var result = new HashSet<string>();
        var lines = File.ReadAllLines(path);
        int start = lines.Length > 0 && lines[0].Trim().Equals("MachineNaam", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        for (int i = start; i < lines.Length; i++)
        {
            var name = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(name)) continue;
            if (name.StartsWith("XXX", StringComparison.OrdinalIgnoreCase)) continue;
            if (name.StartsWith("TEST ", StringComparison.OrdinalIgnoreCase)) continue;
            result.Add(name);
        }
        return result;
    }

    private static HashSet<string> ReadMedewerkersFromCsv(string path)
    {
        var result = new HashSet<string>();
        var lines = File.ReadAllLines(path);
        int start = lines.Length > 0 && lines[0].Trim().Equals("Naam", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        for (int i = start; i < lines.Length; i++)
        {
            var name = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(name)) continue;
            result.Add(name);
        }
        return result;
    }

    private static List<Bedrijf> ReadBedrijvenFromCsv(string path)
    {
        var result = new List<Bedrijf>();
        var seenIds = new HashSet<int>();
        var lines = File.ReadAllLines(path);
        int start = lines.Length > 0 && !int.TryParse(SplitCsvLine(lines[0])[0], out _) ? 1 : 0;

        for (int i = start; i < lines.Length; i++)
        {
            try
            {
                var parts = SplitCsvLine(lines[i]);
                if (parts.Length < 7) continue;
                if (!int.TryParse(parts[0], out var klantId)) continue;
                var naam = parts[1];
                if (naam.StartsWith("XXX", StringComparison.OrdinalIgnoreCase)) continue;
                if (naam.StartsWith("TEST ", StringComparison.OrdinalIgnoreCase)) continue;
                if (seenIds.Contains(klantId)) continue;
                seenIds.Add(klantId);
                result.Add(new Bedrijf
                {
                    klantId = klantId,
                    BedrijfNaam = naam,
                    StraatNaam = string.IsNullOrWhiteSpace(parts[2]) ? null : parts[2],
                    AdresNummer = string.IsNullOrWhiteSpace(parts[3]) ? null : parts[3],
                    Postcode = string.IsNullOrWhiteSpace(parts[4]) ? null : parts[4],
                    Stad = string.IsNullOrWhiteSpace(parts[5]) ? null : parts[5],
                    Land = string.IsNullOrWhiteSpace(parts[6]) ? null : parts[6]
                });
            }
            catch { }
        }
        return result;
    }

    private static string[] SplitCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var current = "";
        foreach (char c in line)
        {
            if (c == '"') { inQuotes = !inQuotes; continue; }
            if (c == ',' && !inQuotes) { result.Add(current.Trim()); current = ""; }
            else current += c;
        }
        result.Add(current.Trim());
        return result.ToArray();
    }
}
