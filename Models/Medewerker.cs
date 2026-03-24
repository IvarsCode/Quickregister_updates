using System.ComponentModel.DataAnnotations;

namespace QuickRegister.Models;

public class Medewerker
{
    public int Id { get; set; }

    [Required]
    public string Naam { get; set; } = null!;

    public override string ToString() => Naam;
}
