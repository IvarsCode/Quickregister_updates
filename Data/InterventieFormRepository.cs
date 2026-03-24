using System;
using System.Linq;
using QuickRegister.Models;
using QuickRegister.Data;
using Microsoft.EntityFrameworkCore;

namespace QuickRegister.Data
{
    public static class InterventieFormRepository
    {
        /// Saves or updates an Interventie and creates a new InterventieCall record
        public static void Save(
            AppDbContext db,
            Interventie? existing,
            string bedrijfsnaam,
            string machine,
            int klantId,
            string? straatNaam,
            string? adresNummer,
            string? postcode,
            string? stad,
            string? land,
            int medewerkerId,
            string contactpersoonNaam,
            string? contactpersoonEmail,
            string? contactpersoonTelefoon,
            string? interneNotities,
            string? externeNotities,
            DateTime? callStartTime,
            DateTime? callEndTime)
        {
            // Round start down to the minute, round end up to the next minute
            if (callStartTime.HasValue)
                callStartTime = new DateTime(
                    callStartTime.Value.Year, callStartTime.Value.Month, callStartTime.Value.Day,
                    callStartTime.Value.Hour, callStartTime.Value.Minute, 0);

            if (callEndTime.HasValue)
                callEndTime = callEndTime.Value.Second > 0 || callEndTime.Value.Millisecond > 0
                    ? new DateTime(
                        callEndTime.Value.Year, callEndTime.Value.Month, callEndTime.Value.Day,
                        callEndTime.Value.Hour, callEndTime.Value.Minute, 0).AddMinutes(1)
                    : new DateTime(
                        callEndTime.Value.Year, callEndTime.Value.Month, callEndTime.Value.Day,
                        callEndTime.Value.Hour, callEndTime.Value.Minute, 0);

            var helpers = new AppStateHelpers(db);
            int callDurationSeconds = 0;

            if (callStartTime.HasValue && callEndTime.HasValue)
            {
                if (callEndTime < callStartTime)
                    throw new ArgumentException("callEndTime cannot be before callStartTime");
                callDurationSeconds = (int)(callEndTime.Value - callStartTime.Value).TotalSeconds;
            }

            using var transaction = db.Database.BeginTransaction();

            if (existing != null)
            {
                var interventie = db.Interventies.FirstOrDefault(i => i.Id == existing.Id)
                    ?? throw new Exception($"Interventie with ID {existing.Id} not found");

                // Update fields
                interventie.Machine = machine;
                interventie.BedrijfNaam = bedrijfsnaam;
                interventie.KlantId = klantId;

                // Only update address if new values are provided
                if (straatNaam != null) interventie.StraatNaam = straatNaam;
                if (adresNummer != null) interventie.AdresNummer = adresNummer;
                if (postcode != null) interventie.Postcode = postcode;
                if (stad != null) interventie.Stad = stad;
                if (land != null) interventie.Land = land;

                // Create new call record for this session
                var newCall = new InterventieCall
                {
                    Id = helpers.GetNextPrefixedId("interventie_call"),
                    InterventieId = interventie.Id,
                    MedewerkerId = medewerkerId,
                    ContactpersoonNaam = contactpersoonNaam,
                    ContactpersoonEmail = contactpersoonEmail,
                    ContactpersoonTelefoonNummer = contactpersoonTelefoon,
                    InterneNotities = interneNotities,
                    ExterneNotities = externeNotities,
                    StartCall = callStartTime,
                    EindCall = callEndTime
                };
                db.InterventieCalls.Add(newCall);

                // Update TotaleLooptijd and most recent call
                interventie.TotaleLooptijd += callDurationSeconds;
                interventie.IdRecentsteCall = newCall.Id;

                db.SaveChanges();
            }
            else
            {
                // Create new intervention
                var interventie = new Interventie
                {
                    Id = helpers.GetNextPrefixedId("interventies"),
                    Machine = machine,
                    BedrijfNaam = bedrijfsnaam,
                    KlantId = klantId,
                    StraatNaam = straatNaam,
                    AdresNummer = adresNummer,
                    Postcode = postcode,
                    Stad = stad,
                    Land = land,
                    TotaleLooptijd = callDurationSeconds,
                    Afgerond = 0,
                    IdRecentsteCall = 0
                };
                db.Interventies.Add(interventie);
                db.SaveChanges();

                // Create first call
                var newCall = new InterventieCall
                {
                    Id = helpers.GetNextPrefixedId("interventie_call"),
                    InterventieId = interventie.Id,
                    MedewerkerId = medewerkerId,
                    ContactpersoonNaam = contactpersoonNaam,
                    ContactpersoonEmail = contactpersoonEmail,
                    ContactpersoonTelefoonNummer = contactpersoonTelefoon,
                    InterneNotities = interneNotities,
                    ExterneNotities = externeNotities,
                    StartCall = callStartTime,
                    EindCall = callEndTime
                };
                db.InterventieCalls.Add(newCall);

                // Link to recent call
                interventie.IdRecentsteCall = newCall.Id;
                db.SaveChanges();
            }

            transaction.Commit();
        }

        public static void UpdateCall(
            AppDbContext db,
            int callId,
            string contactpersoonNaam,
            string? contactpersoonEmail,
            string? contactpersoonTelefoon,
            string? interneNotities,
            string? externeNotities)
        {
            var call = db.InterventieCalls.FirstOrDefault(c => c.Id == callId);
            if (call == null) throw new Exception($"InterventieCall with ID {callId} not found");

            call.ContactpersoonNaam = contactpersoonNaam;
            call.ContactpersoonEmail = contactpersoonEmail;
            call.ContactpersoonTelefoonNummer = contactpersoonTelefoon;
            call.InterneNotities = interneNotities;
            call.ExterneNotities = externeNotities;

            db.SaveChanges();
        }
    }
}
