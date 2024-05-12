using System.ComponentModel.DataAnnotations;

namespace KolosMarcin.Models;

public class Medicament
{
    [MaxLength(100)]
    public string Name { get; set; }
    [MaxLength(100)]
    public string Description { get; set; }
    [MaxLength(100)]
    public string Type { get; set; }
}