using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuickRegister.Models;

public class Bedrijf
{
    public int Id { get; set; }

    [Required]
    public int klantId { get; set; }

    [Required]
    public string BedrijfNaam { get; set; } = null!;

    public string? StraatNaam { get; set; }
    public string? AdresNummer { get; set; }
    public string? Postcode { get; set; }
    public string? Stad { get; set; }
    public string? Land { get; set; }
}
