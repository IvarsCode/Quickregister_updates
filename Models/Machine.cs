using System;
using System.ComponentModel.DataAnnotations;

namespace QuickRegister.Models;

public class Machine
{
    [Required]
    public string MachineNaam { get; set; } = null!;
}
