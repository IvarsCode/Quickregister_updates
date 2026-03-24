using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuickRegister.Models;

public class Interventie
{
    public int Id { get; set; }

    [Required]
    public string Machine { get; set; } = null!;

    public int TotaleLooptijd { get; set; }

    [Required]
    public int IdRecentsteCall { get; set; }

    // Fields from Bedrijf
    [Required]
    public int KlantId { get; set; }

    [Required]
    public string BedrijfNaam { get; set; } = null!;
    public string? StraatNaam { get; set; }
    public string? AdresNummer { get; set; }
    public string? Postcode { get; set; }
    public string? Stad { get; set; }
    public string? Land { get; set; }



    public int Afgerond { get; set; } // 0 = false, 1 = true

    public ICollection<InterventieCall> Calls { get; set; } = new List<InterventieCall>();
}
