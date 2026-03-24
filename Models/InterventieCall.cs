using System;
using System.ComponentModel.DataAnnotations;

namespace QuickRegister.Models;

public class InterventieCall
{
    public int Id { get; set; }

    [Required]
    public int InterventieId { get; set; }
    public Interventie Interventie { get; set; } = null!;

    [Required]
    public int MedewerkerId { get; set; }
    public Medewerker Medewerker { get; set; } = null!;

    [Required]
    public string ContactpersoonNaam { get; set; } = null!;

    public string? ContactpersoonEmail { get; set; }
    public string? ContactpersoonTelefoonNummer { get; set; }

    public string? InterneNotities { get; set; }
    public string? ExterneNotities { get; set; }

    public DateTime? StartCall { get; set; }
    public DateTime? EindCall { get; set; }

}
