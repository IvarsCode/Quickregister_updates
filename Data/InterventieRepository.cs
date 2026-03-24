using System;
using System.Collections.Generic;
using System.Linq;
using QuickRegister.Models;
using Microsoft.EntityFrameworkCore;

namespace QuickRegister.Data
{
    public enum InterventieFilterType
    {
        Bedrijfsnaam,
        Machine,
        Datum
    }

    public static class InterventieRepository
    {
        public static List<Interventie> GetAll(AppDbContext db)
        {
            try
            {
                var interventies = db.Interventies
                    .Include(i => i.Calls)
                    .Where(i => i.Afgerond == 0)
                    .ToList();

                return interventies
                    .OrderByDescending(GetMostRecentCallDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetAll Interventies] Exception: {ex}");
                return new List<Interventie>();
            }
        }

        public static Interventie? GetById(AppDbContext db, int id)
        {
            try
            {
                return db.Interventies
                    .Include(i => i.Calls)
                    .FirstOrDefault(i => i.Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetInterventieById] Exception: {ex}");
                return null;
            }
        }

        public static void Add(AppDbContext db, Interventie interventie)
        {
            try
            {
                // Auto-fill address fields from existing Bedrijf record (from Doc 1)
                var bedrijf = db.Bedrijven
                    .FirstOrDefault(b => b.BedrijfNaam == interventie.BedrijfNaam);

                if (bedrijf != null)
                {
                    interventie.BedrijfNaam = bedrijf.BedrijfNaam;
                    interventie.StraatNaam = bedrijf.StraatNaam;
                    interventie.AdresNummer = bedrijf.AdresNummer;
                    interventie.Postcode = bedrijf.Postcode;
                    interventie.Stad = bedrijf.Stad;
                    interventie.Land = bedrijf.Land;
                }

                db.Interventies.Add(interventie);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Add Interventie] Exception: {ex}");
            }
        }

        public static void Update(AppDbContext db, Interventie interventie)
        {
            try
            {
                db.Interventies.Update(interventie);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Update Interventie] Exception: {ex}");
            }
        }

        public static List<Interventie> GetFiltered(
            AppDbContext db,
            InterventieFilterType filter,
            string? searchText,
            DateTimeOffset? fromDate,
            DateTimeOffset? toDate)
        {
            try
            {
                var query = db.Interventies
                    .Include(i => i.Calls)
                    .Where(i => i.Afgerond == 0)
                    .AsQueryable();

                switch (filter)
                {
                    case InterventieFilterType.Bedrijfsnaam:
                        if (!string.IsNullOrWhiteSpace(searchText))
                            query = query.Where(i =>
                                EF.Functions.Like(i.BedrijfNaam, $"%{searchText}%"));
                        break;

                    case InterventieFilterType.Machine:
                        if (!string.IsNullOrWhiteSpace(searchText))
                            query = query.Where(i =>
                                EF.Functions.Like(i.Machine, $"%{searchText}%"));
                        break;

                    case InterventieFilterType.Datum:
                        var allForDateFilter = query.ToList();

                        if (fromDate.HasValue || toDate.HasValue)
                        {
                            allForDateFilter = allForDateFilter.Where(i =>
                                i.Calls.Any(call =>
                                    (!fromDate.HasValue ||
                                     (call.StartCall.HasValue &&
                                      call.StartCall.Value >= fromDate.Value.DateTime)) &&
                                    (!toDate.HasValue ||
                                     (call.EindCall.HasValue &&
                                      call.EindCall.Value <= toDate.Value.DateTime))
                                )).ToList();
                        }

                        return allForDateFilter
                            .OrderByDescending(GetMostRecentCallDate)
                            .ToList();
                }

                return query
                    .ToList()
                    .OrderByDescending(GetMostRecentCallDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetFiltered Interventies] Exception: {ex}");
                return new List<Interventie>();
            }
        }

        public static List<Interventie> GetAllArchived(AppDbContext db)
        {
            try
            {
                var interventies = db.Interventies
                    .Include(i => i.Calls)
                    .Where(i => i.Afgerond == 1)
                    .ToList();

                return interventies
                    .OrderByDescending(GetMostRecentCallDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetAllArchived Interventies] Exception: {ex}");
                return new List<Interventie>();
            }
        }

        public static List<Interventie> GetFilteredArchived(
            AppDbContext db,
            InterventieFilterType filter,
            string? searchText,
            DateTimeOffset? fromDate,
            DateTimeOffset? toDate)
        {
            try
            {
                var query = db.Interventies
                    .Include(i => i.Calls)
                    .Where(i => i.Afgerond == 1)
                    .AsQueryable();

                switch (filter)
                {
                    case InterventieFilterType.Bedrijfsnaam:
                        if (!string.IsNullOrWhiteSpace(searchText))
                            query = query.Where(i =>
                                EF.Functions.Like(i.BedrijfNaam, $"%{searchText}%"));
                        break;

                    case InterventieFilterType.Machine:
                        if (!string.IsNullOrWhiteSpace(searchText))
                            query = query.Where(i =>
                                EF.Functions.Like(i.Machine, $"%{searchText}%"));
                        break;

                    case InterventieFilterType.Datum:
                        var allForDateFilter = query.ToList();

                        if (fromDate.HasValue || toDate.HasValue)
                        {
                            allForDateFilter = allForDateFilter.Where(i =>
                                i.Calls.Any(call =>
                                    (!fromDate.HasValue ||
                                     (call.StartCall.HasValue &&
                                      call.StartCall.Value >= fromDate.Value.DateTime)) &&
                                    (!toDate.HasValue ||
                                     (call.EindCall.HasValue &&
                                      call.EindCall.Value <= toDate.Value.DateTime))
                                )).ToList();
                        }

                        return allForDateFilter
                            .OrderByDescending(GetMostRecentCallDate)
                            .ToList();
                }

                return query
                    .ToList()
                    .OrderByDescending(GetMostRecentCallDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetFilteredArchived Interventies] Exception: {ex}");
                return new List<Interventie>();
            }
        }

        private static DateTime GetMostRecentCallDate(Interventie interventie)
        {
            var call = interventie.Calls
                .Where(c => c.StartCall.HasValue)
                .OrderByDescending(c => c.StartCall)
                .FirstOrDefault();

            return call?.StartCall ?? DateTime.MinValue;
        }
    }
}
